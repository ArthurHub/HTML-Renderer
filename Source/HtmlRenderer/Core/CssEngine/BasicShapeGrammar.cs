using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Shared, layer-agnostic grammar for the CSS <c>&lt;basic-shape&gt;</c> function values used by
    /// <c>clip-path</c> (CSS Masking Level 1 / CSS Shapes Level 1): <c>polygon()</c>, <c>inset()</c>,
    /// <c>circle()</c> and <c>ellipse()</c>. Like <see cref="BackgroundPositionGrammar"/> /
    /// <see cref="BackgroundSizeGrammar"/>, it validates the grammar and captures the value's structure
    /// as <b>raw component strings / enums</b> (never resolved numbers), so both Layer A (the CSS-OM
    /// converter, which only needs to accept/reject and preserve the authored text) and Layer B (the
    /// render-time resolver in <c>TheArtOfDev.HtmlRenderer.Core</c>, which resolves each component against the
    /// element's reference box) share a single parser rather than re-implementing the grammar twice.
    /// </summary>
    internal static class BasicShapeGrammar
    {
        internal enum BasicShapeKind { Polygon, Inset, Circle, Ellipse }

        internal enum FillRule { Nonzero, Evenodd }

        internal enum ShapeRadiusKind { LengthPercentage, ClosestSide, FarthestSide }

        /// <summary>A <c>&lt;shape-radius&gt;</c>: either an explicit length-percentage (component string
        /// in <see cref="Length"/>) or one of the <c>closest-side</c>/<c>farthest-side</c> keywords.</summary>
        internal readonly struct ShapeRadius
        {
            public ShapeRadiusKind Kind { get; }

            /// <summary>The length-percentage component string when <see cref="Kind"/> is
            /// <see cref="ShapeRadiusKind.LengthPercentage"/>; otherwise null.</summary>
            public string Length { get; }

            private ShapeRadius(ShapeRadiusKind kind, string length)
            {
                Kind = kind;
                Length = length;
            }

            public static readonly ShapeRadius ClosestSide = new ShapeRadius(ShapeRadiusKind.ClosestSide, null);
            public static readonly ShapeRadius FarthestSide = new ShapeRadius(ShapeRadiusKind.FarthestSide, null);
            public static ShapeRadius FromLength(string length) => new ShapeRadius(ShapeRadiusKind.LengthPercentage, length);
        }

        /// <summary>A single <c>&lt;length-percentage&gt; &lt;length-percentage&gt;</c> polygon vertex,
        /// stored as the two authored component strings.</summary>
        internal readonly struct Point
        {
            public string X { get; }
            public string Y { get; }

            public Point(string x, string y)
            {
                X = x;
                Y = y;
            }
        }

        internal sealed class ParsedBasicShape
        {
            public BasicShapeKind Kind { get; private set; }

            // --- polygon() ---
            public FillRule PolygonFillRule { get; private set; }
            public IReadOnlyList<Point> PolygonPoints { get; private set; }

            // --- inset() ---
            /// <summary>The four inset offsets in [top, right, bottom, left] order (CSS shorthand-filled),
            /// each an authored length-percentage component string.</summary>
            public IReadOnlyList<string> InsetEdges { get; private set; }

            /// <summary>Whether an <c>inset(... round &lt;border-radius&gt;)</c> corner radius was present.
            /// The radius itself is captured in <see cref="InsetRoundRadius"/> but not rendered - inset is
            /// drawn as a rectangle (see the resolver / docs).</summary>
            public bool InsetHasRound { get; private set; }
            public IReadOnlyList<Token> InsetRoundRadius { get; private set; }

            // --- circle() / ellipse() ---
            /// <summary>circle: the single radius. ellipse: the x-radius.</summary>
            public ShapeRadius RadiusX { get; private set; }
            /// <summary>ellipse: the y-radius. Unused for circle.</summary>
            public ShapeRadius RadiusY { get; private set; }
            /// <summary>Center x as an authored length-percentage component string (position keywords
            /// already resolved to <c>0%</c>/<c>50%</c>/<c>100%</c>). Default <c>50%</c>.</summary>
            public string CenterX { get; private set; }
            /// <summary>Center y, same convention as <see cref="CenterX"/>.</summary>
            public string CenterY { get; private set; }

            internal static ParsedBasicShape Polygon(FillRule fillRule, IReadOnlyList<Point> points) => new ParsedBasicShape
            {
                Kind = BasicShapeKind.Polygon,
                PolygonFillRule = fillRule,
                PolygonPoints = points,
            };

            internal static ParsedBasicShape Inset(IReadOnlyList<string> edges, bool hasRound, IReadOnlyList<Token> roundRadius) => new ParsedBasicShape
            {
                Kind = BasicShapeKind.Inset,
                InsetEdges = edges,
                InsetHasRound = hasRound,
                InsetRoundRadius = roundRadius,
            };

            internal static ParsedBasicShape Circle(ShapeRadius radius, string centerX, string centerY) => new ParsedBasicShape
            {
                Kind = BasicShapeKind.Circle,
                RadiusX = radius,
                CenterX = centerX,
                CenterY = centerY,
            };

            internal static ParsedBasicShape Ellipse(ShapeRadius radiusX, ShapeRadius radiusY, string centerX, string centerY) => new ParsedBasicShape
            {
                Kind = BasicShapeKind.Ellipse,
                RadiusX = radiusX,
                RadiusY = radiusY,
                CenterX = centerX,
                CenterY = centerY,
            };
        }

        /// <summary>
        /// Parses a <c>clip-path</c> value's tokens into a <see cref="ParsedBasicShape"/>, or returns
        /// null when the value is not a valid basic shape - <b>including the literal <c>none</c></b>,
        /// which callers treat as "no clip". (Layer A therefore accepts <c>none</c> separately, since a
        /// null result here can't distinguish <c>none</c> from an invalid value.)
        /// </summary>
        internal static ParsedBasicShape TryParse(IReadOnlyList<Token> tokens)
        {
            var significant = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (significant.Length != 1) return null;
            var function = significant[0] as FunctionToken;
            if (function == null) return null;

            var args = function.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (function.Data.Isi(FunctionNames.Polygon)) return ParsePolygon(args);
            if (function.Data.Isi(FunctionNames.Inset)) return ParseInset(args);
            if (function.Data.Isi(FunctionNames.Circle)) return ParseCircle(args);
            if (function.Data.Isi(FunctionNames.Ellipse)) return ParseEllipse(args);

            return null;
        }

        private static ParsedBasicShape ParsePolygon(IReadOnlyList<Token> args)
        {
            var groups = SplitByComma(args);
            if (groups.Count == 0) return null;

            var fillRule = FillRule.Nonzero;
            var firstGroup = 0;

            // An optional leading fill-rule ident is its own comma-separated group: "polygon(evenodd, x y, ...)".
            if (groups[0].Count == 1 && TryFillRule(groups[0][0], out var parsedRule))
            {
                fillRule = parsedRule;
                firstGroup = 1;
            }

            var points = new List<Point>();

            for (var i = firstGroup; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group.Count != 2) return null;
                if (!IsLengthPercentage(group[0]) || !IsLengthPercentage(group[1])) return null;
                points.Add(new Point(group[0].ToValue(), group[1].ToValue()));
            }

            return points.Count == 0 ? null : ParsedBasicShape.Polygon(fillRule, points);
        }

        private static ParsedBasicShape ParseInset(IReadOnlyList<Token> args)
        {
            if (args.Count == 0) return null;

            var roundIndex = -1;
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i].Type == TokenType.Ident && args[i].Data.Isi(Keywords.Round))
                {
                    roundIndex = i;
                    break;
                }
            }

            var lengthTokens = roundIndex >= 0 ? args.Take(roundIndex).ToArray() : args.ToArray();
            Token[] roundTokens = roundIndex >= 0 ? args.Skip(roundIndex + 1).ToArray() : new Token[0];

            if (lengthTokens.Length < 1 || lengthTokens.Length > 4) return null;
            if (lengthTokens.Any(t => !IsLengthPercentage(t))) return null;

            // "round" with no radius following it is malformed.
            if (roundIndex >= 0 && roundTokens.Length == 0) return null;

            var values = lengthTokens.Select(t => t.ToValue()).ToArray();
            var (top, right, bottom, left) = ExpandBox(values);

            return ParsedBasicShape.Inset(new[] { top, right, bottom, left }, roundIndex >= 0, roundTokens);
        }

        private static ParsedBasicShape ParseCircle(IReadOnlyList<Token> args)
        {
            if (!SplitAtKeyword(args, Keywords.At, out var radiusTokens, out var hasAt, out var positionTokens))
                return null;

            ShapeRadius radius;
            switch (radiusTokens.Count)
            {
                case 0:
                    radius = ShapeRadius.ClosestSide;
                    break;
                case 1:
                    if (!TryShapeRadius(radiusTokens[0], out radius)) return null;
                    break;
                default:
                    return null;
            }

            if (!ResolveCenter(hasAt, positionTokens, out var centerX, out var centerY)) return null;

            return ParsedBasicShape.Circle(radius, centerX, centerY);
        }

        private static ParsedBasicShape ParseEllipse(IReadOnlyList<Token> args)
        {
            if (!SplitAtKeyword(args, Keywords.At, out var radiusTokens, out var hasAt, out var positionTokens))
                return null;

            ShapeRadius rx, ry;
            switch (radiusTokens.Count)
            {
                case 0:
                    rx = ShapeRadius.ClosestSide;
                    ry = ShapeRadius.ClosestSide;
                    break;
                case 2:
                    if (!TryShapeRadius(radiusTokens[0], out rx)) return null;
                    if (!TryShapeRadius(radiusTokens[1], out ry)) return null;
                    break;
                default:
                    return null;
            }

            if (!ResolveCenter(hasAt, positionTokens, out var centerX, out var centerY)) return null;

            return ParsedBasicShape.Ellipse(rx, ry, centerX, centerY);
        }

        /// <summary>Resolves the optional <c>at &lt;position&gt;</c> tail (via the shared
        /// <see cref="BackgroundPositionGrammar"/>) to center-x/center-y component strings; defaults to
        /// <c>50% 50%</c> when absent.</summary>
        private static bool ResolveCenter(bool hasAt, IReadOnlyList<Token> positionTokens, out string centerX, out string centerY)
        {
            centerX = "50%";
            centerY = "50%";

            if (!hasAt) return true;

            var parsed = BackgroundPositionGrammar.TryParse(positionTokens);
            if (parsed is null) return false;

            centerX = ComponentToLength(parsed.X, horizontal: true);
            centerY = ComponentToLength(parsed.Y, horizontal: false);
            return true;
        }

        private static string ComponentToLength(BackgroundPositionGrammar.Component c, bool horizontal)
        {
            switch (c.Keyword)
            {
                case BackgroundPositionGrammar.AxisKeyword.Center:
                    return "50%";
                case BackgroundPositionGrammar.AxisKeyword.Left:
                case BackgroundPositionGrammar.AxisKeyword.Top:
                    // "left"/"top" == 0% edge; "left 20px" == 20px in from that edge.
                    return c.Offset != null ? c.Offset.ToValue() : "0%";
                case BackgroundPositionGrammar.AxisKeyword.Right:
                case BackgroundPositionGrammar.AxisKeyword.Bottom:
                    // "right"/"bottom" == 100% edge; "right 20px" == 20px in from the far edge.
                    return c.Offset != null ? $"calc(100% - {c.Offset.ToValue()})" : "100%";
                default: // a bare length-percentage
                    return c.Offset.ToValue();
            }
        }

        /// <summary>Splits tokens at the first standalone ident equal to <paramref name="keyword"/>
        /// (e.g. <c>at</c>). Fails if the keyword appears with nothing after it.</summary>
        private static bool SplitAtKeyword(IReadOnlyList<Token> tokens, string keyword,
            out IReadOnlyList<Token> before, out bool found, out IReadOnlyList<Token> after)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.Ident && tokens[i].Data.Isi(keyword))
                {
                    before = tokens.Take(i).ToArray();
                    after = tokens.Skip(i + 1).ToArray();
                    found = true;
                    return after.Count > 0;
                }
            }

            before = tokens.ToArray();
            after = new Token[0];
            found = false;
            return true;
        }

        private static List<List<Token>> SplitByComma(IReadOnlyList<Token> tokens)
        {
            var groups = new List<List<Token>>();
            var current = new List<Token>();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Comma)
                {
                    groups.Add(current);
                    current = new List<Token>();
                }
                else
                {
                    current.Add(token);
                }
            }

            groups.Add(current);

            // A trailing/leading/doubled comma yields an empty group, which is always malformed here.
            return groups.Any(g => g.Count == 0) && tokens.Count > 0 ? new List<List<Token>>() : groups;
        }

        private static (string top, string right, string bottom, string left) ExpandBox(IReadOnlyList<string> values)
        {
            switch (values.Count)
            {
                case 1: return (values[0], values[0], values[0], values[0]);
                case 2: return (values[0], values[1], values[0], values[1]);
                case 3: return (values[0], values[1], values[2], values[1]);
                default: return (values[0], values[1], values[2], values[3]);
            }
        }

        private static bool TryFillRule(Token token, out FillRule fillRule)
        {
            if (token.Type == TokenType.Ident)
            {
                if (token.Data.Isi(Keywords.Nonzero)) { fillRule = FillRule.Nonzero; return true; }
                if (token.Data.Isi(Keywords.Evenodd)) { fillRule = FillRule.Evenodd; return true; }
            }

            fillRule = FillRule.Nonzero;
            return false;
        }

        private static bool TryShapeRadius(Token token, out ShapeRadius radius)
        {
            if (token.Type == TokenType.Ident)
            {
                if (token.Data.Isi(Keywords.ClosestSide)) { radius = ShapeRadius.ClosestSide; return true; }
                if (token.Data.Isi(Keywords.FarthestSide)) { radius = ShapeRadius.FarthestSide; return true; }
                radius = default;
                return false;
            }

            if (IsLengthPercentage(token))
            {
                // A <shape-radius> is a non-negative <length-percentage> (CSS Shapes 1 §3.2); a negative
                // radius makes the whole clip-path value invalid.
                var unitToken = token as UnitToken;
                var numberToken = token as NumberToken;
                if ((unitToken != null && unitToken.Value < 0f) || (numberToken != null && numberToken.Value < 0f))
                {
                    radius = default(ShapeRadius);
                    return false;
                }

                radius = ShapeRadius.FromLength(token.ToValue());
                return true;
            }

            radius = default;
            return false;
        }

        private static bool IsLengthPercentage(Token token)
        {
            if (token.Type == TokenType.Dimension || token.Type == TokenType.Percentage) return true;
            // A calc()-family function computes to a <length-percentage> at used-value time. The render-time
            // resolver (CssClipPathResolver → CssValueParser.ParseLength) evaluates it, and the token's
            // ToValue() reconstructs the full "calc(…)" text, so accept it here rather than invalidating the
            // whole shape (this is what lets a Charts.css area/line polygon vertex be a calc() expression).
            var function = token as FunctionToken;
            if (function != null && CalcParser.IsCalcFamily(function.Data)) return true;
            // Unitless zero is a valid length.
            var number = token as NumberToken;
            return number != null && number.Value == 0f;
        }
    }
}
