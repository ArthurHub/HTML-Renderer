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
using System.Windows.Forms;

namespace HtmlRenderer.Demo.WinForms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //            string html = Resource1.String1;
            //            string html = "a<p style='background-color: yellow'>hello <b>world</b></p>";

            //            var doc = PdfGenerator.GeneratePdf(html, PageSize.A4);

            //            doc.Save(@"d:\doc.pdf");
            //            return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DemoForm());

            //            Application.Run(new PerfForm());

            //            PerfForm.Run();
        }
    }
}