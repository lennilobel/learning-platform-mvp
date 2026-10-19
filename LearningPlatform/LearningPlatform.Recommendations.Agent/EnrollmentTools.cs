using LearningPlatform.BusinessLogic.Enrollments;
using LearningPlatform.Shared;
using System.ComponentModel;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class EnrollmentTools
	{
		private readonly IEnrollmentService _enrollmentService;

		public EnrollmentTools(
			IEnrollmentService enrollmentService)
		{
			this._enrollmentService = enrollmentService;
		}

		[Description("Enrolls a learner in a course.")]
		public async Task<string> EnrollLearner(
			[Description("The learner ID.")] long learnerId,
			[Description("The course ID.")] long courseId,
			CancellationToken cancellationToken = default)
		{
			var enrollment = new Enrollment
			{
				LearnerId = learnerId,
				CourseId = courseId
			};

			var result = await this._enrollmentService.CreateEnrollment(enrollment, cancellationToken);

			if (result.ValidationErrors.LearnerId is not null || result.ValidationErrors.CourseId is not null)
			{
				return
					$"Enrollment failed. " +
					$"LearnerId: {result.ValidationErrors.LearnerId ?? "valid"}. " +
					$"CourseId: {result.ValidationErrors.CourseId ?? "valid"}.";
			}

			if (result.IsDuplicate)
			{
				return $"Learner {learnerId} is already enrolled in course {courseId}.";
			}

			return $"Learner {learnerId} has been enrolled in course {courseId}.";
		}
	}
}
