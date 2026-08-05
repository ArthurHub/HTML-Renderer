using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StepsTimingFunction : ITimingFunction
    {
        public StepsTimingFunction(int intervals, bool start = false)
        {
            Intervals = Math.Max(1, intervals);
            IsStart = start;
        }

        public int Intervals { get; }
        public bool IsStart { get; }
    }
}