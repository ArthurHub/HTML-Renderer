// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Diagnostics;

namespace HtmlRenderer.Demo.Common
{
    public class DemoUtils
    {
        private const int Iterations = 20;

        public static string RunSamplesPerformanceTest(Action<String> setHtmlDelegate)
        {
            GC.Collect();

            double baseMemory;
            var baseStopwatch = RunTest(setHtmlDelegate, false, out baseMemory);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            double runMemory;
            var runStopwatch = RunTest(setHtmlDelegate, true, out runMemory);

            double memory = runMemory - baseMemory;
            var elapsedMilliseconds = runStopwatch.ElapsedMilliseconds - baseStopwatch.ElapsedMilliseconds;

            float htmlSize = 0;
            foreach (var sample in SamplesLoader.ShowcaseSamples)
                htmlSize += sample.Html.Length * 2;
            htmlSize = htmlSize / 1024f;

            var sampleCount = SamplesLoader.ShowcaseSamples.Count;
            var msg = string.Format("{0} HTMLs ({1:N0} KB)\r\n{2} Iterations", sampleCount, htmlSize, Iterations);
            msg += "\r\n\r\n";
            msg += string.Format("CPU:\r\nTotal: {0} msec\r\nIterationAvg: {1:N2} msec\r\nSingleAvg: {2:N2} msec",
                elapsedMilliseconds, elapsedMilliseconds / (double)Iterations, elapsedMilliseconds / (double)Iterations / sampleCount);
            msg += "\r\n\r\n";
            msg += string.Format("Memory:\r\nTotal: {0:N0} KB\r\nIterationAvg: {1:N0} KB\r\nSingleAvg: {2:N0} KB\r\nOverhead: {3:N0}%",
                memory, memory / Iterations, memory / Iterations / sampleCount, 100 * (memory / Iterations) / htmlSize);
            msg += "\r\n\r\n\r\n";
            msg += string.Format("Full CPU:\r\nTotal: {0} msec\r\nIterationAvg: {1:N2} msec\r\nSingleAvg: {2:N2} msec",
                runStopwatch.ElapsedMilliseconds, runStopwatch.ElapsedMilliseconds / (double)Iterations, runStopwatch.ElapsedMilliseconds / (double)Iterations / sampleCount);
            msg += "\r\n\r\n";
            msg += string.Format("Full Memory:\r\nTotal: {0:N0} KB\r\nIterationAvg: {1:N0} KB\r\nSingleAvg: {2:N0} KB\r\nOverhead: {3:N0}%",
                runMemory, runMemory / Iterations, runMemory / Iterations / sampleCount, 100 * (runMemory / Iterations) / htmlSize);
            
            return msg;
        }

        private static Stopwatch RunTest(Action<String> setHtmlDelegate, bool real, out double totalMem)
        {
#if NET_40
            AppDomain.MonitoringIsEnabled = true;
            var startMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
#endif
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < DemoUtils.Iterations; i++)
            {
                foreach (var sample in SamplesLoader.ShowcaseSamples)
                {
                    setHtmlDelegate(real ? sample.Html : string.Empty);
                }
            }

            sw.Stop();

#if NET_40
            var endMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
            totalMem = (endMemory - startMemory) / 1024f;
#else
            totalMem = 0;
#endif
            return sw;
        }
    }
}