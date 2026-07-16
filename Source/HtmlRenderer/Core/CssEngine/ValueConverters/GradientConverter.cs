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

        private static IPropertyValue[] ToGradientStops(List<List<Token>> values, int offset)
        {
            var stops = new IPropertyValue[values.Count - offset];

            for (int i = offset, k = 0; i < values.Count; i++, k++)
            {
                stops[k] = ToGradientStop(values[i]);

                if (stops[k] == null) return null;
            }

            return stops;
        }

        private static IPropertyValue ToGradientStop(List<Token> value)
        {
            var color = default(IPropertyValue);
            var position = default(IPropertyValue);
            var items = value.ToItems();

            if (items.Count != 0)
            {
                position = LengthOrPercentConverter.Convert(items[items.Count - 1]);

                if (position != null)
                {
                    items.RemoveAt(items.Count - 1);
                    // Two-position shorthand: "red 0 50%" — discard the extra position token
                    if (items.Count != 0 && LengthOrPercentConverter.Convert(items[items.Count - 1]) != null)
                        items.RemoveAt(items.Count - 1);
                }
            }

            if (items.Count != 0)
            {
                color = ColorConverter.Convert(items[items.Count - 1]);

                if (color != null) items.RemoveAt(items.Count - 1);
            }

            return items.Count == 0 ? new StopValue(color, position, value) : null;
        }

        protected abstract IPropertyValue ConvertFirstArgument(IEnumerable<Token> value);

        private sealed class StopValue : IPropertyValue
        {
            private readonly IPropertyValue _color;
            private readonly IPropertyValue _position;

            public StopValue(IPropertyValue color, IPropertyValue position, IEnumerable<Token> tokens)
            {
                _color = color;
                _position = position;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    if (_color == null && _position != null) return _position.CssText;

                    if (_color != null && _position == null) return _color.CssText;

                    return string.Concat(_color?.CssText, " ", _position.CssText);
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }

        // Factory used by derived classes to return a placeholder IPropertyValue
        // for "in <colorspace>" modifier groups that are otherwise unrecognized.
        protected static IPropertyValue CreateInColorSpacePlaceholder(IEnumerable<Token> tokens)
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
            // "in <colorspace> [<direction>]" — accept the whole first group as a modifier
            foreach (var t in value)
            {
                if (t.Type == TokenType.Ident &&
                    string.Equals(t.Data, "in", System.StringComparison.OrdinalIgnoreCase))
                    return CreateInColorSpacePlaceholder(value);
            }
            return _converter.Convert(value);
        }
    }

    // Permissive validator for conic-gradient — actual parsing is in CssValueParser.ParseConicGradient
    internal sealed class ConicGradientConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value) => new PassThroughValue(value);
        public IPropertyValue Construct(Property[] properties) => null;

        private sealed class PassThroughValue : IPropertyValue
        {
            public PassThroughValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }
            public string CssText => Original.Text;
            public TokenValue Original { get; }
            public TokenValue ExtractFor(string name) => Original;
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
            // "in <colorspace> [...]" — accept the whole first group as a modifier
            foreach (var t in value)
            {
                if (t.Type == TokenType.Ident &&
                    string.Equals(t.Data, "in", System.StringComparison.OrdinalIgnoreCase))
                    return CreateInColorSpacePlaceholder(value);
            }

            // If the first non-whitespace token is an ident that is not a known gradient
            // shape/size keyword, it must be a CSS color name (e.g. "red" in "red 0 8px").
            // In that case, this comma group is a color stop, not a shape/size modifier.
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