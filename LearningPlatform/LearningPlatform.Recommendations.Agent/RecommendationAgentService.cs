using LearningPlatform.Recommendations.Agent.LearningPlatform.Recommendations.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class RecommendationAgentService : IRecommendationAgentService
	{
		private readonly string _endpoint;
		private readonly string _deploymentName;
		private readonly RecommendationTools _recommendationTools;
		private readonly ILogger<RecommendationAgentService> _logger;

		public RecommendationAgentService(
			string endpoint,
			string deploymentName,
			RecommendationTools recommendationTools,
			ILogger<RecommendationAgentService> logger)
		{
			this._endpoint = endpoint;
			this._deploymentName = deploymentName;
			this._recommendationTools = recommendationTools;
			this._logger = logger;
		}

		public async Task<string> RunAsync(string prompt)
		{
			// Create a read-only agent. EnrollmentTools is intentionally omitted,
			// so this stateless HTTP endpoint cannot perform side-effecting enrollment.
			var agent =
				RecommendationAgentFactory.Create(
					this._endpoint,
					this._deploymentName,
					this._recommendationTools);

			// Run the live agent request.
			var response = await agent.RunAsync(prompt);

			// Evaluate the response that was just produced.
			await this.EvaluateOnlineAsync(agent, prompt, response);

			return response.ToString();
		}

		private async Task EvaluateOnlineAsync(ChatClientAgent agent, string prompt, AgentResponse response)
		{
			var evaluator =
				// LocalEvaluator runs checks locally; it does not make a second LLM-as-judge call.
				new LocalEvaluator(
					CreateResponseNotEmptyCheck(),
					CreateRecommendationToolUsageCheck(prompt));

			// EvaluateAsync evaluates the already-generated agent response against the original prompt; it does not run the agent again.
			// It builds an EvalItem from the captured response/conversation/tool calls and feeds that to the local evaluator.
			var results = await agent.EvaluateAsync(
				responses: [response],
				queries: [prompt],
				evaluator,
				evalName: "Recommendation Agent - Online Evaluation");

			this._logger.LogInformation("Online evaluation: Passed={Passed}, Failed={Failed}, Total={Total}, AllPassed={AllPassed}",
				results.Passed,
				results.Failed,
				results.Total,
				results.AllPassed);

			foreach (var item in results.Items)
			{
				foreach (var metric in item.Metrics.Values)
				{
					this._logger.LogInformation("Online evaluation metric: {Metric}, Failed={Failed}, Reason={Reason}",
						metric.Name,
						metric.Interpretation?.Failed,
						metric.Reason);
				}
			}
		}

		private static EvalCheck CreateResponseNotEmptyCheck()
		{
			const string checkName = "response_not_empty";

			return FunctionEvaluator.Create(checkName, (EvalItem item) =>
			{
				var hasAssistantResponse =
					item.Conversation
						.Where(message => message.Role == ChatRole.Assistant)
						.Any(message => !string.IsNullOrWhiteSpace(message.Text));

				return new EvalCheckResult(
					Passed: hasAssistantResponse,
					Reason: hasAssistantResponse ?
						"Agent returned a non-empty response." :
						"Agent returned an empty response.",
					checkName);
			});
		}

		private static EvalCheck CreateRecommendationToolUsageCheck(string prompt)
		{
			const string checkName = "recommendation_tool_usage";

			return FunctionEvaluator.Create(checkName, (EvalItem item) =>
			{
				if (!prompt.Contains("recommend", StringComparison.OrdinalIgnoreCase))
				{
					return new EvalCheckResult(Passed: true, Reason: "Prompt did not request a recommendation.", checkName);
				}

				var toolCalled =
					item.Conversation
						.SelectMany(message => message.Contents)
						.OfType<FunctionCallContent>()
						.Any(content =>
							content.Name == nameof(RecommendationTools.GetRecommendations));

				return new EvalCheckResult(
					Passed: toolCalled,
					Reason: toolCalled ?
						"Recommendation request used GetRecommendations." :
						"Recommendation request did not use GetRecommendations.",
					checkName);
			});
		}
	}
}
