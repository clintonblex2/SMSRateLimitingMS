using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;
using SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit;
using SMSRateLimitingMS.Domain.Entities;
using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Tests.Application.Handlers
{
    public class CheckSMSRateLimitHandlerTests
    {
        private readonly Mock<IRateLimitRepository> _rateLimitRepositoryMock;
        private readonly Mock<IRateLimitHistoryRepository> _historyRepositoryMock;
        private readonly Mock<ILogger<CheckSMSRateLimitCommandHandler>> _loggerMock;
        private readonly Mock<IOptions<SMSRateLimitSettings>> _settingsMock;
        private readonly CheckSMSRateLimitCommandHandler _handler;
        private readonly SMSRateLimitSettings _settings;

        public CheckSMSRateLimitHandlerTests()
        {
            _rateLimitRepositoryMock = new Mock<IRateLimitRepository>();
            _historyRepositoryMock = new Mock<IRateLimitHistoryRepository>();
            _loggerMock = new Mock<ILogger<CheckSMSRateLimitCommandHandler>>();

            _settings = new SMSRateLimitSettings
            {
                MaxMessagesPerBusinessPhoneNumberPerSecond = 5,
                MaxMessagesPerAccountPerSecond = 10
            };
            _settingsMock = new Mock<IOptions<SMSRateLimitSettings>>();
            _settingsMock.Setup(x => x.Value).Returns(_settings);

            _handler = new CheckSMSRateLimitCommandHandler(
                _rateLimitRepositoryMock.Object,
                _historyRepositoryMock.Object,
                _loggerMock.Object,
                _settingsMock.Object.Value);
        }

        [Fact]
        public async Task Handle_WhenBothLimitsAllowed_ShouldReturnSuccess()
        {
            // Arrange
            var request = new CheckSMSRateLimitCommand("+1234567890");
            var phoneRateLimit = new RateLimit(request.BusinessPhoneNumber, _settings.MaxMessagesPerBusinessPhoneNumberPerSecond, TimeSpan.FromSeconds(1));
            var globalAccountRateLimit = new RateLimit(Constants.GLOBAL_ACCOUNT, _settings.MaxMessagesPerAccountPerSecond, TimeSpan.FromSeconds(1));

            _rateLimitRepositoryMock
                .Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(phoneRateLimit);

            _rateLimitRepositoryMock
                .Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(globalAccountRateLimit);

            // Mock successful increments
            var phoneStats = new CounterStatistics(1, 4, 5, TimeSpan.FromSeconds(1));
            var globalStats = new CounterStatistics(1, 9, 10, TimeSpan.FromSeconds(1));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.CanSendSMS.Should().BeTrue();
            result.ReasonForDenial.Should().BeNull();

            // Verify history was recorded
            _historyRepositoryMock.Verify(x =>
                x.RecordMessageRateAsync(
                    request.BusinessPhoneNumber,
                    It.IsAny<DateTime>(),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenPhoneNumberLimitExceeded_ShouldReturnFailure()
        {
            // Arrange
            var request = new CheckSMSRateLimitCommand("+1234567890");
            var phoneRateLimit = new RateLimit(request.BusinessPhoneNumber, 0, TimeSpan.FromSeconds(1));

            _rateLimitRepositoryMock
                .Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(phoneRateLimit);

            // Mock phone limit exceeded
            var phoneStats = new CounterStatistics(5, 0, 5, TimeSpan.FromSeconds(1));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.CanSendSMS.Should().BeFalse();

            // Verify history was recorded
            _historyRepositoryMock.Verify(x =>
                x.RecordMessageRateAsync(
                    request.BusinessPhoneNumber,
                    It.IsAny<DateTime>(),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenGlobalLimitExceeded_ShouldReturnFailure()
        {
            // Arrange
            var request = new CheckSMSRateLimitCommand("+1234567890");
            var globalRateLimit = new RateLimit(Constants.GLOBAL_ACCOUNT, 0, TimeSpan.FromSeconds(1));

            _rateLimitRepositoryMock
                .Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(globalRateLimit);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.CanSendSMS.Should().BeFalse();

            // Verify history was recorded
            _historyRepositoryMock.Verify(x =>
                x.RecordMessageRateAsync(
                    request.BusinessPhoneNumber,
                    It.IsAny<DateTime>(),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
