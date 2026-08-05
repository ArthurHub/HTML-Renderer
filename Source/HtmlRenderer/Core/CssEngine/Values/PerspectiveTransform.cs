namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PerspectiveTransform : ITransform
    {
        private readonly Length _distance;

        internal PerspectiveTransform(Length distance)
        {
            _distance = distance;
        }

        public TransformMatrix ComputeMatrix()
        {
            return new TransformMatrix(
                1f,
                0f,
                0f,
                0f,
                1f,
                0f,
                0f,
                0f,
                1f,
                0f,
                0f,
                0f,
                0f,
                0f,
                -1f / _distance.ToPixel());
        }
    }
}