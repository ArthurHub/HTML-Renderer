using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The block form of <c>@layer</c>, CSS Cascade 5 §6.4.4.1. <c>CssData</c> indexes its rules as
    /// if they were unlayered.
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
