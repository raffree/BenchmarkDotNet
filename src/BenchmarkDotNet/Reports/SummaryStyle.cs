﻿using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Horology;

namespace BenchmarkDotNet.Reports
{
    public class SummaryStyle : ISummaryStyle
    {
        public bool PrintUnitsInHeader { get; private set; }
        public bool PrintUnitsInContent { get; private set; } = true;
        public SizeUnit SizeUnit { get; private set; }
        public TimeUnit TimeUnit { get; private set; }

        public static SummaryStyle Default => new SummaryStyle
        {
            PrintUnitsInHeader = false,
            PrintUnitsInContent = true,
            SizeUnit = null,
            TimeUnit = null
        };

        public ISummaryStyle WithTimeUnit(TimeUnit timeUnit)
        {
            return new SummaryStyle
            {
                PrintUnitsInHeader = PrintUnitsInHeader,
                PrintUnitsInContent = PrintUnitsInContent,
                SizeUnit = SizeUnit,
                TimeUnit = timeUnit
            };
        }

        public ISummaryStyle WithSizeUnit(SizeUnit sizeUnit)
        {
            return new SummaryStyle
            {
                PrintUnitsInHeader = PrintUnitsInHeader,
                PrintUnitsInContent = PrintUnitsInContent,
                SizeUnit = sizeUnit,
                TimeUnit = TimeUnit
            };
        }
    }
}
