using LearningPlatform.Recommendations;
using LearningPlatform.Recommendations.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LearningPlatform.DataLayer.Recommendations
{
	public sealed class SqlLearningDataRepository : ILearningDataRepository
	{
		private readonly LearningPlatformDbContext _context;

		public SqlLearningDataRepository(LearningPlatformDbContext context)
		{
			this._context = context;
		}

		public async Task<LearnerContext> GetLearnerContextAsync(long learnerId, CancellationToken cancellationToken = default)
		{
			var results = await this._context
				.Set<LearnerContextResult>()
				.FromSqlInterpolated($"EXEC dbo.GetLearnerContext @LearnerId = {learnerId}")
				.AsNoTracking()
				.ToListAsync(cancellationToken);

			var result = results.SingleOrDefault()
				?? throw new KeyNotFoundException($"Learner {learnerId} was not found.");

			return new LearnerContext
			{
				LearnerId = result.LearnerId,
				TargetRole = result.TargetRole,
				CompletedCourseIds = DeserializeSet<long>(result.CompletedCourseIds),
				Skills = DeserializeSet<string>(result.Skills, StringComparer.OrdinalIgnoreCase)
			};
		}

		// POC: Load the full catalog. A production implementation should query a reduced candidate set.
		public async Task<IReadOnlyList<Course>> GetCourseCatalogAsync(CancellationToken cancellationToken = default)
		{
			var results = await this._context
				.Set<CourseResult>()
				.FromSqlRaw("EXEC dbo.GetCourseCatalog")
				.AsNoTracking()
				.ToListAsync(cancellationToken);

			return results
				.Select(TransformCourseResult)
				.ToList();
		}

		public async Task<Course> GetCourseAsync(long courseId, CancellationToken cancellationToken = default)
		{
			var results = await this._context
				.Set<CourseResult>()
				.FromSqlInterpolated($"EXEC dbo.GetCourse @CourseId = {courseId}")
				.AsNoTracking()
				.ToListAsync(cancellationToken);

			var result = results.SingleOrDefault()
				?? throw new KeyNotFoundException($"Course {courseId} was not found.");

			return TransformCourseResult(result);
		}

		private static Course TransformCourseResult(CourseResult result)
		{
			return new Course
			{
				CourseId = result.CourseId,
				Title = result.Title,
				Role = result.Role,
				IsActive = result.IsActive,
				PrerequisiteCourseIds = DeserializeSet<long>(result.PrerequisiteCourseIds),
				Skills = DeserializeSet(result.Skills, StringComparer.OrdinalIgnoreCase)
			};
		}

		private static IReadOnlySet<T> DeserializeSet<T>(string json, IEqualityComparer<T>? comparer = null)
		{
			var values = JsonSerializer.Deserialize<T[]>(json) ?? [];

			return comparer is null
				? new HashSet<T>(values)
				: new HashSet<T>(values, comparer);
		}
	}
}
