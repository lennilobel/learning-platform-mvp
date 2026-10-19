using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public interface ILearningDataRepository
	{
		Task<LearnerContext> GetLearnerContextAsync(long learnerId, CancellationToken cancellationToken = default);

		// POC: Load the full catalog. A production implementation should query a reduced candidate set.
		Task<IReadOnlyList<Course>> GetCourseCatalogAsync(CancellationToken cancellationToken = default);

		Task<Course> GetCourseAsync(long courseId, CancellationToken cancellationToken = default);
	}
}
