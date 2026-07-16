using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PageRule : Rule, IPageRule
    {
        internal PageRule(StylesheetParser parser)
            : base(RuleType.Page, parser)
        {
            //AppendChild(SimpleSelector.All);
            AppendChild(new StyleDeclaration(this));
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(formatter.Rule("@page", Selector == null ? "" : SelectorText, "{"));

            Style.ToCss(writer, formatter);

            if (Style.Any())
            {
                writer.Write("; ");
                foreach (var margin in Margins) margin.ToCss(writer, formatter);
            }

            writer.Write("}");
        }

        public string SelectorText
        {
            get => Selector.Text;
            set => Selector = Parser.ParseSelector(value);
        }

        public ISelector Selector
        {
            get => Children.OfType<ISelector>().FirstOrDefault();
            set => ReplaceSingle(Selector, value);
        }

        public StyleDeclaration Style => Children.OfType<StyleDeclaration>().FirstOrDefault();
        public IEnumerable<MarginStyleRule> Margins => Children.OfType<MarginStyleRule>();
    }
}