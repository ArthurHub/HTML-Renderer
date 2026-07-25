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

        /// <summary>
        /// The converter used to validate a colour stop's position. Length/percentage for linear and
        /// radial gradients (the default); overridden by <see cref="ConicGradientConverter"/>, whose
        /// angular stops are positioned by an &lt;angle&gt; or &lt;percentage&gt; instead.
        /// </summary>
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

            // <color-stop-length> = <length-percentage>{1,2} (CSS Images 4 §3.5.1): a stop may carry two
            // positions, equivalent to two same-colour stops, one at each position. Parsing right-to-left,
            // the position taken above is the second (rightmost); a position immediately before it is the
            // first. Both are kept, in source order, so the value round-trips as authored - the render
            // layer (CssValueParser.ParseLinearGradient) reads the property's serialized value and already
            // expands two positions into two stops, so dropping one here collapsed the solid band it draws.
            // (For conic gradients the same two-position shorthand applies, positioned by <angle>.)
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

            // The two-position form is only defined for a <color-stop>; a bare <length-percentage> with no
            // colour is a <linear-color-hint>, which takes exactly one position.
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

        // Factory used by derived classes to return an opaque placeholder IPropertyValue that echoes
        // its tokens back as CssText - for a group whose grammar is accepted but not further modelled
        // here (e.g. an "in <colorspace>" modifier, or a conic bare-0 / calc() angular stop position).
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
            // "[ <angle> | to <side-or-corner> ] || <color-interpolation-method>" (CSS Images 4). Validate
            // any "in <colorspace> [<hue-method>]" prelude, then validate what remains as the direction.
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (hasIn)
                return remainder.Count == 0 || _converter.Convert(remainder) != null ? CreatePlaceholder(value) : null;

            return _converter.Convert(value);
        }
    }

    // Validates conic-gradient() at parse time (mirroring RadialGradientConverter), so a malformed
    // value is dropped per CSS Cascade §4.1 instead of silently painting nothing at render. The
    // grammar matches CssValueParser.ParseConicGradient exactly.
    internal sealed class ConicGradientConverter : GradientConverter
    {
        private readonly IValueConverter _prelude;

        public ConicGradientConverter()
        {
            // [ from <angle> ]? [ at <position> ]? — same combinator shape RadialGradientConverter
            // uses for its own prelude, just with conic's from/at keywords.
            _prelude = WithAny(
                AngleConverter.StartsWithKeyword(Keywords.From).Option(),
                PointConverter.StartsWithKeyword(Keywords.At).Option(Point.Center));
        }

        // A conic <angular-color-stop> is positioned by <angle> | <percentage>, with the same bare-0
        // and calc()-family acceptance the renderer's TryParseConicAngle allows (AngleConverter already
        // covers calc()-typed angles; a length such as 5px is correctly rejected).
        protected override IValueConverter StopPositionConverter { get; } =
            AngleConverter.Or(PercentConverter).Or(new ZeroOrCalcPositionConverter());

        protected override IPropertyValue ConvertFirstArgument(IEnumerable<Token> value)
        {
            // "[ from <angle> ]? [ at <position> ]? || <color-interpolation-method>" — validate any
            // "in <colorspace> [<hue-method>]" prelude, then validate what remains as from/at.
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (!hasIn)
                return ConvertPrelude(value);

            return remainder.Count == 0 || ConvertPrelude(remainder) != null ? CreatePlaceholder(value) : null;
        }

        private IPropertyValue ConvertPrelude(IEnumerable<Token> value)
        {
            // If the first non-whitespace token is an ident that is not a prelude keyword, this comma
            // group is a color stop, not a prelude — return null so it flows to stop validation.
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

        // Accepts a bare `0` (the universal zero, which <angle> otherwise requires a unit for) or a
        // single calc()-family function used as an angular stop position — matching the corresponding
        // branches of CssValueParser.TryParseConicAngle, using the same primitives.
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
            // "[ <ending-shape> || <size> ]? [ at <position> ]? || <color-interpolation-method>" — validate
            // any "in <colorspace> [<hue-method>]" prelude, then validate what remains as shape/size/position.
            if (!ColorInterpolationMethodGrammar.TryExtractInterpolationMethod(value, out var remainder, out var hasIn))
                return null;

            if (!hasIn)
                return ConvertShape(value);

            return remainder.Count == 0 || ConvertShape(remainder) != null ? CreatePlaceholder(value) : null;
        }

        private IPropertyValue ConvertShape(IEnumerable<Token> value)
        {
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