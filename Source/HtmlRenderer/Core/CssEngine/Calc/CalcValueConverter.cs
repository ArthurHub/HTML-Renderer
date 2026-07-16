using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Accepts a calc()/min()/max()/clamp() declaration whose resolved type-checking category is within
    /// <c>allowed</c>, composed onto the existing length/percent/number converters via
    /// <c>.Or(...)</c> (see Converters.cs) so support cascades to every property built on them. Runs only
    /// for non-var() declarations — var()-containing values are already routed to Converters.Any by
    /// Property.TrySetValue, and reach this converter again (for the substituted text) via
    /// DomParser.ApplyResolvedPropertyValue's re-parse.
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
