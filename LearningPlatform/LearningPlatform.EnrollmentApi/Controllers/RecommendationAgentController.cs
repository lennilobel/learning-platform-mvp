using LearningPlatform.Recommendations.Agent.LearningPlatform.Recommendations.Agent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningPlatform.EnrollmentApi.Controllers
{
	[ApiController]
	[Authorize]
	public class RecommendationAgentController : ControllerBase
	{
		private readonly IRecommendationAgentService _recommendationAgentService;

		public RecommendationAgentController(
			IRecommendationAgentService recommendationAgentService)
		{
			this._recommendationAgentService = recommendationAgentService;
		}

		[HttpPost]
		[Route("api/recommendation-agent")]
		public async Task<IActionResult> Run(RecommendationAgentRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Prompt))
			{
				return BadRequest("Prompt is required.");
			}

			var response = await this._recommendationAgentService.RunAsync(request.Prompt);

			return Ok(new RecommendationAgentResponse
			{
				Response = response
			});
		}
	}

	public sealed class RecommendationAgentRequest
	{
		public required string Prompt { get; init; }
	}

	public sealed class RecommendationAgentResponse
	{
		public required string Response { get; init; }
	}
}
