using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class SkewTransform : ITransform
    {
        internal SkewTransform(float alpha, float beta)
        {
            Alpha = alpha;
            Beta = beta;
        }

        public TransformMatrix ComputeMatrix()
        {
            var a = (float)Math.Tan(Alpha);
            var b = (float)Math.Tan(Beta);
            return new TransformMatrix(1f, a, 0f, b, 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
        }

        public float Alpha { get; }
        public float Beta { get; }
    }
}