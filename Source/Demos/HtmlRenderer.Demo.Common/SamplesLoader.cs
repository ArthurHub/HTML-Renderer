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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Demo.Common
{
    public static class SamplesLoader
    {
        /// <summary>
        /// Samples to showcase the HTML Renderer capabilities
        /// </summary>
        private static readonly List<HtmlSample> _showcaseSamples = new List<HtmlSample>();

        /// <summary>
        /// Samples to test the different features of HTML Renderer that they work correctly
        /// </summary>
        private static readonly List<HtmlSample> _testSamples = new List<HtmlSample>();

        /// <summary>
        /// Samples used to test extreme performance
        /// </summary>
        private static readonly List<HtmlSample> _performanceSamples = new List<HtmlSample>();

        /// <summary>
        /// Init.
        /// </summary>
        public static void Init(string platform, string version)
        {
            LoadSamples(platform, version);
        }

        /// <summary>
        /// Samples to showcase the HTML Renderer capabilities
        /// </summary>
        public static List<HtmlSample> ShowcaseSamples
        {
            get { return _showcaseSamples; }
        }

        /// <summary>
        /// Samples to test the different features of HTML Renderer that they work correctly
        /// </summary>
        public static List<HtmlSample> TestSamples
        {
            get { return _testSamples; }
        }

        /// <summary>
        /// Samples used to test extreme performance
        /// </summary>
        public static List<HtmlSample> PerformanceSamples
        {
            get { return _performanceSamples; }
        }

        /// <summary>
        /// Loads the tree of document samples
        /// </summary>
        private static void LoadSamples(string platform, string version)
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Array.Sort(names);
            foreach (string name in names)
            {
                int extPos = name.LastIndexOf('.');
                int namePos = extPos > 0 && name.Length > 1 ? name.LastIndexOf('.', extPos - 1) : 0;
                string ext = name.Substring(extPos >= 0 ? extPos : 0);
                string shortName = namePos > 0 && name.Length > 2 ? name.Substring(namePos + 1, name.Length - namePos - ext.Length - 1) : name;

                if (".htm".IndexOf(ext, StringComparison.Ordinal) >= 0)
                {
                    var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                    if (resourceStream != null)
                    {
                        using (var sreader = new StreamReader(resourceStream, Encoding.Default))
                        {
                            var html = sreader.ReadToEnd();

                            if (name.Contains("TestSamples."))
                            {
                                _testSamples.Add(new HtmlSample(shortName, name, html));
                            }
                            else if (name.Contains("PerfSamples"))
                            {
                                _performanceSamples.Add(new HtmlSample(shortName, name, html));
                            }
                            else
                            {
                                html = html.Replace("$$Platform$$", platform).Replace("$$Release$$", version);
                                _showcaseSamples.Add(new HtmlSample(shortName, name, html));
                            }
                        }
                    }
                }
            }
        }
    }
}