using FluentAssertions;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Infrastructure.Persistence;

namespace SMSRateLimitingMS.Tests.Infrastructure
{
    public class InMemoryRateLimitHistoryRepositoryTests
    {
        private readonly InMemoryRateLimitHistoryRepository _repository;

        public InMemoryRateLimitHistoryRepositoryTests()
        {
            _repository = new InMemoryRateLimitHistoryRepository();
        }

        [Fact]
        public async Task RecordMessageRateAsync_ShouldAddNewRecord()
        {
            // Arrange
            var phoneNumber = "+1234567890";
            var timestamp = DateTime.UtcNow;
            
            // Act
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp, true);

            // Get records to verify
            var records = await _repository.GetMessageRatesAsync(
                phoneNumber,
                timestamp.AddMinutes(-1),
                timestamp.AddMinutes(1),
                TimeSpan.FromSeconds(1));

            // Assert
            records.Should().NotBeEmpty();
            var record = records.First();
            record.PhoneNumber.Should().Be(phoneNumber);
            record.TotalRequests.Should().Be(1);
            record.RejectedRequests.Should().Be(0);
        }

        [Fact]
        public async Task RecordMessageRateAsync_WhenFailed_ShouldIncrementRejectedRequests()
        {
            // Arrange
            var phoneNumber = "+1234567890";
            var timestamp = DateTime.UtcNow;

            // Act
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp, false);

            // Get records to verify
            var records = await _repository.GetMessageRatesAsync(
                phoneNumber,
                timestamp.AddMinutes(-1),
                timestamp.AddMinutes(1),
                TimeSpan.FromSeconds(1));

            // Assert
            records.Should().NotBeEmpty();
            var record = records.First();
            record.RejectedRequests.Should().Be(1);
            record.TotalRequests.Should().Be(1);
        }

        [Fact]
        public async Task RecordMessageRateAsync_ShouldStoreMessageRate()
        {
            // Arrange
            var phoneNumber = "+231234567890";
            var timestamp = DateTime.UtcNow;
            var wasSuccessful = true;

            // Act
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp, wasSuccessful);

            var rates = await _repository.GetMessageRatesAsync(phoneNumber, timestamp.AddMinutes(-1), timestamp.AddMinutes(1), TimeSpan.FromSeconds(1));

            // Assert
            Assert.Single(rates);
            var rate = rates.First();
            Assert.Equal(phoneNumber, rate.PhoneNumber);
            Assert.Equal(1, rate.TotalRequests);
            Assert.Equal(0, rate.RejectedRequests);
        }

        [Fact]
        public async Task GetMessageRatesAsync_ShouldRespectTimeRange()
        {
            // Arrange
            var phoneNumber = "+1234567890";
            var timestamp = DateTime.UtcNow;

            // Add records at different times
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp.AddMinutes(-2), true);
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp, true);
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp.AddMinutes(2), true);

            // Act
            var records = await _repository.GetMessageRatesAsync(
                phoneNumber,
                timestamp.AddMinutes(-1),
                timestamp.AddMinutes(1),
                TimeSpan.FromSeconds(1));

            // Assert
            records.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMessageRatesAsync_WithNoPhoneNumber_ShouldReturnAccountStats()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;

            // Add records for different phone numbers
            await _repository.RecordMessageRateAsync(Constants.GLOBAL_ACCOUNT, timestamp, true);
            await _repository.RecordMessageRateAsync(Constants.GLOBAL_ACCOUNT, timestamp, true);

            // Act
            var records = await _repository.GetMessageRatesAsync(
                null, // null for Account stats
                timestamp.AddMinutes(-1),
                timestamp.AddMinutes(1),
                TimeSpan.FromSeconds(1));

            // Assert
            records.Should().HaveCount(1);
            var record = records.First();
            record.TotalRequests.Should().Be(2);
        }

        [Fact]
        public async Task GetSummaryStatisticsAsync_ShouldReturnCorrectSummary()
        {
            // Arrange
            var phoneNumber = "+231234567890";
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddMinutes(2);

            // Act
            await _repository.RecordMessageRateAsync(phoneNumber, startTime, true);
            await _repository.RecordMessageRateAsync(phoneNumber, startTime, true);
            await _repository.RecordMessageRateAsync(phoneNumber, startTime, false);

            var summary = await _repository.GetSummaryStatisticsAsync(phoneNumber, startTime.AddMinutes(-1), endTime);

            // Assert
            Assert.NotNull(summary);
            Assert.Equal(3, summary.TotalRequests);
            Assert.Equal(1, summary.RejectedRequests);
        }

        [Fact]
        public async Task GetSummaryStatisticsAsync_ShouldCalculateCorrectly()
        {
            // Arrange
            var phoneNumber = "+1234567890";
            var timestamp = DateTime.UtcNow;

            // Add two requests in first second
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp, true);
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp, true);

            // Add one request in next second
            await _repository.RecordMessageRateAsync(phoneNumber, timestamp.AddSeconds(1), false);

            // Act
            var summary = await _repository.GetSummaryStatisticsAsync(
                phoneNumber,
                timestamp.AddMinutes(-1),
                timestamp.AddMinutes(1));

            // Assert
            summary.TotalRequests.Should().Be(3);
            summary.RejectedRequests.Should().Be(1);
        }

        [Fact]
        public async Task GetSummaryStatisticsAsync_WithNoData_ShouldReturnZeroStats()
        {
            // Act
            var summary = await _repository.GetSummaryStatisticsAsync(
                "+1234567890",
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow);

            // Assert
            summary.TotalRequests.Should().Be(0);
            summary.RejectedRequests.Should().Be(0);
            summary.PeakRequestsPerSecond.Should().Be(0);
            summary.AverageRequestsPerSecond.Should().Be(0);
            summary.PeakTime.Should().BeNull();
        }

        [Fact]
        public async Task CleanupHistoryAsync_ShouldRemoveOldEntries()
        {
            // Arrange
            var phoneNumber = "+231234567890";
            var currentTime = DateTime.UtcNow;

            await _repository.RecordMessageRateAsync(phoneNumber, currentTime.AddMinutes(-10), true);
            await _repository.RecordMessageRateAsync(phoneNumber, currentTime, true);

            // Act
            await _repository.CleanupHistoryAsync(currentTime.AddMinutes(-5));

            var rates = await _repository.GetMessageRatesAsync(phoneNumber, currentTime.AddMinutes(-20), currentTime.AddMinutes(5), TimeSpan.FromSeconds(1));

            // Assert
            
            Assert.Single(rates);
        }
    }
}
