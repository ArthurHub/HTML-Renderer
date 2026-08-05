using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class SupportsRule : ConditionRule, ISupportsRule
    {
        internal SupportsRule(StylesheetParser parser)
            : base(RuleType.Supports, parser)
        {
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            writer.Write(formatter.Rule("@supports", ConditionText, rules));
        }


        public string ConditionText
        {
            get => Condition.ToCss();
            set
            {
                var condition = Parser.ParseCondition(value);

                Condition = condition ?? throw new ParseException("Unable to parse condition");
            }
        }

        public IConditionFunction Condition
        {
            get => Children.OfType<IConditionFunction>().FirstOrDefault() ?? new EmptyCondition();
            set
            {
                if (value == null) return;

                RemoveChild(Condition);
                AppendChild(value);
            }
        }
    }
}