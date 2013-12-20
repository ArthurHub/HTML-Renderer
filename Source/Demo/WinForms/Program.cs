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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace HtmlRenderer.Demo.WinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
//            TestPrint();
//            TestImage1();
//            return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new DemoForm());

//            Application.Run(new PerfForm());

//            PerfForm.Run();
        }

        private static void TestImage1()
        {
            var html1 = "<p><h2>Hellow</h2><img height='80' src='d:\\b.png'/><h2>world</h2><img height='80' src='http://i.tinyuploads.com/Co66A2.jpg'/><h2>bla</h2></p>";
            var img = HtmlRender.RenderToImageGdiPlus(html1, new Size(600, 400));
            img.Save(@"d:\test1.png", ImageFormat.Png);
        }

        private static void TestImage2()
        {
            var html1 = "<p style='font-size:2em;color: gray'>This is an <b>HtmlLabel</b> on transparent background with <span style='color: red'>colors</span> and links: <a href='http://htmlrenderer.codeplex.com/'>HTML Renderer</a></p>";
            var img = HtmlRender.RenderToImageGdiPlus(html1, new Size(600, 200));
            // var img = HtmlRender.RenderToImage(html, new Size(600, 200));
            img.Save(@"d:\test1.png", ImageFormat.Png);

            var image = Image.FromFile(@"d:\b.png");
            HtmlRender.RenderToImage(image, html1, new Point(50, 0), new Size(620, 0));
            image.Save(@"d:\test2.png");

            var html2 = "<html><body style=\"margin: 0px; height: 461px; background-image:url(d:/b.png);background-repeat:no-repeat;\"><h1 style=\" margin-top:40px;margin-left:150px;\">"
            + "<span style=\"font-family:Monotype Corsiva;\"><em>Test HTML</em></span></h1><div style=\"width:560px;height:200px;margin-top:120px;margin-right:55px;margin-left:150px;\"><em>"
            + "<span style=\"font-family:Monotype Corsiva;\"><span style=\"font-size:22px;\">Some Text Here</span></span></em></div></body></html>";
            var img2 = HtmlRender.RenderToImage(html2, new Size(755, 461));
            img2.Save(@"d:\test3.png", ImageFormat.Png);
        }

        private static void TestPrint()
                {
            var html = "<p style='font-size:3em;color: gray'>This is an <b>HtmlLabel</b> on transparent background with <span style='color: red'>colors</span> and links: <a href='http://htmlrenderer.codeplex.com/'>HTML Renderer</a></p>";
            var doc = new PrintDocument();
            doc.PrintPage += (sender, args) => HtmlRender.RenderGdiPlus(args.Graphics, html, args.MarginBounds.Location, args.MarginBounds.Size);
            doc.Print();
        }
    }
}