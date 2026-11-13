using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// An <c>@counter-style</c> at-rule (CSS Counter Styles Level 3): defines a named, reusable counter style.
    /// The descriptor block is stored like <see cref="FontPaletteValuesRule"/>; the name is captured from the
    /// prelude like <see cref="KeyframesRule.Name"/>.
    /// </summary>
    internal sealed class CounterStyleRule : DeclarationRule, ICounterStyleRule
    {
        /// <summary>The descriptors the rule accepts; any other declaration is dropped.</summary>
        internal static readonly string[] DescriptorNames =
        {
            "system", "negative", "prefix", "suffix", "range", "pad", "fallback", "symbols", "additive-symbols", "speak-as"
        };

        internal CounterStyleRule(StylesheetParser parser)
            : base(RuleType.CounterStyle, RuleNames.CounterStyle, parser)
        {
        }

        protected override Property CreateNewProperty(string name)
        {
            return PropertyFactory.Instance.CreateCounterStyleDescriptor(name);
        }

        protected override void ReplaceWith(IRule rule)
        {
            if (rule is CounterStyleRule other) Name = other.Name;
            base.ReplaceWith(rule);
        }

        public string Name { get; set; }

        public string System
        {
            get => GetValue("system");
            set => SetValue("system", value);
        }

        public string Negative
        {
            get => GetValue("negative");
            set => SetValue("negative", value);
        }

        public string Prefix
        {
            get => GetValue("prefix");
            set => SetValue("prefix", value);
        }

        public string Suffix
        {
            get => GetValue("suffix");
            set => SetValue("suffix", value);
        }

        public string Range
        {
            get => GetValue("range");
            set => SetValue("range", value);
        }

        public string Pad
        {
            get => GetValue("pad");
            set => SetValue("pad", value);
        }

        public string Fallback
        {
            get => GetValue("fallback");
            set => SetValue("fallback", value);
        }

        public string Symbols
        {
            get => GetValue("symbols");
            set => SetValue("symbols", value);
        }

        public string AdditiveSymbols
        {
            get => GetValue("additive-symbols");
            set => SetValue("additive-symbols", value);
        }

        public string SpeakAs
        {
            get => GetValue("speak-as");
            set => SetValue("speak-as", value);
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var declarations = formatter.Declarations(Declarations.Where(d => d.HasValue).Select(d => d.ToCss(formatter)));
            writer.Write(string.Concat("@counter-style ", Name, " { ", declarations, " }"));
        }
    }
}
