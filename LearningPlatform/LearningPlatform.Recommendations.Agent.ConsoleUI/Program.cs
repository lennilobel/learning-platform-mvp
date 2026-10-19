#define HITL	// Human-In-The-Loop

using LearningPlatform.BusinessLogic.Enrollments;
using LearningPlatform.DataLayer;
using LearningPlatform.DataLayer.Enrollment;
using LearningPlatform.DataLayer.Recommendations;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace LearningPlatform.Recommendations.Agent.ConsoleUI
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			const string SourceName = "LearningPlatform.Recommendations.Agent";

			var builder = Host.CreateApplicationBuilder(args);

			builder.Services.AddDbContext<LearningPlatformDbContext>(options =>
				options.UseSqlServer(
					builder.Configuration.GetSection("ConnectionStrings")["LearningPlatform"]));

			builder.Services.AddSingleton<ICourseRanker, WeightedCourseRanker>();
			builder.Services.AddSingleton<IEligibilityService, EligibilityService>();
			builder.Services.AddSingleton<IRecommendationService, RecommendationService>();

			builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
			builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
			builder.Services.AddScoped<ILearningDataRepository, SqlLearningDataRepository>();

			builder.Services.AddScoped<EnrollmentTools>();
			builder.Services.AddScoped<RecommendationTools>();

			var endpoint = builder.Configuration["Foundry:ProjectEndpoint"]
				?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

			var deploymentName = builder.Configuration["Foundry:Model"]
				?? throw new InvalidOperationException("Foundry:Model is not configured.");

			using var host = builder.Build();
			using var scope = host.Services.CreateScope();

			var recommendationTools = scope.ServiceProvider.GetRequiredService<RecommendationTools>();
			var enrollmentTools = scope.ServiceProvider.GetRequiredService<EnrollmentTools>();

			// Give us a ChatClientAgent for our composed agent pipeline. Including enrollment tools enables HITL flow.
			var baseAgent = RecommendationAgentFactory.Create(endpoint, deploymentName, recommendationTools, enrollmentTools);

			var agent = baseAgent
				.AsBuilder()                                // Get an AIAgentBuilder so we can build the agent pipeline around the ChatClientAgent.
				.UseOpenTelemetry(							// Add OpenTelementry instrumentation to the AI agent.
					sourceName: SourceName,
					configure: options =>
						options.EnableSensitiveData = true) // Useful for controlled development; generally avoid in production.
				.Build();                                   // Get an AIAgent representing a composed agent pipeline that wraps the original ChatClientAgent and adds telemetry.

			// Configure a tracer provider that listens for the agent's telemetry and exports completed activities to the console.
			using var tracerProvider =
				Sdk.CreateTracerProviderBuilder()
					.AddSource(SourceName)
					// If custom color-coding isn't needed, the standard console exporter could be used instead:
					//.AddConsoleExporter()
					.AddProcessor(new SimpleActivityExportProcessor(new ColoredConsoleActivityExporter(ConsoleColor.Cyan)))
					.Build();

#if HITL
			// This agent uses HITL (human-in-the-loop) involvement

			// Use a session so the approval response can continue the same agent interaction.
			var session = await agent.CreateSessionAsync();

			var response = await agent.RunAsync("For learner 1, recommend the best course and enroll the learner in it.", session);

			// Retrieve whatever approvals may be required to continue the flow.
			var approvalRequests = response.Messages
				.SelectMany(message => message.Contents)
				.OfType<ToolApprovalRequestContent>()
				.ToList();

			// Process any and all approval requests.
			while (approvalRequests.Count > 0)
			{
				// Create a list to accumulate approval response messages.
				var approvalResponses = new List<ChatMessage>();

				// Iterate each approval request.
				foreach (var approvalRequest in approvalRequests)
				{
					// Obtain the function call based on the request's tool call reference.
					// Note: The hard cast is acceptable here because we deliberately wrapped a function tool (EnrollLearner) with approval.
					// In a generic approval-processing infrastructure that could encounter other tool-call types, we'd pattern-match instead.
					var functionCall = (FunctionCallContent)approvalRequest.ToolCall;

					Console.WriteLine();
					Console.WriteLine("APPROVAL REQUIRED");
					Console.WriteLine($"Tool: {functionCall.Name}");

					// Show the arguments for the function call.
					if (functionCall.Arguments is not null)
					{
						foreach (var argument in functionCall.Arguments)
						{
							Console.WriteLine($"  {argument.Key}: {argument.Value}");
						}
					}

					// Obtain approval from the humanoid.
					Console.Write("Approve? (Y/N): ");
					var approved = Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true;

					approvalResponses.Add(new ChatMessage(ChatRole.User, [approvalRequest.CreateResponse(approved)]));
				}

				// Continue running the agent after obtaining the approval response(s).
				response = await agent.RunAsync(approvalResponses, session);

				// Retrieve any new approvals that may be required to continue the flow.
				approvalRequests = response.Messages
					.SelectMany(message => message.Contents)
					.OfType<ToolApprovalRequestContent>()
					.ToList();
			}

			Console.WriteLine();
			Console.WriteLine(response);
#else
			// This is a simple agent run without an explicitly managed session (no HITL).
			var response =
				await agent.RunAsync(
					"""
					For learner 1, recommend the best two courses.

					Also tell me:
					- the learner's target role,
					- the learner's existing skills,
					- the exact prerequisite course IDs for each recommendation,
					- and all skills developed by each recommended course.

					Explain why the first recommendation ranks above the second.
					""");

			Console.WriteLine(response);
#endif
		}
	}
}
