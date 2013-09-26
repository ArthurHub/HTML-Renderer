// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace HtmlRenderer.Demo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new DemoForm());

//            Application.Run(new PerfForm());

//            PerfForm.Run();

//            ObfuscateHtml();
        }

        /// <summary>
        /// Used so html samples can be added where the content of the html is confidential.
        /// </summary>
        public static void ObfuscateHtml()
        {
            var html = GetHtmlFromResource("HtmlRenderer.Demo.PerfSamples.2.Tables.htm");
            var sb = new StringBuilder(html);

            var rand = new Random();

            int idx = 0;
            while (idx < html.Length)
            {
                var tagIdx = html.IndexOf('<', idx);
                if (tagIdx < 0)
                    tagIdx = html.Length;

                for (int i = idx; i < tagIdx; i++)
                {
                    if(char.IsLetter(html,i))
                    {
                        sb[i] = Convert.ToChar(97 + rand.Next(18));
                    }
                }

                if(tagIdx < html.Length)
                {
                    tagIdx = html.IndexOf('>',tagIdx);
                    if (tagIdx < 0)
                        tagIdx = html.Length;
                }

                idx = tagIdx;
            }

            var result = sb.ToString();

            result = Regex.Replace(result, "href\\W*=\\W*[\"'].*?[\"']", "href='http://www.google.com'");
        }

        /// <summary>
        /// Get html from resource by resource name.
        /// </summary>
        private static string GetHtmlFromResource(string name)
        {
            var htmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if(htmlStream != null)
            {
                using (StreamReader sreader = new StreamReader(htmlStream, Encoding.Default))
                {
                    return sreader.ReadToEnd();
                }
            }
            return null;
        }
    }
}