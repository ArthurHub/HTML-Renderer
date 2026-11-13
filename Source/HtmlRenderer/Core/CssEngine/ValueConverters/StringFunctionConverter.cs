using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>string()</c> function used in <c>content</c> and <c>string-set</c> (GCPM §7.4):
    /// <c>string(name)</c> with an optional keyword <c>first</c>/<c>last</c>/<c>start</c>/
    /// <c>first-except</c>. Default keyword is <c>first</c>.
    /// </summary>
    internal sealed class StringFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidKeywords = { "first", "last", "start", "first-except" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            if (value.OnlyOrDefault() is not FunctionToken function ||
                !function.Data.Equals(FunctionNames.StringFn, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var args = function.ArgumentTokens
                .Where(t => t.Type != TokenType.Comma && t.Type != TokenType.Whitespace &&
                            t.Type != TokenType.RoundBracketClose)
                .ToArray();

            if (args.Length == 0 || args[0] is not KeywordToken nameToken) return null;

            var keyword = "first";

            if (args.Length > 1)
            {
                if (args[1] is not KeywordToken keywordToken) return null;

                var name = keywordToken.Data.ToLowerInvariant();
                if (!ValidKeywords.Contains(name)) return null;
                keyword = name;
            }

            if (args.Length > 2) return null;

            return new StringFunctionValue(nameToken.Data, keyword, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<StringFunctionValue>();
        }

        private sealed class StringFunctionValue : IPropertyValue
        {
            private readonly string _name;
            private readonly string _keyword;

            public StringFunctionValue(string name, string keyword, IEnumerable<Token> tokens)
            {
                _name = name;
                _keyword = keyword;
                Original = new TokenValue(tokens);
            }

            public string CssText => _keyword == "first"
                ? $"string({_name})"
                : $"string({_name}, {_keyword})";

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
