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

        /// <summary>Validates a color stop position, a length or percentage unless overridden.</summary>
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

            // CSS Images 4 §3.5.1: a stop can have two positions. Both are kept, the painter reads the
            // serialized value.
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
                color = ColorConverter.Convert(items[items.Count - 1]);

                if (color != null) items.RemoveAt(items.Count - 1);
            }

            // A position without a color is a color hint, which only takes one position
            if (secondPosition != null && color == null) return null;

            return items.Count == 0 ? new StopValue(color, firstPosition, secondPosition, value) : null;
        }

        protected abstract IPropertyValue ConvertFirstArgument(IEnumerable<Token> value);

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

        // A validated value that isn't modeled further, serialized as written
        protected static IPropertyValue CreatePlaceholder(IEnumerable<Token> tokens)
            => new PlaceholderValue(tokens);

        private sealed class PlaceholderValue : IPropertyValue
        {
            public PlaceholderValue(IEnumerable<Token> tokens) { Original = new TokenValue(tokens); }
            public string CssText => Original.Text;
            public TokenValue Original { get; }
            public TokenValue ExtractFor(string name) => Original;
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
            // CSS Images 4 §3.1
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (hasIn)
                return remainder.Count == 0 || _converter.Convert(remainder) != null ? CreatePlaceholder(value) : null;

            return _converter.Convert(value);
        }
    }

    // CSS Images 4 §3.3.1
    internal sealed class ConicGradientConverter : GradientConverter
    {
        private readonly IValueConverter _prelude;

        public ConicGradientConverter()
        {
            _prelude = WithAny(
                AngleConverter.StartsWithKeyword(Keywords.From).Option(),
                PointConverter.StartsWithKeyword(Keywords.At).Option(Point.Center));
        }

        // <angle-percentage> | <zero>
        protected override IValueConverter StopPositionConverter { get; } =
            AngleConverter.Or(PercentConverter).Or(new ZeroOrCalcPositionConverter());

        protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
        {
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (!hasIn)
                return ConvertPrelude(value);

            return remainder.Count == 0 || ConvertPrelude(remainder) != null ? CreatePlaceholder(value) : null;
        }

        private IPropertyValue ConvertPrelude(IEnumerable<Token> value)
        {
            // A group starting with any other ident is the first color stop
            Token first = null;
            foreach (var t in value)
            {
                if (t.Type != TokenType.Whitespace) { first = t; break; }
            }
            if (first != null && first.Type == TokenType.Ident)
            {
                var id = first.Data;
                if (!string.Equals(id, Keywords.From, System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, Keywords.At, System.StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            return _prelude.Convert(value);
        }

        // A unitless 0, or any calc()-family function
        private sealed class ZeroOrCalcPositionConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var only = value.OnlyOrDefault();
                if (only == null) return null;

                if (only.Type == TokenType.Number && ((NumberToken)only).Value == 0f)
                    return CreatePlaceholder(value);

                if (only is FunctionToken function && CalcParser.IsCalcFamily(function.Data))
                    return CreatePlaceholder(value);

                return null;
            }

            public IPropertyValue Construct(Property[] properties) => null;
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
            // CSS Images 4 §3.2.1
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (!hasIn)
                return ConvertShape(value);

            return remainder.Count == 0 || ConvertShape(remainder) != null ? CreatePlaceholder(value) : null;
        }

        private IPropertyValue ConvertShape(IEnumerable<Token> value)
        {
            // A group starting with any other ident is the first color stop
            Token first = null;
            foreach (var t in value)
            {
                if (t.Type != TokenType.Whitespace) { first = t; break; }
            }
            if (first != null && first.Type == TokenType.Ident)
            {
                var id = first.Data;
                if (!string.Equals(id, "circle", System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, "ellipse", System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, "at", System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, "closest-side", System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, "farthest-corner", System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, "closest-corner", System.StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(id, "farthest-side", System.StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            return _converter.Convert(value);
        }
    }
}