using BenchmarkDotNet.Running;
using SMSRateLimitingMS.Tests.Performance.Benchmarks;

namespace SMSRateLimitingMS.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RateLimiterBenchmarks>();
        }
    }
}
