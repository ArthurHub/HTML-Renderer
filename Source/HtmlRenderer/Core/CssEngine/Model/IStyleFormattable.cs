using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IStyleFormattable
    {
        void ToCss(TextWriter writer, IStyleFormatter formatter);
    }
}