using FluentAssertions;
using Moq;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.UseCases.GetPhoneNumbers;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Domain.Entities;

namespace SMSRateLimitingMS.Tests.Application.Handlers
{
    public class GetPhoneNumbersQueryHandlerTests
    {
        private readonly Mock<IRateLimitHistoryRepository> _mockHistoryRepository;
        private readonly GetPhoneNumbersQueryHandler _handler;

        public GetPhoneNumbersQueryHandlerTests()
        {
            _mockHistoryRepository = new Mock<IRateLimitHistoryRepository>();
            _handler = new GetPhoneNumbersQueryHandler(_mockHistoryRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnDistinctPhoneNumbers_ExcludingGlobalAccount()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var expectedPhoneNumbers = new[] { "+1234567890", "+9876543210" };
         
            var messageRates = new List<MessageRateAggregate>
            {
                new("+1234567890", now, TimeSpan.FromSeconds(1), 5, 1),
                new("+1234567890", now, TimeSpan.FromSeconds(1), 3, 0),
                new("+9876543210", now, TimeSpan.FromSeconds(1), 2, 0),
                new(Constants.GLOBAL_ACCOUNT, now, TimeSpan.FromSeconds(1), 10, 3)
            };

            _mockHistoryRepository.Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageRates);

            // Act
            var result = await _handler.Handle(new GetPhoneNumbersQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedPhoneNumbers);
            result.Should().BeInAscendingOrder();
            result.Should().NotContain(Constants.GLOBAL_ACCOUNT);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoPhoneNumbersExist()
        {
            // Arrange
            var messageRates = new List<MessageRateAggregate>
            {
                new(Constants.GLOBAL_ACCOUNT, DateTime.UtcNow, TimeSpan.FromSeconds(1), 10, 0)
            };

            _mockHistoryRepository.Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageRates);

            // Act
            var result = await _handler.Handle(new GetPhoneNumbersQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectTimeRange()
        {
            // Arrange
            var now = DateTime.UtcNow;
            DateTime? capturedStartTime = null;
            DateTime? capturedEndTime = null;

            _mockHistoryRepository.Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string?, DateTime, DateTime, TimeSpan, CancellationToken>((_, start, end, _, _) =>
                {
                    capturedStartTime = start;
                    capturedEndTime = end;
                })
                .ReturnsAsync(new List<MessageRateAggregate>());

            // Act
            await _handler.Handle(new GetPhoneNumbersQuery(), CancellationToken.None);

            // Assert
            capturedStartTime.Should().BeCloseTo(now.AddHours(-24), TimeSpan.FromSeconds(1));
            capturedEndTime.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_ShouldHandleNullPhoneNumbers()
        {
            // Arrange
            var messageRates = new List<MessageRateAggregate>
            {
                new(null, DateTime.UtcNow, TimeSpan.FromSeconds(1), 5, 1),
                new("+1234567890", DateTime.UtcNow, TimeSpan.FromSeconds(1), 3, 0)
            };

            _mockHistoryRepository.Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(messageRates);

            // Act
            var result = await _handler.Handle(new GetPhoneNumbersQuery(), CancellationToken.None);

            // Assert
            result.Should().ContainSingle(x => x == "+1234567890");
        }

        [Fact]
        public async Task Handle_ShouldUseCancellationToken()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            _mockHistoryRepository.Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    cancellationToken))
                .ReturnsAsync(new List<MessageRateAggregate>());

            // Act
            await _handler.Handle(new GetPhoneNumbersQuery(), cancellationToken);

            // Assert
            _mockHistoryRepository.Verify(x => x.GetMessageRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<TimeSpan>(),
                cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRespectRepositoryErrors()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Repository error");
            _mockHistoryRepository.Setup(x => x.GetMessageRatesAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var action = () => _handler.Handle(new GetPhoneNumbersQuery(), CancellationToken.None);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Repository error");
        }
    }
}
