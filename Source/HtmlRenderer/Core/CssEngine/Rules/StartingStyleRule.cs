using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>@starting-style { rules }</c> grouping rule (CSS Transitions 2). Holds the style rules that
    /// define the starting values for transitions when an element is first rendered.
    /// </summary>
    internal sealed class StartingStyleRule : GroupingRule, IStartingStyleRule
    {
        internal StartingStyleRule(StylesheetParser parser)
            : base(RuleType.StartingStyle, parser)
        {
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            writer.Write(formatter.Rule("@starting-style", null, rules));
        }
    }
}
