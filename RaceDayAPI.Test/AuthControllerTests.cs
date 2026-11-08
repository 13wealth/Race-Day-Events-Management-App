using Xunit;
using RaceDay.Contracts;
using RaceDay.API.Models;

namespace RaceDayAPI.Test
{
    public class AuthControllerTests
    {
        [Fact]
        public void RegisterRequest_WithValidData_ShouldInitialize()
        {
            // Arrange & Act
            var registerRequest = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Assert
            Assert.NotNull(registerRequest);
            Assert.Equal("testuser", registerRequest.Username);
            Assert.Equal("test@example.com", registerRequest.Email);
        }

        [Fact]
        public void LoginRequest_WithValidCredentials_ShouldInitialize()
        {
            // Arrange & Act
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "SecurePassword123!"
            };

            // Assert
            Assert.NotNull(loginRequest);
            Assert.Equal("testuser", loginRequest.Username);
            Assert.Equal("SecurePassword123!", loginRequest.Password);
        }

        [Fact]
        public void RaceDayUser_WithValidData_ShouldInitialize()
        {
            // Arrange & Act
            var user = new RaceDayUser
            {
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com"
            };

            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user.UserId);
            Assert.Equal("testuser", user.Username);
        }

        [Fact]
        public void Participant_WithValidData_ShouldInitialize()
        {
            // Arrange & Act
            var participant = new Participant
            {
                ParticipantId = 1,
                FirstName = "John",
                LastName = "Doe",
                ProfileId = 1
            };

            // Assert
            Assert.NotNull(participant);
            Assert.Equal("John", participant.FirstName);
            Assert.Equal("Doe", participant.LastName);
        }
    }
}
