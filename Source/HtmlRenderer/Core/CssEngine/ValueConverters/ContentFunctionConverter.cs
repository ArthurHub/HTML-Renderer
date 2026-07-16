using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Converter for the content() function used in string-set property.
    /// Supports: content(text), content(before), content(after), content(first-letter)
    /// </summary>
    internal sealed class ContentFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidModes = { "text", "before", "after", "first-letter" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var first = value.OnlyOrDefault();

            if (!(first is FunctionToken funcToken) || !funcToken.Data.Equals(FunctionNames.Content, System.StringComparison.OrdinalIgnoreCase))
                return null;

            // Default mode is "text" if no argument provided
            var mode = "text";

            if (funcToken.ArgumentTokens.Any())
            {
                var argToken = funcToken.ArgumentTokens.FirstOrDefault();
                if (argToken is KeywordToken keywordToken)
                {
                    var keyword = keywordToken.Data.ToLowerInvariant();
                    if (!ValidModes.Contains(keyword))
                        return null;

                    mode = keyword;
                }
                else
                {
                    return null;
                }
            }

            return new ContentFunctionValue(mode, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<ContentFunctionValue>();
        }

        private sealed class ContentFunctionValue : IPropertyValue
        {
            private readonly string _mode;

            public ContentFunctionValue(string mode, IEnumerable<Token> tokens)
            {
                _mode = mode;
                Original = new TokenValue(tokens);
            }

            public string CssText => _mode == "text"
                ? "content()"
                : $"content({_mode})";

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }

            public string Mode => _mode;
        }
    }
}
