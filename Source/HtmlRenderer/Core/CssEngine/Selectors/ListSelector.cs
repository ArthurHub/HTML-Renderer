using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListSelector : Selectors, ISelector
    {
        public bool IsInvalid { get; internal set; }

        // Selectors 4 §15, the most specific selector of an :is()/:not()/:has() argument. A rule's
        // own selector list counts with the selector that matched, see CssData.GetMatchedSpecificity.
        public override Priority Specificity =>
            _selectors.Count == 0 ? Priority.Zero : _selectors.Max(s => s.Specificity);

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            if (_selectors.Count <= 0) return;
            writer.Write(_selectors[0].Text);

            for (var i = 1; i < _selectors.Count; i++)
            {
                writer.Write(Symbols.Comma);
                writer.Write(_selectors[i].Text);
            }
        }
    }
}