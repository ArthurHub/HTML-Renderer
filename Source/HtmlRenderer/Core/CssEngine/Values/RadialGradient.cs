using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RadialGradient : IImageSource
    {
        internal enum SizeMode : byte
        {
            None,
            ClosestCorner,
            ClosestSide,
            FarthestCorner,
            FarthestSide
        }

        public RadialGradient(bool circle, Point pt, Length width, Length height, SizeMode sizeMode,
            GradientStop[] stops, bool repeating = false)
        {
            _stops = stops;
            Position = pt;
            MajorRadius = width;
            MinorRadius = height;
            IsRepeating = repeating;
            IsCircle = circle;
            Mode = sizeMode;
        }

        private readonly GradientStop[] _stops;

        public bool IsCircle { get; }

        public SizeMode Mode { get; }
        public Point Position { get; }
        public Length MajorRadius { get; }
        public Length MinorRadius { get; }
        public IEnumerable<GradientStop> Stops => _stops.AsEnumerable();
        public bool IsRepeating { get; }
    }
}