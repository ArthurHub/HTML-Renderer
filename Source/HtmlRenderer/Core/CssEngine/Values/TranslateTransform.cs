namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TranslateTransform : ITransform
    {
        internal TranslateTransform(Length x, Length y, Length z)
        {
            Dx = x;
            Dy = y;
            Dz = z;
        }

        public TransformMatrix ComputeMatrix()
        {
            var dx = Dx.ToPixel();
            var dy = Dy.ToPixel();
            var dz = Dz.ToPixel();
            return new TransformMatrix(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, dx, dy, dz, 0f, 0f, 0f);
        }

        public Length Dx { get; }
        public Length Dy { get; }
        public Length Dz { get; }
    }
}