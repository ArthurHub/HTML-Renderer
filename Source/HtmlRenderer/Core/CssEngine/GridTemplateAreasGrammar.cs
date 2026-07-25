using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A parsed <c>grid-template-areas</c> value (CSS Grid §7.3): a rectangular grid of named cells. Each
    /// named area's 0-based inclusive bounding rectangle is recorded; a <c>.</c> (or a run of only dots) is
    /// an unnamed empty cell.
    /// </summary>
    internal sealed class GridAreas
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }

        /// <summary>The area name at each cell (<c>[row, col]</c>), or null for an empty <c>.</c> cell.</summary>
        public string[,] Cells { get; set; }

        /// <summary>Each named area's 0-based inclusive rectangle (rowStart, colStart, rowEnd, colEnd).</summary>
        public IReadOnlyDictionary<string, (int R1, int C1, int R2, int C2)> Areas { get; set; }
    }

    /// <summary>
    /// Shared grammar for a <c>grid-template-areas</c> value (a series of quoted strings). Used by Layer A
    /// (accept/reject) and Layer B (the engine derives implicit <c>name-start</c>/<c>name-end</c> lines).
    /// Returns null for anything invalid — including the <c>none</c> keyword, which the property accepts
    /// separately.
    /// </summary>
    internal static class GridTemplateAreasGrammar
    {
        internal static GridAreas TryParse(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return null;

            // Every token must be a string; each string is one row.
            if (toks.Any(t => t.Type != TokenType.String)) return null;

            var rows = new List<string[]>(toks.Length);
            int colCount = -1;
            foreach (var t in toks)
            {
                var cells = ((StringToken)t).Data
                    .Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                if (cells.Length == 0) return null;          // an empty string is invalid
                if (colCount < 0) colCount = cells.Length;
                else if (cells.Length != colCount) return null; // ragged rows are invalid

                var normalized = new string[colCount];
                for (var c = 0; c < colCount; c++)
                {
                    if (IsEmptyCell(cells[c])) { normalized[c] = null; continue; }
                    // A named cell is a <custom-ident> — a null token (a mixed "a.b" is invalid, not a name).
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

            // Each named area must be a single filled rectangle: every cell in its bounding box carries the
            // name (which also guarantees no cell with the name lies outside the box).
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

        /// <summary>A cell of only U+002E FULL STOP characters (<c>.</c>, <c>...</c>) is an empty cell.</summary>
        private static bool IsEmptyCell(string cell) => cell.All(ch => ch == '.');
    }
}
