namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct GradientStop
    {
        public GradientStop(Color color, Length location)
        {
            Color = color;
            Location = location;
        }

        public Color Color { get; }
        public Length Location { get; }
    }
}