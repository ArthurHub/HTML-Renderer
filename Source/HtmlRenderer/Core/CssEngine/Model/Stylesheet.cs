using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class Stylesheet : StylesheetNode
    {
        private readonly StylesheetParser _parser;

        internal Stylesheet(StylesheetParser parser)
        {
            _parser = parser;
            Rules = new RuleList(this);
        }

        internal RuleList Rules { get; }

        internal bool IsUserAgent { get; set; }

        /// <summary>
        /// The resolved absolute URI this stylesheet was loaded from (null for stylesheets with no
        /// external location, e.g. an inline &lt;style&gt; block or caller-supplied CSS text). Relative
        /// <c>url()</c> references inside this stylesheet (nested <c>@import</c>, <c>@font-face src</c>)
        /// resolve against this, not the document's base URI, matching browser behavior for fetched CSS.
        /// </summary>
        internal Uri BaseUri { get; set; }

        public IEnumerable<ICharsetRule> CharacterSetRules => Rules.Where(r => r is CharsetRule).Cast<ICharsetRule>();
        public IEnumerable<IFontFaceRule> FontfaceSetRules => Rules.Where(r => r is FontFaceRule).Cast<IFontFaceRule>();
        public IEnumerable<IMediaRule> MediaRules => Rules.Where(r => r is MediaRule).Cast<IMediaRule>();
        public IEnumerable<IContainerRule> ContainerRules => Rules.Where(r => r is ContainerRule).Cast<IContainerRule>();
        public IEnumerable<IImportRule> ImportRules => Rules.Where(r => r is ImportRule).Cast<IImportRule>();

        public IEnumerable<INamespaceRule> NamespaceRules =>
            Rules.Where(r => r is NamespaceRule).Cast<INamespaceRule>();

        public IEnumerable<IPageRule> PageRules => Rules.Where(r => r is PageRule).Cast<IPageRule>();
        public IEnumerable<IStyleRule> StyleRules => Rules.Where(r => r is StyleRule).Cast<IStyleRule>();

        public IRule Add(RuleType ruleType)
        {
            var rule = _parser.CreateRule(ruleType);
            Rules.Add(rule);
            return rule;
        }

        public void RemoveAt(int index)
        {
            Rules.RemoveAt(index);
        }

        public int Insert(string ruleText, int index)
        {
            var rule = _parser.ParseRule(ruleText);
            rule.Owner = this;
            Rules.Insert(index, rule);

            return index;
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(formatter.Sheet(Rules));
        }
    }
}