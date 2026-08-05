using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The block form of <c>@layer</c> (<c>@layer name { rules }</c> or anonymous <c>@layer { rules }</c>).
    /// A grouping rule holding the style rules assigned to a cascade layer; cascade precedence between
    /// layers is resolved when the rules are indexed (see <c>CssData</c>), not here.
    /// </summary>
    internal sealed class LayerRule : GroupingRule, ILayerRule
    {
        internal LayerRule(StylesheetParser parser)
            : base(RuleType.Layer, parser)
        {
        }

        public string Name { get; set; } = string.Empty;

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            var name = string.IsNullOrEmpty(Name) ? "@layer" : $"@layer {Name}";
            writer.Write(formatter.Rule(name, null, rules));
        }
    }
}
