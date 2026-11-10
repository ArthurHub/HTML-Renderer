using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>grid</c>, CSS Grid 2 §7.8. A longhand without a slice is reset to its initial value.</summary>
    internal sealed class GridShorthandValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            // Not ToList(), the IEnumerable<Token> extension of that name splits on commas.
            var tokens = new List<Token>(value);

            if (GridTemplateShorthand.TryParseTemplate(tokens, out var rows, out var columns, out var areas))
            {
                var templateMap = new Dictionary<string, IReadOnlyList<Token>>();
                if (rows != null) templateMap[PropertyNames.GridTemplateRows] = rows;
                if (columns != null) templateMap[PropertyNames.GridTemplateColumns] = columns;
                if (areas != null) templateMap[PropertyNames.GridTemplateAreas] = areas;
                return new MappedShorthandValue(templateMap, tokens);
            }

            var groups = GridTemplateShorthand.SplitTopLevelSlash(tokens);
            if (groups.Count != 2) return null;

            var leftRaw = groups[0];
            var rightRaw = groups[1];
            var left = GridTemplateShorthand.Significant(leftRaw);
            var right = GridTemplateShorthand.Significant(rightRaw);

            var leftHasFlow = left.Any(t => t.Type == TokenType.Ident && t.Data.Isi(Keywords.AutoFlow));
            var rightHasFlow = right.Any(t => t.Type == TokenType.Ident && t.Data.Isi(Keywords.AutoFlow));
            if (leftHasFlow == rightHasFlow) return null;

            var map = new Dictionary<string, IReadOnlyList<Token>>();

            if (rightHasFlow)
            {
                // <grid-template-rows> / [ auto-flow && dense? ] <grid-auto-columns>?
                var rowsSlice = GridTemplateShorthand.TryTemplateAxis(leftRaw);
                if (rowsSlice is null) return null;
                if (!GridTemplateShorthand.TryParseAutoFlowSide(rightRaw, out var dense, out var autoColumns)) return null;

                map[PropertyNames.GridTemplateRows] = rowsSlice;
                map[PropertyNames.GridAutoFlow] = GridTemplateShorthand.BuildAutoFlow(column: true, dense);
                if (autoColumns != null) map[PropertyNames.GridAutoColumns] = autoColumns;
            }
            else
            {
                // [ auto-flow && dense? ] <grid-auto-rows>? / <grid-template-columns>
                var columnsSlice = GridTemplateShorthand.TryTemplateAxis(rightRaw);
                if (columnsSlice is null) return null;
                if (!GridTemplateShorthand.TryParseAutoFlowSide(leftRaw, out var dense, out var autoRows)) return null;

                map[PropertyNames.GridTemplateColumns] = columnsSlice;
                map[PropertyNames.GridAutoFlow] = GridTemplateShorthand.BuildAutoFlow(column: false, dense);
                if (autoRows != null) map[PropertyNames.GridAutoRows] = autoRows;
            }

            return new MappedShorthandValue(map, tokens);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<MappedShorthandValue>();
    }
}
