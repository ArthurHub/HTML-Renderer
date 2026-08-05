using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RotateTransform : ITransform
    {
        internal RotateTransform(float x, float y, float z, float angle)
        {
            X = x;
            Y = y;
            Z = z;
            Angle = angle;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float Angle { get; }

        public TransformMatrix ComputeMatrix()
        {
            var norm = 1f / (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            var sina = (float)Math.Sin(Angle);
            var cosa = (float)Math.Cos(Angle);

            var l = X * norm;
            var m = Y * norm;
            var n = Z * norm;
            var omc = 1f - cosa;

            return new TransformMatrix(
                l * l * omc + cosa, m * l * omc - n * sina, n * l * omc + m * sina,
                l * m * omc + n * sina, m * m * omc + cosa, n * m * omc - l * sina,
                l * n * omc - m * sina, m * n * omc + l * sina, n * n * omc + cosa,
                0f, 0f, 0f, 0f, 0f, 0f);
        }
    }
}