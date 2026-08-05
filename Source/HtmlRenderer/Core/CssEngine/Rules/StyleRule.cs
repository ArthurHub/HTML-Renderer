using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StyleRule : Rule, IStyleRule
    {
        // CSS Nesting: rules written inside this rule's block, resolved to absolute selectors against
        // this parent. Kept in a dedicated list (not Children) so Selector/Style lookups and ToCss are
        // unaffected. Populated by StylesheetComposer.FillDeclarations.
        private readonly List<IStyleRule> _nestedRules = new List<IStyleRule>();

        public StyleRule(StylesheetParser parser) : base(RuleType.Style, parser)
        {
            AppendChild(AllSelector.Create());
            AppendChild(new StyleDeclaration(this));
        }

        public IReadOnlyList<IStyleRule> NestedRules => _nestedRules;

        public void AddNestedRule(IStyleRule rule) => _nestedRules.Add(rule);

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(formatter.Style(SelectorText, Style));
        }

        public ISelector Selector
        {
            get => Children.OfType<ISelector>().FirstOrDefault();
            set => ReplaceSingle(Selector, value);
        }

        public string SelectorText
        {
            get => Selector.Text;
            set => Selector = Parser.ParseSelector(value);
        }

        public StyleDeclaration Style => Children.OfType<StyleDeclaration>().FirstOrDefault();
    }
}