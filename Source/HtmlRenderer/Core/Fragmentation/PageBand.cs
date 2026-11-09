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
            : this(top, bottom, top)
        {
        }

        public PageBand(double top, double bottom, double origin)
        {
            Top = top;
            Bottom = bottom;
            Origin = origin;
        }

        public double Top { get; }
        public double Bottom { get; }

        /// <summary>
        /// The document Y that maps to Y=0 on the fragmentainer's own surface. On a page this is the band's
        /// top minus the page's top margin, so content keeps the margin above it.
        /// </summary>
        public double Origin { get; }
        public double Height => Bottom - Top;

        public bool Contains(double y) => y >= Top && y < Bottom;
    }
}
