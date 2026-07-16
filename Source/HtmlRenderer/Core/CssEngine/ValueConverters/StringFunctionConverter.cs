using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Converter for the string() function used in content and string-set properties.
    /// Supports: string(name), string(name, first), string(name, last), string(name, start), string(name, first-except)
    /// </summary>
    internal sealed class StringFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidKeywords = { "first", "last", "start", "first-except" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var first = value.OnlyOrDefault();

            if (!(first is FunctionToken funcToken) || !funcToken.Data.Equals("string", System.StringComparison.OrdinalIgnoreCase))
                return null;

            // First argument must be the string name (identifier)
            var argToken = funcToken.ArgumentTokens.FirstOrDefault();

            if (!(argToken is KeywordToken nameToken))
                return null;

            var name = nameToken.Data;

            // Default keyword is "first" if not specified
            var keyword = "first";

            // Check for optional second argument (keyword)
            var args = funcToken.ArgumentTokens
 .Where(t => t.Type != TokenType.Comma && t.Type != TokenType.Whitespace)
.ToArray();

            if (args.Length > 1)
            {
                if (args[1] is KeywordToken keywordToken)
                {
                    var kw = keywordToken.Data.ToLowerInvariant();
                    if (!ValidKeywords.Contains(kw))
                        return null;

                    keyword = kw;
                }
                else
                {
                    return null;
                }
            }

            return new StringFunctionValue(name, keyword, value);
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

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }

            public string Name => _name;
            public string Keyword => _keyword;
        }
    }
}
