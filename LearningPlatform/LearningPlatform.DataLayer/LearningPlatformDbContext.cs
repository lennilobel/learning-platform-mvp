using LearningPlatform.DataLayer.Enrollment;
using LearningPlatform.DataLayer.LearningProgress;
using LearningPlatform.DataLayer.Outbox;
using LearningPlatform.DataLayer.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.DataLayer
{
	public class LearningPlatformDbContext : DbContext
	{
		public LearningPlatformDbContext(DbContextOptions<LearningPlatformDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Keyless entity type
			modelBuilder.Entity<CreateEnrollmentResult>().HasNoKey();
			modelBuilder.Entity<OutboxMessage>().HasNoKey();
			modelBuilder.Entity<ProcessEnrollmentCreatedResult>().HasNoKey();
			modelBuilder.Entity<LearnerContextResult>().HasNoKey();
			modelBuilder.Entity<CourseResult>().HasNoKey();
		}
	}
}
