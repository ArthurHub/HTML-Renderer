using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CompoundSelector : Selectors, ISelector
    {
        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            foreach (var selector in _selectors) writer.Write(selector.Text);
        }
    }
}