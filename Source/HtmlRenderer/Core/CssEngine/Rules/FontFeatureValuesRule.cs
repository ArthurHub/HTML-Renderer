using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontFeatureValuesRule : Rule, IFontFeatureValuesRule
    {
        internal FontFeatureValuesRule(StylesheetParser parser)
            : base(RuleType.FontFeatureValues, parser)
        {
            Rules = new RuleList(this);
        }

        protected override void ReplaceWith(IRule rule)
        {
            if (rule is FontFeatureValuesRule newRule)
            {
                FamilyList.Clear();
                FamilyList.AddRange(newRule.FamilyList);
            }

            base.ReplaceWith(rule);
        }

        internal List<string> FamilyList { get; } = new List<string>();
        public IReadOnlyList<string> FontFamilies => FamilyList;

        public RuleList Rules { get; }
        IRuleList IFontFeatureValuesRule.Rules => Rules;

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            writer.Write(formatter.Rule("@font-feature-values", string.Join(", ", FamilyList), rules));
        }
    }
}
