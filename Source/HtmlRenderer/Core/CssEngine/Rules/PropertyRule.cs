using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// An <c>@property</c> at-rule (CSS Properties &amp; Values API Level 1 §3): registers a typed custom
    /// property with a <c>syntax</c>, <c>initial-value</c>, and <c>inherits</c> descriptor. The descriptor
    /// block is stored like <see cref="FontFaceRule"/>; the registered name (a dashed-ident such as
    /// <c>--my-color</c>) is captured from the prelude like <see cref="KeyframesRule.Name"/>.
    /// </summary>
    internal sealed class PropertyRule : DeclarationRule, IPropertyRule
    {
        internal PropertyRule(StylesheetParser parser)
            : base(RuleType.Property, RuleNames.Property, parser)
        {
        }

        protected override Property CreateNewProperty(string name)
        {
            return PropertyFactory.Instance.CreatePropertyDescriptor(name);
        }

        protected override void ReplaceWith(IRule rule)
        {
            var other = rule as PropertyRule;
            if (other != null) Name = other.Name;
            base.ReplaceWith(rule);
        }

        public string Name { get; set; }

        public string Syntax
        {
            get { return GetValue(PropertyNames.Syntax); }
            set { SetValue(PropertyNames.Syntax, value); }
        }

        public string InitialValue
        {
            get { return GetValue(PropertyNames.InitialValue); }
            set { SetValue(PropertyNames.InitialValue, value); }
        }

        public string Inherits
        {
            get { return GetValue(PropertyNames.Inherits); }
            set { SetValue(PropertyNames.Inherits, value); }
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var declarations = formatter.Declarations(Declarations.Where(d => d.HasValue).Select(d => d.ToCss(formatter)));
            writer.Write(string.Concat("@property ", Name, " { ", declarations, " }"));
        }
    }
}
