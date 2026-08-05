using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a single grid <c>&lt;grid-line&gt;</c> (<c>grid-column-start</c>/<c>-end</c>,
    /// <c>grid-row-start</c>/<c>-end</c>) through the shared <see cref="GridLineGrammar"/> — <c>auto</c>, an
    /// integer line, or <c>span N</c>. The authored text is preserved for the grid placement engine.
    /// </summary>
    internal sealed class GridLineValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();
            return GridLineGrammar.TryParse(tokens.Where(t => t.Type != TokenType.Whitespace).ToArray()) is null
                ? null
                : new GridLineValue(tokens);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GridLineValue>();
        }

        private sealed class GridLineValue : IPropertyValue
        {
            public GridLineValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
