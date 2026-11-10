using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-auto-columns</c>/<c>-rows</c>, CSS Grid 2 §7.6.</summary>
    internal sealed class GridAutoTracksValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.ToArray();
            return GridTrackListGrammar.TryParseTrackSizeList(tokens.Where(t => t.Type != TokenType.Whitespace).ToArray()) is null
                ? null
                : new GridAutoTracksValue(tokens);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<GridAutoTracksValue>();

        private sealed class GridAutoTracksValue : IPropertyValue
        {
            public GridAutoTracksValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;
            public TokenValue Original { get; }
            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
