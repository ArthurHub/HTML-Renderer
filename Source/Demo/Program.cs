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
using System.Drawing.Printing;
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
        private static void Main()
        {
            // TestPrint();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new DemoForm());

//            Application.Run(new PerfForm());

//            PerfForm.Run();

//            ObfuscateHtml();
        }

        private static void TestPrint()
        {
//            string html = File.ReadAllText(@"c:\source\html.txt");
            string html = File.ReadAllText(@"C:\Source\GitHub\HTML-Renderer\Source\Demo\Samples\02.text.htm");

            var doc = new PrintDocument();
            doc.PrintPage += (sender, args) =>
                {
                    var fact = args.Graphics.DpiX/96f;

//                    args.Graphics.PageScale = 1/fact;

//                    var font = new Font("Arial", 10*fact);
//                    TextRenderer.DrawText(args.Graphics,"Hello World of PDF", font, new Point((int) (50*fact),(int) (50*fact)), Color.Red);

//                    var font2 = new Font("Arial", 10);
//                    args.Graphics.DrawString("Hello World of PDF", font2, Brushes.DarkGreen, new PointF(50, 50));

//                    Point location = new Point((int) (args.MarginBounds.Location.X*fact), (int) (args.MarginBounds.Location.Y*fact));
//                    Size maxSize = new Size((int)(args.MarginBounds.Size.Width * fact), (int) (args.MarginBounds.Size.Height*fact));
//                    HtmlRender.Render(args.Graphics, html, args.MarginBounds.Location, args.MarginBounds.Size);

                    var image = HtmlRender.RenderToImage(html, args.MarginBounds.Size);

                    args.Graphics.DrawImageUnscaled(image, args.MarginBounds.Location);
                };
//            doc.Print();

//            var bitmap = HtmlRender.RenderToImage(File.ReadAllText(@"c:\source\html.txt"), 350, 500);
//            var bitmap = HtmlRender.RenderToImage(html, 600);
//            bitmap.Save(@"c:\source\test.png", ImageFormat.Png);
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