using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FontFeatureValueSetRule : DeclarationRule, IFontFeatureValueSetRule
    {
        internal FontFeatureValueSetRule(StylesheetParser parser, string name)
            : base(RuleType.FontFeatureValueSet, name, parser)
        {
            Name = name;
        }

        public string Name { get; }

        internal static bool IsSetName(string name)
        {
            return name.Is(RuleNames.Styleset) || name.Is(RuleNames.CharacterVariant) ||
                   name.Is(RuleNames.Swash) || name.Is(RuleNames.Ornaments) ||
                   name.Is(RuleNames.Annotation) || name.Is(RuleNames.Stylistic);
        }

        protected override Property CreateNewProperty(string name)
        {
            return PropertyFactory.Instance.CreateFontFeatureValueDescriptor(name);
        }

        public IReadOnlyList<int> GetValues(string name)
        {
            var value = GetValue(name);

            return string.IsNullOrEmpty(value)
                ? new int[0]
                : value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var declarations = formatter.Declarations(Declarations.Where(d => d.HasValue).Select(d => d.ToCss(formatter)));
            writer.Write(string.Concat("@", Name, " { ", declarations, " }"));
        }
    }
}
