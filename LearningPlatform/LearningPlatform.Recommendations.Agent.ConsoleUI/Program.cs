using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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

			var endpoint = builder.Configuration["Foundry:ProjectEndpoint"]
				?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

			var deploymentName = builder.Configuration["Foundry:Model"]
				?? throw new InvalidOperationException("Foundry:Model is not configured.");

			var eligibilityService = new EligibilityService();
			var courseRanker = new WeightedCourseRanker();

			var recommendationService = new RecommendationService(eligibilityService, courseRanker);

			var learningData = new DemoLearningData();

			var recommendationTools = new RecommendationTools(recommendationService, learningData);

			// Give us a ChatClientAgent for our composed agent pipeline.
			var baseAgent = RecommendationAgentFactory.Create(endpoint, deploymentName, recommendationTools);

			var agent =
				baseAgent
				.AsBuilder()                                // Gives us an AIAgentBuilder so we can build the agent pipeline.
				.UseOpenTelemetry(
					sourceName: SourceName,
					configure: options =>
						options.EnableSensitiveData = true) // Useful for controlled development; generally avoid in production.
				.Build();                                   // Gives us an AIAgent representing a composed agent pipeline
															// that wraps the original ChatClientAgent and adds telemetry.

			// Configure a tracer provider that listens for the agent's telemetry and exports completed activities to the console.
			using var tracerProvider =
				Sdk.CreateTracerProviderBuilder()
					.AddSource(SourceName)
					// If custom color-coding isn't needed, the standard console exporter could be used instead:
					//.AddConsoleExporter()
					.AddProcessor(new SimpleActivityExportProcessor(new ColoredConsoleActivityExporter(ConsoleColor.Cyan)))
					.Build();

			//// This is a simple agent run without an explicitly managed session.
			//var response =
			//	await agent.RunAsync(
			//		"""
			//		For learner 1, recommend the best two courses.

			//		Also tell me:
			//		- the learner's target role,
			//		- the learner's existing skills,
			//		- the exact prerequisite course IDs for each recommendation,
			//		- and all skills developed by each recommended course.

			//		Explain why the first recommendation ranks above the second.
			//		""");

			//Console.WriteLine(response);

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
		}
	}
}
