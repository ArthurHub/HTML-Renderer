using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The single home for CSS color-space conversion math (CSS Color 4/5): forward/reverse transforms
    /// between gamma-encoded sRGB and every interpolation space, plus polar-hue interpolation. Kept in
    /// the CSS-OM (rather than the render layer) so that solid-color resolution (<see cref="Color"/>'s
    /// <c>oklch()</c>/<c>lab()</c>/<c>color-mix()</c> factories) and any gradient interpolation that
    /// needs a non-sRGB space share one implementation.
    /// </summary>
    internal static class ColorSpaceMath
    {
        internal enum Space { Srgb, SrgbLinear, DisplayP3, Lab, Oklab, XyzD65, XyzD50, Hsl, Hwb, Lch, Oklch }

        internal enum HueMethod { Shorter, Longer, Increasing, Decreasing }

        public static bool IsPolar(Space cs)
        {
            return cs == Space.Hsl || cs == Space.Hwb || cs == Space.Lch || cs == Space.Oklch;
        }

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

        public static double NormalizeHue(double h)
        {
            return ((h % 360) + 360) % 360;
        }

        public static double InterpolateHue(double h1, double h2, double t, HueMethod hue)
        {
            h1 = NormalizeHue(h1);
            h2 = NormalizeHue(h2);
            var diff = h2 - h1;
            switch (hue)
            {
                case HueMethod.Longer:
                    if (diff > 0 && diff < 180) diff -= 360;
                    else if (diff < 0 && diff > -180) diff += 360;
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
            return h1 + t * diff;
        }

        private static double Clamp01(double v)
        {
            return v < 0 ? 0 : (v > 1 ? 1 : v);
        }

        // Cube root that accepts negative input, for targets whose Math has no Cbrt.
        private static double Cbrt(double v)
        {
            return v < 0 ? -Math.Pow(-v, 1.0 / 3.0) : Math.Pow(v, 1.0 / 3.0);
        }

        // sRGB gamma

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

        // Linear sRGB <-> XYZ-D65 (IEC 61966-2-1)

        private static (double X, double Y, double Z) LinSrgbToXyzD65(double r, double g, double b)
        {
            return (0.4124564 * r + 0.3575761 * g + 0.1804375 * b,
                    0.2126729 * r + 0.7151522 * g + 0.0721750 * b,
                    0.0193339 * r + 0.1191920 * g + 0.9503041 * b);
        }

        private static (double R, double G, double B) XyzD65ToLinSrgb(double x, double y, double z)
        {
            return ( 3.2404542 * x - 1.5371385 * y - 0.4985314 * z,
                    -0.9692660 * x + 1.8760108 * y + 0.0415560 * z,
                     0.0556434 * x - 0.2040259 * y + 1.0572252 * z);
        }

        // XYZ-D65 <-> XYZ-D50 (Bradford)

        private static (double X, double Y, double Z) XyzD65ToXyzD50(double x, double y, double z)
        {
            return ( 1.0478112 * x + 0.0228866 * y - 0.0501270 * z,
                     0.0295424 * x + 0.9904844 * y - 0.0170491 * z,
                    -0.0092345 * x + 0.0150436 * y + 0.7521316 * z);
        }

        private static (double X, double Y, double Z) XyzD50ToXyzD65(double x, double y, double z)
        {
            return ( 0.9554734 * x - 0.0230393 * y + 0.0631636 * z,
                    -0.0282895 * x + 1.0099416 * y + 0.0210077 * z,
                     0.0122982 * x - 0.0204830 * y + 1.3299098 * z);
        }

        // Display-P3 <-> XYZ-D65 (P3 uses sRGB gamma)

        private static (double X, double Y, double Z) LinP3ToXyzD65(double r, double g, double b)
        {
            return (0.4865709 * r + 0.2656677 * g + 0.1982173 * b,
                    0.2289746 * r + 0.6917386 * g + 0.0792869 * b,
                    0.0000000 * r + 0.0451134 * g + 1.0439444 * b);
        }

        private static (double R, double G, double B) XyzD65ToLinP3(double x, double y, double z)
        {
            return ( 2.4934969 * x - 0.9313836 * y - 0.4027108 * z,
                    -0.8294890 * x + 1.7626641 * y + 0.0236247 * z,
                     0.0358458 * x - 0.0761724 * y + 0.9568845 * z);
        }

        // CIE L*a*b*

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

        // OKLab (Björn Ottosson)

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

        // HSL

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

        // HWB

        private static (double H, double W, double Bk) SrgbToHwb(double r, double g, double b)
        {
            var hsl = SrgbToHsl(r, g, b);
            return (hsl.H,
                    Math.Min(r, Math.Min(g, b)) * 100,
                    (1 - Math.Max(r, Math.Max(g, b))) * 100);
        }

        private static (double R, double G, double B) HwbToSrgb(double h, double w, double bk)
        {
            w /= 100; bk /= 100;
            if (w + bk >= 1) { double grey = w / (w + bk); return (grey, grey, grey); }
            var rgb = HslToSrgb(h, 100, 50);
            double f = 1 - w - bk;
            return (rgb.R * f + w, rgb.G * f + w, rgb.B * f + w);
        }

        // LCH / OKLch (rectangular <-> polar)

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

        // Forward: gamma-sRGB (0..1) -> color-space coordinates

        public static double[] ToSpace(double r, double g, double b, Space cs)
        {
            switch (cs)
            {
                case Space.SrgbLinear:
                    return new[] { Linearize(r), Linearize(g), Linearize(b) };

                case Space.DisplayP3:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var p3 = XyzD65ToLinP3(xyz.X, xyz.Y, xyz.Z);
                    return new[] { Delinearize(p3.R), Delinearize(p3.G), Delinearize(p3.B) };
                }

                case Space.Lab:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var xyz50 = XyzD65ToXyzD50(xyz.X, xyz.Y, xyz.Z);
                    var lab = XyzD50ToLab(xyz50.X, xyz50.Y, xyz50.Z);
                    return new[] { lab.L, lab.a, lab.b };
                }

                case Space.Oklab:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var lab = XyzD65ToOkLab(xyz.X, xyz.Y, xyz.Z);
                    return new[] { lab.L, lab.a, lab.b };
                }

                case Space.XyzD65:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    return new[] { xyz.X, xyz.Y, xyz.Z };
                }

                case Space.XyzD50:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var xyz50 = XyzD65ToXyzD50(xyz.X, xyz.Y, xyz.Z);
                    return new[] { xyz50.X, xyz50.Y, xyz50.Z };
                }

                case Space.Hsl:
                {
                    var hsl = SrgbToHsl(r, g, b);
                    return new[] { hsl.H, hsl.S, hsl.L };
                }

                case Space.Hwb:
                {
                    var hwb = SrgbToHwb(r, g, b);
                    return new[] { hwb.H, hwb.W, hwb.Bk };
                }

                case Space.Lch:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var xyz50 = XyzD65ToXyzD50(xyz.X, xyz.Y, xyz.Z);
                    var lab = XyzD50ToLab(xyz50.X, xyz50.Y, xyz50.Z);
                    var lch = RectToPolar(lab.L, lab.a, lab.b);
                    return new[] { lch.L, lch.C, lch.H };
                }

                case Space.Oklch:
                {
                    var xyz = LinSrgbToXyzD65(Linearize(r), Linearize(g), Linearize(b));
                    var lab = XyzD65ToOkLab(xyz.X, xyz.Y, xyz.Z);
                    var lch = RectToPolar(lab.L, lab.a, lab.b);
                    return new[] { lch.L, lch.C, lch.H };
                }

                default: // Srgb
                    return new[] { r, g, b };
            }
        }

        // Reverse: color-space coordinates -> gamut-mapped sRGB (0..1)

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
                    var xyz = LinP3ToXyzD65(Linearize(v[0]), Linearize(v[1]), Linearize(v[2]));
                    var lin = XyzD65ToLinSrgb(xyz.X, xyz.Y, xyz.Z);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
                    break;
                }

                case Space.Lab:
                {
                    var xyz50 = LabToXyzD50(v[0], v[1], v[2]);
                    var xyz = XyzD50ToXyzD65(xyz50.X, xyz50.Y, xyz50.Z);
                    var lin = XyzD65ToLinSrgb(xyz.X, xyz.Y, xyz.Z);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
                    break;
                }

                case Space.Oklab:
                {
                    var xyz = OkLabToXyzD65(v[0], v[1], v[2]);
                    var lin = XyzD65ToLinSrgb(xyz.X, xyz.Y, xyz.Z);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
                    break;
                }

                case Space.XyzD65:
                {
                    var lin = XyzD65ToLinSrgb(v[0], v[1], v[2]);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
                    break;
                }

                case Space.XyzD50:
                {
                    var xyz = XyzD50ToXyzD65(v[0], v[1], v[2]);
                    var lin = XyzD65ToLinSrgb(xyz.X, xyz.Y, xyz.Z);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
                    break;
                }

                case Space.Hsl:
                {
                    var rgb = HslToSrgb(v[0], v[1], v[2]);
                    r = rgb.R; g = rgb.G; b = rgb.B;
                    break;
                }

                case Space.Hwb:
                {
                    var rgb = HwbToSrgb(v[0], v[1], v[2]);
                    r = rgb.R; g = rgb.G; b = rgb.B;
                    break;
                }

                case Space.Lch:
                {
                    var lab = PolarToRect(v[0], v[1], v[2]);
                    var xyz50 = LabToXyzD50(lab.L, lab.a, lab.b);
                    var xyz = XyzD50ToXyzD65(xyz50.X, xyz50.Y, xyz50.Z);
                    var lin = XyzD65ToLinSrgb(xyz.X, xyz.Y, xyz.Z);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
                    break;
                }

                case Space.Oklch:
                {
                    var lab = PolarToRect(v[0], v[1], v[2]);
                    var xyz = OkLabToXyzD65(lab.L, lab.a, lab.b);
                    var lin = XyzD65ToLinSrgb(xyz.X, xyz.Y, xyz.Z);
                    r = Delinearize(lin.R); g = Delinearize(lin.G); b = Delinearize(lin.B);
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
