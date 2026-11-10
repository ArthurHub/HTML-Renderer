using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Parses calc(), min(), max() and clamp() (CSS Values 4 §10.8), both when a declaration is
    /// converted and again at layout time from the folded text. Returns null on a syntax error.
    /// </summary>
    internal static class CalcParser
    {
        public static CalcNode Parse(FunctionToken function)
        {
            var name = function.Data;

            if (name.Isi(FunctionNames.Calc))
            {
                var tokens = new List<Token>(function.ArgumentTokens);
                var pos = 0;
                var node = ParseSum(tokens, ref pos);
                if (node is null) return null;

                SkipWhitespace(tokens, ref pos);
                return pos == tokens.Count ? node : null;
            }

            if (name.Isi(FunctionNames.Min) || name.Isi(FunctionNames.Max))
            {
                var args = ParseCommaSeparatedSums(function, out var groupCount);
                return groupCount >= 1 && args != null
                    ? new CallCalcNode(name.Isi(FunctionNames.Min) ? FunctionNames.Min : FunctionNames.Max, args)
                    : null;
            }

            if (name.Isi(FunctionNames.Clamp))
            {
                var args = ParseCommaSeparatedSums(function, out var groupCount);
                return groupCount == 3 && args != null ? new CallCalcNode(FunctionNames.Clamp, args) : null;
            }

            return null;
        }

        private static List<CalcNode> ParseCommaSeparatedSums(FunctionToken function, out int groupCount)
        {
            var groups = function.ArgumentTokens.ToList();
            groupCount = groups.Count;

            var args = new List<CalcNode>(groups.Count);
            foreach (var group in groups)
            {
                if (group.Count == 0) return null;

                var pos = 0;
                var node = ParseSum(group, ref pos);
                if (node is null) return null;

                SkipWhitespace(group, ref pos);
                if (pos != group.Count) return null;

                args.Add(node);
            }

            return args;
        }

        private static CalcNode ParseSum(List<Token> tokens, ref int pos)
        {
            var left = ParseProduct(tokens, ref pos);
            if (left is null) return null;

            while (true)
            {
                var beforeWhitespace = pos;
                SkipWhitespace(tokens, ref pos);
                var sawWhitespaceBefore = pos != beforeWhitespace;

                if (pos >= tokens.Count || tokens[pos].Type != TokenType.Delim ||
                    (tokens[pos].Data != "+" && tokens[pos].Data != "-"))
                {
                    pos = beforeWhitespace;
                    break;
                }

                // A sign without whitespace before it was already merged into the number token
                if (!sawWhitespaceBefore) return null;

                var op = tokens[pos].Data[0];
                pos++;

                var afterOp = pos;
                SkipWhitespace(tokens, ref pos);
                if (pos == afterOp) return null;

                var right = ParseProduct(tokens, ref pos);
                if (right is null) return null;

                left = new BinaryCalcNode(op, left, right);
            }

            return left;
        }

        private static CalcNode ParseProduct(List<Token> tokens, ref int pos)
        {
            var left = ParseValue(tokens, ref pos);
            if (left is null) return null;

            while (true)
            {
                var save = pos;
                SkipWhitespace(tokens, ref pos);

                if (pos >= tokens.Count || tokens[pos].Type != TokenType.Delim ||
                    (tokens[pos].Data != "*" && tokens[pos].Data != "/"))
                {
                    pos = save;
                    break;
                }

                var op = tokens[pos].Data[0];
                pos++;
                SkipWhitespace(tokens, ref pos);

                var right = ParseValue(tokens, ref pos);
                if (right is null) return null;

                left = new BinaryCalcNode(op, left, right);
            }

            return left;
        }

        private static CalcNode ParseValue(List<Token> tokens, ref int pos)
        {
            SkipWhitespace(tokens, ref pos);
            if (pos >= tokens.Count) return null;

            var token = tokens[pos];

            switch (token.Type)
            {
                case TokenType.Number:
                    pos++;
                    return new NumberCalcNode(((NumberToken)token).Value);

                case TokenType.Percentage:
                    pos++;
                    return new PercentageCalcNode(((UnitToken)token).Value);

                case TokenType.Dimension:
                {
                    var unitToken = (UnitToken)token;
                    var lengthUnit = Length.GetUnit(unitToken.Unit);
                    if (IsSupportedLengthUnit(lengthUnit))
                    {
                        pos++;
                        return new DimensionCalcNode(unitToken.Value, lengthUnit);
                    }

                    var angleUnit = Angle.GetUnit(unitToken.Unit);
                    if (angleUnit != Angle.Unit.None)
                    {
                        pos++;
                        return new AngleCalcNode(unitToken.Value, angleUnit);
                    }

                    var timeUnit = Time.GetUnit(unitToken.Unit);
                    if (timeUnit != Time.Unit.None)
                    {
                        pos++;
                        return new TimeCalcNode(unitToken.Value, timeUnit);
                    }

                    var resolutionUnit = Resolution.GetUnit(unitToken.Unit);
                    if (resolutionUnit != Resolution.Unit.None)
                    {
                        pos++;
                        return new ResolutionCalcNode(unitToken.Value, resolutionUnit);
                    }

                    return null;
                }

                case TokenType.RoundBracketOpen:
                {
                    pos++;
                    var inner = ParseSum(tokens, ref pos);
                    if (inner is null) return null;

                    SkipWhitespace(tokens, ref pos);
                    if (pos >= tokens.Count || tokens[pos].Type != TokenType.RoundBracketClose) return null;
                    pos++;
                    return inner;
                }

                case TokenType.Delim when token.Data == "+" || token.Data == "-":
                {
                    var negative = token.Data == "-";
                    pos++;

                    // Only reachable before ( or a function, a signed number is a single token
                    if (pos >= tokens.Count ||
                        (tokens[pos].Type != TokenType.RoundBracketOpen && tokens[pos].Type != TokenType.Function))
                    {
                        return null;
                    }

                    var operand = ParseValue(tokens, ref pos);
                    return operand is null ? null : new UnaryCalcNode(negative, operand);
                }

                case TokenType.Function:
                {
                    var nested = (FunctionToken)token;
                    if (!IsCalcFamily(nested.Data)) return null;
                    pos++;
                    return Parse(nested);
                }

                default:
                    return null;
            }
        }

        private static bool IsSupportedLengthUnit(Length.Unit unit)
        {
            return unit == Length.Unit.Em || unit == Length.Unit.Rem || unit == Length.Unit.Ex || unit == Length.Unit.Px ||
                unit == Length.Unit.Mm || unit == Length.Unit.Cm || unit == Length.Unit.In || unit == Length.Unit.Pt || unit == Length.Unit.Pc;
        }

        /// <summary>Whether <paramref name="name"/> is calc, min, max or clamp, ignoring case.</summary>
        public static bool IsCalcFamily(string name)
        {
            return name.Isi(FunctionNames.Calc) || name.Isi(FunctionNames.Min) ||
                   name.Isi(FunctionNames.Max) || name.Isi(FunctionNames.Clamp);
        }

        private static void SkipWhitespace(List<Token> tokens, ref int pos)
        {
            while (pos < tokens.Count && tokens[pos].Type == TokenType.Whitespace) pos++;
        }
    }
}
