using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid-template</c> shorthand (CSS Grid §7.4), expanding to <c>grid-template-rows</c> /
    /// <c>grid-template-columns</c> / <c>grid-template-areas</c>. An omitted axis is reset to its initial
    /// value (<c>none</c>) by <see cref="ShorthandProperty.Export"/> when its slice is absent.
    /// </summary>
    internal sealed class GridTemplateShorthandValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            if (!GridTemplateShorthand.TryParseTemplate(value, out var rows, out var columns, out var areas))
                return null;

            var map = new Dictionary<string, IReadOnlyList<Token>>();
            if (rows != null) map[PropertyNames.GridTemplateRows] = rows;
            if (columns != null) map[PropertyNames.GridTemplateColumns] = columns;
            if (areas != null) map[PropertyNames.GridTemplateAreas] = areas;

            return new MappedShorthandValue(map, value);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<MappedShorthandValue>();
    }
}
