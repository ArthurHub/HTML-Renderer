using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListSelector : Selectors, ISelector
    {
        public bool IsInvalid { get; internal set; }

        // A comma-separated selector list's specificity is NOT the sum of its alternatives (that's
        // only correct for a CompoundSelector, whose members all apply to the SAME element at once).
        // Per spec, when used as the "forgiving selector list" argument to :is()/:not()/:has(), it's
        // the static max across all alternatives - regardless of which one (if any) actually matches
        // a given element. When a ListSelector is a style rule's own top-level selector instead
        // (".a, .b { }"), the spec treats it as shorthand for separate rules, so the specificity that
        // matters there is whichever ONE alternative matched a given box - that per-match resolution
        // is handled separately by CssData.GetMatchedSpecificity, which deliberately does not use this
        // property for that case.
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