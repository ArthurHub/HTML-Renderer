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
using System.IO;
using TheArtOfDev.HtmlRenderer.Core.Entities;

namespace TheArtOfDev.HtmlRenderer.Demo.Common
{
    public class DemoUtils
    {
        private const int Iterations = 20;

        /// <summary>
        /// The HTML text used in sample form for HtmlLabel.
        /// </summary>
        public static String SampleHtmlLabelText
        {
            get
            {
                return "This is an <b>HtmlLabel</b> on transparent background with <span style=\"color: red\">colors</span> and links: " +
                       "<a href=\"http://htmlrenderer.codeplex.com/\">HTML Renderer</a>";
            }
        }

        /// <summary>
        /// The HTML text used in sample form for HtmlPanel.
        /// </summary>
        public static String SampleHtmlPanelText
        {
            get
            {
                return "This is an <b>HtmlPanel</b> with <span style=\"color: red\">colors</span> and links: <a href=\"http://htmlrenderer.codeplex.com/\">HTML Renderer</a>" +
                       "<div style=\"font-size: 1.2em; padding-top: 10px;\" >If there is more text than the size of the control scrollbars will appear.</div>" +
                       "<br/>Click me to change my <code>Text</code> property.";
            }
        }

        /// <summary>
        /// Handle stylesheet resolve.
        /// </summary>
        public static void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            var stylesheet = GetStylesheet(e.Src);
            if (stylesheet != null)
                e.SetStyleSheet = stylesheet;
        }

        /// <summary>
        /// Get stylesheet by given key.
        /// </summary>
        public static string GetStylesheet(string src)
        {
            if (src == "StyleSheet")
            {
                return @"h1, h2, h3 { color: navy; font-weight:normal; }
                    h1 { margin-bottom: .47em }
                    h2 { margin-bottom: .3em }
                    h3 { margin-bottom: .4em }
                    ul { margin-top: .5em }
                    ul li {margin: .25em}
                    body { font:10pt Tahoma }
		            pre  { border:solid 1px gray; background-color:#eee; padding:1em }
                    a:link { text-decoration: none; }
                    a:hover { text-decoration: underline; }
                    .gray    { color:gray; }
                    .example { background-color:#efefef; corner-radius:5px; padding:0.5em; }
                    .whitehole { background-color:white; corner-radius:10px; padding:15px; }
                    .caption { font-size: 1.1em }
                    .comment { color: green; margin-bottom: 5px; margin-left: 3px; }
                    .comment2 { color: green; }";
            }
            return null;
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        public static Stream GetImageStream(string src)
        {
            switch (src.ToLower())
            {
                case "htmlicon":
                    return Resources.Html32;
                case "staricon":
                    return Resources.Favorites32;
                case "fonticon":
                    return Resources.Font32;
                case "commenticon":
                    return Resources.Comment16;
                case "imageicon":
                    return Resources.Image32;
                case "methodicon":
                    return Resources.Method16;
                case "propertyicon":
                    return Resources.Property16;
                case "eventicon":
                    return Resources.Event16;
            }
            return null;
        }

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

            if (Environment.Version.Major >= 4)
            {
                msg += "\r\n\r\n";
                msg += string.Format("Memory:\r\nTotal: {0:N0} KB\r\nIterationAvg: {1:N0} KB\r\nSingleAvg: {2:N0} KB\r\nOverhead: {3:N0}%",
                    memory, memory / Iterations, memory / Iterations / sampleCount, 100 * (memory / Iterations) / htmlSize);
            }

            msg += "\r\n\r\n\r\n";
            msg += string.Format("Full CPU:\r\nTotal: {0} msec\r\nIterationAvg: {1:N2} msec\r\nSingleAvg: {2:N2} msec",
                runStopwatch.ElapsedMilliseconds, runStopwatch.ElapsedMilliseconds / (double)Iterations, runStopwatch.ElapsedMilliseconds / (double)Iterations / sampleCount);

            if (Environment.Version.Major >= 4)
            {
                msg += "\r\n\r\n";
                msg += string.Format("Full Memory:\r\nTotal: {0:N0} KB\r\nIterationAvg: {1:N0} KB\r\nSingleAvg: {2:N0} KB\r\nOverhead: {3:N0}%",
                    runMemory, runMemory / Iterations, runMemory / Iterations / sampleCount, 100 * (runMemory / Iterations) / htmlSize);
            }

            return msg;
        }

        private static Stopwatch RunTest(Action<String> setHtmlDelegate, bool real, out double totalMem)
        {
            totalMem = 0;
            long startMemory = 0;
            if (Environment.Version.Major >= 4)
            {
                typeof(AppDomain).GetProperty("MonitoringIsEnabled").SetValue(null, true, null);
                startMemory = (long)AppDomain.CurrentDomain.GetType().GetProperty("MonitoringTotalAllocatedMemorySize").GetValue(AppDomain.CurrentDomain, null);
            }

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                foreach (var sample in SamplesLoader.ShowcaseSamples)
                {
                    setHtmlDelegate(real ? sample.Html : string.Empty);
                }
            }

            sw.Stop();

            if (Environment.Version.Major >= 4)
            {
                var endMemory = (long)AppDomain.CurrentDomain.GetType().GetProperty("MonitoringTotalAllocatedMemorySize").GetValue(AppDomain.CurrentDomain, null);
                totalMem = (endMemory - startMemory) / 1024f;
            }

            return sw;
        }
    }
}