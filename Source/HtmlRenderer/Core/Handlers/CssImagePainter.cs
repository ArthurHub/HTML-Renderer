// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Turns a parsed <c>linear-gradient()</c> value into a brush ready to paint. Ported from PeachPDF's
    /// Html/Core/Handlers/CssImagePainter.cs, trimmed to the linear-gradient-only, single (non-layered)
    /// background-image case this engine supports - see <see cref="ParsedLinearGradient"/>.
    /// </summary>
    internal static class CssImagePainter
    {
        /// <summary>
        /// Builds the brush for a <c>linear-gradient()</c> background, filling the given rectangle.
        /// </summary>
        public static RBrush GetLinearGradientBrush(RGraphics g, RRect rect, ParsedLinearGradient gradient)
        {
            var (p1, p2) = ComputeGradientLine(rect, gradient.AngleRad);
            double gdx = p2.X - p1.X, gdy = p2.Y - p1.Y;
            double gradientLength = Math.Sqrt(gdx * gdx + gdy * gdy);
            var stops = NormalizeGradientStops(gradient.Stops, gradientLength);
            if (gradient.IsRepeating)
                stops = ExpandRepeatingStops(stops);
            return g.GetLinearGradientBrush(p1, p2, stops);
        }

        /// <summary>
        /// Computes the CSS gradient line endpoints for the given box rect and angle (standard CSS
        /// "corner-to-corner projection" algorithm - the line is centered on the box and long enough that
        /// the box's bounding rectangle is fully spanned along the gradient direction).
        /// </summary>
        private static (RPoint p1, RPoint p2) ComputeGradientLine(RRect rect, double angleRad)
        {
            double dx = Math.Sin(angleRad);
            double dy = -Math.Cos(angleRad);
            double cx = rect.X + rect.Width / 2;
            double cy = rect.Y + rect.Height / 2;
            double halfLen = Math.Abs(dx) * rect.Width / 2 + Math.Abs(dy) * rect.Height / 2;
            var p1 = new RPoint(cx - dx * halfLen, cy - dy * halfLen);
            var p2 = new RPoint(cx + dx * halfLen, cy + dy * halfLen);
            return (p1, p2);
        }

        private static double? ConvertLength(Length? length, double gradientLength, double emPx = 16.0)
        {
            if (!length.HasValue) return null;
            var len = length.Value;
            if (len.Type == Length.Unit.Percent)
                return len.Value / 100.0;
            if (len.IsAbsolute)
                return gradientLength > 0 ? len.ToPixel() / gradientLength : 0.0;
            if (len.Type == Length.Unit.Em)
                return gradientLength > 0 ? len.Value * emPx / gradientLength : 0.0;
            return null;
        }

        private static RColor LerpColor(RColor a, RColor b, double t)
        {
            t = t < 0.0 ? 0.0 : (t > 1.0 ? 1.0 : t);
            return RColor.FromArgb(
                (int)Math.Round(a.A + t * (b.A - a.A)),
                (int)Math.Round(a.R + t * (b.R - a.R)),
                (int)Math.Round(a.G + t * (b.G - a.G)),
                (int)Math.Round(a.B + t * (b.B - a.B)));
        }

        /// <summary>
        /// Resolves each stop's position to a [0,1] fraction of the gradient line (per CSS Images §3.5.5:
        /// the first/last stop default to 0%/100%, middle stops without an explicit position are spaced
        /// evenly between their neighbors, and bare-position "hints" between two color stops bias the
        /// interpolation curve rather than being a stop themselves).
        /// </summary>
        private static (RColor Color, double Position)[] NormalizeGradientStops(
            (RColor? Color, Length? Position, bool IsHint)[] stops,
            double gradientLength,
            double emPx = 16.0)
        {
            var colorStops = stops.Where(s => !s.IsHint).ToArray();
            int n = colorStops.Length;
            if (n == 0) return new (RColor, double)[0];

            var rawPos = new double?[n];
            for (int i = 0; i < n; i++)
                rawPos[i] = ConvertLength(colorStops[i].Position, gradientLength, emPx);

            var resolved = new (RColor Color, double Position)[n];
            double first = rawPos[0] ?? 0.0;
            double last = rawPos[n - 1] ?? 1.0;
            resolved[0] = (colorStops[0].Color.Value, first);
            resolved[n - 1] = (colorStops[n - 1].Color.Value, last);

            int runStart = -1;
            for (int i = 1; i < n - 1; i++)
            {
                if (rawPos[i].HasValue)
                {
                    resolved[i] = (colorStops[i].Color.Value, rawPos[i].Value);
                    if (runStart >= 0)
                    {
                        double posA = resolved[runStart - 1].Position;
                        double posB = resolved[i].Position;
                        int count = i - runStart + 1;
                        for (int j = runStart; j < i; j++)
                        {
                            double t = (double)(j - runStart + 1) / count;
                            resolved[j] = (colorStops[j].Color.Value, posA + t * (posB - posA));
                        }
                        runStart = -1;
                    }
                }
                else
                {
                    if (runStart < 0) runStart = i;
                    resolved[i] = (colorStops[i].Color.Value, 0);
                }
            }
            if (runStart >= 0)
            {
                double posA = resolved[runStart - 1].Position;
                double posB = resolved[n - 1].Position;
                int count = n - 1 - runStart + 1;
                for (int j = runStart; j < n - 1; j++)
                {
                    double t = (double)(j - runStart + 1) / count;
                    resolved[j] = (colorStops[j].Color.Value, posA + t * (posB - posA));
                }
            }

            if (!stops.Any(s => s.IsHint))
                return resolved;

            var result = new List<(RColor Color, double Position)>();
            int colorIdx = 0;
            for (int i = 0; i < stops.Length; i++)
            {
                if (!stops[i].IsHint)
                {
                    result.Add(resolved[colorIdx++]);
                }
                else
                {
                    if (colorIdx == 0 || colorIdx >= n) continue;
                    var s1 = resolved[colorIdx - 1];
                    var s2 = resolved[colorIdx];
                    double range = s2.Position - s1.Position;
                    double hintPos = ConvertLength(stops[i].Position, gradientLength, emPx) ?? (s1.Position + range * 0.5);
                    double h = range > 1e-9
                        ? Math.Min(Math.Max((hintPos - s1.Position) / range, 1e-9), 1.0 - 1e-9)
                        : 0.5;
                    const int kSteps = 7;
                    double logHalf = Math.Log(0.5);
                    double logH = Math.Log(h);
                    for (int k = 1; k <= kSteps; k++)
                    {
                        double t = (double)k / (kSteps + 1);
                        double curved = Math.Pow(t, logHalf / logH);
                        result.Add((LerpColor(s1.Color, s2.Color, curved), s1.Position + t * range));
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Flattens a repeating gradient's stop tile into a single non-repeating stop list spanning the
        /// full [0,1] gradient line, by replicating the tile as many times as needed in both directions.
        /// </summary>
        private static (RColor Color, double Position)[] ExpandRepeatingStops((RColor Color, double Position)[] stops)
        {
            if (stops.Length < 2) return stops;
            double tileStart = stops[0].Position;
            double tileEnd = stops[stops.Length - 1].Position;
            double tileLen = tileEnd - tileStart;
            if (tileLen < 1e-6 || (tileStart <= 0.0 && tileEnd >= 1.0)) return stops;
            const double eps = 0.0001;
            var result = new List<(RColor Color, double Position)>();
            int kMin = (int)Math.Floor(-tileEnd / tileLen);
            int kMax = (int)Math.Ceiling((1.0 - tileStart) / tileLen);
            for (int k = kMin; k <= kMax; k++)
            {
                double kOffset = k * tileLen;
                for (int i = 0; i < stops.Length; i++)
                {
                    double rawPos = stops[i].Position + kOffset;
                    bool isLastStop = i == stops.Length - 1;
                    double adjPos = isLastStop && k < kMax ? rawPos - eps : rawPos;
                    if (adjPos >= -eps && adjPos <= 1.0 + eps)
                        result.Add((stops[i].Color, Math.Min(Math.Max(adjPos, 0.0), 1.0)));
                }
            }
            result.Sort((a, b) => a.Position.CompareTo(b.Position));
            if (result.Count == 0) return stops;
            if (result[0].Position > eps)
                result.Insert(0, (SampleRepeatingColor(stops, tileStart, tileLen, 0.0), 0.0));
            if (result[result.Count - 1].Position < 1.0 - eps)
                result.Add((SampleRepeatingColor(stops, tileStart, tileLen, 1.0), 1.0));
            return result.ToArray();
        }

        private static RColor SampleRepeatingColor((RColor Color, double Position)[] stops, double tileStart, double tileLen, double pos)
        {
            double relPos = (pos - tileStart) % tileLen;
            if (relPos < 0) relPos += tileLen;
            double absWithinTile = tileStart + relPos;
            for (int i = 0; i < stops.Length - 1; i++)
            {
                if (absWithinTile >= stops[i].Position && absWithinTile <= stops[i + 1].Position)
                {
                    double range = stops[i + 1].Position - stops[i].Position;
                    double t = range > 1e-12 ? (absWithinTile - stops[i].Position) / range : 0.0;
                    return LerpColor(stops[i].Color, stops[i + 1].Color, t);
                }
            }
            return absWithinTile <= stops[0].Position ? stops[0].Color : stops[stops.Length - 1].Color;
        }
    }
}
