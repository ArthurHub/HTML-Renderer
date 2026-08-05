using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class LinearGradient : IGradient
    {
        public LinearGradient(Angle angle, GradientStop[] stops, bool repeating = false)
        {
            _stops = stops;
            Angle = angle;
            IsRepeating = repeating;
        }

        private readonly GradientStop[] _stops;

        public Angle Angle { get; }
        public IEnumerable<GradientStop> Stops => _stops.AsEnumerable();
        public bool IsRepeating { get; }
    }
}