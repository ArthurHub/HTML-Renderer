namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// An inclusive range of Unicode scalar values (codepoints), e.g. the <c>U+0000-00FF</c> of a CSS
    /// <c>@font-face</c> <c>unicode-range</c> descriptor. Both ends are inclusive - unlike
    /// <see cref="System.Range"/>, which is half-open and index based. <c>int</c>-based (matching this
    /// codebase's existing <see cref="Symbols.MaximumCodepoint"/>/<see cref="CharExtensions"/>
    /// int-codepoint convention) rather than PeachPDF's <c>Rune</c>-based <c>RuneRange</c> - see
    /// <see cref="Utils.UnicodeRangeParser"/> for why: <c>System.Text.Rune</c> only exists on
    /// <c>net8.0</c>, not this project's <c>netstandard2.0</c> leg. Public: it appears in
    /// <see cref="Adapters.RAdapter.AddFontFaceAsync"/>'s public signature, for a custom adapter implementation
    /// outside this assembly.
    /// </summary>
    public readonly struct CodepointRange
    {
        public CodepointRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>The first codepoint covered by this range (inclusive).</summary>
        public int Start { get; }

        /// <summary>The last codepoint covered by this range (inclusive).</summary>
        public int End { get; }

        /// <summary>Whether <paramref name="codepoint"/> lies within this range (both ends inclusive).</summary>
        public bool Contains(int codepoint) => codepoint >= Start && codepoint <= End;
    }
}
