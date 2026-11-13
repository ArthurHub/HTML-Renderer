using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ListSelector : Selectors, ISelector
    {
        public bool IsInvalid { get; internal set; }

        // A comma-separated selector list's specificity is not the sum of its alternatives (that sum is
        // only meaningful for a CompoundSelector, whose members all constrain one element at once). As the
        // argument of :is()/:not()/:has() it is the static max across the alternatives - the most specific
        // one, regardless of which (if any) matches a given element (CSS Selectors 4 16.1). A top-level
        // list (".a, .b { }") is shorthand for separate rules, each with its own per-alternative
        // specificity, so no single value is meaningful there; max is the closest sensible answer.
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