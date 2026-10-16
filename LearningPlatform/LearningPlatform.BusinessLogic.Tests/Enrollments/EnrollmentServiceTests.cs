using LearningPlatform.BusinessLogic.Enrollments;
using LearningPlatform.DataLayer.Enrollment;
using LearningPlatform.Shared;
using Microsoft.Extensions.Logging;
using Moq;

namespace LearningPlatform.BusinessLogic.Tests.Enrollments
{
	public class EnrollmentServiceTests
	{
		private readonly Mock<ILogger<EnrollmentService>> _logger;
		private readonly Mock<IEnrollmentRepository> _enrollmentRepository;
		private readonly EnrollmentService _enrollmentService;

		public EnrollmentServiceTests()
		{
			this._logger = new Mock<ILogger<EnrollmentService>>();
			this._enrollmentRepository = new Mock<IEnrollmentRepository>();

			this._enrollmentService = new EnrollmentService(
				this._logger.Object,
				this._enrollmentRepository.Object);
		}

		[Fact]
		public async Task CreateEnrollment_ValidEnrollment_ReturnsNotDuplicate()
		{
			// Arrange
			var enrollment = new Enrollment
			{
				LearnerId = 1,
				CourseId = 2
			};

			this._enrollmentRepository
				.Setup(repository => repository.CreateEnrollment(
					It.Is<EnrollmentCreatedEvent>(enrollmentCreatedEvent =>
						enrollmentCreatedEvent.LearnerId == enrollment.LearnerId &&
						enrollmentCreatedEvent.CourseId == enrollment.CourseId),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(false);

			// Act
			var result = await this._enrollmentService.CreateEnrollment(
				enrollment,
				CancellationToken.None);

			// Assert
			Assert.False(result.IsDuplicate);

			this._enrollmentRepository.Verify(
				repository => repository.CreateEnrollment(
					It.IsAny<EnrollmentCreatedEvent>(),
					It.IsAny<CancellationToken>()),
				Times.Once);
		}

		[Fact]
		public async Task CreateEnrollment_DuplicateEnrollment_ReturnsDuplicate()
		{
			// Arrange
			var enrollment = new Enrollment
			{
				LearnerId = 1,
				CourseId = 2
			};

			this._enrollmentRepository
				.Setup(repository => repository.CreateEnrollment(
					It.IsAny<EnrollmentCreatedEvent>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			// Act
			var result = await this._enrollmentService.CreateEnrollment(
				enrollment,
				CancellationToken.None);

			// Assert
			Assert.True(result.IsDuplicate);
		}

		[Fact]
		public async Task CreateEnrollment_InvalidLearner_ReturnsValidationErrorWithoutCallingRepository()
		{
			// Arrange
			var enrollment = new Enrollment
			{
				LearnerId = 666,
				CourseId = 2
			};

			// Act
			var result = await this._enrollmentService.CreateEnrollment(
				enrollment,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result.ValidationErrors);
			Assert.NotNull(result.ValidationErrors.LearnerId);

			this._enrollmentRepository.Verify(
				repository => repository.CreateEnrollment(
					It.IsAny<EnrollmentCreatedEvent>(),
					It.IsAny<CancellationToken>()),
				Times.Never);
		}
	}
}