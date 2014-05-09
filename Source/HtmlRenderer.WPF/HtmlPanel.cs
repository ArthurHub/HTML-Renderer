// // "Therefore those skilled at the unorthodox
// // are infinite as heaven and earth,
// // inexhaustible as the great rivers.
// // When they come to an end,
// // they begin again,
// // like the days and months;
// // they die and are reborn,
// // like the four seasons."
// // 
// // - Sun Tsu,
// // "The Art of War"

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public class HtmlPanel : ScrollViewer
    {
        protected override void OnRender(DrawingContext context)
        {
            HtmlRender.Render(context, "<p><h1>bla</h1>hello <b color='red'>world</b></p>", 10, 10, Width);


//            var formattedText = new FormattedText("hello world", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 18, Brushes.Red);

//            context.DrawText(formattedText, new Point(10, 10));
//            context.DrawGlyphRun(Brushes.Aqua, new GlyphRun());
        }
    }
}