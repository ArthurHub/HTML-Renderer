using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>grid-template</c>, CSS Grid 2 §7.4. <see cref="ShorthandProperty.Export"/> resets a longhand without
    /// a slice to its initial value.
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
