using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal abstract class GradientConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = value.ToList();
            var initial = args.Count != 0 ? ConvertFirstArgument(args[0]) : null;
            var offset = initial != null ? 1 : 0;
            var stops = ToGradientStops(args, offset);
            return stops != null ? new GradientValue(initial, stops, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GradientValue>();
        }

        // The converter that parses a stop's position. Linear/radial stops are positioned with a
        // <length-percentage>; a conic gradient overrides this with an <angle-percentage> (CSS Images 4).
        protected virtual IValueConverter StopPositionConverter => LengthOrPercentConverter;

        private IPropertyValue[] ToGradientStops(List<List<Token>> values, int offset)
        {
            var stops = new IPropertyValue[values.Count - offset];

            for (int i = offset, k = 0; i < values.Count; i++, k++)
            {
                stops[k] = ToGradientStop(values[i]);

                if (stops[k] == null) return null;
            }

            return stops;
        }

        private IPropertyValue ToGradientStop(List<Token> value)
        {
            var color = default(IPropertyValue);
            var firstPosition = default(IPropertyValue);
            var secondPosition = default(IPropertyValue);
            var items = value.ToItems();

            if (items.Count != 0)
            {
                firstPosition = StopPositionConverter.Convert(items[items.Count - 1]);

                if (firstPosition != null) items.RemoveAt(items.Count - 1);
            }

            // <color-stop-length> = <length-percentage>{1,2} (CSS Images 4 3.5.1) - or
            // <angle-percentage>{1,2} for a conic gradient, hence StopPositionConverter: a stop may carry
            // two positions, equivalent to two same-colour stops, one at each position. Parsing
            // right-to-left, the position taken above is the second (rightmost); a position immediately
            // before it is the first, and the two are kept in source order for serialization.
            if (firstPosition != null && items.Count != 0)
            {
                var earlier = StopPositionConverter.Convert(items[items.Count - 1]);

                if (earlier != null)
                {
                    secondPosition = firstPosition;
                    firstPosition = earlier;
                    items.RemoveAt(items.Count - 1);
                }
            }

            if (items.Count != 0)
            {
                color = StopColorConverter.Convert(items[items.Count - 1]);

                if (color != null) items.RemoveAt(items.Count - 1);
            }

            // The two-position form is only defined for a <color-stop>; a bare <length-percentage> with no
            // colour is a <linear-color-hint>, which takes exactly one position.
            if (secondPosition != null && color == null) return null;

            return items.Count == 0 ? new StopValue(color, firstPosition, secondPosition, value) : null;
        }

        protected abstract IPropertyValue ConvertFirstArgument(IEnumerable<Token> value);

        // A gradient stop's colour: any <color>, including currentcolor (CSS Color 4).
        private static IValueConverter StopColorConverter { get; } = ColorConverter.WithCurrentColor();

        // Keeps the authored text of a prelude that carries a <color-interpolation-method>; the grammar is
        // validated by ColorInterpolationMethodGrammar but not otherwise interpreted.
        protected static IPropertyValue CreatePlaceholder(IEnumerable<Token> tokens)
        {
            return new PlaceholderValue(tokens);
        }

        private sealed class PlaceholderValue : IPropertyValue
        {
            public PlaceholderValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }

        private sealed class StopValue : IPropertyValue
        {
            private readonly IPropertyValue _color;
            private readonly IPropertyValue _firstPosition;
            private readonly IPropertyValue _secondPosition;

            public StopValue(IPropertyValue color, IPropertyValue firstPosition, IPropertyValue secondPosition,
                IEnumerable<Token> tokens)
            {
                _color = color;
                _firstPosition = firstPosition;
                _secondPosition = secondPosition;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    var parts = new List<string>(3);

                    if (_color != null) parts.Add(_color.CssText);
                    if (_firstPosition != null) parts.Add(_firstPosition.CssText);
                    if (_secondPosition != null) parts.Add(_secondPosition.CssText);

                    return string.Join(" ", parts);
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }

        private sealed class GradientValue : IPropertyValue
        {
            private readonly IPropertyValue _initial;
            private readonly IPropertyValue[] _stops;

            public GradientValue(IPropertyValue initial, IPropertyValue[] stops, IEnumerable<Token> tokens)
            {
                _initial = initial;
                _stops = stops;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    var count = _stops.Length;

                    if (_initial != null) count++;

                    var args = new string[count];
                    count = 0;

                    if (_initial != null) args[count++] = _initial.CssText;

                    foreach (var propertyValue in _stops) args[count++] = propertyValue.CssText;

                    return string.Join(", ", args);
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }

    internal sealed class LinearGradientConverter : GradientConverter
    {
        private readonly IValueConverter _converter;

        public LinearGradientConverter()
        {
            _converter = AngleConverter.Or(
                SideOrCornerConverter.StartsWithKeyword(Keywords.To));
        }

        protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
        {
            // "[ <angle> | to <side-or-corner> ] || <color-interpolation-method>" (CSS Images 4).
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (hasIn)
                return remainder.Count == 0 || _converter.Convert(remainder) != null ? CreatePlaceholder(value) : null;

            return _converter.Convert(value);
        }
    }

    internal sealed class RadialGradientConverter : GradientConverter
    {
        private readonly IValueConverter _converter;

        public RadialGradientConverter()
        {
            var position = PointConverter.StartsWithKeyword(Keywords.At).Option(Point.Center);
            var circle = WithOrder(WithAny(Assign(Keywords.Circle, true).Option(true),
                    LengthConverter.Option()),
                position);

            var ellipse = WithOrder(WithAny(Assign(Keywords.Ellipse, false).Option(false),
                    LengthOrPercentConverter.Many(2, 2).Option()),
                position);

            var extents = WithOrder(WithAny(Toggle(Keywords.Circle, Keywords.Ellipse).Option(false),
                Map.RadialGradientSizeModes.ToConverter()), position);

            _converter = circle.Or(ellipse.Or(extents));
        }

        protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
        {
            // "[ <ending-shape> || <size> ]? [ at <position> ]? || <color-interpolation-method>".
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (hasIn)
                return remainder.Count == 0 || _converter.Convert(remainder) != null ? CreatePlaceholder(value) : null;

            return _converter.Convert(value);
        }
    }

    internal sealed class ConicGradientConverter : GradientConverter
    {
        private readonly IValueConverter _converter;

        public ConicGradientConverter()
        {
            // The conic prelude is "[ from <angle> ]? [ at <position> ]?" (CSS Images 4 3.4).
            var from = AngleConverter.StartsWithKeyword(Keywords.From).Option();
            var at = PointConverter.StartsWithKeyword(Keywords.At).Option();

            _converter = WithOrder(from, at);
        }

        // A conic gradient's stops are positioned by angle, not length: <angular-color-stop> uses an
        // <angle-percentage> (CSS Images 4 3.4.1).
        protected override IValueConverter StopPositionConverter { get; } =
            AngleConverter.Or(PercentConverter)
                .Or(new ZeroPositionConverter())
                .Or(new CalcValueConverter(CalcCategory.Angle | CalcCategory.Percentage));

        // A bare 0 is accepted as an angular stop position (the universal zero, which <angle> otherwise
        // requires a unit for).
        private sealed class ZeroPositionConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                return value.OnlyOrDefault() is NumberToken number && number.Value == 0f
                    ? CreatePlaceholder(value)
                    : null;
            }

            public IPropertyValue Construct(Property[] properties) => null;
        }

        protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
        {
            // "[ from <angle> ]? [ at <position> ]? || <color-interpolation-method>".
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (hasIn)
                return remainder.Count == 0 || ConvertPrelude(remainder) != null ? CreatePlaceholder(value) : null;

            return ConvertPrelude(value);
        }

        private IPropertyValue ConvertPrelude(IEnumerable<Token> value)
        {
            // The prelude is optional, but this is only called for the gradient's first comma group. If
            // that group is actually the first color stop (no from/at), it must not be swallowed here - a
            // group that begins with a color, an angle, or a percentage is a stop, so reject it as a
            // prelude and let the caller treat it as the first stop.
            foreach (var token in value)
            {
                if (token.Type == TokenType.Whitespace) continue;

                var isPreludeKeyword = token.Type == TokenType.Ident &&
                    (token.Data.Isi(Keywords.From) || token.Data.Isi(Keywords.At));

                if (!isPreludeKeyword) return null;
                break;
            }

            return _converter.Convert(value);
        }
    }
}