using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class DocumentRule : GroupingRule
    {
        internal DocumentRule(StylesheetParser parser)
            : base(RuleType.Document, parser)
        {
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            writer.Write(formatter.Rule("@document", ConditionText, rules));
        }

        public string ConditionText
        {
            get
            {
                var entries = Conditions.Select(m => m.ToCss());
                return string.Join(", ", entries);
            }
            set
            {
                var conditions = Parser.ParseDocumentRules(value);

                if (conditions == null) throw new ParseException("Unable to parse document rules");

                Clear();

                foreach (var condition in conditions) AppendChild(condition);
            }
        }

        public IEnumerable<IDocumentFunction> Conditions => Children.OfType<IDocumentFunction>();
    }
}