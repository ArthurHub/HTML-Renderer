using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Applies the argument ranges CSS Easing puts on <c>steps()</c> and <c>cubic-bezier()</c>, which the plain
    /// number converters cannot express: the step count is at least 1 (at least 2 with <c>jump-none</c>), and the
    /// x control values of a bezier lie in 0 to 1. A value that breaks one is invalid.
    /// </summary>
    internal sealed class EasingArgumentRangeValueConverter : IValueConverter
    {
        private readonly IValueConverter _inner;

        public EasingArgumentRangeValueConverter(IValueConverter inner)
        {
            _inner = inner;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var result = _inner.Convert(value);

            if (result == null) return null;

            if (!(value.OnlyOrDefault() is FunctionToken function)) return result;

            var args = SplitArguments(function.ArgumentTokens);

            if (function.Data.Isi(FunctionNames.Steps))
            {
                if (args.Count == 0 || !(args[0].Count == 1 && args[0][0] is NumberToken count) || count.IntegerValue < 1)
                {
                    return null;
                }

                if (args.Count > 1 && args[1].Count == 1 && args[1][0].Data.Isi(Keywords.JumpNone) && count.IntegerValue < 2)
                {
                    return null;
                }
            }
            else if (function.Data.Isi(FunctionNames.CubicBezier))
            {
                if (args.Count != 4 || !IsUnitRange(args[0]) || !IsUnitRange(args[2])) return null;
            }

            return result;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _inner.Construct(properties);
        }

        private static bool IsUnitRange(List<Token> argument) =>
            argument.Count == 1 && argument[0] is NumberToken number && number.Value >= 0f && number.Value <= 1f;

        private static List<List<Token>> SplitArguments(IEnumerable<Token> tokens)
        {
            var arguments = new List<List<Token>> { new List<Token>() };

            foreach (var token in tokens.Where(t => t.Type != TokenType.Whitespace))
            {
                if (token.Type == TokenType.Comma)
                {
                    arguments.Add(new List<Token>());
                }
                else
                {
                    arguments[arguments.Count - 1].Add(token);
                }
            }

            return arguments;
        }
    }
}
