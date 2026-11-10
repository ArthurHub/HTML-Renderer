using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Accepts a calc(), min(), max() or clamp() whose type is one of <c>allowed</c>. A value with var()
    /// gets here only after substitution.
    /// </summary>
    internal sealed class CalcValueConverter : IValueConverter
    {
        private readonly CalcCategory _allowed;

        public CalcValueConverter(CalcCategory allowed)
        {
            _allowed = allowed;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            if (!(value.OnlyOrDefault() is FunctionToken function)) return null;
            if (!CalcParser.IsCalcFamily(function.Data)) return null;

            var node = CalcParser.Parse(function);
            if (node is null) return null;

            var category = CalcTypeChecker.Check(node);
            if (category is null || (category.Value & ~_allowed) != 0) return null;

            return new CalcValue(node, category.Value, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<CalcValue>();
        }
    }
}
