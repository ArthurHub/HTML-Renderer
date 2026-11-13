using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The single home for CSS color-space conversion math (CSS Color 4/5): forward/reverse transforms
    /// between gamma-encoded sRGB and every interpolation space, plus polar-hue interpolation. Kept in
    /// the object model so both solid-color resolution (<see cref="Color"/>'s
    /// <c>oklch()</c>/<c>lab()</c>/<c>color-mix()</c> factories) and any future gradient interpolation
    /// share one implementation.
    /// </summary>
    internal static class ColorSpaceMath
    {
        internal enum Space { Srgb, SrgbLinear, DisplayP3, Lab, Oklab, XyzD65, XyzD50, Hsl, Hwb, Lch, Oklch, A98Rgb, ProphotoRgb, Rec2020 }

        internal enum HueMethod { Shorter, Longer, Increasing, Decreasing }

        public static bool IsPolar(Space cs) =>
            cs == Space.Hsl || cs == Space.Hwb || cs == Space.Lch || cs == Space.Oklch;

        // The index of the hue coordinate in a space's ToSpace/FromSpace layout, or -1 if rectangular.
        // HSL/HWB carry hue first ([h, s/w, l/bk]); LCH/OKLch carry it last ([L, C, H]).
        public static int HueIndex(Space cs)
        {
            switch (cs)
            {
                case Space.Hsl:
                case Space.Hwb:
                    return 0;
                case Space.Lch:
                case Space.Oklch:
                    return 2;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Whether the hue of a color in lch, oklch, hsl or hwb means nothing because the color is achromatic, as it is for a gray: CSS Color 4 takes
        /// such a hue as missing when colors are interpolated (Interpolating with Missing Components), and a mix then uses the other color's.
        /// For lch and oklch the limit is a chroma of 0.02 or less (in lab for lch, in oklab for oklch), which is where browsers draw the line
        /// (the specification's own epsilons are smaller, 0.0015 and 0.000004); it sits above the noise converting an 8-bit gray leaves behind.
        /// </summary>
        public static bool IsHuePowerless(double[] v, Space cs)
        {
            switch (cs)
            {
                case Space.Lch:
                case Space.Oklch:
                    return v[1] <= 0.02;
                // CSS Color 4 sections 7 and 8: an hsl() color with no saturation, and an hwb() one whose whiteness and blackness fill the whole range, are achromatic.
                case Space.Hsl:
                    return v[1] <= 0.001;
                case Space.Hwb:
                    return v[1] + v[2] >= 99.999;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gives a gray its missing hue before two colors in a polar space are mixed in a color-mix() (see <see cref="IsHuePowerless"/>): the other color's hue
        /// stands in for it, and where both are gray the hue is 0. Does nothing in a rectangular space.
        /// </summary>
        public static void ResolveMissingHues(double[] v1, double[] v2, Space cs, bool hue1IsWritten = false, bool hue2IsWritten = false)
        {
            var hueIndex = HueIndex(cs);
            if (hueIndex < 0) return;

            var powerless1 = !hue1IsWritten && IsHuePowerless(v1, cs);
            var powerless2 = !hue2IsWritten && IsHuePowerless(v2, cs);
            if (powerless1 && powerless2) v1[hueIndex] = v2[hueIndex] = 0;
            else if (powerless1) v1[hueIndex] = v2[hueIndex];
            else if (powerless2) v2[hueIndex] = v1[hueIndex];
        }

        /// <summary>
        /// A color's components as they were written, in the space they were written in (lab(), lch(), oklab(), oklch(), color(), and rgb()/hsl()/hwb() when
        /// a component is <c>none</c>): what a mix in that space (or the polar/rectangular form of it) takes the color from, where converting through Oklab
        /// would replace a gray's hue with whatever angle the round trip leaves, and move the chroma of a near-gray.
        /// </summary>
        internal readonly struct WrittenCoords
        {
            public WrittenCoords(Space space, double[] values, int missing = 0)
            {
                Space = space;
                Values = values;
                Missing = missing;
            }

            public Space Space { get; }
            public double[] Values { get; }

            /// <summary>The components written as <c>none</c>: bits 0 to 2 mark the three components in the written space's order, <see cref="AlphaMissing"/> the alpha.</summary>
            public int Missing { get; }
        }

        /// <summary>The bit of <see cref="WrittenCoords.Missing"/> that marks a missing alpha.</summary>
        public const int AlphaMissing = 8;

        // The kind of quantity a coordinate is, so a missing component can be carried to the analogous component of another space (CSS Color 4 section 12.4,
        // Interpolating with Missing Components): lightness to lightness, hue to hue, a red to a red or an x. A coordinate with no analogue is Other.
        private enum Quantity { Other, Lightness, A, B, Chroma, Hue, Red, Green, Blue }

        private static Quantity QuantityOf(Space cs, int slot)
        {
            switch (cs)
            {
                case Space.Lab:
                case Space.Oklab:
                    return slot == 0 ? Quantity.Lightness : slot == 1 ? Quantity.A : Quantity.B;
                case Space.Lch:
                case Space.Oklch:
                    return slot == 0 ? Quantity.Lightness : slot == 1 ? Quantity.Chroma : Quantity.Hue;
                case Space.Hsl:
                    return slot == 0 ? Quantity.Hue : slot == 1 ? Quantity.Chroma : Quantity.Lightness;
                case Space.Hwb:
                    return slot == 0 ? Quantity.Hue : Quantity.Other;
                default:
                    return slot == 0 ? Quantity.Red : slot == 1 ? Quantity.Green : Quantity.Blue;
            }
        }

        /// <summary>
        /// The slot of <paramref name="to"/> that is analogous to <paramref name="slot"/> of <paramref name="from"/> (the same quantity), or -1 when it has none: a
        /// missing component is only missing in the other space where one is.
        /// </summary>
        public static int AnalogousSlot(Space from, int slot, Space to)
        {
            var quantity = QuantityOf(from, slot);
            if (quantity == Quantity.Other) return -1;

            for (var i = 0; i < 3; i++)
            {
                if (QuantityOf(to, i) == quantity) return i;
            }

            return -1;
        }

        /// <summary>The slots of <paramref name="to"/> a color's missing components (<see cref="WrittenCoords.Missing"/>, given in <paramref name="from"/>) land on, as the same kind of bit mask; the alpha bit is carried as it is.</summary>
        public static int MapMissing(Space from, int missing, Space to)
        {
            var mapped = missing & AlphaMissing;
            for (var slot = 0; slot < 3; slot++)
            {
                if ((missing & (1 << slot)) == 0) continue;
                var target = AnalogousSlot(from, slot, to);
                if (target >= 0) mapped |= 1 << target;
            }

            return mapped;
        }

        /// <summary>
        /// The written coordinates in <paramref name="cs"/> when that is the space they were written in, or its polar/rectangular form (lab and lch share a
        /// white point, as do oklab and oklch, so the conversion is exact); null when they would have to cross to another family.
        /// </summary>
        public static double[] ToSpaceFromWritten(WrittenCoords written, Space cs)
        {
            var v = written.Values;
            if (written.Space == cs) return new[] { v[0], v[1], v[2] };

            if ((written.Space == Space.Lab && cs == Space.Lch) || (written.Space == Space.Oklab && cs == Space.Oklch))
            {
                var (l, c, h) = RectToPolar(v[0], v[1], v[2]);
                return new[] { l, c, h };
            }

            if ((written.Space == Space.Lch && cs == Space.Lab) || (written.Space == Space.Oklch && cs == Space.Oklab))
            {
                var (l, a, b) = PolarToRect(v[0], v[1], v[2]);
                return new[] { l, a, b };
            }

            return null;
        }

        // Portable helpers: netstandard2.0 / net48 lack Math.Clamp and Math.Cbrt.
        internal static double Clamp01(double c) => c < 0 ? 0 : (c > 1 ? 1 : c);

        internal static double Cbrt(double x) =>
            x < 0 ? -Math.Pow(-x, 1.0 / 3.0) : Math.Pow(x, 1.0 / 3.0);

        public static double NormalizeHue(double h) => ((h % 360) + 360) % 360;

        public static double InterpolateHue(double h1, double h2, double t, HueMethod hue)
        {
            h1 = NormalizeHue(h1);
            h2 = NormalizeHue(h2);
            var diff = h2 - h1;
            switch (hue)
            {
                case HueMethod.Longer:
                    if (diff > 0 && diff < 180) diff -= 360;
                    else if (diff <= 0 && diff > -180) diff += 360;
                    break;
                case HueMethod.Increasing:
                    if (diff < 0) diff += 360;
                    break;
                case HueMethod.Decreasing:
                    if (diff > 0) diff -= 360;
                    break;
                default: // Shorter
                    if (diff > 180) diff -= 360;
                    else if (diff < -180) diff += 360;
                    break;
            }
            return NormalizeHue(h1 + t * diff);
        }

        // ── sRGB gamma ────────────────────────────────────────────────────────

        private static double Linearize(double c)
        {
            c = Clamp01(c);
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        private static double Delinearize(double c)
        {
            c = Clamp01(c);
            return c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        }

        // ── Linear sRGB ↔ XYZ-D65 (IEC 61966-2-1) ───────────────────────────

        private static (double X, double Y, double Z) LinSrgbToXyzD65(double r, double g, double b) =>
            (0.4124564 * r + 0.3575761 * g + 0.1804375 * b,
             0.2126729 * r + 0.7151522 * g + 0.0721750 * b,
             0.0193339 * r + 0.1191920 * g + 0.9503041 * b);

        private static (double R, double G, double B) XyzD65ToLinSrgb(double x, double y, double z) =>
            ( 3.2404542 * x - 1.5371385 * y - 0.4985314 * z,
             -0.9692660 * x + 1.8760108 * y + 0.0415560 * z,
              0.0556434 * x - 0.2040259 * y + 1.0572252 * z);

        // ── XYZ-D65 ↔ XYZ-D50 (Bradford) ────────────────────────────────────

        private static (double X, double Y, double Z) XyzD65ToXyzD50(double x, double y, double z) =>
            ( 1.0478112 * x + 0.0228866 * y - 0.0501270 * z,
              0.0295424 * x + 0.9904844 * y - 0.0170491 * z,
             -0.0092345 * x + 0.0150436 * y + 0.7521316 * z);

        private static (double X, double Y, double Z) XyzD50ToXyzD65(double x, double y, double z) =>
            ( 0.9554734 * x - 0.0230393 * y + 0.0631636 * z,
             -0.0282895 * x + 1.0099416 * y + 0.0210077 * z,
              0.0122982 * x - 0.0204830 * y + 1.3299098 * z);

        // ── Display-P3 ↔ XYZ-D65 (P3 uses sRGB gamma) ───────────────────────

        private static (double X, double Y, double Z) LinP3ToXyzD65(double r, double g, double b) =>
            (0.4865709 * r + 0.2656677 * g + 0.1982173 * b,
             0.2289746 * r + 0.6917386 * g + 0.0792869 * b,
             0.0000000 * r + 0.0451134 * g + 1.0439444 * b);

        private static (double R, double G, double B) XyzD65ToLinP3(double x, double y, double z) =>
            ( 2.4934969 * x - 0.9313836 * y - 0.4027108 * z,
             -0.8294890 * x + 1.7626641 * y + 0.0236247 * z,
              0.0358458 * x - 0.0761724 * y + 0.9568845 * z);

        // -- A98-RGB, ProPhoto RGB and Rec. 2020 (CSS Color 4, sections 10.7 - 10.9) --

        private static readonly double[] A98ToXyzD65 =
        {
            0.5766690429101305, 0.1855582379065463, 0.1882286462349947,
            0.29734497525053605, 0.6273635662554661, 0.07529145849399788,
            0.02703136138641234, 0.07068885253582723, 0.9913375368376388
        };

        private static readonly double[] ProphotoToXyzD50 =
        {
            0.7977604896723027, 0.13518583717574031, 0.0313493495815248,
            0.2880711282292934, 0.7118432178101014, 0.00008565396060525902,
            0.0, 0.0, 0.8251046025104601
        };

        private static readonly double[] Rec2020ToXyzD65 =
        {
            0.636958048301291, 0.144616903586208, 0.168880975164172,
            0.262700212011267, 0.677998071518871, 0.059301716469862,
            0.0, 0.028072693049087, 1.060985057710791
        };

        private static readonly double[] XyzD65ToA98 = Invert3(A98ToXyzD65);
        private static readonly double[] XyzD50ToProphoto = Invert3(ProphotoToXyzD50);
        private static readonly double[] XyzD65ToRec2020 = Invert3(Rec2020ToXyzD65);

        private static double[] Invert3(double[] m)
        {
            var c00 = m[4] * m[8] - m[5] * m[7];
            var c01 = m[5] * m[6] - m[3] * m[8];
            var c02 = m[3] * m[7] - m[4] * m[6];
            var det = m[0] * c00 + m[1] * c01 + m[2] * c02;
            return new[]
            {
                c00 / det, (m[2] * m[7] - m[1] * m[8]) / det, (m[1] * m[5] - m[2] * m[4]) / det,
                c01 / det, (m[0] * m[8] - m[2] * m[6]) / det, (m[2] * m[3] - m[0] * m[5]) / det,
                c02 / det, (m[1] * m[6] - m[0] * m[7]) / det, (m[0] * m[4] - m[1] * m[3]) / det
            };
        }

        private static (double, double, double) Multiply(double[] m, double x, double y, double z) =>
            (m[0] * x + m[1] * y + m[2] * z, m[3] * x + m[4] * y + m[5] * z, m[6] * x + m[7] * y + m[8] * z);

        private static double A98ToLinear(double c) => Math.Sign(c) * Math.Pow(Math.Abs(c), 563.0 / 256.0);

        private static double A98FromLinear(double c) => Math.Sign(c) * Math.Pow(Math.Abs(c), 256.0 / 563.0);

        private static double ProphotoToLinear(double c) => Math.Abs(c) <= 16.0 / 512.0 ? c / 16.0 : Math.Sign(c) * Math.Pow(Math.Abs(c), 1.8);

        private static double ProphotoFromLinear(double c) => Math.Abs(c) < 1.0 / 512.0 ? c * 16.0 : Math.Sign(c) * Math.Pow(Math.Abs(c), 1.0 / 1.8);

        private const double Rec2020Alpha = 1.09929682680944, Rec2020Beta = 0.018053968510807;

        private static double Rec2020ToLinear(double c) =>
            Math.Abs(c) < Rec2020Beta * 4.5 ? c / 4.5 : Math.Sign(c) * Math.Pow((Math.Abs(c) + Rec2020Alpha - 1) / Rec2020Alpha, 1 / 0.45);

        private static double Rec2020FromLinear(double c) =>
            Math.Abs(c) < Rec2020Beta ? c * 4.5 : Math.Sign(c) * (Rec2020Alpha * Math.Pow(Math.Abs(c), 0.45) - (Rec2020Alpha - 1));

        // One of the three spaces above, as XYZ-D65 and back, with no clip to any gamut.
        private static (double X, double Y, double Z) ExtraToXyzD65(Space cs, double[] v)
        {
            switch (cs)
            {
                case Space.A98Rgb:
                    return Multiply(A98ToXyzD65, A98ToLinear(v[0]), A98ToLinear(v[1]), A98ToLinear(v[2]));
                case Space.ProphotoRgb:
                {
                    var (x, y, z) = Multiply(ProphotoToXyzD50, ProphotoToLinear(v[0]), ProphotoToLinear(v[1]), ProphotoToLinear(v[2]));
                    return XyzD50ToXyzD65(x, y, z);
                }
                default:
                    return Multiply(Rec2020ToXyzD65, Rec2020ToLinear(v[0]), Rec2020ToLinear(v[1]), Rec2020ToLinear(v[2]));
            }
        }

        private static double[] ExtraFromXyzD65(Space cs, double x, double y, double z)
        {
            switch (cs)
            {
                case Space.A98Rgb:
                {
                    var (r, g, b) = Multiply(XyzD65ToA98, x, y, z);
                    return new[] { A98FromLinear(r), A98FromLinear(g), A98FromLinear(b) };
                }
                case Space.ProphotoRgb:
                {
                    var (x50, y50, z50) = XyzD65ToXyzD50(x, y, z);
                    var (r, g, b) = Multiply(XyzD50ToProphoto, x50, y50, z50);
                    return new[] { ProphotoFromLinear(r), ProphotoFromLinear(g), ProphotoFromLinear(b) };
                }
                default:
                {
                    var (r, g, b) = Multiply(XyzD65ToRec2020, x, y, z);
                    return new[] { Rec2020FromLinear(r), Rec2020FromLinear(g), Rec2020FromLinear(b) };
                }
            }
        }

        // ── CIE L*a*b* ────────────────────────────────────────────────────────

        private const double Xn50 = 0.96422, Yn50 = 1.0, Zn50 = 0.82521;

        private static double LabF(double t)
        {
            const double d = 6.0 / 29.0;
            return t > d * d * d ? Cbrt(t) : t / (3 * d * d) + 4.0 / 29.0;
        }

        private static double LabFInv(double t)
        {
            const double d = 6.0 / 29.0;
            return t > d ? t * t * t : 3 * d * d * (t - 4.0 / 29.0);
        }

        private static (double L, double a, double b) XyzD50ToLab(double x, double y, double z)
        {
            double fx = LabF(x / Xn50), fy = LabF(y / Yn50), fz = LabF(z / Zn50);
            return (116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz));
        }

        private static (double X, double Y, double Z) LabToXyzD50(double L, double a, double b)
        {
            var fy = (L + 16) / 116;
            return (Xn50 * LabFInv(a / 500 + fy), Yn50 * LabFInv(fy), Zn50 * LabFInv(fy - b / 200));
        }

        // ── OKLab (Björn Ottosson) ────────────────────────────────────────────

        private static (double L, double a, double b) XyzD65ToOkLab(double x, double y, double z)
        {
            var l = Cbrt(0.8189330101 * x + 0.3618667424 * y - 0.1288597137 * z);
            var m = Cbrt(0.0329845436 * x + 0.9293118715 * y + 0.0361456387 * z);
            var s = Cbrt(0.0482003018 * x + 0.2643662691 * y + 0.6338517070 * z);
            return (0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
                    1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
                    0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s);
        }

        private static (double X, double Y, double Z) OkLabToXyzD65(double L, double a, double b)
        {
            double l = L + 0.3963377774 * a + 0.2158037573 * b; l = l * l * l;
            double m = L - 0.1055613458 * a - 0.0638541728 * b; m = m * m * m;
            double s = L - 0.0894841775 * a - 1.2914855480 * b; s = s * s * s;
            return ( 1.2270138511 * l - 0.5577999807 * m + 0.2812561490 * s,
                    -0.0405801784 * l + 1.1122568696 * m - 0.0716766787 * s,
                    -0.0763812845 * l - 0.4214819784 * m + 1.5861632204 * s);
        }

        // ── HSL ───────────────────────────────────────────────────────────────

        private static (double H, double S, double L) SrgbToHsl(double r, double g, double b)
        {
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double d = max - min;
            double l = (max + min) / 2;
            if (d < 1e-10) return (0, 0, l * 100);
            double s = d / (1 - Math.Abs(2 * l - 1));
            double h;
            if (max == r)      h = 60 * (((g - b) / d) % 6);
            else if (max == g) h = 60 * ((b - r) / d + 2);
            else               h = 60 * ((r - g) / d + 4);
            if (h < 0) h += 360;
            return (h, s * 100, l * 100);
        }

        private static (double R, double G, double B) HslToSrgb(double h, double s, double l)
        {
            s /= 100; l /= 100;
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
            double m = l - c / 2;
            double r = 0, g = 0, b = 0;
            switch ((int)(h / 60) % 6)
            {
                case 0: r = c; g = x; break;
                case 1: r = x; g = c; break;
                case 2: g = c; b = x; break;
                case 3: g = x; b = c; break;
                case 4: r = x; b = c; break;
                case 5: r = c; b = x; break;
            }
            return (r + m, g + m, b + m);
        }

        // ── HWB ───────────────────────────────────────────────────────────────

        private static (double H, double W, double Bk) SrgbToHwb(double r, double g, double b)
        {
            var (h, _, _) = SrgbToHsl(r, g, b);
            return (h,
                    Math.Min(r, Math.Min(g, b)) * 100,
                    (1 - Math.Max(r, Math.Max(g, b))) * 100);
        }

        private static (double R, double G, double B) HwbToSrgb(double h, double w, double bk)
        {
            w /= 100; bk /= 100;
            if (w + bk >= 1) { double grey = w / (w + bk); return (grey, grey, grey); }
            var (r, g, b) = HslToSrgb(h, 100, 50);
            double f = 1 - w - bk;
            return (r * f + w, g * f + w, b * f + w);
        }

        // ── LCH / OKLch (rectangular ↔ polar) ────────────────────────────────

        private static (double L, double C, double H) RectToPolar(double L, double a, double b)
        {
            double c = Math.Sqrt(a * a + b * b);
            double h = Math.Atan2(b, a) * (180.0 / Math.PI);
            if (h < 0) h += 360;
            return (L, c, h);
        }

        private static (double L, double a, double b) PolarToRect(double L, double c, double h)
        {
            double rad = h * (Math.PI / 180.0);
            return (L, c * Math.Cos(rad), c * Math.Sin(rad));
        }

        // ── Forward: gamma-sRGB (0..1) → color-space coordinates ─────────────

        public static double[] ToSpace(double r, double g, double b, Space cs)
        {
            switch (cs)
            {
                case Space.SrgbLinear:
                    return new[] { Linearize(r), Linearize(g), Linearize(b) };

                case Space.DisplayP3:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var (pr, pg, pb) = XyzD65ToLinP3(x, y, z);
                    return new[] { Delinearize(pr), Delinearize(pg), Delinearize(pb) };
                }

                case Space.Lab:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var (x50, y50, z50) = XyzD65ToXyzD50(x, y, z);
                    var (L, a, bb) = XyzD50ToLab(x50, y50, z50);
                    return new[] { L, a, bb };
                }

                case Space.Oklab:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var (L, a, bb) = XyzD65ToOkLab(x, y, z);
                    return new[] { L, a, bb };
                }

                case Space.A98Rgb:
                case Space.ProphotoRgb:
                case Space.Rec2020:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    return ExtraFromXyzD65(cs, x, y, z);
                }

                case Space.XyzD65:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    return new[] { x, y, z };
                }

                case Space.XyzD50:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var (x50, y50, z50) = XyzD65ToXyzD50(x, y, z);
                    return new[] { x50, y50, z50 };
                }

                case Space.Hsl:
                {
                    var (h, s, l) = SrgbToHsl(r, g, b);
                    return new[] { h, s, l };
                }

                case Space.Hwb:
                {
                    var (h, w, bk) = SrgbToHwb(r, g, b);
                    return new[] { h, w, bk };
                }

                case Space.Lch:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var (x50, y50, z50) = XyzD65ToXyzD50(x, y, z);
                    var (L, a, bb) = XyzD50ToLab(x50, y50, z50);
                    var (lL, lC, lH) = RectToPolar(L, a, bb);
                    return new[] { lL, lC, lH };
                }

                case Space.Oklch:
                {
                    var (x, y, z) = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var (L, a, bb) = XyzD65ToOkLab(x, y, z);
                    var (oL, oC, oH) = RectToPolar(L, a, bb);
                    return new[] { oL, oC, oH };
                }

                default: // Srgb
                    return new[] { r, g, b };
            }
        }

        // -- Color-space coordinates <-> Oklab, without going through (clipped) sRGB --
        //
        // The Lab and Oklab families can name colors outside the sRGB gamut; ToSpace(FromSpace(v)) would clip
        // them first. ToOklab converts straight through XYZ, so an out-of-gamut color keeps the Oklab coordinates
        // it was written with (a mix of two colors, CSS Color 4 section 13, is made on those). So do the spaces a mix result
        // can leave the sRGB gamut in (sRGB, linear sRGB, Display P3, the other wide-gamut RGB spaces, XYZ); hsl and hwb, which hold no such color, go
        // through the clipped sRGB.

        public static double[] ToOklab(double[] v, Space cs)
        {
            switch (cs)
            {
                case Space.Oklab:
                    return new[] { v[0], v[1], v[2] };

                case Space.Oklch:
                {
                    var (l, a, b) = PolarToRect(v[0], v[1], v[2]);
                    return new[] { l, a, b };
                }

                case Space.Lab:
                case Space.Lch:
                {
                    var (l, a, b) = cs == Space.Lch ? PolarToRect(v[0], v[1], v[2]) : (v[0], v[1], v[2]);
                    var (x50, y50, z50) = LabToXyzD50(l, a, b);
                    var (x, y, z) = XyzD50ToXyzD65(x50, y50, z50);
                    var (ol, oa, ob) = XyzD65ToOkLab(x, y, z);
                    return new[] { ol, oa, ob };
                }

                case Space.Srgb:
                case Space.SrgbLinear:
                case Space.DisplayP3:
                {
                    (double, double, double) xyz;
                    if (cs == Space.Srgb) xyz = LinSrgbToXyzD65(LinearizeExtended(v[0]), LinearizeExtended(v[1]), LinearizeExtended(v[2]));
                    else if (cs == Space.SrgbLinear) xyz = LinSrgbToXyzD65(v[0], v[1], v[2]);
                    else xyz = LinP3ToXyzD65(LinearizeExtended(v[0]), LinearizeExtended(v[1]), LinearizeExtended(v[2]));
                    var (ol, oa, ob) = XyzD65ToOkLab(xyz.Item1, xyz.Item2, xyz.Item3);
                    return new[] { ol, oa, ob };
                }

                case Space.A98Rgb:
                case Space.ProphotoRgb:
                case Space.Rec2020:
                {
                    var (xs, ys, zs) = ExtraToXyzD65(cs, v);
                    var (ol, oa, ob) = XyzD65ToOkLab(xs, ys, zs);
                    return new[] { ol, oa, ob };
                }

                case Space.XyzD65:
                case Space.XyzD50:
                {
                    var (xs, ys, zs) = cs == Space.XyzD65 ? (v[0], v[1], v[2]) : XyzD50ToXyzD65(v[0], v[1], v[2]);
                    var (ol, oa, ob) = XyzD65ToOkLab(xs, ys, zs);
                    return new[] { ol, oa, ob };
                }

                default:
                {
                    var (r, g, b) = FromSpace(v, cs);
                    return ToSpace(r, g, b, Space.Oklab);
                }
            }
        }

        // The inverse of ToOklab for a color that may lie outside the sRGB gamut (a color-mix() operand written
        // as a wide lab()/oklch()): the coordinates in cs keep their out-of-range values, so the mix is made on
        // them and the clip comes once, at the end. hsl and hwb get the clipped color (a gamut-mapped one has no
        // meaning there).

        public static double[] ToSpaceFromOklab(double[] oklab, Space cs)
        {
            switch (cs)
            {
                case Space.Oklab:
                    return new[] { oklab[0], oklab[1], oklab[2] };

                case Space.Oklch:
                {
                    var (l, c, h) = RectToPolar(oklab[0], oklab[1], oklab[2]);
                    return new[] { l, c, h };
                }

                case Space.Lab:
                case Space.Lch:
                {
                    var (x, y, z) = OkLabToXyzD65(oklab[0], oklab[1], oklab[2]);
                    var (x50, y50, z50) = XyzD65ToXyzD50(x, y, z);
                    var (l, a, b) = XyzD50ToLab(x50, y50, z50);

                    // The Oklab matrices do not quite share lab's white, so an exact Oklab gray comes out of lab with a chroma of up to 0.033: it stays a gray.
                    if (Math.Sqrt(oklab[1] * oklab[1] + oklab[2] * oklab[2]) < 5e-5) a = b = 0;
                    if (cs == Space.Lab) return new[] { l, a, b };
                    var (pl, pc, ph) = RectToPolar(l, a, b);
                    return new[] { pl, pc, ph };
                }

                case Space.XyzD65:
                {
                    var (x, y, z) = OkLabToXyzD65(oklab[0], oklab[1], oklab[2]);
                    return new[] { x, y, z };
                }

                case Space.XyzD50:
                {
                    var (x, y, z) = OkLabToXyzD65(oklab[0], oklab[1], oklab[2]);
                    var (x50, y50, z50) = XyzD65ToXyzD50(x, y, z);
                    return new[] { x50, y50, z50 };
                }

                case Space.Srgb:
                case Space.SrgbLinear:
                {
                    var (x, y, z) = OkLabToXyzD65(oklab[0], oklab[1], oklab[2]);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    return cs == Space.SrgbLinear
                        ? new[] { rl, gl, bl }
                        : new[] { DelinearizeExtended(rl), DelinearizeExtended(gl), DelinearizeExtended(bl) };
                }

                case Space.DisplayP3:
                {
                    var (x, y, z) = OkLabToXyzD65(oklab[0], oklab[1], oklab[2]);
                    var (pr, pg, pb) = XyzD65ToLinP3(x, y, z);
                    return new[] { DelinearizeExtended(pr), DelinearizeExtended(pg), DelinearizeExtended(pb) };
                }

                case Space.A98Rgb:
                case Space.ProphotoRgb:
                case Space.Rec2020:
                {
                    var (x, y, z) = OkLabToXyzD65(oklab[0], oklab[1], oklab[2]);
                    return ExtraFromXyzD65(cs, x, y, z);
                }

                default:
                {
                    var (r, g, b) = FromSpace(oklab, Space.Oklab);
                    return ToSpace(r, g, b, cs);
                }
            }
        }

        // The sRGB transfer functions extended past 0..1 by odd symmetry, as CSS Color 4 section 10.2 does.
        private static double LinearizeExtended(double c)
        {
            var magnitude = Math.Abs(c);
            var linear = magnitude <= 0.04045 ? magnitude / 12.92 : Math.Pow((magnitude + 0.055) / 1.055, 2.4);
            return c < 0 ? -linear : linear;
        }

        private static double DelinearizeExtended(double c)
        {
            var magnitude = Math.Abs(c);
            var encoded = magnitude <= 0.0031308 ? magnitude * 12.92 : 1.055 * Math.Pow(magnitude, 1.0 / 2.4) - 0.055;
            return c < 0 ? -encoded : encoded;
        }

        // ── Reverse: color-space coordinates → gamut-mapped sRGB (0..1) ──────

        public static (double R, double G, double B) FromSpace(double[] v, Space cs)
        {
            double r, g, b;

            switch (cs)
            {
                case Space.SrgbLinear:
                    r = Delinearize(v[0]); g = Delinearize(v[1]); b = Delinearize(v[2]);
                    break;

                case Space.DisplayP3:
                {
                    var (x, y, z) = LinP3ToXyzD65(Linearize(v[0]), Linearize(v[1]), Linearize(v[2]));
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.Lab:
                {
                    var (x50, y50, z50) = LabToXyzD50(v[0], v[1], v[2]);
                    var (x, y, z) = XyzD50ToXyzD65(x50, y50, z50);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.Oklab:
                {
                    var (x, y, z) = OkLabToXyzD65(v[0], v[1], v[2]);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.XyzD65:
                {
                    var (rl, gl, bl) = XyzD65ToLinSrgb(v[0], v[1], v[2]);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.XyzD50:
                {
                    var (x, y, z) = XyzD50ToXyzD65(v[0], v[1], v[2]);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.A98Rgb:
                case Space.ProphotoRgb:
                case Space.Rec2020:
                {
                    var (x, y, z) = ExtraToXyzD65(cs, v);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.Hsl:
                    (r, g, b) = HslToSrgb(v[0], v[1], v[2]);
                    break;

                case Space.Hwb:
                    (r, g, b) = HwbToSrgb(v[0], v[1], v[2]);
                    break;

                case Space.Lch:
                {
                    var (L, a, bb) = PolarToRect(v[0], v[1], v[2]);
                    var (x50, y50, z50) = LabToXyzD50(L, a, bb);
                    var (x, y, z) = XyzD50ToXyzD65(x50, y50, z50);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                case Space.Oklch:
                {
                    var (L, a, bb) = PolarToRect(v[0], v[1], v[2]);
                    var (x, y, z) = OkLabToXyzD65(L, a, bb);
                    var (rl, gl, bl) = XyzD65ToLinSrgb(x, y, z);
                    r = Delinearize(rl); g = Delinearize(gl); b = Delinearize(bl);
                    break;
                }

                default: // Srgb
                    r = v[0]; g = v[1]; b = v[2];
                    break;
            }

            return (Clamp01(r), Clamp01(g), Clamp01(b));
        }
    }
}
