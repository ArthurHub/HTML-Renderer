using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>clip-path</c> value (CSS Masking 1 §5.1): the keyword <c>none</c> or a
    /// <c>&lt;basic-shape&gt;</c> - <c>polygon()</c>, <c>inset()</c>, <c>circle()</c> or <c>ellipse()</c>
    /// (CSS Shapes 1 §3, plus <c>rect()</c>/<c>xywh()</c> from CSS Shapes 2 and <c>path()</c>),
    /// optionally combined with a <c>&lt;geometry-box&gt;</c> in either order, or a lone
    /// <c>&lt;geometry-box&gt;</c>. The authored text is preserved. The <c>&lt;url&gt;</c> form is out of scope here.
    /// </summary>
    internal sealed class ClipPathValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.None))
                return new ClipPathValue(value);

            return IsShapeAndBox(tokens) ? new ClipPathValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<ClipPathValue>();
        }

        private static readonly string[] GeometryBoxes =
        {
            Keywords.BorderBox, Keywords.PaddingBox, Keywords.ContentBox,
            "margin-box", "fill-box", "stroke-box", "view-box"
        };

        // <basic-shape> || <geometry-box>  (each at most once, either order)
        private static bool IsShapeAndBox(IReadOnlyList<Token> tokens)
        {
            if (tokens.Count is < 1 or > 2) return false;

            var shapes = 0;
            var boxes = 0;

            foreach (var token in tokens)
            {
                if (IsGeometryBox(token)) boxes++;
                else if (IsBasicShape(token)) shapes++;
                else return false;
            }

            return shapes <= 1 && boxes <= 1;
        }

        private static bool IsGeometryBox(Token token) =>
            token.Type == TokenType.Ident && GeometryBoxes.Any(b => token.Data.Isi(b));

        // Used by shape-outside as well: a single-token <basic-shape>.
        internal static bool IsBasicShape(IReadOnlyList<Token> tokens) =>
            tokens.Count == 1 && IsBasicShape(tokens[0]);

        internal static bool IsBasicShape(Token token)
        {
            if (token is not FunctionToken function) return false;

            var args = function.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (function.Data.Isi(FunctionNames.Polygon)) return IsPolygon(args);
            if (function.Data.Isi(FunctionNames.Inset)) return IsInset(args);
            if (function.Data.Isi(FunctionNames.Circle)) return IsCircle(args);
            if (function.Data.Isi(FunctionNames.Ellipse)) return IsEllipse(args);
            if (function.Data.Isi(FunctionNames.Rect)) return IsRect(args);
            if (function.Data.Isi("xywh")) return IsXywh(args);
            if (function.Data.Isi("path")) return IsPath(args);

            return false;
        }

        // polygon( <fill-rule>? , [ <length-percentage> <length-percentage> ]# )
        private static bool IsPolygon(IReadOnlyList<Token> args)
        {
            var groups = SplitByComma(args);
            if (groups == null || groups.Count == 0) return false;

            var first = 0;

            if (groups[0].Count == 1 && IsFillRule(groups[0][0]))
                first = 1;

            if (first >= groups.Count) return false;

            for (var i = first; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group.Count != 2 || !IsLengthPercentage(group[0]) || !IsLengthPercentage(group[1]))
                    return false;
            }

            return true;
        }

        // inset( <length-percentage>{1,4} [ round <border-radius> ]? )
        private static bool IsInset(IReadOnlyList<Token> args) => IsBoxShape(args, false, false, false);

        // rect( [ <length-percentage> | auto ]{4} [ round <border-radius> ]? )
        private static bool IsRect(IReadOnlyList<Token> args) => IsBoxShape(args, true, true, false);

        // xywh( <length-percentage>{4} [ round <border-radius> ]? ); width and height must be non-negative
        private static bool IsXywh(IReadOnlyList<Token> args) => IsBoxShape(args, true, false, true);

        private static bool IsBoxShape(IReadOnlyList<Token> args, bool exactlyFour, bool allowAuto,
            bool nonNegativeSize)
        {
            if (args.Count == 0) return false;

            var roundIndex = -1;
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i].Type == TokenType.Ident && args[i].Data.Isi(Keywords.Round))
                {
                    roundIndex = i;
                    break;
                }
            }

            var lengths = roundIndex >= 0 ? args.Take(roundIndex).ToArray() : args.ToArray();

            if (exactlyFour ? lengths.Length != 4 : lengths.Length is < 1 or > 4) return false;
            if (lengths.Any(t => !(IsLengthPercentage(t) || allowAuto && IsAuto(t)))) return false;
            if (nonNegativeSize && (IsNegative(lengths[2]) || IsNegative(lengths[3]))) return false;

            // "round" must be followed by a valid <border-radius>.
            return roundIndex < 0 || IsBorderRadius(args.Skip(roundIndex + 1).ToArray());
        }

        // <length-percentage [0,inf]>{1,4} [ / <length-percentage [0,inf]>{1,4} ]?
        private static bool IsBorderRadius(IReadOnlyList<Token> tokens)
        {
            var slash = -1;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Delim || tokens[i].Data != "/") continue;
                if (slash >= 0) return false;
                slash = i;
            }

            var horizontal = slash >= 0 ? tokens.Take(slash).ToArray() : tokens.ToArray();
            var vertical = slash >= 0 ? tokens.Skip(slash + 1).ToArray() : new Token[0];

            return IsRadiusList(horizontal) && (slash < 0 || IsRadiusList(vertical));
        }

        private static bool IsRadiusList(IReadOnlyList<Token> tokens) =>
            tokens.Count is >= 1 and <= 4 && tokens.All(t => IsLengthPercentage(t) && !IsNegative(t));

        private static bool IsAuto(Token token) => token.Type == TokenType.Ident && token.Data.Isi("auto");

        private static bool IsNegative(Token token) => token is UnitToken { Value: < 0f } or NumberToken { Value: < 0f };

        // path( <fill-rule>? , <string> ) where the string is well-formed, non-empty SVG path data.
        private static bool IsPath(IReadOnlyList<Token> args)
        {
            var groups = SplitByComma(args);
            if (groups == null || groups.Count is not (1 or 2)) return false;

            if (groups.Count == 2 && !(groups[0].Count == 1 && IsFillRule(groups[0][0]))) return false;

            var str = groups[groups.Count - 1];
            return str.Count == 1 && str[0].Type == TokenType.String && IsPathData(str[0].Data);
        }

        // Validates the SVG path-data mini-language (SVG 1.1 8.3.9): starts with a moveto, then commands with
        // the right number of numeric arguments (implicitly repeatable); arc flags are single '0'/'1' digits.
        internal static bool IsPathData(string data)
        {
            var pos = 0;
            var first = true;
            var command = '\0';

            SkipSeparators(data, ref pos);
            if (pos >= data.Length) return false;

            while (pos < data.Length)
            {
                var c = data[pos];

                if (char.IsLetter(c))
                {
                    if ("MmLlHhVvCcSsQqTtAaZz".IndexOf(c) < 0) return false;
                    if (first && c != 'M' && c != 'm') return false;

                    command = c;
                    first = false;
                    pos++;
                    SkipSeparators(data, ref pos);

                    if (command is 'Z' or 'z') continue;
                }
                else if (command == '\0' || command is 'Z' or 'z')
                {
                    return false;
                }

                var count = ArgumentCount(command);

                for (var i = 0; i < count; i++)
                {
                    var isFlag = (command is 'A' or 'a') && i is 3 or 4;

                    if (isFlag ? !ReadFlag(data, ref pos) : !ReadNumber(data, ref pos)) return false;

                    SkipSeparators(data, ref pos);
                }

                // A moveto's implicit repeats are linetos.
                if (command == 'M') command = 'L';
                else if (command == 'm') command = 'l';
            }

            return true;
        }

        private static int ArgumentCount(char command)
        {
            switch (char.ToUpperInvariant(command))
            {
                case 'M': case 'L': case 'T': return 2;
                case 'H': case 'V': return 1;
                case 'C': return 6;
                case 'S': case 'Q': return 4;
                case 'A': return 7;
                default: return 0;
            }
        }

        private static void SkipSeparators(string s, ref int pos)
        {
            while (pos < s.Length && (char.IsWhiteSpace(s[pos]) || s[pos] == ',')) pos++;
        }

        private static bool ReadFlag(string s, ref int pos)
        {
            if (pos >= s.Length || s[pos] != '0' && s[pos] != '1') return false;
            pos++;
            return true;
        }

        // [+-]? (digits [. digits?]? | . digits) ([eE] [+-]? digits)?
        private static bool ReadNumber(string s, ref int pos)
        {
            var i = pos;
            if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;

            var digits = 0;
            while (i < s.Length && char.IsDigit(s[i])) { i++; digits++; }

            if (i < s.Length && s[i] == '.')
            {
                i++;
                while (i < s.Length && char.IsDigit(s[i])) { i++; digits++; }
            }

            if (digits == 0) return false;

            if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
            {
                var j = i + 1;
                if (j < s.Length && (s[j] == '+' || s[j] == '-')) j++;
                var expDigits = 0;
                while (j < s.Length && char.IsDigit(s[j])) { j++; expDigits++; }
                if (expDigits > 0) i = j;
            }

            pos = i;
            return true;
        }

        // circle( <shape-radius>? [ at <position> ]? )
        private static bool IsCircle(IReadOnlyList<Token> args)
        {
            if (!SplitAt(args, out var radius, out var position)) return false;
            return radius.Count switch
            {
                0 => true,
                1 => IsShapeRadius(radius[0]),
                _ => false
            } && IsPosition(position);
        }

        // ellipse( [ <shape-radius>{2} ]? [ at <position> ]? )
        private static bool IsEllipse(IReadOnlyList<Token> args)
        {
            if (!SplitAt(args, out var radius, out var position)) return false;
            return radius.Count switch
            {
                0 => true,
                2 => IsShapeRadius(radius[0]) && IsShapeRadius(radius[1]),
                _ => false
            } && IsPosition(position);
        }

        // Splits the arguments at a standalone "at" ident. Returns false when "at" has nothing after it.
        private static bool SplitAt(IReadOnlyList<Token> args, out IReadOnlyList<Token> before,
            out IReadOnlyList<Token> after)
        {
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i].Type == TokenType.Ident && args[i].Data.Isi(Keywords.At))
                {
                    before = args.Take(i).ToArray();
                    after = args.Skip(i + 1).ToArray();
                    return after.Count > 0;
                }
            }

            before = args;
            after = new Token[0];
            return true;
        }

        // A permissive <position>: nothing (absent), or 1-4 tokens each an edge/center keyword or a
        // length-percentage. Absent is valid (the caller only reaches here with the post-"at" tokens).
        private static bool IsPosition(IReadOnlyList<Token> tokens)
        {
            if (tokens.Count == 0) return true;
            if (tokens.Count > 4) return false;

            return tokens.All(t => IsPositionKeyword(t) || IsLengthPercentage(t));
        }

        private static bool IsPositionKeyword(Token token)
        {
            if (token.Type != TokenType.Ident) return false;
            return token.Data.Isi(Keywords.Left) || token.Data.Isi(Keywords.Right) ||
                   token.Data.Isi(Keywords.Top) || token.Data.Isi(Keywords.Bottom) ||
                   token.Data.Isi(Keywords.Center);
        }

        private static bool IsShapeRadius(Token token)
        {
            if (token.Type == TokenType.Ident)
                return token.Data.Isi(Keywords.ClosestSide) || token.Data.Isi(Keywords.FarthestSide);

            // A <shape-radius> is a non-negative <length-percentage> (CSS Shapes 1 §3.2).
            if (token is UnitToken { Value: < 0f } or NumberToken { Value: < 0f }) return false;

            return IsLengthPercentage(token);
        }

        private static bool IsFillRule(Token token) =>
            token.Type == TokenType.Ident && (token.Data.Isi(Keywords.Nonzero) || token.Data.Isi(Keywords.Evenodd));

        // CSS Values 4 §10, a math function resolving to a <length-percentage> is one wherever that type is
        private static bool IsLengthPercentage(Token token) =>
            token.Type is TokenType.Dimension or TokenType.Percentage || token is NumberToken { Value: 0f } ||
            token is FunctionToken && Converters.LengthOrPercentConverter.Convert(new[] { token }) != null;

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

            // A leading, trailing or doubled comma leaves an empty group - always malformed here.
            return tokens.Count > 0 && groups.Any(g => g.Count == 0) ? null : groups;
        }

        private sealed class ClipPathValue : IPropertyValue
        {
            public ClipPathValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
