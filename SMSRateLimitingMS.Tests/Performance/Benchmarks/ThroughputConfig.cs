using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRateLimitingMS.Tests.Performance.Benchmarks
{
    public class ThroughputConfig : ManualConfig
    {
        public ThroughputConfig()
        {
            AddJob(Job.Default
                .WithInvocationCount(100000)
                .WithUnrollFactor(1)
                .WithIterationCount(5)
                .WithWarmupCount(2));

            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Error);
            AddColumn(new ThroughputColumn()); // Custom column for throughput
        }
    }

    public class ThroughputColumn : IColumn
    {
        public string Id => "Throughput";
        public string ColumnName => "Ops/sec";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => "Operations per second";
        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var stats = summary[benchmarkCase].ResultStatistics;
            if (stats == null) return "-";

            var opsPerSecond = 1_000_000_000.0 / stats.Mean; // Convert nanoseconds to ops/sec
            return $"{opsPerSecond:N0}";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
            => GetValue(summary, benchmarkCase);
    }
}
