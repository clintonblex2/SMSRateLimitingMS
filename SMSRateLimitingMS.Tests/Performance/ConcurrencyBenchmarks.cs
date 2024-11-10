using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;
using SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit;
using SMSRateLimitingMS.Infrastructure.Persistence;

namespace SMSRateLimitingMS.Tests.Performance
{
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80,
        iterationCount: 5,
        launchCount: 1,
        warmupCount: 3)]
    public class ConcurrencyBenchmarks
    {
        private IRateLimitRepository _rateLimitRepository;
        private IRateLimitHistoryRepository _historyRepository;
        private CheckSMSRateLimitCommandHandler _checkHandler;
        private readonly List<string> _phoneNumbers;
        private readonly int _concurrentRequests;
        private readonly TimeSpan _testDuration;

        public ConcurrencyBenchmarks()
        {
            _phoneNumbers = Enumerable.Range(1, 100)
                .Select(i => $"+1{i:D10}")
                .ToList();
            _concurrentRequests = 100;
            _testDuration = TimeSpan.FromSeconds(10);
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
        }

        [Benchmark]
        public async Task ConcurrentRateLimitChecks()
        {
            var cts = new CancellationTokenSource(_testDuration);
            var random = new Random();
            var tasks = new List<Task>();

            for (int i = 0; i < _concurrentRequests; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        var phoneNumber = _phoneNumbers[random.Next(_phoneNumbers.Count)];
                        var command = new CheckSMSRateLimitCommand(phoneNumber);
                        await _checkHandler.Handle(command, cts.Token);
                        await Task.Delay(10); // Simulate some processing time
                    }
                }, cts.Token));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                
            }
        }
    }

}
