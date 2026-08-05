namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class Rule : StylesheetNode, IRule
    {
        private IRule _parentRule;

        internal Rule(RuleType type, StylesheetParser parser)
        {
            Type = type;
            Parser = parser;
        }

        internal StylesheetParser Parser { get; }
        public Stylesheet Owner { get; internal set; }
        public RuleType Type { get; }

        public string Text
        {
            get => this.ToCss();
            set
            {
                var rule = Parser.ParseRule(value);

                if (rule == null) throw new ParseException("Unable to parse rule");
                if (rule.Type != Type) throw new ParseException("Invalid rule type");
                ReplaceWith(rule);
            }
        }

        public IRule Parent
        {
            get => _parentRule;
            internal set
            {
                _parentRule = value;

                if (value != null) Owner = _parentRule.Owner;
            }
        }

        protected virtual void ReplaceWith(IRule rule)
        {
            ReplaceAll(rule);
        }

        protected void ReplaceSingle(IStylesheetNode oldNode, IStylesheetNode newNode)
        {
            if (oldNode != null)
            {
                if (newNode != null)
                {
                    ReplaceChild(oldNode, newNode);
                }
                else
                {
                    RemoveChild(oldNode);
                }
            }
            else if (newNode != null)
            {
                AppendChild(newNode);
            }
        }
    }
}