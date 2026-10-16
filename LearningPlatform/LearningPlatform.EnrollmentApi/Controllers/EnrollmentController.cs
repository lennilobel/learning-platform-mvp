using LearningPlatform.BusinessLogic.Enrollments;
using LearningPlatform.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace LearningPlatform.EnrollmentApi.Controllers
{
	[ApiController]
	[Authorize]
	public class EnrollmentController : ControllerBase
	{
		private readonly IEnrollmentService _enrollmentService;

		public EnrollmentController(IEnrollmentService enrollmentService)
		{
			this._enrollmentService = enrollmentService;
		}

		[HttpPost]
		[Route("api/enrollments")]
		[RequiredScope("Enrollment.Write")]
		public async Task<IActionResult> CreateEnrollment(Enrollment enrollment, CancellationToken cancellationToken)
		{
			var result = await this._enrollmentService.CreateEnrollment(enrollment, cancellationToken);

			if (result.IsDuplicate)
			{
				return Conflict(result);
			}

			return Ok(result);
		}
	}
}
