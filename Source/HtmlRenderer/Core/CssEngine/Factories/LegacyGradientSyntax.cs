using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    ///     Rewrites the pre-standard vendor gradient functions (<c>-webkit-linear-gradient()</c>,
    ///     <c>-moz-radial-gradient()</c>, and their <c>repeating-</c> forms; <c>-o-</c> and <c>-ms-</c> spellings too)
    ///     into the standard <c>linear-gradient()</c>/<c>radial-gradient()</c> token stream, so the value converters
    ///     only ever see standard syntax. Only applied when <see cref="ParserOptions.EnableVendorPrefixAliases" /> is on.
    /// </summary>
    /// <remarks>
    ///     Not a pure rename, the prefixed grammar differs from the standard one:
    ///     <list type="bullet">
    ///         <item>
    ///             Linear: an angle is measured counter-clockwise from the east axis (standard: clockwise from north),
    ///             so <c>standard = 90deg - legacy</c>; a side/corner keyword names where the line <em>starts</em>
    ///             (<c>top</c> = the standard <c>to bottom</c>), so each keyword is flipped to its opposite; with neither,
    ///             both default to top-to-bottom.
    ///         </item>
    ///         <item>
    ///             Radial: the center position comes <em>first</em> (standard: after <c>at</c>), the size keywords are
    ///             <c>cover</c>/<c>contain</c> (standard <c>farthest-corner</c>/<c>closest-side</c>).
    ///         </item>
    ///     </list>
    ///     A function whose first group already uses the standard <c>to &lt;side&gt;</c> form passes through unchanged.
    ///     <c>-webkit-gradient()</c> (a different function with a different grammar) is not handled.
    /// </remarks>
    internal static class LegacyGradientSyntax
    {
        private static readonly string[] Prefixes = {"-webkit-", "-moz-", "-o-", "-ms-"};

        private static readonly string[] StandardNames =
        {
            FunctionNames.LinearGradient, FunctionNames.RepeatingLinearGradient,
            FunctionNames.RadialGradient, FunctionNames.RepeatingRadialGradient
        };

        /// <summary>
        ///     Returns <paramref name="value" /> with every legacy gradient function (at any nesting depth) rewritten
        ///     to standard syntax, or <paramref name="value" /> itself when it contains none.
        /// </summary>
        internal static TokenValue Rewrite(TokenValue value)
        {
            if (value == null) return null;

            List<Token> tokens = null;
            for (var i = 0; i < value.Count; i++)
            {
                var token = value[i];
                var rewritten = Rewrite(token);

                if (ReferenceEquals(rewritten, token))
                {
                    tokens?.Add(token);
                    continue;
                }

                if (tokens == null)
                {
                    tokens = new List<Token>(value.Count);
                    for (var j = 0; j < i; j++) tokens.Add(value[j]);
                }

                tokens.Add(rewritten);
            }

            return tokens == null ? value : new TokenValue(tokens) {StylesheetText = value.StylesheetText};
        }

        /// <summary>
        ///     Returns <paramref name="token" /> rewritten to standard syntax if it is (or contains) a legacy gradient
        ///     function; otherwise the same instance.
        /// </summary>
        internal static Token Rewrite(Token token)
        {
            if (!(token is FunctionToken function)) return token;

            // Rewrite nested functions first (a gradient inside image-set(), cross-fade(), ...).
            var arguments = new List<Token>();
            var changed = false;
            foreach (var argument in function)
            {
                var rewrittenArgument = Rewrite(argument);
                changed |= !ReferenceEquals(rewrittenArgument, argument);
                arguments.Add(rewrittenArgument);
            }

            if (TryGetStandardName(function.Data, out var standardName, out var isLinear))
            {
                var groups = SplitGroups(Without(arguments));
                if (groups.Count > 0) groups = isLinear ? RewriteLinear(groups) : RewriteRadial(groups);

                var rewritten = new FunctionToken(standardName, function.Position);
                for (var i = 0; i < groups.Count; i++)
                {
                    if (i > 0)
                    {
                        rewritten.AddArgumentToken(Token.Comma);
                        rewritten.AddArgumentToken(Token.Whitespace);
                    }

                    foreach (var t in groups[i]) rewritten.AddArgumentToken(t);
                }

                rewritten.AddArgumentToken(new Token(TokenType.RoundBracketClose, ")", function.Position));
                return rewritten;
            }

            if (!changed) return token;

            var copy = new FunctionToken(function.Data, function.Position);
            foreach (var argument in arguments) copy.AddArgumentToken(argument);
            return copy;
        }

        // The arguments without the function's own closing parenthesis.
        private static List<Token> Without(List<Token> arguments)
        {
            var result = new List<Token>(arguments);
            if (result.Count > 0 && result[result.Count - 1].Type == TokenType.RoundBracketClose)
                result.RemoveAt(result.Count - 1);
            return result;
        }

        private static bool TryGetStandardName(string name, out string standardName, out bool isLinear)
        {
            standardName = string.Empty;
            isLinear = false;

            if (name.Length < 2 || name[0] != '-') return false;

            foreach (var prefix in Prefixes)
            {
                if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                var rest = name.Substring(prefix.Length);
                foreach (var candidate in StandardNames)
                {
                    if (!rest.Equals(candidate, StringComparison.OrdinalIgnoreCase)) continue;

                    standardName = candidate;
                    isLinear = candidate.IndexOf("linear", StringComparison.Ordinal) >= 0;
                    return true;
                }

                return false;
            }

            return false;
        }

        // Splits on top-level commas (nested functions are self-contained tokens, so a flat split is top-level),
        // trimming surrounding whitespace from each group.
        private static List<List<Token>> SplitGroups(IEnumerable<Token> tokens)
        {
            var groups = new List<List<Token>> {new List<Token>()};

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Comma) groups.Add(new List<Token>());
                else groups[groups.Count - 1].Add(token);
            }

            foreach (var group in groups)
            {
                while (group.Count > 0 && group[0].Type == TokenType.Whitespace) group.RemoveAt(0);
                while (group.Count > 0 && group[group.Count - 1].Type == TokenType.Whitespace)
                    group.RemoveAt(group.Count - 1);
            }

            return groups;
        }

        private static List<List<Token>> RewriteLinear(List<List<Token>> groups)
        {
            var first = groups[0];

            if (TryGetAngleDegrees(first, out var legacyDegrees))
            {
                var standard = (90 - legacyDegrees) % 360;
                if (standard < 0) standard += 360;

                groups[0] = new List<Token>
                {
                    new UnitToken(TokenType.Dimension,
                        standard.ToString("0.####", CultureInfo.InvariantCulture), "deg", first[0].Position)
                };
            }
            else if (TryFlipSideKeywords(first, out var flipped))
            {
                groups[0] = flipped;
            }

            // Otherwise no direction group was written (or it is already standard `to ...`): the legacy default,
            // `top`, is the standard default, `to bottom`, so the groups pass through untouched.
            return groups;
        }

        private static bool TryGetAngleDegrees(List<Token> group, out double degrees)
        {
            degrees = 0;
            if (group.Count != 1) return false;

            var token = group[0];
            if (token is NumberToken number) return number.Value == 0;
            if (!(token is UnitToken unit) || unit.Type != TokenType.Dimension) return false;

            double value = unit.Value;
            if (unit.Unit.Isi("deg")) degrees = value;
            else if (unit.Unit.Isi("grad")) degrees = value * 0.9;
            else if (unit.Unit.Isi("rad")) degrees = value * 180 / Math.PI;
            else if (unit.Unit.Isi("turn")) degrees = value * 360;
            else return false;

            return true;
        }

        // `top`/`left`/`bottom`/`right` (one, or a vertical+horizontal pair) -> the standard `to <opposite...>`.
        private static bool TryGetSideKeyword(Token token, out string opposite, out bool vertical)
        {
            opposite = string.Empty;
            vertical = false;
            if (token.Type != TokenType.Ident) return false;

            if (token.Data.Isi("top"))
            {
                opposite = "bottom";
                vertical = true;
            }
            else if (token.Data.Isi("bottom"))
            {
                opposite = "top";
                vertical = true;
            }
            else if (token.Data.Isi("left")) opposite = "right";
            else if (token.Data.Isi("right")) opposite = "left";
            else return false;

            return true;
        }

        private static bool TryFlipSideKeywords(List<Token> group, out List<Token> flipped)
        {
            flipped = new List<Token>();
            string vert = null, horiz = null;

            foreach (var token in group)
            {
                if (token.Type == TokenType.Whitespace) continue;
                if (!TryGetSideKeyword(token, out var opposite, out var vertical)) return false;

                if (vertical)
                {
                    if (vert != null) return false;
                    vert = opposite;
                }
                else
                {
                    if (horiz != null) return false;
                    horiz = opposite;
                }
            }

            if (vert == null && horiz == null) return false;

            var position = group[0].Position;
            flipped.Add(Ident("to", position));
            if (vert != null)
            {
                flipped.Add(Token.Whitespace);
                flipped.Add(Ident(vert, position));
            }

            if (horiz != null)
            {
                flipped.Add(Token.Whitespace);
                flipped.Add(Ident(horiz, position));
            }

            return true;
        }

        private static Token Ident(string text, TextPosition position)
        {
            return new KeywordToken(TokenType.Ident, text, position);
        }

        private static bool IsShapeOrSizeKeyword(string s)
        {
            return s.Isi("circle") || s.Isi("ellipse") || s.Isi("cover") || s.Isi("contain") ||
                   s.Isi("closest-side") || s.Isi("closest-corner") || s.Isi("farthest-side") ||
                   s.Isi("farthest-corner");
        }

        private static bool IsPositionKeyword(string s)
        {
            return s.Isi("left") || s.Isi("right") || s.Isi("top") || s.Isi("bottom") || s.Isi("center");
        }

        // A header group has no color, so it can only hold position/shape/size keywords and numeric lengths -
        // which is what tells it apart from a color stop (every stop carries a color).
        private static bool IsHeaderGroup(List<Token> group)
        {
            if (group.Count == 0) return false;

            foreach (var token in group)
            {
                switch (token.Type)
                {
                    case TokenType.Whitespace:
                    case TokenType.Dimension:
                    case TokenType.Percentage:
                    case TokenType.Number:
                        break;
                    case TokenType.Ident when IsShapeOrSizeKeyword(token.Data) || IsPositionKeyword(token.Data):
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool HasShapeOrSize(List<Token> group)
        {
            foreach (var token in group)
            {
                if (token.Type == TokenType.Ident && IsShapeOrSizeKeyword(token.Data)) return true;
            }

            return false;
        }

        private static List<List<Token>> RewriteRadial(List<List<Token>> groups)
        {
            // Legacy header: [<position>,]? [<shape> || <size>,]? then the color stops.
            var headerCount = 0;
            while (headerCount < 2 && headerCount < groups.Count && IsHeaderGroup(groups[headerCount])) headerCount++;

            if (headerCount == 0) return groups;

            List<Token> position = null, shapeSize = null;
            if (headerCount == 2)
            {
                position = groups[0];
                shapeSize = groups[1];
            }
            else if (HasShapeOrSize(groups[0])) shapeSize = groups[0];
            else position = groups[0];

            var header = new List<Token>();
            if (shapeSize != null)
            {
                foreach (var token in shapeSize)
                {
                    if (token.Type == TokenType.Ident && token.Data.Isi("cover"))
                        header.Add(Ident("farthest-corner", token.Position));
                    else if (token.Type == TokenType.Ident && token.Data.Isi("contain"))
                        header.Add(Ident("closest-side", token.Position));
                    else header.Add(token);
                }
            }

            if (position != null)
            {
                if (header.Count > 0) header.Add(Token.Whitespace);
                header.Add(Ident("at", position[0].Position));
                header.Add(Token.Whitespace);
                header.AddRange(position);
            }

            var result = new List<List<Token>> {header};
            for (var i = headerCount; i < groups.Count; i++) result.Add(groups[i]);
            return result;
        }
    }
}
