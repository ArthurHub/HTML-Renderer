namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class Shape
    {
        public Shape(Length top, Length right, Length bottom, Length left)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        public Length Top { get; }
        public Length Right { get; }
        public Length Bottom { get; }
        public Length Left { get; }
    }
}