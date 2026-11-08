using Xunit;
using RaceDay.Contracts;
using RaceDay.API.Models;

namespace RaceDayAPI.Test
{
    public class EventControllerTests
    {
        [Fact]
        public void RaceEvent_WithValidData_ShouldInitialize()
        {
            // Arrange & Act
            var raceEvent = new RaceEvent
            {
                EventId = 1,
                EventName = "Marathon 2026",
                EventDate = DateTime.Now.AddDays(30),
                Location = "City Center"
            };

            // Assert
            Assert.NotNull(raceEvent);
            Assert.Equal(1, raceEvent.EventId);
            Assert.Equal("Marathon 2026", raceEvent.EventName);
        }

        [Fact]
        public void RaceEvent_WithMultipleInstances_ShouldCompareCorrectly()
        {
            // Arrange
            var event1 = new RaceEvent { EventId = 1, EventName = "Marathon 2026", EventDate = DateTime.Now.AddDays(30) };
            var event2 = new RaceEvent { EventId = 2, EventName = "Half Marathon 2026", EventDate = DateTime.Now.AddDays(60) };

            // Act & Assert
            Assert.NotEqual(event1.EventId, event2.EventId);
            Assert.NotEqual(event1.EventName, event2.EventName);
        }

        [Fact]
        public void EventRequest_WithValidData_ShouldInitialize()
        {
            // Arrange & Act
            var eventRequest = new EventRequest
            {
                EventName = "New Marathon",
                Location = "City Center"
            };

            // Assert
            Assert.NotNull(eventRequest);
            Assert.Equal("New Marathon", eventRequest.EventName);
            Assert.Equal("City Center", eventRequest.Location);
        }

        [Fact]
        public void RaceEvent_WithDescription_ShouldStore()
        {
            // Arrange & Act
            var raceEvent = new RaceEvent
            {
                EventId = 1,
                EventName = "Marathon 2026",
                EventDate = DateTime.Now.AddDays(30),
                Location = "City Center",
                Description = "Annual city marathon event"
            };

            // Assert
            Assert.NotNull(raceEvent.Description);
            Assert.Equal("Annual city marathon event", raceEvent.Description);
        }
    }
}
