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
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
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
        public static PdfDocument GeneratePdf(string html, PageSize pageSize, int margin = 25, CssData cssData = null, EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null, EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var document = new PdfDocument();
            document.Settings.TrimMargins.All = XUnit.FromPoint(margin);
            var size = PageSizeConverter.ToSize(pageSize);

            if (!string.IsNullOrEmpty(html))
            {
                using (var container = new HtmlContainer())
                {
                    if (stylesheetLoad != null)
                        container.StylesheetLoad += stylesheetLoad;
                    if (imageLoad != null)
                        container.ImageLoad += imageLoad;

                    container.MaxSize = new XSize(size.Width, 0);

                    container.SetHtml(html, cssData);

                    using (var measure = XGraphics.CreateMeasureContext(size, XGraphicsUnit.Point, XPageDirection.Downwards))
                    {
                        container.PerformLayout(measure);
                    }

                    var page = document.AddPage();
                    page.Size = pageSize;
                    using (var g = XGraphics.FromPdfPage(page))
                    {
                        container.PerformPaint(g);
                    }
                }
            }

            return document;
        }
    }
}