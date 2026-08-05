using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class NamespaceRule : Rule, INamespaceRule
    {
        private string _namespaceUri;
        private string _prefix;

        internal NamespaceRule(StylesheetParser parser)
            : base(RuleType.Namespace, parser)
        {
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var space = string.IsNullOrEmpty(_prefix) ? string.Empty : " ";
            var value = string.Concat(_prefix, space, _namespaceUri.StylesheetUrl());
            writer.Write(formatter.Rule("@namespace", value));
        }

        protected override void ReplaceWith(IRule rule)
        {
            var newRule = rule as NamespaceRule;
            _namespaceUri = newRule?._namespaceUri;
            _prefix = newRule?._prefix;
            base.ReplaceWith(rule);
        }

        public string NamespaceUri
        {
            get => _namespaceUri;
            set
            {
                CheckValidity();
                _namespaceUri = value ?? string.Empty;
            }
        }

        public string Prefix
        {
            get => _prefix;
            set
            {
                CheckValidity();
                _prefix = value ?? string.Empty;
            }
        }

        private static bool IsNotSupported(RuleType type)
        {
            return type != RuleType.Charset && type != RuleType.Import && type != RuleType.Namespace;
        }

        private void CheckValidity()
        {
            var parent = Owner;
            var list = parent?.Rules;

            if (list == null) return;

            if (list.Any(entry => IsNotSupported(entry.Type))) throw new ParseException("Rule is not supported");
        }
    }
}