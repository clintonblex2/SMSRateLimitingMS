using FluentAssertions;
using SMSRateLimitingMS.Domain.Entities;

namespace SMSRateLimitingMS.Tests.Domain
{
    public class RateLimitTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var rateLimit = new RateLimit("test", 5, TimeSpan.FromSeconds(1));

            // Assert
            rateLimit.Id.Should().Be("test");
            var stats = rateLimit.GetStats();
            stats.MaxRequests.Should().Be(5);
            stats.WindowDuration.Should().Be(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void TryIncrementWindowCounter_ShouldUpdateLastAccessed()
        {
            // Arrange
            var rateLimit = new RateLimit("test", 5, TimeSpan.FromSeconds(1));
            var initialLastAccessed = rateLimit.LastAccessed;
            Thread.Sleep(10);

            // Act
            rateLimit.TryIncrementWindowCounter();

            // Assert
            rateLimit.LastAccessed.Should().BeAfter(initialLastAccessed);
        }

        [Fact]
        public void IsInactive_WhenRecentlyAccessed_ShouldReturnFalse()
        {
            // Arrange
            var rateLimit = new RateLimit("test", 5, TimeSpan.FromSeconds(1));
            rateLimit.TryIncrementWindowCounter();

            // Act
            var isInactive = rateLimit.IsInactive(TimeSpan.FromSeconds(1));

            // Assert
            isInactive.Should().BeFalse();
        }

        [Fact]
        public void IsInactive_WhenNotRecentlyAccessed_ShouldReturnTrue()
        {
            // Arrange
            var rateLimit = new RateLimit("test", 5, TimeSpan.FromSeconds(1));
            rateLimit.TryIncrementWindowCounter();
            Thread.Sleep(1200); // Wait longer than the threshold of 1 sec

            // Act
            var isInactive = rateLimit.IsInactive(TimeSpan.FromSeconds(1));

            // Assert
            isInactive.Should().BeTrue();
        }
    }
}
