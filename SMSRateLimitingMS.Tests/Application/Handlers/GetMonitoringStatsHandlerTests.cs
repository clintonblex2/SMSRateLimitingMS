using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;
using SMSRateLimitingMS.Application.UseCases.GetMonitoringStats;
using SMSRateLimitingMS.Domain.Entities;
using SMSRateLimitingMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMSRateLimitingMS.Tests.Application.Handlers
{
    public class GetMonitoringStatsHandlerTests
    {
        private readonly Mock<IRateLimitRepository> _rateLimitRepositoryMock;
        private readonly Mock<IRateLimitHistoryRepository> _historyRepositoryMock;
        private readonly Mock<ILogger<GetMonitoringStatsQueryHandler>> _loggerMock;
        private readonly Mock<IOptions<SMSRateLimitSettings>> _settingsMock;
        private readonly GetMonitoringStatsQueryHandler _handler;
        private readonly SMSRateLimitSettings _settings;

        public GetMonitoringStatsHandlerTests()
        {
            _rateLimitRepositoryMock = new Mock<IRateLimitRepository>();
            _historyRepositoryMock = new Mock<IRateLimitHistoryRepository>();
            _loggerMock = new Mock<ILogger<GetMonitoringStatsQueryHandler>>();

            _settings = new SMSRateLimitSettings
            {
                MaxMessagesPerBusinessPhoneNumberPerSecond = 5,
                MaxMessagesPerAccountPerSecond = 10
            };
            _settingsMock = new Mock<IOptions<SMSRateLimitSettings>>();
            _settingsMock.Setup(x => x.Value).Returns(_settings);

            _handler = new GetMonitoringStatsQueryHandler(
                _historyRepositoryMock.Object,
                _rateLimitRepositoryMock.Object,
                 _settingsMock.Object.Value,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectStats()
        {
            // Arrange
            var phoneNumber = "+1234567890";
            var startTime = DateTime.UtcNow.AddMinutes(-5);
            var endTime = DateTime.UtcNow;
            var query = new GetMonitoringStatsQuery(startTime, endTime, phoneNumber);

            var messageRates = new List<MessageRateAggregate>
            {
                new(phoneNumber, startTime, TimeSpan.FromSeconds(1), 5, 1),
                new(phoneNumber, startTime.AddSeconds(1), TimeSpan.FromSeconds(1), 3, 0)
            };

            var summaryStats = new RateLimitHistorySummary(5, 1, 0.1, 0.2, endTime);

            _historyRepositoryMock
                .Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageRates);

            var currentStats = new CounterStatistics(2, 3, 5, TimeSpan.FromSeconds(1));
            _rateLimitRepositoryMock
                .Setup(x => x.GetRateLimitStatistics(It.IsAny<string>()))
                .ReturnsAsync(currentStats);
            
            var phoneRateLimit = new RateLimit(phoneNumber, _settings.MaxMessagesPerBusinessPhoneNumberPerSecond, TimeSpan.FromSeconds(1));

            _rateLimitRepositoryMock
                .Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(phoneRateLimit);

            _historyRepositoryMock.Setup(x => x.GetSummaryStatisticsAsync(
                It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(summaryStats);
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.PhoneNumber.Should().Be(phoneNumber);
            result.CurrentStats.CurrentCount.Should().Be(0);
            result.CurrentStats.RemainingCapacity.Should().Be(5);
            result.HistoricalStats.TotalRequests.Should().Be(5);
            result.HistoricalStats.TotalBlocked.Should().Be(1);
        }

        [Fact]
        public async Task Handle_WithNoHistory_ShouldReturnEmptyStats()
        {
            // Arrange
            var query = new GetMonitoringStatsQuery(
                DateTime.UtcNow.AddMinutes(-5),
                DateTime.UtcNow,
                "+1234567890");

            _historyRepositoryMock
                .Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MessageRateAggregate>());

            _rateLimitRepositoryMock
                .Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(new RateLimit(null, 0, default));

            _historyRepositoryMock.Setup(x => x.GetSummaryStatisticsAsync(
                It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RateLimitHistorySummary(0, 0, 0, 0, default));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HistoricalStats.TotalRequests.Should().Be(0);
            result.HistoricalStats.TotalBlocked.Should().Be(0);
            result.HistoricalStats.PeakRequestsPerSecond.Should().Be(0);
            result.HistoricalStats.AverageRequestsPerSecond.Should().Be(0);
        }
    }
}
