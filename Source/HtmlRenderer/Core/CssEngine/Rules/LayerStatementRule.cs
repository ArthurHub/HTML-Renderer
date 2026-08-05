using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The statement form of <c>@layer</c> (<c>@layer a, b, c;</c>). It declares the relative order of
    /// one or more named cascade layers without providing any rules; a later block-form
    /// <see cref="LayerRule"/> attaches rules to a layer already ordered by this statement, per
    /// <see href="https://www.w3.org/TR/css-cascade-5/#layer-empty">CSS Cascade 5</see>.
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
