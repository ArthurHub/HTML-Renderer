using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global


namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Unicode)]
    internal struct Color : IEquatable<Color>, IComparable<Color>, IFormattable
    {
        [FieldOffset(0)] private readonly byte _alpha;
        [FieldOffset(1)] private readonly byte _red;
        [FieldOffset(2)] private readonly byte _green;
        [FieldOffset(3)] private readonly byte _blue;
        [FieldOffset(0)] private readonly int _hashcode;


        #region Basic colors

        /// <summary>
        ///      #000000.
        /// </summary>
        public static readonly Color Black = new Color(0, 0, 0);

        /// <summary>
        ///      #FFFFFF.
        /// </summary>
        public static readonly Color White = new Color(255, 255, 255);

        /// <summary>
        ///      #FF0000.
        /// </summary>
        public static readonly Color Red = new Color(255, 0, 0);

        /// <summary>
        ///      #FF00FF.
        /// </summary>
        public static readonly Color Magenta = new Color(255, 0, 255);

        /// <summary>
        ///      #008000.
        /// </summary>
        public static readonly Color Green = new Color(0, 128, 0);

        /// <summary>
        ///      #00FF00.
        /// </summary>
        public static readonly Color PureGreen = new Color(0, 255, 0);

        /// <summary>
        ///      #0000FF.
        /// </summary>
        public static readonly Color Blue = new Color(0, 0, 255);

        /// <summary>
        ///      #00000000.
        /// </summary>
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        #endregion

        public Color(byte r, byte g, byte b)
        {
            _hashcode = 0;
            _alpha = 255;
            _red = r;
            _blue = b;
            _green = g;
        }

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            _hashcode = 0;
            _alpha = alpha;
            _red = red;
            _blue = blue;
            _green = green;
        }

        public static Color FromRgba(byte red, byte green, byte blue, float alpha)
        {
            return new Color(red, green, blue, Normalize(alpha));
        }

        public static Color FromRgba(float red, float green, float blue, float alpha)
        {
            return new Color(Normalize(red), Normalize(green), Normalize(blue), Normalize(alpha));
        }

        public static Color FromGray(byte number, float alpha = 1f)
        {
            return new Color(number, number, number, Normalize(alpha));
        }

        public static Color FromGray(float value, float alpha = 1f)
        {
            return FromGray(Normalize(value), alpha);
        }

        public static Color? FromName(string name)
        {
            return Colors.GetColor(name);
        }

        public static Color FromRgb(byte red, byte green, byte blue)
        {
            return new Color(red, green, blue);
        }

        public static Color FromHex(string color)
        {
            int r = 0, g = 0, b = 0, a = 255;

            switch (color.Length)
            {
                case 4:
                    a = 17 * color[3].FromHex();
                    goto case 3;
                case 3:
                    r = 17 * color[0].FromHex();
                    g = 17 * color[1].FromHex();
                    b = 17 * color[2].FromHex();
                    break;
                case 8:
                    a = 16 * color[6].FromHex() + color[7].FromHex();
                    goto case 6;
                case 6:
                    r = 16 * color[0].FromHex() + color[1].FromHex();
                    g = 16 * color[2].FromHex() + color[3].FromHex();
                    b = 16 * color[4].FromHex() + color[5].FromHex();
                    break;
            }

            return new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static bool TryFromHex(string color, out Color value)
        {
            if (color.Length == 6 || color.Length == 3 || color.Length == 8 || color.Length == 4)
            {
                for (var i = 0; i < color.Length; i++)
                    if (!color[i].IsHex())
                        goto fail;

                value = FromHex(color);
                return true;
            }

            fail:
            value = new Color();
            return false;
        }

        public static Color FromFlexHex(string color)
        {
            var length = Math.Max(color.Length, 3);
            var remaining = length % 3;

            if (remaining != 0) length += 3 - remaining;

            var n = length / 3;
            var d = Math.Min(2, n);
            var s = Math.Max(n - 8, 0);
            var chars = new char[length];

            for (var i = 0; i < color.Length; i++) chars[i] = color[i].IsHex() ? color[i] : '0';

            for (var i = color.Length; i < length; i++) chars[i] = '0';

            if (d == 1)
            {
                var r = chars[0 * n + s].FromHex();
                var g = chars[1 * n + s].FromHex();
                var b = chars[2 * n + s].FromHex();
                return new Color((byte)r, (byte)g, (byte)b);
            }
            else
            {
                var r = 16 * chars[0 * n + s].FromHex() + chars[0 * n + s + 1].FromHex();
                var g = 16 * chars[1 * n + s].FromHex() + chars[1 * n + s + 1].FromHex();
                var b = 16 * chars[2 * n + s].FromHex() + chars[2 * n + s + 1].FromHex();
                return new Color((byte)r, (byte)g, (byte)b);
            }
        }

        public static Color FromHsl(float hue, float saturation, float luminosity)
        {
            return FromHsla(hue, saturation, luminosity, 1f);
        }

        public static Color FromHsla(float hue, float saturation, float luminosity, float alpha)
        {
            const float third = 1f / 3f;

            var m2 = luminosity <= 0.5f
                ? luminosity * (saturation + 1f)
                : luminosity + saturation - luminosity * saturation;

            var m1 = 2f * luminosity - m2;
            var r = Convert(HueToRgb(m1, m2, hue + third));
            var g = Convert(HueToRgb(m1, m2, hue));
            var b = Convert(HueToRgb(m1, m2, hue - third));
            return new Color(r, g, b, Normalize(alpha));
        }

        public static Color FromHwb(float hue, float whiteness, float blackness)
        {
            return FromHwba(hue, whiteness, blackness, 1f);
        }

        public static Color FromHwba(float hue, float whiteness, float blackness, float alpha)
        {
            var ratio = 1f / (whiteness + blackness);
            if (ratio < 1f)
            {
                whiteness *= ratio;
                blackness *= ratio;
            }

            var p = (int)(6 * hue);
            var f = 6 * hue - p;

            if ((p & 0x01) != 0) f = 1 - f;

            var v = 1 - blackness;
            var n = whiteness + f * (v - whiteness);

            float red;
            float green;
            float blue;
            switch (p)
            {
                default:
                case 6:
                case 0:
                    red = v;
                    green = n;
                    blue = whiteness;
                    break;
                case 1:
                    red = n;
                    green = v;
                    blue = whiteness;
                    break;
                case 2:
                    red = whiteness;
                    green = v;
                    blue = n;
                    break;
                case 3:
                    red = whiteness;
                    green = n;
                    blue = v;
                    break;
                case 4:
                    red = n;
                    green = whiteness;
                    blue = v;
                    break;
                case 5:
                    red = v;
                    green = whiteness;
                    blue = n;
                    break;
            }

            return FromRgba(red, green, blue, alpha);
        }

        // CSS Color 4/5 spaces (lab/oklab/lch/oklch) and color-mix().
        // Components are in each space's native units (L: lab/lch 0..100, oklab/oklch 0..1; a/b and C
        // in native chroma units; H in degrees). The conversion math is shared with gradient
        // interpolation via ColorSpaceMath so there is a single implementation of the color science.

        public static Color FromLab(double l, double a, double b, float alpha = 1f)
        {
            return FromSpaceComponents(new[] { l, a, b }, ColorSpaceMath.Space.Lab, alpha);
        }

        public static Color FromOklab(double l, double a, double b, float alpha = 1f)
        {
            return FromSpaceComponents(new[] { l, a, b }, ColorSpaceMath.Space.Oklab, alpha);
        }

        public static Color FromLch(double l, double c, double h, float alpha = 1f)
        {
            return FromSpaceComponents(new[] { l, c, h }, ColorSpaceMath.Space.Lch, alpha);
        }

        public static Color FromOklch(double l, double c, double h, float alpha = 1f)
        {
            return FromSpaceComponents(new[] { l, c, h }, ColorSpaceMath.Space.Oklch, alpha);
        }

        private static Color FromSpaceComponents(double[] v, ColorSpaceMath.Space cs, float alpha)
        {
            var rgb = ColorSpaceMath.FromSpace(v, cs);
            return new Color(ToByte(rgb.Item1), ToByte(rgb.Item2), ToByte(rgb.Item3), Normalize(alpha));
        }

        /// <summary>
        /// <see href="https://www.w3.org/TR/css-color-5/#color-mix">CSS Color 5 <c>color-mix()</c></see>:
        /// premultiplied-alpha interpolation in <paramref name="cs"/> with weight <paramref name="t"/>
        /// toward <paramref name="c2"/>, the result alpha then scaled by <paramref name="alphaMultiplier"/>
        /// (below 1 only when the two percentages summed to under 100%).
        /// </summary>
        public static Color Mix(Color c1, Color c2, double t, ColorSpaceMath.Space cs, ColorSpaceMath.HueMethod hue, double alphaMultiplier)
        {
            t = Math.Min(Math.Max(t, 0.0), 1.0);
            double a1 = c1.A / 255.0, a2 = c2.A / 255.0;

            var v1 = ColorSpaceMath.ToSpace(c1.R / 255.0, c1.G / 255.0, c1.B / 255.0, cs);
            var v2 = ColorSpaceMath.ToSpace(c2.R / 255.0, c2.G / 255.0, c2.B / 255.0, cs);
            var hueIndex = ColorSpaceMath.HueIndex(cs);

            for (var i = 0; i < 3; i++)
            {
                if (i == hueIndex) continue;
                v1[i] *= a1;
                v2[i] *= a2;
            }

            var mixedAlpha = a1 + t * (a2 - a1);
            var v = new double[3];
            for (var i = 0; i < 3; i++)
            {
                if (i == hueIndex)
                {
                    v[i] = ColorSpaceMath.InterpolateHue(v1[i], v2[i], t, hue);
                    continue;
                }

                var mixed = v1[i] + t * (v2[i] - v1[i]);
                v[i] = mixedAlpha > 1e-10 ? mixed / mixedAlpha : 0;
            }

            var rgb = ColorSpaceMath.FromSpace(v, cs);
            var outAlpha = (byte)Math.Round(Math.Min(Math.Max(mixedAlpha * alphaMultiplier, 0), 1) * 255);
            return new Color(ToByte(rgb.Item1), ToByte(rgb.Item2), ToByte(rgb.Item3), outAlpha);
        }

        private static byte ToByte(double v)
        {
            return (byte)Math.Round(Math.Min(Math.Max(v, 0), 1) * 255);
        }

        public int Value => _hashcode;
        public byte A => _alpha;
        public double Alpha => Math.Round(_alpha / 255.0, 2);
        public byte R => _red;
        public byte G => _green;
        public byte B => _blue;

        public static bool operator ==(Color a, Color b)
        {
            return a._hashcode == b._hashcode;
        }

        public static bool operator !=(Color a, Color b)
        {
            return a._hashcode != b._hashcode;
        }

        public bool Equals(Color other)
        {
            return _hashcode == other._hashcode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Color?;

            if (other != null) return Equals(other.Value);

            return false;
        }

        int IComparable<Color>.CompareTo(Color other)
        {
            return _hashcode - other._hashcode;
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public static Color Mix(Color above, Color below)
        {
            return Mix(above.Alpha, above, below);
        }

        public static Color Mix(double alpha, Color above, Color below)
        {
            var gamma = 1.0 - alpha;
            var r = gamma * below.R + alpha * above.R;
            var g = gamma * below.G + alpha * above.G;
            var b = gamma * below.B + alpha * above.B;
            return new Color((byte)r, (byte)g, (byte)b);
        }

        private static byte Normalize(float value)
        {
            return (byte)Math.Max(Math.Min(Math.Round(255 * value), 255), 0);
        }

        private static byte Convert(float value)
        {
            return (byte)Math.Round(255f * value);
        }

        private static float HueToRgb(float m1, float m2, float h)
        {
            const float oneSixth = 1f / 6f;
            const float twoThird = 2f / 3f;

            if (h < 0f)
                h += 1f;
            else if (h > 1f) h -= 1f;

            if (h < oneSixth) return m1 + (m2 - m1) * h * 6f;
            if (h < 0.5) return m2;
            if (h < twoThird) return m1 + (m2 - m1) * (twoThird - h) * 6f;

            return m1;
        }

        public override string ToString()
        {
            if (_alpha == 255)
            {
                var arguments = string.Join(", ", R.ToString(), G.ToString(), B.ToString());
                return FunctionNames.Rgb.StylesheetFunction(arguments);
            }
            else
            {
                var arguments = string.Join(", ", R.ToString(), G.ToString(), B.ToString(), Alpha.ToString());
                return FunctionNames.Rgba.StylesheetFunction(arguments);
            }
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (_alpha == 255)
            {
                var arguments = string.Join(", ", R.ToString(format, formatProvider),
                    G.ToString(format, formatProvider),
                    B.ToString(format, formatProvider));
                return FunctionNames.Rgb.StylesheetFunction(arguments);
            }
            else
            {
                var arguments = string.Join(", ", R.ToString(format, formatProvider),
                    G.ToString(format, formatProvider),
                    B.ToString(format, formatProvider), Alpha.ToString(format, formatProvider));
                return FunctionNames.Rgba.StylesheetFunction(arguments);
            }
        }
    }
}