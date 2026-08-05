using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>clip-path</c> value: accepts the keyword <c>none</c> or any <c>&lt;basic-shape&gt;</c>
    /// the shared <see cref="BasicShapeGrammar"/> recognizes (<c>polygon()</c>/<c>inset()</c>/<c>circle()</c>/
    /// <c>ellipse()</c>), and rejects everything else so an unknown value (e.g. <c>clip-path: banana</c>)
    /// is dropped at parse time. The authored value text is preserved verbatim (<see cref="ClipPathValue.CssText"/>
    /// returns the original tokens) so the render-time resolver (Layer B) sees exactly what the author wrote.
    /// </summary>
    internal sealed class ClipPathValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            var isNone = tokens.Length == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.None);

            if (!isNone && BasicShapeGrammar.TryParse(tokens) == null)
                return null;

            return new ClipPathValue(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<ClipPathValue>();
        }

        private sealed class ClipPathValue : IPropertyValue
        {
            public ClipPathValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            // Preserve the authored text verbatim - Layer B re-tokenizes and resolves this string.
            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
