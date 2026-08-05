using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-column</c> / <c>grid-row</c> shorthand: <c>&lt;grid-line&gt; [ / &lt;grid-line&gt; ]?</c>
    /// expanding to the axis's <c>-start</c>/<c>-end</c> longhands. Implements the CSS Grid §8.3.1
    /// omitted-value copy rule that the generic <c>WithOrder(...).Option()</c> DSL cannot: when the end is
    /// omitted, it copies a bare <c>&lt;custom-ident&gt;</c> start (so <c>grid-column: main</c> spans the
    /// whole <c>main</c> area) but leaves a line number / span as <c>auto</c>.
    /// </summary>
    internal sealed class GridColumnRowShorthandValueConverter : IValueConverter
    {
        private readonly string _startName;
        private readonly string _endName;

        public GridColumnRowShorthandValueConverter(string startName, string endName)
        {
            _startName = startName;
            _endName = endName;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var components = GridPlacementShorthand.Split(value, maxComponents: 2);
            if (components is null) return null;

            var start = components[0];
            // end omitted → copy a bare custom-ident start, else auto.
            var end = components.Count > 1 ? components[1]
                : start.IsBareCustomIdent ? start : (GridPlacementShorthand.Component?)null;

            var map = new Dictionary<string, IReadOnlyList<Token>>
            {
                [_startName] = start.Tokens,
                [_endName] = GridPlacementShorthand.Or(end),
            };
            return new MappedShorthandValue(map, value);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<MappedShorthandValue>();
    }
}
