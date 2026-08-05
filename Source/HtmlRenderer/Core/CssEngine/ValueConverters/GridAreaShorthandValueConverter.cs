using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-area</c> shorthand: <c>&lt;grid-line&gt; [ / &lt;grid-line&gt; ]{0,3}</c> in the order
    /// row-start / column-start / row-end / column-end. Implements the CSS Grid §8.3.1 omitted-value copy
    /// rule — a bare <c>&lt;custom-ident&gt;</c> propagates to the paired/all edges, so <c>grid-area: main</c>
    /// fills the whole <c>main</c> area, while <c>grid-area: 2</c> leaves the other edges <c>auto</c>.
    /// </summary>
    internal sealed class GridAreaShorthandValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var c = GridPlacementShorthand.Split(value, maxComponents: 4);
            if (c is null) return null;

            var rowStart = c[0];
            // column-start omitted → if row-start is a bare custom-ident, all four take it; else auto.
            var colStart = c.Count > 1 ? c[1]
                : rowStart.IsBareCustomIdent ? rowStart : (GridPlacementShorthand.Component?)null;
            // row-end omitted → copy a bare custom-ident row-start, else auto.
            var rowEnd = c.Count > 2 ? c[2]
                : rowStart.IsBareCustomIdent ? rowStart : (GridPlacementShorthand.Component?)null;
            // column-end omitted → copy a bare custom-ident column-start, else auto.
            var colEnd = c.Count > 3 ? c[3]
                : colStart.HasValue && colStart.Value.IsBareCustomIdent ? colStart : (GridPlacementShorthand.Component?)null;

            var map = new Dictionary<string, IReadOnlyList<Token>>
            {
                [PropertyNames.GridRowStart] = rowStart.Tokens,
                [PropertyNames.GridColumnStart] = GridPlacementShorthand.Or(colStart),
                [PropertyNames.GridRowEnd] = GridPlacementShorthand.Or(rowEnd),
                [PropertyNames.GridColumnEnd] = GridPlacementShorthand.Or(colEnd),
            };
            return new MappedShorthandValue(map, value);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<MappedShorthandValue>();
    }
}
