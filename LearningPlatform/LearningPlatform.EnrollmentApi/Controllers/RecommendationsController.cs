using LearningPlatform.BusinessLogic.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningPlatform.EnrollmentApi.Controllers
{
	[ApiController]
	[Authorize]
	public class RecommendationsController : ControllerBase
	{
		private readonly IRecommendationApplicationService _recommendationApplicationService;

		public RecommendationsController(
			IRecommendationApplicationService recommendationApplicationService)
		{
			this._recommendationApplicationService = recommendationApplicationService;
		}

		[HttpGet]
		[Route("api/learners/{learnerId:long}/recommendations")]
		public async Task<IActionResult> GetRecommendations(long learnerId, int maxResults = 2, CancellationToken cancellationToken = default)
		{
			if (maxResults <= 0)
			{
				return BadRequest("maxResults must be greater than zero.");
			}

			try
			{
				var recommendations = await this._recommendationApplicationService.GetRecommendationsAsync(learnerId, maxResults, cancellationToken);

				return Ok(recommendations);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (ArgumentOutOfRangeException)
			{
				return BadRequest("maxResults must be greater than zero.");
			}
		}
	}
}
