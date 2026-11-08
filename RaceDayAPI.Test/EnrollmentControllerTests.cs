using Xunit;
using RaceDay.API.DTO;
using RaceDay.API.Models;

namespace RaceDayAPI.Test
{
    public class EnrollmentControllerTests
    {
        [Fact]
        public void Enrollment_WithValidData_ShouldInitialize()
        {
            // Arrange & Act
            var enrollment = new Enrollment
            {
                EnrollmentId = 1,
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                EntryDate = DateTime.Now
            };

            // Assert
            Assert.NotNull(enrollment);
            Assert.Equal(1, enrollment.EnrollmentId);
            Assert.Equal(1, enrollment.ParticipantId);
            Assert.Equal(1, enrollment.EventId);
        }

        [Fact]
        public void Enrollment_WithStatus_ShouldStoreCorrectly()
        {
            // Arrange & Act
            var enrollment = new Enrollment
            {
                EnrollmentId = 1,
                ParticipantId = 1,
                EventId = 1,
                CategoryId = 1,
                EntryDate = DateTime.Now,
                Status = "registered"
            };

            // Assert
            Assert.Equal("registered", enrollment.Status);
        }

        [Fact]
        public void EnrollmentRequest_WithValidCategory_ShouldInitialize()
        {
            // Arrange & Act
            var enrollmentRequest = new EnrollmentRequest
            {
                CategoryId = 1
            };

            // Assert
            Assert.NotNull(enrollmentRequest);
            Assert.Equal(1, enrollmentRequest.CategoryId);
        }

        [Fact]
        public void MultipleEnrollments_ForSameEvent_ShouldDifferByParticipant()
        {
            // Arrange
            var enrollment1 = new Enrollment { EnrollmentId = 1, ParticipantId = 1, EventId = 1, CategoryId = 1, EntryDate = DateTime.Now };
            var enrollment2 = new Enrollment { EnrollmentId = 2, ParticipantId = 2, EventId = 1, CategoryId = 1, EntryDate = DateTime.Now };

            // Act & Assert
            Assert.Equal(1, enrollment1.EventId);
            Assert.Equal(1, enrollment2.EventId);
            Assert.NotEqual(enrollment1.ParticipantId, enrollment2.ParticipantId);
        }
    }
}
