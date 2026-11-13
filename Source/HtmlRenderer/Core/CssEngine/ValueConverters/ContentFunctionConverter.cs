using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>content()</c> function used in <c>content</c> and <c>string-set</c> (GCPM §7.5):
    /// <c>content()</c>, <c>content(text)</c>, <c>content(before)</c>, <c>content(after)</c>,
    /// <c>content(first-letter)</c>. Default mode is <c>text</c>.
    /// </summary>
    internal sealed class ContentFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidModes = { "text", "before", "after", "first-letter" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            if (value.OnlyOrDefault() is not FunctionToken function ||
                !function.Data.Equals(FunctionNames.Content, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var mode = "text";
            var arg = function.ArgumentTokens.FirstOrDefault(t =>
                t.Type != TokenType.Whitespace && t.Type != TokenType.RoundBracketClose);

            if (arg != null)
            {
                if (arg is not KeywordToken keyword) return null;

                var name = keyword.Data.ToLowerInvariant();
                if (!ValidModes.Contains(name)) return null;
                mode = name;
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

            public string CssText => _mode == "text" ? "content()" : $"content({_mode})";

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
