using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates <c>linear(&lt;linear-stop-list&gt;)</c> (CSS Easing 2 2.1), where
    /// <c>&lt;linear-stop-list&gt; = [ &lt;linear-stop&gt; ]#{2,}</c> and
    /// <c>&lt;linear-stop&gt; = &lt;number&gt; &amp;&amp; &lt;percentage&gt;{0,2}</c>. The number may come before or
    /// after its (adjacent) percentages. The authored text is preserved.
    /// </summary>
    internal sealed class LinearEasingValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var function = value.OnlyOrDefault() as FunctionToken;

            if (function == null || !function.Data.Equals(FunctionNames.Linear, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return IsValidStopList(function.ArgumentTokens) ? new LinearEasingValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<LinearEasingValue>();
        }

        private static bool IsValidStopList(IEnumerable<Token> arguments)
        {
            var stops = new List<List<Token>> { new List<Token>() };

            foreach (var token in arguments.Where(t => t.Type != TokenType.Whitespace))
            {
                if (token.Type == TokenType.Comma)
                {
                    stops.Add(new List<Token>());
                }
                else
                {
                    stops[stops.Count - 1].Add(token);
                }
            }

            return stops.Count >= 2 && stops.All(IsValidStop);
        }

        // <number> <percentage>{0,2}  or  <percentage>{1,2} <number>
        private static bool IsValidStop(List<Token> stop)
        {
            if (stop.Count < 1 || stop.Count > 3) return false;

            var numbers = stop.Count(IsFiniteNumber);
            var percents = stop.Count(IsFinitePercentage);

            if (numbers != 1 || numbers + percents != stop.Count) return false;

            // The percentages must be adjacent: the number is first or last, never between them.
            return IsFiniteNumber(stop[0]) || IsFiniteNumber(stop[stop.Count - 1]);
        }

        private static bool IsFiniteNumber(Token token) =>
            token is NumberToken number && IsFinite(number.Value);

        private static bool IsFinitePercentage(Token token) =>
            token.Type == TokenType.Percentage && token is UnitToken percent && IsFinite(percent.Value);

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private sealed class LinearEasingValue : IPropertyValue
        {
            public LinearEasingValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
