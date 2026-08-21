namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// A fragmentainer's block-axis extent: the coordinates its content may occupy. The value form of the
    /// band <see cref="FragmentainerContext"/> exposes, so "which fragmentainer is this coordinate in" can
    /// be asked of the page grid without a live <see cref="FragmentainerContext"/> to hand - which matters
    /// because a box being laid out is not always inside the fragmentainer currently being filled
    /// (monolithic content, a box below a tall margin).
    /// </summary>
    internal readonly struct PageBand
    {
        public PageBand(double top, double bottom)
        {
            Top = top;
            Bottom = bottom;
        }

        public double Top { get; }
        public double Bottom { get; }
        public double Height => Bottom - Top;

        public bool Contains(double y) => y >= Top && y < Bottom;
    }
}
