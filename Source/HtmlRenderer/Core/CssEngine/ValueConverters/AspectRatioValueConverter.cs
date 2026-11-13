using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates the <c>aspect-ratio</c> value <c>[ auto || &lt;ratio&gt; ]</c> (CSS Sizing 4 4.1), where
    /// <c>&lt;ratio&gt; = &lt;number [0,∞]&gt; [ / &lt;number [0,∞]&gt; ]?</c>. The <c>||</c> permits
    /// <c>auto</c> and the ratio in either order, and either alone. The authored text is preserved.
    /// </summary>
    internal sealed class AspectRatioValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length == 0) return null;

            var index = 0;
            var hasAuto = false;

            if (IsAuto(tokens[index]))
            {
                hasAuto = true;
                index++;
            }

            if (index == tokens.Length) return hasAuto ? new AspectRatioValue(value) : null;

            if (!TryConsumeRatio(tokens, ref index)) return null;

            // "||" allows a trailing auto too (e.g. "16 / 9 auto"), but only if one wasn't already given.
            if (index < tokens.Length && !hasAuto && IsAuto(tokens[index])) index++;

            return index == tokens.Length ? new AspectRatioValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<AspectRatioValue>();
        }

        private static bool TryConsumeRatio(IReadOnlyList<Token> tokens, ref int index)
        {
            if (index >= tokens.Count || !IsNonNegativeNumber(tokens[index])) return false;
            index++;

            if (index < tokens.Count && IsSlash(tokens[index]))
            {
                index++;
                if (index >= tokens.Count || !IsNonNegativeNumber(tokens[index])) return false;
                index++;
            }

            return true;
        }

        private static bool IsAuto(Token token) =>
            token.Type == TokenType.Ident && token.Data.Isi(Keywords.Auto);

        private static bool IsSlash(Token token) =>
            token.Type == TokenType.Delim && token.Data == "/";

        private static bool IsNonNegativeNumber(Token token) =>
            token is NumberToken number && number.Value >= 0f;

        private sealed class AspectRatioValue : IPropertyValue
        {
            public AspectRatioValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
