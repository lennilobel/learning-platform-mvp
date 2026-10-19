using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace LearningPlatform.Recommendations.Agent.Evaluation.ConsoleUI
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			// Build the AI agent.

			var builder = Host.CreateApplicationBuilder(args);

			var endpoint = builder.Configuration["Foundry:ProjectEndpoint"]
				?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

			var deploymentName = builder.Configuration["Foundry:Model"]
				?? throw new InvalidOperationException("Foundry:Model is not configured.");

			var eligibilityService = new EligibilityService();
			var courseRanker = new WeightedCourseRanker();

			var recommendationService = new RecommendationService(eligibilityService, courseRanker);

			var learningDataRepository = new DemoLearningDataRepository();

			var recommendationTools = new RecommendationTools(recommendationService, learningDataRepository);

			var agent = RecommendationAgentFactory.Create(endpoint, deploymentName, recommendationTools);	// Omitting enrollment tools suppresses the HITL flow.

			var localResults = await RunDeterministicEvaluation(agent);
			ShowResults(localResults);
			localResults.AssertAllPassed();

			//// TODO: Revisit Foundry semantic evaluation.
			//// Agent Framework 1.5.0 currently fails while processing Foundry evaluation results.
			//var foundryResults = await RunSemanticEvaluation(endpoint, deploymentName, localResults);
			//ShowResults(foundryResults);
			//foundryResults.AssertAllPassed();
		}

		private static void ShowResults(AgentEvaluationResults results)
		{
			Console.WriteLine($"Provider: {results.ProviderName}");
			Console.WriteLine($"Passed:   {results.Passed}");
			Console.WriteLine($"Failed:   {results.Failed}");
			Console.WriteLine($"Total:    {results.Total}");
			Console.WriteLine($"All pass: {results.AllPassed}");

			var itemCount = 0;
			foreach (var item in results.Items)
			{
				Console.WriteLine($"Item # {++itemCount}");
				var metricCount = 0;
				foreach (var metric in item.Metrics.Values)
				{
					Console.WriteLine($"Metric # {++metricCount}");
					var status = metric.Interpretation?.Failed switch
					{
						true => "FAIL",
						false => "PASS",
						null => "N/A"
					};

					var currentColor = Console.ForegroundColor;
					Console.ForegroundColor = status switch
					{
						"FAIL" => ConsoleColor.Red,
						"PASS" => ConsoleColor.Green,
						_ => currentColor
					};

					Console.WriteLine($"{metric.Name}: {status}");
					Console.WriteLine($"  Reason: {metric.Reason}");

					Console.ForegroundColor = currentColor;
				}
			}
		}


		// =============================================================================================================================
		// Deterministic (local) offline evaluation.
		// =============================================================================================================================

		private static async Task<AgentEvaluationResults> RunDeterministicEvaluation(ChatClientAgent agent)
		{
			var evaluator =
				// LocalEvaluator runs checks locally; it does not make a second LLM-as-judge call.
				new LocalEvaluator(

					// Expect all of these tools to be called.
					EvalChecks.ToolCalledCheck(
						nameof(RecommendationTools.GetLearnerProfile),
						nameof(RecommendationTools.GetRecommendations),
						nameof(RecommendationTools.GetCourseDetails)),

					//// Also check the tool *arguments* for expected values. BUT...
					////  In our Agent Framework 1.5.0 test, ToolCallArgsMatch reported a mismatch for a numeric JsonElement even when the expected numeric value matched.
					////  So we'll handcraft our own custom check methods and use them instead.
					//EvalChecks.ToolCallArgsMatch(),

					// Expect GetLearnerProfile to be called with learnerId = 1.
					CreateFirstToolArgumentCheck<long>(
						toolName: nameof(RecommendationTools.GetLearnerProfile),
						argumentName: "learnerId",
						expectedValue: 1L),

					// Expect GetRecommendations to be called with learnerId = 1.
					CreateFirstToolArgumentCheck<long>(
						toolName: nameof(RecommendationTools.GetRecommendations),
						argumentName: "learnerId",
						expectedValue: 1L),

					// Expect GetRecommendations to be called with maxResults = 2.
					CreateFirstToolArgumentCheck<int>(
						toolName: nameof(RecommendationTools.GetRecommendations),
						argumentName: "maxResults",
						expectedValue: 2),

					// Expect GetCourseDetails to be called at least once with courseId = 200.
					CreateAnyToolArgumentCheck<long>(
						toolName: nameof(RecommendationTools.GetCourseDetails),
						argumentName: "courseId",
						expectedValue: 200L),

					// Expect GetCourseDetails to be called at least once with courseId = 400.
					CreateAnyToolArgumentCheck<long>(
						toolName: nameof(RecommendationTools.GetCourseDetails),
						argumentName: "courseId",
						expectedValue: 400L),

					// Expect all of these keywords to appear in output.
					EvalChecks.KeywordCheck(
						"Azure App Service",
						"Power BI Fundamentals"));

			var query = """
				For learner 1, recommend the best two courses.

				Also tell me:
				- the learner's target role,
				- the learner's existing skills,
				- the exact prerequisite course IDs for each recommendation,
				- and all skills developed by each recommended course.

				Explain why the first recommendation ranks above the second.
			""";

			//// For Agent Framework 1.5.0's ToolCallArgsMatch...
			//// Expected tool calls that check for matching parameter/argument values.
			//var expectedToolCalls = new[]
			//{
			//	new[]
			//	{
			//		// Expect that the GetLearnerProfile tool is called with a learnerId parameter value of 1.
			//		new ExpectedToolCall(
			//			nameof(RecommendationTools.GetLearnerProfile),
			//			new Dictionary<string, object>
			//			{
			//				["learnerId"] = 1L
			//			}
			//		)
			//	}
			//};

			//// For Agent Framework 1.5.0's ToolCallArgsMatch...
			//// Show the argument details for each expected tool call
			//foreach (var expectedCallsForQuery in expectedToolCalls)
			//{
			//	foreach (var expectedCall in expectedCallsForQuery)
			//	{
			//		Console.WriteLine($"Expected tool: {expectedCall.Name}");

			//		if (expectedCall.Arguments is not null)
			//		{
			//			foreach (var argument in expectedCall.Arguments)
			//			{
			//				Console.WriteLine(
			//					$"  {argument.Key} = {argument.Value} " +
			//					$"({argument.Value?.GetType().FullName ?? "null"})");
			//			}
			//		}
			//	}
			//}

			// EvaluateAsync actually runs the agent against the query, captures the conversation/tool calls, and then feeds the
			// resulting EvalItem to the local evaluator.
			var results = await agent.EvaluateAsync(
				[query],
				evaluator,
				evalName: "Recommendation Agent - Local Evaluation"
			//, expectedToolCalls: expectedToolCalls
			);

			//// For Agent Framework 1.5.0's ToolCallArgsMatch...
			//// Show each tool's underlying function call arguments.
			//if (results.InputItems is not null)
			//{
			//	foreach (var item in results.InputItems)
			//	{
			//		foreach (var message in item.Conversation)
			//		{
			//			foreach (var content in message.Contents)
			//			{
			//				if (content is FunctionCallContent functionCall)
			//				{
			//					Console.WriteLine($"Tool: {functionCall.Name}");

			//					if (functionCall.Arguments is not null)
			//					{
			//						foreach (var argument in functionCall.Arguments)
			//						{
			//							if (argument.Value is JsonElement element)
			//							{
			//								Console.WriteLine(
			//									$"  {argument.Key} = {element} " +
			//									$"({argument.Value.GetType().FullName}, {element.ValueKind})");
			//							}
			//							else
			//							{
			//								Console.WriteLine(
			//									$"  {argument.Key} = {argument.Value} " +
			//									$"({argument.Value?.GetType().FullName ?? "null"})");
			//							}
			//						}
			//					}
			//				}
			//			}
			//		}
			//	}
			//}

			return results;
		}

		// Check the first call to a tool for the expected argument value.
		private static EvalCheck CreateFirstToolArgumentCheck<T>(string toolName, string argumentName, T expectedValue)
		{
			var checkName = $"{toolName}_{argumentName}_check";

			return FunctionEvaluator.Create(checkName, (EvalItem item) =>
			{
				var toolCall = item.Conversation
					.SelectMany(message => message.Contents)
					.OfType<FunctionCallContent>()
					.FirstOrDefault(content => content.Name == toolName);

				if (toolCall is null)
				{
					return new EvalCheckResult(Passed: false, Reason: $"Tool not called: {toolName}", checkName);
				}

				if (toolCall.Arguments is null || !toolCall.Arguments.TryGetValue(argumentName, out var argumentValue))
				{
					return new EvalCheckResult(Passed: false, Reason: $"{toolName} did not contain argument: {argumentName}", checkName);
				}

				if (argumentValue is not JsonElement element)
				{
					return new EvalCheckResult(Passed: false, Reason: $"{toolName}.{argumentName} was not a JSON value.", checkName);
				}

				T? actualValue;

				try
				{
					actualValue = element.Deserialize<T>();
				}
				catch (JsonException)
				{
					return new EvalCheckResult(Passed: false, Reason: $"{toolName}.{argumentName} could not be converted to {typeof(T).Name}.", checkName);
				}

				if (!EqualityComparer<T>.Default.Equals(actualValue, expectedValue))
				{
					return new EvalCheckResult(Passed: false, Reason: $"{toolName}.{argumentName} (first call) expected {expectedValue}, actual {actualValue}", checkName);
				}

				return new EvalCheckResult(Passed: true, Reason: $"{toolName}.{argumentName} (first call) matched expected value: {expectedValue}", checkName);
			});
		}

		// Check all calls to a tool for one containing the expected argument value.
		private static EvalCheck CreateAnyToolArgumentCheck<T>(string toolName, string argumentName, T expectedValue)
		{
			var checkName = $"{toolName}_{argumentName}_{expectedValue}_check";

			return FunctionEvaluator.Create(checkName, (EvalItem item) =>
			{
				var toolCalls = item.Conversation
					.SelectMany(message => message.Contents)
					.OfType<FunctionCallContent>()
					.Where(content => content.Name == toolName);

				foreach (var toolCall in toolCalls)
				{
					if (toolCall.Arguments is null || !toolCall.Arguments.TryGetValue(argumentName, out var argumentValue) || argumentValue is not JsonElement element)
					{
						continue;
					}

					T? actualValue;

					try
					{
						actualValue = element.Deserialize<T>();
					}
					catch (JsonException)
					{
						continue;
					}

					if (EqualityComparer<T>.Default.Equals(actualValue, expectedValue))
					{
						return new EvalCheckResult(Passed: true, Reason: $"{toolName}.{argumentName} (any call) contained expected value: {expectedValue}", checkName);
					}
				}

				return new EvalCheckResult(Passed: false, Reason: $"{toolName}.{argumentName} (any call) never contained expected value: {expectedValue}", checkName);
			});
		}


		// =============================================================================================================================
		// Semantic offline evaluation using an LLM as judge.
		// =============================================================================================================================

		private static async Task<AgentEvaluationResults> RunSemanticEvaluation(string endpoint, string deploymentName, AgentEvaluationResults localResults)
		{
			var projectClient = new AIProjectClient(
				new Uri(endpoint),
				new DefaultAzureCredential());

			//// TODO: Revisit Foundry semantic evaluation.
			//// Agent Framework 1.5.0 currently fails while processing Foundry evaluation results.

			var foundryEvaluator = new FoundryEvals(
				projectClient,
				deploymentName,
				FoundryEvals.ResponseCompleteness,
				FoundryEvals.TaskAdherence,
				FoundryEvals.ToolOutputUtilization);

			var foundryResults = await foundryEvaluator.EvaluateAsync(
				items: localResults.InputItems ?? throw new InvalidOperationException("Evaluation input items are unavailable."),
				evalName: "Recommendation Agent - Foundry Evaluation");

			return foundryResults;
		}
	}
}
