using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;
using SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit;
using SMSRateLimitingMS.Application.UseCases.GetMonitoringStats;
using SMSRateLimitingMS.Infrastructure.Persistence;

namespace SMSRateLimitingMS.Tests.Performance
{
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80,
        iterationCount: 5,
        invocationCount: 3,
        warmupCount: 2)]
    public class RateLimiterBenchmarks
    {
        private IRateLimitRepository _rateLimitRepository;
        private IRateLimitHistoryRepository _historyRepository;
        private CheckSMSRateLimitCommandHandler _checkHandler;
        private GetMonitoringStatsQueryHandler _monitoringHandler;
        private readonly string[] _phoneNumbers;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;

        public RateLimiterBenchmarks()
        {
            _phoneNumbers = Enumerable.Range(1, 1000)
                .Select(i => $"+1{i:D10}")
                .ToArray();
            _startTime = DateTime.UtcNow.AddHours(-1);
            _endTime = DateTime.UtcNow;
        }

        [GlobalSetup]
        public void Setup()
        {
            var settings = new SMSRateLimitSettings
            {
                MaxMessagesPerBusinessPhoneNumberPerSecond = 5,
                MaxMessagesPerAccountPerSecond = 100
            };
            var optionsWrapper = Options.Create(settings);

            _rateLimitRepository = new InMemoryRateLimitRepository();
            _historyRepository = new InMemoryRateLimitHistoryRepository();

            _checkHandler = new CheckSMSRateLimitCommandHandler(
                _rateLimitRepository,
                _historyRepository,
                NullLogger<CheckSMSRateLimitCommandHandler>.Instance,
                optionsWrapper.Value);

            _monitoringHandler = new GetMonitoringStatsQueryHandler(
                _historyRepository,
                _rateLimitRepository,
                optionsWrapper.Value,
                NullLogger<GetMonitoringStatsQueryHandler>.Instance);

            // Seed initial data
            SeedInitialData().GetAwaiter().GetResult();
        }

        private async Task SeedInitialData()
        {
            // Seed some historical data
            var random = new Random(42);
            var timestamps = Enumerable.Range(0, 3600) // One hour of data
                .Select(i => _startTime.AddSeconds(i))
                .ToList();

            foreach (var timestamp in timestamps)
            {
                var phoneNumber = _phoneNumbers[random.Next(_phoneNumbers.Length)];
                await _historyRepository.RecordMessageRateAsync(
                    phoneNumber,
                    timestamp,
                    wasSuccessful: random.NextDouble() > 0.1); // 10% failure rate
            }
        }

        [Benchmark]
        public async Task CheckRateLimit_SinglePhoneNumber()
        {
            var command = new CheckSMSRateLimitCommand(_phoneNumbers[0]);
            await _checkHandler.Handle(command, CancellationToken.None);
        }

        [Benchmark]
        public async Task CheckRateLimit_MultiplePhoneNumbers_Sequential()
        {
            for (int i = 0; i < 100; i++)
            {
                var command = new CheckSMSRateLimitCommand(_phoneNumbers[i]);
                await _checkHandler.Handle(command, CancellationToken.None);
            }
        }

        [Benchmark]
        public async Task CheckRateLimit_MultiplePhoneNumbers_Parallel()
        {
            var tasks = Enumerable.Range(0, 100)
                .Select(i => new CheckSMSRateLimitCommand(_phoneNumbers[i]))
                .Select(command => _checkHandler.Handle(command, CancellationToken.None));

            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task GetMonitoringStats_SinglePhoneNumber()
        {
            var query = new GetMonitoringStatsQuery(_startTime, _endTime, _phoneNumbers[0]);
            await _monitoringHandler.Handle(query, CancellationToken.None);
        }

        [Benchmark]
        public async Task GetMonitoringStats_Global()
        {
            var query = new GetMonitoringStatsQuery(_startTime, _endTime, null);
            await _monitoringHandler.Handle(query, CancellationToken.None);
        }

        [Benchmark]
        public async Task GetMonitoringStats_HighLoad()
        {
            // Simulate high load by running multiple queries in parallel
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(GetMonitoringStats_SinglePhoneNumber());
                if (i % 3 == 0) // Mix in some global stats queries
                {
                    tasks.Add(GetMonitoringStats_Global());
                }
            }
            await Task.WhenAll(tasks);
        }

    }
}
