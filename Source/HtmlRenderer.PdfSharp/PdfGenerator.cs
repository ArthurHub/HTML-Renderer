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

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace HtmlRenderer.PdfSharp
{
    /// <summary>
    /// 
    /// </summary>
    public static class PdfGenerator
    {
        public static PdfDocument GeneratePdf(string html, PageSize pageSize)
        {
            var size = PageSizeConverter.ToSize(pageSize);

            using (var measure = XGraphics.CreateMeasureContext(size, XGraphicsUnit.Point, XPageDirection.Downwards))
            { }

            var document = new PdfDocument();

            var page = document.AddPage();

            return document;
        }
    }
}