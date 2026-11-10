using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// An <c>@property</c> rule (CSS Properties and Values API 1 §3). Parsed only, the registration has no
    /// effect on the cascade.
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
