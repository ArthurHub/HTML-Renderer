using System;
using System.Globalization;
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

        // CSS Color 5 device-cmyk(): a color authored that way is carried in its native components and is never
        // collapsed to an sRGB approximation (there is no profile-free RGB<->CMYK conversion that is not a guess).
        // _red/_green/_blue stay 0 for it and mean nothing; alpha is colorspace-independent and still lives in _alpha.
        [FieldOffset(4)] private readonly bool _isDeviceCmyk;
        [FieldOffset(8)] private readonly float _cyan;
        [FieldOffset(12)] private readonly float _magenta;
        [FieldOffset(16)] private readonly float _yellow;
        [FieldOffset(20)] private readonly float _key;


        #region Basic colors

        /// <summary>
        ///      #000000.
        /// </summary>
        public static readonly Color Black = new(0, 0, 0);

        /// <summary>
        ///      #FFFFFF.
        /// </summary>
        public static readonly Color White = new(255, 255, 255);

        /// <summary>
        ///      #FF0000.
        /// </summary>
        public static readonly Color Red = new(255, 0, 0);

        /// <summary>
        ///      #FF00FF.
        /// </summary>
        public static readonly Color Magenta = new(255, 0, 255);

        /// <summary>
        ///      #008000.
        /// </summary>
        public static readonly Color Green = new(0, 128, 0);

        /// <summary>
        ///      #00FF00.
        /// </summary>
        public static readonly Color PureGreen = new(0, 255, 0);

        /// <summary>
        ///      #0000FF.
        /// </summary>
        public static readonly Color Blue = new(0, 0, 255);

        /// <summary>
        ///      #00000000.
        /// </summary>
        public static readonly Color Transparent = new(0, 0, 0, 0);

        #endregion

        public Color(byte r, byte g, byte b)
        {
            _hashcode = 0;
            _alpha = 255;
            _red = r;
            _blue = b;
            _green = g;
            _isDeviceCmyk = false;
            _cyan = 0f;
            _magenta = 0f;
            _yellow = 0f;
            _key = 0f;
        }

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            _hashcode = 0;
            _alpha = alpha;
            _red = red;
            _blue = blue;
            _green = green;
            _isDeviceCmyk = false;
            _cyan = 0f;
            _magenta = 0f;
            _yellow = 0f;
            _key = 0f;
        }

        private Color(float cyan, float magenta, float yellow, float key, byte alpha)
        {
            _hashcode = 0;
            _alpha = alpha;
            _red = 0;
            _green = 0;
            _blue = 0;
            _isDeviceCmyk = true;
            _cyan = ClampUnit(cyan);
            _magenta = ClampUnit(magenta);
            _yellow = ClampUnit(yellow);
            _key = ClampUnit(key);
        }

        /// <summary>
        /// A <see href="https://www.w3.org/TR/css-color-5/#device-cmyk">CSS Color 5 <c>device-cmyk()</c></see> color, carried in its
        /// own components: <see cref="IsDeviceCmyk"/> is true and <see cref="C"/>/<see cref="M"/>/<see cref="Y"/>/<see cref="K"/> hold
        /// them, each clamped to 0..1. No sRGB value is computed for it (that needs an ICC profile), so <see cref="R"/>, <see cref="G"/>
        /// and <see cref="B"/> are 0 and must not be read for it. <paramref name="alpha"/> is 0..1, as in
        /// <see cref="FromRgba(float, float, float, float)"/>.
        /// </summary>
        public static Color FromDeviceCmyk(float cyan, float magenta, float yellow, float key, float alpha = 1f)
        {
            return new Color(cyan, magenta, yellow, key, Normalize(alpha));
        }

        private static float ClampUnit(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);

        public static Color FromRgba(byte red, byte green, byte blue, float alpha)
        {
            return new(red, green, blue, Normalize(alpha));
        }

        public static Color FromRgba(float red, float green, float blue, float alpha)
        {
            return new(Normalize(red), Normalize(green), Normalize(blue), Normalize(alpha));
        }

        public static Color FromGray(byte number, float alpha = 1f)
        {
            return new(number, number, number, Normalize(alpha));
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
            return new(red, green, blue);
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

            return new Color((byte) r, (byte) g, (byte) b, (byte) a);
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
                return new Color((byte) r, (byte) g, (byte) b);
            }
            else
            {
                var r = 16 * chars[0 * n + s].FromHex() + chars[0 * n + s + 1].FromHex();
                var g = 16 * chars[1 * n + s].FromHex() + chars[1 * n + s + 1].FromHex();
                var b = 16 * chars[2 * n + s].FromHex() + chars[2 * n + s + 1].FromHex();
                return new Color((byte) r, (byte) g, (byte) b);
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

            var p = (int) (6 * hue);
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

        // ── CSS Color 4/5 spaces (lab/oklab/lch/oklch) and color-mix() ────────
        // Components are in each space's native units (L: lab/lch 0..100, oklab/oklch 0..1; a/b and C
        // in native chroma units; H in degrees). The conversion math is shared with any future gradient
        // interpolation via ColorSpaceMath so there is a single implementation of the color science.

        public static Color FromLab(double l, double a, double b, float alpha = 1f)
            => FromSpaceComponents(new[] { l, a, b }, ColorSpaceMath.Space.Lab, alpha);

        public static Color FromOklab(double l, double a, double b, float alpha = 1f)
            => FromSpaceComponents(new[] { l, a, b }, ColorSpaceMath.Space.Oklab, alpha);

        public static Color FromLch(double l, double c, double h, float alpha = 1f)
            => FromSpaceComponents(new[] { l, c, h }, ColorSpaceMath.Space.Lch, alpha);

        public static Color FromOklch(double l, double c, double h, float alpha = 1f)
            => FromSpaceComponents(new[] { l, c, h }, ColorSpaceMath.Space.Oklch, alpha);

        internal static Color FromSpaceComponents(double[] v, ColorSpaceMath.Space cs, float alpha)
        {
            var (r, g, b) = ColorSpaceMath.FromSpace(v, cs);
            return new Color(ToByte(r), ToByte(g), ToByte(b), Normalize(alpha));
        }

        /// <summary>
        /// <see href="https://www.w3.org/TR/css-color-5/#color-mix">CSS Color 5 <c>color-mix()</c></see>:
        /// premultiplied-alpha interpolation in <paramref name="cs"/> with weight <paramref name="t"/>
        /// toward <paramref name="c2"/>, the result alpha then scaled by <paramref name="alphaMultiplier"/>
        /// (below 1 only when the two percentages summed to under 100%).
        /// </summary>
        internal static Color Mix(Color c1, Color c2, double t, ColorSpaceMath.Space cs, ColorSpaceMath.HueMethod hue, double alphaMultiplier,
            double[] oklab1 = null, double[] oklab2 = null, ColorSpaceMath.WrittenCoords? written1 = null, ColorSpaceMath.WrittenCoords? written2 = null)
        {
            var v = MixComponents(c1, c2, t, cs, hue, out var mixedAlpha, oklab1, oklab2, written1, written2);
            var (r, g, b) = ColorSpaceMath.FromSpace(v, cs);
            var outAlpha = (byte)Math.Round(ColorSpaceMath.Clamp01(mixedAlpha * alphaMultiplier) * 255);
            return new Color(ToByte(r), ToByte(g), ToByte(b), outAlpha);
        }

        // An operand's coordinates in the mix's space: as written when it was written in that space (or its polar/rectangular form), so a gray keeps the
        // hue it was written with; else from its Oklab coordinates when it lies outside the gamut its Color is clipped to; else from the Color.
        private static double[] Operand(Color c, double[] oklab, ColorSpaceMath.WrittenCoords? written, ColorSpaceMath.Space cs)
        {
            if (written.HasValue)
            {
                var asWritten = ColorSpaceMath.ToSpaceFromWritten(written.Value, cs);
                if (asWritten != null) return asWritten;
            }

            return oklab != null
                ? ColorSpaceMath.ToSpaceFromOklab(oklab, cs)
                : ColorSpaceMath.ToSpace(c.R / 255.0, c.G / 255.0, c.B / 255.0, cs);
        }

        /// <summary>
        /// The premultiplied mix <see cref="Mix(Color, Color, double, ColorSpaceMath.Space, ColorSpaceMath.HueMethod, double, double[], double[], ColorSpaceMath.WrittenCoords?, ColorSpaceMath.WrittenCoords?)"/>
        /// makes, as its coordinates in <paramref name="cs"/> before they are mapped to sRGB: a color outside the sRGB gamut keeps its
        /// out-of-range coordinates here, where <c>Mix</c> clips them. <paramref name="oklab1"/> and <paramref name="oklab2"/> are an operand's
        /// Oklab coordinates when it lies outside the sRGB gamut its <see cref="Color"/> is clipped to; <paramref name="written1"/> and
        /// <paramref name="written2"/> are the components an operand was written with, and which of them were <c>none</c>.
        /// </summary>
        internal static double[] MixComponents(Color c1, Color c2, double t, ColorSpaceMath.Space cs, ColorSpaceMath.HueMethod hue, out double mixedAlpha,
            double[] oklab1 = null, double[] oklab2 = null, ColorSpaceMath.WrittenCoords? written1 = null, ColorSpaceMath.WrittenCoords? written2 = null)
        {
            t = ColorSpaceMath.Clamp01(t);
            double a1 = c1.A / 255.0, a2 = c2.A / 255.0;

            var v1 = Operand(c1, oklab1, written1, cs);
            var v2 = Operand(c2, oklab2, written2, cs);
            var hueIndex = ColorSpaceMath.HueIndex(cs);

            // A missing component (CSS Color 4 section 12.2, written as none) takes the other color's value, so the mix leaves it as that color has it; it only
            // counts where the other space has an analogous component (section 12.4). A missing alpha does the same with the alpha.
            var missing1 = written1.HasValue ? ColorSpaceMath.MapMissing(written1.Value.Space, written1.Value.Missing, cs) : 0;
            var missing2 = written2.HasValue ? ColorSpaceMath.MapMissing(written2.Value.Space, written2.Value.Missing, cs) : 0;
            for (var i = 0; i < 3; i++)
            {
                var bit = 1 << i;
                if ((missing1 & bit) != 0 && (missing2 & bit) == 0) v1[i] = v2[i];
                else if ((missing2 & bit) != 0 && (missing1 & bit) == 0) v2[i] = v1[i];
            }

            if ((missing1 & ColorSpaceMath.AlphaMissing) != 0 && (missing2 & ColorSpaceMath.AlphaMissing) == 0) a1 = a2;
            else if ((missing2 & ColorSpaceMath.AlphaMissing) != 0 && (missing1 & ColorSpaceMath.AlphaMissing) == 0) a2 = a1;

            // A gray has no hue of its own: the other color's stands in for it (CSS Color 4, interpolating with a missing hue), instead of whatever angle the
            // conversion leaves behind. (A color written in the mix's own space keeps the hue it was written with, even at a chroma of 0:
            // only a hue that conversion produced is powerless.)
            ColorSpaceMath.ResolveMissingHues(v1, v2, cs,
                written1.HasValue && written1.Value.Space == cs,
                written2.HasValue && written2.Value.Space == cs);

            for (var i = 0; i < 3; i++)
            {
                if (i == hueIndex) continue;
                v1[i] *= a1;
                v2[i] *= a2;
            }

            mixedAlpha = a1 + t * (a2 - a1);
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

            return v;
        }

        /// <summary>
        /// <c>color-mix()</c> between two <c>device-cmyk()</c> operands - an extension, not literal spec behavior: CSS Color 5 has no
        /// "cmyk" interpolation space, and the fallback it defines for mixing a device-cmyk() color without an ICC profile is the naive
        /// CMYK-to-RGB formula. Mixing the C/M/Y/K components directly (premultiplied by alpha, like the other spaces) keeps two CMYK
        /// operands lossless. The <c>in &lt;space&gt;</c> of the mix is not consulted.
        /// </summary>
        internal static Color MixCmyk(Color c1, Color c2, double t, double alphaMultiplier)
        {
            t = ColorSpaceMath.Clamp01(t);
            double a1 = c1.A / 255.0, a2 = c2.A / 255.0;
            var mixedAlpha = a1 + t * (a2 - a1);

            float Channel(float v1, float v2)
            {
                var mixed = v1 * a1 + t * (v2 * a2 - v1 * a1);
                return mixedAlpha > 1e-10 ? (float)(mixed / mixedAlpha) : 0f;
            }

            var outAlpha = (float)ColorSpaceMath.Clamp01(mixedAlpha * alphaMultiplier);
            return FromDeviceCmyk(Channel(c1._cyan, c2._cyan), Channel(c1._magenta, c2._magenta),
                Channel(c1._yellow, c2._yellow), Channel(c1._key, c2._key), outAlpha);
        }

        private static byte ToByte(double v) => (byte)Math.Round(ColorSpaceMath.Clamp01(v) * 255);

        public int Value => _hashcode;
        public byte A => _alpha;
        public double Alpha => Math.Round(_alpha / 255.0, 2);
        public byte R => _red;
        public byte G => _green;
        public byte B => _blue;

        /// <summary>
        /// True if this color was written with <c>device-cmyk()</c> and is carried in its native
        /// <see cref="C"/>/<see cref="M"/>/<see cref="Y"/>/<see cref="K"/> components; <see cref="R"/>, <see cref="G"/> and <see cref="B"/> are
        /// meaningless (0) for it.
        /// </summary>
        public bool IsDeviceCmyk => _isDeviceCmyk;

        // C/M/Y/K rather than Cyan/Magenta/...: Magenta is already a named-color field of this type.
        public float C => _cyan;
        public float M => _magenta;
        public float Y => _yellow;
        public float K => _key;

        public static bool operator ==(Color a, Color b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Color a, Color b)
        {
            return !a.Equals(b);
        }

        public bool Equals(Color other)
        {
            if (_isDeviceCmyk || other._isDeviceCmyk)
            {
                return _isDeviceCmyk == other._isDeviceCmyk && _alpha == other._alpha &&
                       _cyan.Equals(other._cyan) && _magenta.Equals(other._magenta) &&
                       _yellow.Equals(other._yellow) && _key.Equals(other._key);
            }

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
            if (_isDeviceCmyk || other._isDeviceCmyk)
            {
                // sRGB colors order before device-cmyk() ones, which order by alpha and then C, M, Y, K.
                if (_isDeviceCmyk != other._isDeviceCmyk) return _isDeviceCmyk ? 1 : -1;
                var order = _alpha.CompareTo(other._alpha);
                if (order == 0) order = _cyan.CompareTo(other._cyan);
                if (order == 0) order = _magenta.CompareTo(other._magenta);
                if (order == 0) order = _yellow.CompareTo(other._yellow);
                return order != 0 ? order : _key.CompareTo(other._key);
            }

            return _hashcode - other._hashcode;
        }

        public override int GetHashCode()
        {
            if (!_isDeviceCmyk) return _hashcode;

            unchecked
            {
                var hash = 17;
                hash = hash * 31 + _alpha;
                hash = hash * 31 + _cyan.GetHashCode();
                hash = hash * 31 + _magenta.GetHashCode();
                hash = hash * 31 + _yellow.GetHashCode();
                hash = hash * 31 + _key.GetHashCode();
                return hash;
            }
        }

        public static Color Mix(Color above, Color below)
        {
            return Mix(above.Alpha, above, below);
        }

        public static Color Mix(double alpha, Color above, Color below)
        {
            // Two device-cmyk() colors blend in their own components; one of them with an sRGB color has no profile-free result.
            if (above._isDeviceCmyk && below._isDeviceCmyk)
            {
                var weight = (float)alpha;
                return FromDeviceCmyk(
                    weight * above._cyan + (1f - weight) * below._cyan,
                    weight * above._magenta + (1f - weight) * below._magenta,
                    weight * above._yellow + (1f - weight) * below._yellow,
                    weight * above._key + (1f - weight) * below._key);
            }

            if (above._isDeviceCmyk || below._isDeviceCmyk)
                throw new NotSupportedException("A device-cmyk() color cannot be mixed with an sRGB color: there is no profile-free conversion between them.");

            var gamma = 1.0 - alpha;
            var r = gamma * below.R + alpha * above.R;
            var g = gamma * below.G + alpha * above.G;
            var b = gamma * below.B + alpha * above.B;
            return new Color((byte) r, (byte) g, (byte) b);
        }

        private static byte Normalize(float value)
        {
            return (byte) Math.Max(Math.Min(Math.Round(255 * value), 255), 0);
        }

        private static byte Convert(float value)
        {
            return (byte) Math.Round(255f * value);
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
            if (_isDeviceCmyk) return ToDeviceCmykString(null, CultureInfo.InvariantCulture);

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

        // Re-serializes a device-cmyk() color as CSS Color 5 space-separated syntax, so it round-trips.
        private string ToDeviceCmykString(string format, IFormatProvider formatProvider)
        {
            var components = string.Join(" ", _cyan.ToString(format, formatProvider),
                _magenta.ToString(format, formatProvider),
                _yellow.ToString(format, formatProvider),
                _key.ToString(format, formatProvider));

            var arguments = _alpha == 255 ? components : components + " / " + Alpha.ToString(format, formatProvider);
            return FunctionNames.DeviceCmyk.StylesheetFunction(arguments);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (_isDeviceCmyk) return ToDeviceCmykString(format, formatProvider);

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