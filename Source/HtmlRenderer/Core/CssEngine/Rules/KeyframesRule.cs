using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class KeyframesRule : Rule, IKeyframesRule
    {
        internal KeyframesRule(StylesheetParser parser)
            : base(RuleType.Keyframes, parser)
        {
            Rules = new RuleList(this);
        }

        protected override void ReplaceWith(IRule rule)
        {
            var newRule = rule as KeyframesRule;
            Name = newRule?.Name;
            base.ReplaceWith(rule);
        }

        public string Name { get; set; }
        public RuleList Rules { get; }
        IRuleList IKeyframesRule.Rules => Rules;

        public void Add(string ruleText)
        {
            var rule = Parser.ParseKeyframeRule(ruleText);
            Rules.Add(rule);
        }

        public void Remove(string key)
        {
            var element = Find(key);
            Rules.Remove(element);
        }

        public KeyframeRule Find(string key)
        {
            return Rules.OfType<KeyframeRule>().FirstOrDefault(m => key.Isi(m.KeyText));
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            writer.Write(formatter.Rule("@keyframes", Name, rules));
        }

        IKeyframeRule IKeyframesRule.Find(string key)
        {
            return Find(key);
        }
    }
}