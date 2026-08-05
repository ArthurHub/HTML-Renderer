namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class Shadow
    {
        public Shadow(bool inset, Length offsetX, Length offsetY, Length blurRadius, Length spreadRadius, Color color)
        {
            IsInset = inset;
            OffsetX = offsetX;
            OffsetY = offsetY;
            BlurRadius = blurRadius;
            SpreadRadius = spreadRadius;
            Color = color;
        }

        public Color Color { get; }
        public Length OffsetX { get; }
        public Length OffsetY { get; }
        public Length BlurRadius { get; }
        public Length SpreadRadius { get; }
        public bool IsInset { get; }
    }
}