using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid-area</c>, CSS Grid 2 §8.4.</summary>
    internal sealed class GridAreaShorthandValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var c = GridPlacementShorthand.Split(value, maxComponents: 4);
            if (c is null) return null;

            var rowStart = c[0];
            var colStart = c.Count > 1 ? c[1]
                : rowStart.IsBareCustomIdent ? rowStart : (GridPlacementShorthand.Component?)null;
            var rowEnd = c.Count > 2 ? c[2]
                : rowStart.IsBareCustomIdent ? rowStart : (GridPlacementShorthand.Component?)null;
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
