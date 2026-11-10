using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>A <c>grid-template-areas</c> value, CSS Grid 2 §7.3.</summary>
    internal sealed class GridAreas
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }

        /// <summary>The area name per <c>[row, col]</c>, null for a null cell token.</summary>
        public string[,] Cells { get; set; }

        /// <summary>0-based inclusive cell rectangle per area name.</summary>
        public IReadOnlyDictionary<string, (int R1, int C1, int R2, int C2)> Areas { get; set; }
    }

    /// <summary>
    /// The <c>&lt;string&gt;+</c> of <c>grid-template-areas</c>, CSS Grid 2 §7.3. Returns null for <c>none</c>
    /// too, the property handles that keyword.
    /// </summary>
    internal static class GridTemplateAreasGrammar
    {
        internal static GridAreas TryParse(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            if (toks.Any(t => t.Type != TokenType.String)) return null;

            var rows = new List<string[]>(toks.Length);
            int colCount = -1;
            foreach (var t in toks)
            {
                var cells = ((StringToken)t).Data
                    .Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length == 0) return null;
                if (colCount < 0) colCount = cells.Length;
                else if (cells.Length != colCount) return null;

                var normalized = new string[colCount];
                for (var c = 0; c < colCount; c++)
                {
                    if (IsEmptyCell(cells[c])) { normalized[c] = null; continue; }
                    if (cells[c].Contains('.')) return null;
                    normalized[c] = cells[c];
                }
                rows.Add(normalized);
            }

            var rowCount = rows.Count;
            var grid = new string[rowCount, colCount];
            var bounds = new Dictionary<string, (int R1, int C1, int R2, int C2)>();

            for (var r = 0; r < rowCount; r++)
                for (var c = 0; c < colCount; c++)
                {
                    var name = rows[r][c];
                    grid[r, c] = name;
                    if (name is null) continue;
                    if (bounds.TryGetValue(name, out var b))
                        bounds[name] = (System.Math.Min(b.R1, r), System.Math.Min(b.C1, c),
                                        System.Math.Max(b.R2, r), System.Math.Max(b.C2, c));
                    else
                        bounds[name] = (r, c, r, c);
                }

            // Every cell in an area's bounding box has to carry its name.
            foreach (var entry in bounds)
            {
                var name = entry.Key;
                var b = entry.Value;
                for (var r = b.R1; r <= b.R2; r++)
                    for (var c = b.C1; c <= b.C2; c++)
                        if (grid[r, c] != name) return null;
            }

            return new GridAreas { RowCount = rowCount, ColCount = colCount, Cells = grid, Areas = bounds };
        }

        private static bool IsEmptyCell(string cell) => cell.All(ch => ch == '.');
    }
}
