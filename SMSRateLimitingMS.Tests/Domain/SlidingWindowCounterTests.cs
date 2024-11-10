using FluentAssertions;
using SMSRateLimitingMS.Domain.Entities;

namespace SMSRateLimitingMS.Tests.Domain
{
    public class SlidingWindowCounterTests
    {
        [Fact]
        public void TryIncrementCounter_WhenBelowLimit_ShouldReturnTrue()
        {
            // Arrange
            var counter = new SlidingWindowCounter(TimeSpan.FromSeconds(1), 5);

            // Act
            var result = counter.TryIncrementCounter();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void TryIncrementCounter_WhenAtLimit_ShouldReturnFalse()
        {
            // Arrange
            var counter = new SlidingWindowCounter(TimeSpan.FromSeconds(1), 5);
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();

            // Act
            var result = counter.TryIncrementCounter();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetStats_ShouldReturnCorrectCounterStats()
        {
            // Arrange
            var window = TimeSpan.FromSeconds(1);
            var maximumRequests = 5;
            var counter = new SlidingWindowCounter(window, maximumRequests);
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();

            // Act
            var stats = counter.GetStats();

            // Assert
            stats.CurrentCount.Should().Be(3);
            stats.RemainingCapacity.Should().Be(maximumRequests - 3);
            stats.MaxRequests.Should().Be(maximumRequests);
            stats.WindowDuration.Should().Be(window);
        }

        [Fact]
        public void GetStats_AfterWindowExpires_ShouldResetCounter()
        {
            // Arrange
            var counter = new SlidingWindowCounter(TimeSpan.FromMilliseconds(400), 10);
            counter.TryIncrementCounter();
            counter.TryIncrementCounter();

            // Act
            Thread.Sleep(500); // Wait for window to expire
            var stats = counter.GetStats();

            // Assert
            stats.CurrentCount.Should().Be(0);
            stats.RemainingCapacity.Should().Be(10);
        }
    }
}