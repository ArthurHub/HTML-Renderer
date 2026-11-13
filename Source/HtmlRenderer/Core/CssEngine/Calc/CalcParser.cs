using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Recursive-descent parser for calc()/min()/max()/clamp() expressions (CSS Values 4 §10). Grammar:
    /// <c>&lt;calc-sum&gt; := &lt;calc-product&gt; (('+'|'-') &lt;calc-product&gt;)*</c>,
    /// <c>&lt;calc-product&gt; := &lt;calc-value&gt; (('*'|'/') &lt;calc-value&gt;)*</c>,
    /// <c>&lt;calc-value&gt; := &lt;number&gt; | &lt;dimension&gt; | &lt;percentage&gt; | '(' &lt;calc-sum&gt; ')' | &lt;nested-call&gt; | &lt;unary-sign&gt; &lt;calc-value&gt;</c>.
    /// Binary <c>+</c>/<c>-</c> require whitespace on both sides; <c>*</c>/<c>/</c> do not. Returns null on
    /// any grammar violation. An optional set of <c>variables</c> binds identifiers to numbers (the channel keywords
    /// of a relative color, CSS Color 5 section 4): a bound identifier is a <c>&lt;calc-value&gt;</c> that stands for its number.
    /// </summary>
    internal static class CalcParser
    {
        public static CalcNode Parse(FunctionToken function, IReadOnlyDictionary<string, double> variables = null)
        {
            var name = function.Data;

            if (name.Isi(FunctionNames.Calc))
            {
                var tokens = new List<Token>(function.ArgumentTokens);
                var pos = 0;
                var node = ParseSum(tokens, ref pos, variables);
                if (node == null) return null;

                SkipWhitespace(tokens, ref pos);
                return pos == tokens.Count ? node : null;
            }

            if (name.Isi(FunctionNames.Min) || name.Isi(FunctionNames.Max))
            {
                var args = ParseCommaSeparatedSums(function, variables, out var groupCount);
                return groupCount >= 1 && args != null
                    ? new CallCalcNode(name.Isi(FunctionNames.Min) ? FunctionNames.Min : FunctionNames.Max, args)
                    : null;
            }

            if (name.Isi(FunctionNames.Clamp))
            {
                var args = ParseCommaSeparatedSums(function, variables, out var groupCount);
                return groupCount == 3 && args != null ? new CallCalcNode(FunctionNames.Clamp, args) : null;
            }

            return null;
        }

        public static bool IsCalcFamily(string name)
        {
            return name.Isi(FunctionNames.Calc) || name.Isi(FunctionNames.Min) ||
                   name.Isi(FunctionNames.Max) || name.Isi(FunctionNames.Clamp);
        }

        private static List<CalcNode> ParseCommaSeparatedSums(FunctionToken function, IReadOnlyDictionary<string, double> variables, out int groupCount)
        {
            var groups = function.ArgumentTokens.ToList();
            groupCount = groups.Count;

            var args = new List<CalcNode>(groups.Count);

            foreach (var group in groups)
            {
                if (group.Count == 0) return null;

                var pos = 0;
                var node = ParseSum(group, ref pos, variables);
                if (node == null) return null;

                SkipWhitespace(group, ref pos);
                if (pos != group.Count) return null;

                args.Add(node);
            }

            return args;
        }

        private static CalcNode ParseSum(List<Token> tokens, ref int pos, IReadOnlyDictionary<string, double> variables)
        {
            var left = ParseProduct(tokens, ref pos, variables);
            if (left == null) return null;

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

                // Binary +/- must have whitespace on both sides; without it this can't be a valid binary
                // operator here (an attached sign is already merged into the adjacent numeric token).
                if (!sawWhitespaceBefore) return null;

                var op = tokens[pos].Data[0];
                pos++;

                var afterOp = pos;
                SkipWhitespace(tokens, ref pos);
                if (pos == afterOp) return null;

                var right = ParseProduct(tokens, ref pos, variables);
                if (right == null) return null;

                left = new BinaryCalcNode(op, left, right);
            }

            return left;
        }

        private static CalcNode ParseProduct(List<Token> tokens, ref int pos, IReadOnlyDictionary<string, double> variables)
        {
            var left = ParseValue(tokens, ref pos, variables);
            if (left == null) return null;

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

                var right = ParseValue(tokens, ref pos, variables);
                if (right == null) return null;

                left = new BinaryCalcNode(op, left, right);
            }

            return left;
        }

        private static CalcNode ParseValue(List<Token> tokens, ref int pos, IReadOnlyDictionary<string, double> variables)
        {
            SkipWhitespace(tokens, ref pos);
            if (pos >= tokens.Count) return null;

            var token = tokens[pos];

            switch (token.Type)
            {
                case TokenType.Number:
                    pos++;
                    return new NumberCalcNode(((NumberToken) token).Value);

                case TokenType.Percentage:
                    pos++;
                    return new PercentageCalcNode(((UnitToken) token).Value);

                // An identifier bound by the caller stands for its number.
                case TokenType.Ident when variables != null && variables.TryGetValue(token.Data.ToLowerInvariant(), out var bound):
                    pos++;
                    return new NumberCalcNode(bound);

                case TokenType.Dimension:
                    return ParseDimension((UnitToken) token, tokens, ref pos);

                case TokenType.RoundBracketOpen:
                {
                    pos++;
                    var inner = ParseSum(tokens, ref pos, variables);
                    if (inner == null) return null;

                    SkipWhitespace(tokens, ref pos);
                    if (pos >= tokens.Count || tokens[pos].Type != TokenType.RoundBracketClose) return null;
                    pos++;
                    return inner;
                }

                case TokenType.Delim when token.Data == "+" || token.Data == "-":
                {
                    var negative = token.Data == "-";
                    pos++;

                    // A unary sign must be immediately followed by a parenthesized group or a nested
                    // function call (a signed number/dimension is already a single token from the lexer).
                    if (pos >= tokens.Count ||
                        (tokens[pos].Type != TokenType.RoundBracketOpen && tokens[pos].Type != TokenType.Function))
                    {
                        return null;
                    }

                    var operand = ParseValue(tokens, ref pos, variables);
                    return operand == null ? null : new UnaryCalcNode(negative, operand);
                }

                case TokenType.Function:
                {
                    var nested = (FunctionToken) token;
                    if (!IsCalcFamily(nested.Data)) return null;
                    pos++;
                    return Parse(nested, variables);
                }

                default:
                    return null;
            }
        }

        private static CalcNode ParseDimension(UnitToken token, List<Token> tokens, ref int pos)
        {
            var lengthUnit = Length.GetUnit(token.Unit);
            if (IsSupportedLengthUnit(lengthUnit))
            {
                pos++;
                return new DimensionCalcNode(token.Value, lengthUnit);
            }

            var angleUnit = Angle.GetUnit(token.Unit);
            if (angleUnit != Angle.Unit.None)
            {
                pos++;
                return new AngleCalcNode(token.Value, angleUnit);
            }

            var timeUnit = Time.GetUnit(token.Unit);
            if (timeUnit != Time.Unit.None)
            {
                pos++;
                return new TimeCalcNode(token.Value, timeUnit);
            }

            var resolutionUnit = Resolution.GetUnit(token.Unit);
            if (resolutionUnit != Resolution.Unit.None)
            {
                pos++;
                return new ResolutionCalcNode(token.Value, resolutionUnit);
            }

            return null;
        }

        private static bool IsSupportedLengthUnit(Length.Unit unit)
        {
            switch (unit)
            {
                case Length.Unit.Em:
                case Length.Unit.Rem:
                case Length.Unit.Ex:
                case Length.Unit.Ch:
                case Length.Unit.Vw:
                case Length.Unit.Vh:
                case Length.Unit.Vmin:
                case Length.Unit.Vmax:
                case Length.Unit.Px:
                case Length.Unit.Mm:
                case Length.Unit.Cm:
                case Length.Unit.In:
                case Length.Unit.Pt:
                case Length.Unit.Pc:
                case Length.Unit.Q:
                case Length.Unit.Cap:
                case Length.Unit.Ic:
                case Length.Unit.Lh:
                case Length.Unit.Rlh:
                case Length.Unit.Rex:
                case Length.Unit.Rch:
                case Length.Unit.Rcap:
                case Length.Unit.Ric:
                case Length.Unit.Vi:
                case Length.Unit.Vb:
                case Length.Unit.Svw:
                case Length.Unit.Svh:
                case Length.Unit.Svi:
                case Length.Unit.Svb:
                case Length.Unit.Svmin:
                case Length.Unit.Svmax:
                case Length.Unit.Lvw:
                case Length.Unit.Lvh:
                case Length.Unit.Lvi:
                case Length.Unit.Lvb:
                case Length.Unit.Lvmin:
                case Length.Unit.Lvmax:
                case Length.Unit.Dvw:
                case Length.Unit.Dvh:
                case Length.Unit.Dvi:
                case Length.Unit.Dvb:
                case Length.Unit.Dvmin:
                case Length.Unit.Dvmax:
                case Length.Unit.Cqw:
                case Length.Unit.Cqh:
                case Length.Unit.Cqi:
                case Length.Unit.Cqb:
                case Length.Unit.Cqmin:
                case Length.Unit.Cqmax:
                    return true;
                default:
                    return false;
            }
        }

        private static void SkipWhitespace(List<Token> tokens, ref int pos)
        {
            while (pos < tokens.Count && tokens[pos].Type == TokenType.Whitespace) pos++;
        }
    }
}
