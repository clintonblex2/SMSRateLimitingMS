using FluentAssertions;
using SMSRateLimitingMS.Infrastructure.Persistence;

namespace SMSRateLimitingMS.Tests.Infrastructure
{
    public class InMemoryRateLimitRepositoryTests
    {
        private readonly InMemoryRateLimitRepository _repository;

        public InMemoryRateLimitRepositoryTests()
        {
            _repository = new InMemoryRateLimitRepository();
        }

        [Fact]
        public async Task GetOrCreateAsync_ShouldCreateNewRateLimit()
        {
            // Arrange
            var id = "test";
            var maximumRequests = 5;
            var window = TimeSpan.FromSeconds(1);

            // Act
            var rateLimit = await _repository.GetOrCreateAsync(id, maximumRequests, window);

            // Assert
            rateLimit.Should().NotBeNull();
            rateLimit.Id.Should().Be(id);
            var stats = rateLimit.GetStats();
            stats.MaxRequests.Should().Be(maximumRequests);
            stats.WindowDuration.Should().Be(window);
        }

        [Fact]
        public async Task GetOrCreateAsync_ShouldReturnExistingRateLimit()
        {
            // Arrange
            var id = "test";
            var first = await _repository.GetOrCreateAsync(id, 5, TimeSpan.FromSeconds(1));

            // Act
            var second = await _repository.GetOrCreateAsync(id, 5, TimeSpan.FromSeconds(1));

            // Assert
            second.Should().BeSameAs(first);
        }

        [Fact]
        public async Task CleanupInactiveAsync_ShouldRemoveInactiveRateLimits()
        {
            // Arrange
            var id = "test";
            await _repository.GetOrCreateAsync(id, 5, TimeSpan.FromSeconds(1));
            Thread.Sleep(1100); // Wait for rate to become inactive

            // Act
            await _repository.CleanupInactiveAsync(TimeSpan.FromSeconds(1));
            var stats = await _repository.GetRateLimitStatistics(id);

            // Assert
            stats.Should().BeNull();
        }
    }
}
