using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>One of the CSS Borders 4 corner shorthands: a name, the grammar it parses and the longhands it expands to.</summary>
    internal sealed class CornerShorthandProperty : ShorthandProperty
    {
        private readonly IValueConverter _converter;

        internal CornerShorthandProperty(string name, IValueConverter converter)
            : base(name, PropertyFlags.Animatable)
        {
            _converter = converter;
        }

        internal override IValueConverter Converter => _converter;
    }

    /// <summary>
    /// The shorthands of CSS Borders 4: <c>border-top-radius</c>... (3.6), <c>corner-shape</c>,
    /// <c>corner-top-shape</c>... (3.8) and <c>corner</c>, <c>corner-top</c>, <c>corner-top-left</c>... (3.9).
    /// </summary>
    internal static class CornerShorthands
    {
        private static readonly string[] PhysicalCorners = { "top-left", "top-right", "bottom-right", "bottom-left" };

        private static readonly string[] LogicalCorners = { "start-start", "start-end", "end-start", "end-end" };

        // Side name -> the two corners it covers, in the order the shorthand's two values apply to them.
        private static readonly string[][] Sides =
        {
            new[] { "top", "top-left", "top-right" },
            new[] { "right", "top-right", "bottom-right" },
            new[] { "bottom", "bottom-left", "bottom-right" },
            new[] { "left", "top-left", "bottom-left" },
            new[] { "block-start", "start-start", "start-end" },
            new[] { "block-end", "end-start", "end-end" },
            new[] { "inline-start", "start-start", "end-start" },
            new[] { "inline-end", "start-end", "end-end" }
        };

        private static string Shape(string corner) => "corner-" + corner + "-shape";

        private static string Radius(string corner) => "border-" + corner + "-radius";

        /// <summary>Hands every shorthand to <paramref name="add"/> as (name, converter, longhands).</summary>
        public static void Register(Action<string, IValueConverter, string[]> add)
        {
            var radii = new[]
            {
                PropertyNames.BorderTopLeftRadius, PropertyNames.BorderTopRightRadius,
                PropertyNames.BorderBottomRightRadius, PropertyNames.BorderBottomLeftRadius
            };
            var shapes = new string[4];

            for (var i = 0; i < 4; i++) shapes[i] = Shape(PhysicalCorners[i]);

            // corner-shape: <corner-shape-value>{1,4}
            add(PropertyNames.CornerShape, CornerShapeConverter.Periodic(shapes).OrDefault(), shapes);

            // corner: <corner-shape> || <border-radius>
            add(PropertyNames.Corner, WithAny(
                    CornerShapeConverter.Periodic(shapes).Option().For(shapes),
                    BorderRadiusShorthandConverter.Option().For(radii)).OrDefault(),
                Concat(shapes, radii));

            foreach (var side in Sides)
            {
                var sideShapes = new[] { Shape(side[1]), Shape(side[2]) };
                var sideRadii = new[] { Radius(side[1]), Radius(side[2]) };
                var radiusConverter = new BorderRadiusConverter(sideRadii);

                // corner-top-shape: <corner-shape-value>{1,2}
                add("corner-" + side[0] + "-shape", CornerShapeConverter.Periodic(sideShapes).OrDefault(), sideShapes);

                // border-top-radius: <length-percentage>{1,2} [ / <length-percentage>{1,2} ]?
                add("border-" + side[0] + "-radius", radiusConverter.OrDefault(), sideRadii);

                // corner-top: <corner-top-shape> || <border-top-radius>
                add("corner-" + side[0], WithAny(
                        CornerShapeConverter.Periodic(sideShapes).Option().For(sideShapes),
                        radiusConverter.Option().For(sideRadii)).OrDefault(),
                    Concat(sideShapes, sideRadii));
            }

            foreach (var corners in new[] { PhysicalCorners, LogicalCorners })
            {
                foreach (var corner in corners)
                {
                    var shape = Shape(corner);
                    var radius = Radius(corner);

                    // corner-top-left: <corner-top-left-shape> || <border-top-left-radius>
                    add("corner-" + corner, WithAny(
                            CornerShapeConverter.Option().For(shape),
                            Converters.BorderRadiusConverter.Option().For(radius)).OrDefault(),
                        new[] { shape, radius });
                }
            }
        }

        private static string[] Concat(string[] a, string[] b)
        {
            var result = new string[a.Length + b.Length];

            a.CopyTo(result, 0);
            b.CopyTo(result, a.Length);

            return result;
        }
    }
}
