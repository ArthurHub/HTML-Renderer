using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates and serializes a single-layer <c>background-size</c> value via the shared
    /// <see cref="BackgroundSizeGrammar"/>.
    /// </summary>
    internal sealed class BackgroundSizeValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();
            var parsed = BackgroundSizeGrammar.TryParse(tokens);
            return parsed != null ? new BackgroundSizeValue(parsed, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<BackgroundSizeValue>();
        }

        private sealed class BackgroundSizeValue : IPropertyValue
        {
            private readonly BackgroundSizeGrammar.ParsedSize _parsed;

            public BackgroundSizeValue(BackgroundSizeGrammar.ParsedSize parsed, IEnumerable<Token> tokens)
            {
                _parsed = parsed;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    if (_parsed.IsCover) return Keywords.Cover;
                    if (_parsed.IsContain) return Keywords.Contain;

                    var width = ComponentText(_parsed.Width);
                    return _parsed.HasExplicitHeight ? width + " " + ComponentText(_parsed.Height) : width;
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;

            private static string ComponentText(BackgroundSizeGrammar.Component c) =>
                c.IsAuto ? Keywords.Auto : Converters.LengthOrPercentConverter.Convert(new[] { c.Value })?.CssText ?? string.Empty;
        }
    }
}
