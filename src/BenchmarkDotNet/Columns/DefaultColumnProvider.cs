﻿using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using JetBrains.Annotations;

namespace BenchmarkDotNet.Columns
{
    public static class DefaultColumnProviders
    {
        [PublicAPI] public static readonly IColumnProvider Descriptor = new DescriptorColumnProvider();
        [PublicAPI] public static readonly IColumnProvider Job = new JobColumnProvider();
        [PublicAPI] public static readonly IColumnProvider Statistics = new StatisticsColumnProvider();
        [PublicAPI] public static readonly IColumnProvider Params = new ParamsColumnProvider();
        [PublicAPI] public static readonly IColumnProvider Diagnosers = new DiagnosersColumnProvider();

        public static readonly IColumnProvider[] Instance = { Descriptor, Job, Statistics, Params, Diagnosers };

        private class DescriptorColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary)
            {
                if (summary.BenchmarksCases.Select(b => b.Descriptor.Type.Namespace).Distinct().Count() > 1)
                    yield return TargetMethodColumn.Namespace;
                if (summary.BenchmarksCases.Select(b => b.Descriptor.Type.GetDisplayName()).Distinct().Count() > 1)
                    yield return TargetMethodColumn.Type;
                yield return TargetMethodColumn.Method;
            }
        }

        private class JobColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary) => JobCharacteristicColumn.AllColumns;
        }

        private class StatisticsColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary)
            {
                yield return StatisticColumn.Mean;
                yield return StatisticColumn.Error;

                if (NeedToShow(summary, s => s.Percentiles.P95 > s.Mean + 3 * s.StandardDeviation))
                    yield return StatisticColumn.P95;
                if (NeedToShow(summary, s => s.N >= 3 && !s.GetConfidenceInterval(ConfidenceLevel.L99, s.N).Contains(s.Median)))
                    yield return StatisticColumn.Median;
                if (NeedToShow(summary, s => s.StandardDeviation > 1e-9))
                    yield return StatisticColumn.StdDev;

                if (summary.Reports != null && summary.BenchmarksCases.Any(b => b.IsBaseline()))
                {
                    yield return BaselineScaledColumn.Scaled;
                    var stdDevColumn = BaselineScaledColumn.ScaledStdDev;
                    var stdDevColumnValues = summary.BenchmarksCases.Select(b => stdDevColumn.GetValue(summary, b));

                    // Hide ScaledSD column if values is small
                    // TODO: rewrite and check raw values
                    bool hide = stdDevColumnValues.All(value => value == "0.00" || value == "0.01");
                    if (!hide)
                        yield return BaselineScaledColumn.ScaledStdDev;
                }
            }

            private static bool NeedToShow(Summary summary, Func<Statistics, bool> check)
            {
                return summary.Reports != null && summary.Reports.Any(r => r.ResultStatistics != null && check(r.ResultStatistics));
            }
        }

        private class ParamsColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary) => summary
                .BenchmarksCases
                .SelectMany(b => b.Parameters.Items.Select(item => item.Name))
                .Distinct()
                .Select(name => new ParamColumn(name));
        }

        private class DiagnosersColumnProvider : IColumnProvider
        {
            public IEnumerable<IColumn> GetColumns(Summary summary) => summary
                .Config
                .GetDiagnosers()
                .Select(d => d.GetColumnProvider())
                .SelectMany(cp => cp.GetColumns(summary));
        }
    }
}