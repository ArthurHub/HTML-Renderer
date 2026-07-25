using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>grid</c> shorthand (CSS Grid §7.8): either a <c>&lt;grid-template&gt;</c>, or one of the two
    /// auto-flow forms
    /// <c>&lt;grid-template-rows&gt; / [ auto-flow &amp;&amp; dense? ] &lt;grid-auto-columns&gt;?</c> and
    /// <c>[ auto-flow &amp;&amp; dense? ] &lt;grid-auto-rows&gt;? / &lt;grid-template-columns&gt;</c>. Expands to
    /// the three <c>grid-template-*</c> longhands plus <c>grid-auto-flow</c>/<c>-rows</c>/<c>-columns</c>;
    /// every longhand it doesn't set is reset to its initial value (an absent slice — §7.8).
    /// </summary>
    internal sealed class GridShorthandValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            // NB: the domain-specific IEnumerable<Token>.ToList() extension splits on commas; materialize the
            // flat token list explicitly instead.
            var tokens = new List<Token>(value);

            // Form 1: <grid-template>. The three template longhands are set; grid-auto-flow/-rows/-columns
            // are omitted here so they reset to their initial values (row / auto / auto).
            if (GridTemplateShorthand.TryParseTemplate(tokens, out var rows, out var columns, out var areas))
            {
                var templateMap = new Dictionary<string, IReadOnlyList<Token>>();
                if (rows != null) templateMap[PropertyNames.GridTemplateRows] = rows;
                if (columns != null) templateMap[PropertyNames.GridTemplateColumns] = columns;
                if (areas != null) templateMap[PropertyNames.GridTemplateAreas] = areas;
                return new MappedShorthandValue(templateMap, tokens);
            }

            // Forms 2/3: an auto-flow form — requires exactly one top-level '/'.
            var groups = GridTemplateShorthand.SplitTopLevelSlash(tokens);
            if (groups.Count != 2) return null;

            var leftRaw = groups[0];
            var rightRaw = groups[1];
            var left = GridTemplateShorthand.Significant(leftRaw);
            var right = GridTemplateShorthand.Significant(rightRaw);

            var leftHasFlow = left.Any(t => t.Type == TokenType.Ident && t.Data.Isi(Keywords.AutoFlow));
            var rightHasFlow = right.Any(t => t.Type == TokenType.Ident && t.Data.Isi(Keywords.AutoFlow));
            if (leftHasFlow == rightHasFlow) return null; // exactly one side carries auto-flow

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
