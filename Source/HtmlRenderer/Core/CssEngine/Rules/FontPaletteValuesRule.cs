using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A <c>@font-palette-values</c> at-rule (CSS Fonts Module Level 4): associates a named palette override
    /// (a dashed-ident such as <c>--my-palette</c>) with a font family, selecting a <c>base-palette</c> and
    /// optionally overriding individual colors (<c>override-colors</c>). The descriptor block is stored like
    /// <see cref="PropertyRule"/>; the name is captured from the prelude like <see cref="KeyframesRule.Name"/>.
    /// </summary>
    internal sealed class FontPaletteValuesRule : DeclarationRule, IFontPaletteValuesRule
    {
        internal FontPaletteValuesRule(StylesheetParser parser)
            : base(RuleType.FontPaletteValues, RuleNames.FontPaletteValues, parser)
        {
        }

        protected override Property CreateNewProperty(string name)
        {
            return PropertyFactory.Instance.CreateFontPaletteDescriptor(name);
        }

        protected override void ReplaceWith(IRule rule)
        {
            if (rule is FontPaletteValuesRule other) Name = other.Name;
            base.ReplaceWith(rule);
        }

        public string Name { get; set; }

        public string Family
        {
            get => GetValue(PropertyNames.FontFamily);
            set => SetValue(PropertyNames.FontFamily, value);
        }

        public string BasePalette
        {
            get => GetValue(PropertyNames.BasePalette);
            set => SetValue(PropertyNames.BasePalette, value);
        }

        public string OverrideColors
        {
            get => GetValue(PropertyNames.OverrideColors);
            set => SetValue(PropertyNames.OverrideColors, value);
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var declarations = formatter.Declarations(Declarations.Where(d => d.HasValue).Select(d => d.ToCss(formatter)));
            writer.Write(string.Concat("@font-palette-values ", Name, " { ", declarations, " }"));
        }
    }
}
