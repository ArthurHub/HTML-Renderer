using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The statement form of <c>@layer</c>, CSS Cascade 5 §6.4.4.2. Parsed, but the layer order it
    /// declares isn't used.
    /// </summary>
    internal sealed class LayerStatementRule : Rule
    {
        internal LayerStatementRule(StylesheetParser parser)
            : base(RuleType.LayerStatement, parser)
        {
        }

        public List<string> Names { get; } = new List<string>();

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(formatter.Rule("@layer", string.Join(", ", Names)));
        }
    }
}
