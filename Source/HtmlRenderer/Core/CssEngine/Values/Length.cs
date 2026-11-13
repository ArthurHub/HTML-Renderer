using System;

// ReSharper disable UnusedMember.Global


namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Length : IEquatable<Length>, IComparable<Length>, IFormattable
    {
        /// <summary>
        ///     Gets a zero pixel length value.
        /// </summary>
        public static readonly Length Zero = new(0f, Unit.Px);

        /// <summary>
        ///     Gets the half relative length, i.e. 50%.
        /// </summary>
        public static readonly Length Half = new(50f, Unit.Percent);

        /// <summary>
        ///     Gets the full relative length, i.e. 100%.
        /// </summary>
        public static readonly Length Full = new(100f, Unit.Percent);

        /// <summary>
        ///     Gets a thin length value.
        /// </summary>
        public static readonly Length Thin = new(1f, Unit.Px);

        /// <summary>
        ///     Gets a medium length value.
        /// </summary>
        public static readonly Length Medium = new(3f, Unit.Px);

        /// <summary>
        ///     Gets a thick length value.
        /// </summary>
        public static readonly Length Thick = new(5f, Unit.Px);

        /// <summary>
        ///     Gets the missing value.
        /// </summary>
        public static readonly Length Missing = new(-1f, Unit.Ch);

        public Length(float value, Unit unit)
        {
            Value = value;
            Type = unit;
        }

        /// <summary>
        ///     Gets if the length is given in absolute units.
        ///     Such a length may be converted to pixels.
        /// </summary>
        public bool IsAbsolute =>
            Type == Unit.In || Type == Unit.Mm || Type == Unit.Pc || Type == Unit.Px ||
            Type == Unit.Pt || Type == Unit.Cm || Type == Unit.Q;

        /// <summary>
        ///     Gets if the length is given in relative units.
        ///     Such a length cannot be converted to pixels.
        /// </summary>
        public bool IsRelative => !IsAbsolute;

        /// <summary>
        ///     Gets the type of the length.
        /// </summary>
        public Unit Type { get; }

        /// <summary>
        ///     Gets the value of the length.
        /// </summary>
        public float Value { get; }

        /// <summary>
        ///     Gets the representation of the unit as a string.
        /// </summary>
        public string UnitString
        {
            get
            {
                switch (Type)
                {
                    case Unit.Px:
                        return UnitNames.Px;
                    case Unit.Em:
                        return UnitNames.Em;
                    case Unit.Ex:
                        return UnitNames.Ex;
                    case Unit.Cm:
                        return UnitNames.Cm;
                    case Unit.Mm:
                        return UnitNames.Mm;
                    case Unit.In:
                        return UnitNames.In;
                    case Unit.Pt:
                        return UnitNames.Pt;
                    case Unit.Pc:
                        return UnitNames.Pc;
                    case Unit.Ch:
                        return UnitNames.Ch;
                    case Unit.Rem:
                        return UnitNames.Rem;
                    case Unit.Vw:
                        return UnitNames.Vw;
                    case Unit.Vh:
                        return UnitNames.Vh;
                    case Unit.Vmin:
                        return UnitNames.Vmin;
                    case Unit.Vmax:
                        return UnitNames.Vmax;
                    case Unit.Q:
                        return UnitNames.Q;
                    case Unit.Cap:
                        return UnitNames.Cap;
                    case Unit.Ic:
                        return UnitNames.Ic;
                    case Unit.Lh:
                        return UnitNames.Lh;
                    case Unit.Rlh:
                        return UnitNames.Rlh;
                    case Unit.Rex:
                        return UnitNames.Rex;
                    case Unit.Rch:
                        return UnitNames.Rch;
                    case Unit.Rcap:
                        return UnitNames.Rcap;
                    case Unit.Ric:
                        return UnitNames.Ric;
                    case Unit.Vi:
                        return UnitNames.Vi;
                    case Unit.Vb:
                        return UnitNames.Vb;
                    case Unit.Svw:
                        return UnitNames.Svw;
                    case Unit.Svh:
                        return UnitNames.Svh;
                    case Unit.Svi:
                        return UnitNames.Svi;
                    case Unit.Svb:
                        return UnitNames.Svb;
                    case Unit.Svmin:
                        return UnitNames.Svmin;
                    case Unit.Svmax:
                        return UnitNames.Svmax;
                    case Unit.Lvw:
                        return UnitNames.Lvw;
                    case Unit.Lvh:
                        return UnitNames.Lvh;
                    case Unit.Lvi:
                        return UnitNames.Lvi;
                    case Unit.Lvb:
                        return UnitNames.Lvb;
                    case Unit.Lvmin:
                        return UnitNames.Lvmin;
                    case Unit.Lvmax:
                        return UnitNames.Lvmax;
                    case Unit.Dvw:
                        return UnitNames.Dvw;
                    case Unit.Dvh:
                        return UnitNames.Dvh;
                    case Unit.Dvi:
                        return UnitNames.Dvi;
                    case Unit.Dvb:
                        return UnitNames.Dvb;
                    case Unit.Dvmin:
                        return UnitNames.Dvmin;
                    case Unit.Dvmax:
                        return UnitNames.Dvmax;
                    case Unit.Cqw:
                        return UnitNames.Cqw;
                    case Unit.Cqh:
                        return UnitNames.Cqh;
                    case Unit.Cqi:
                        return UnitNames.Cqi;
                    case Unit.Cqb:
                        return UnitNames.Cqb;
                    case Unit.Cqmin:
                        return UnitNames.Cqmin;
                    case Unit.Cqmax:
                        return UnitNames.Cqmax;
                    case Unit.Percent:
                        return UnitNames.Percent;
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        ///     Compares the magnitude of two lengths.
        /// </summary>
        public static bool operator >=(Length a, Length b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == 1;
        }

        /// <summary>
        ///     Compares the magnitude of two lengths.
        /// </summary>
        public static bool operator >(Length a, Length b)
        {
            return a.CompareTo(b) == 1;
        }

        /// <summary>
        ///     Compares the magnitude of two lengths.
        /// </summary>
        public static bool operator <=(Length a, Length b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == -1;
        }

        /// <summary>
        ///     Compares the magnitude of two lengths.
        /// </summary>
        public static bool operator <(Length a, Length b)
        {
            return a.CompareTo(b) == -1;
        }

        /// <summary>
        ///     Compares the current length against the given one.
        /// </summary>
        /// <param name="other">The length to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public int CompareTo(Length other)
        {
            if (Type == other.Type) return Value.CompareTo(other.Value);

            if (IsAbsolute && other.IsAbsolute) return ToPixel().CompareTo(other.ToPixel());

            return 0;
        }

        public static bool TryParse(string s, out Length result)
        {
            var unitString = s.StylesheetUnit(out var value);
            var unit = GetUnit(unitString);

            if (unit != Unit.None)
            {
                result = new Length(value, unit);
                return true;
            }

            // Only a genuinely parsed unitless "0" may omit its unit (CSS Values 3 5.2). StylesheetUnit
            // returns an empty unit string for a plain number but null when it could not tokenize a number
            // at all, and both leave value at 0 - so the unit string has to be checked as well, otherwise
            // arbitrary non-length input is accepted as zero.
            if (unitString != null && unitString.Length == 0 && value == 0f)
            {
                result = Zero;
                return true;
            }

            result = default;
            return false;
        }

        public static Unit GetUnit(string s)
        {
            return s?.ToLowerInvariant() switch
            {
                "ch" => Unit.Ch,
                "cm" => Unit.Cm,
                "em" => Unit.Em,
                "ex" => Unit.Ex,
                "in" => Unit.In,
                "mm" => Unit.Mm,
                "pc" => Unit.Pc,
                "pt" => Unit.Pt,
                "px" => Unit.Px,
                "rem" => Unit.Rem,
                "vh" => Unit.Vh,
                "vmax" => Unit.Vmax,
                "vmin" => Unit.Vmin,
                "vw" => Unit.Vw,
                "q" => Unit.Q,
                "cap" => Unit.Cap,
                "ic" => Unit.Ic,
                "lh" => Unit.Lh,
                "rlh" => Unit.Rlh,
                "rex" => Unit.Rex,
                "rch" => Unit.Rch,
                "rcap" => Unit.Rcap,
                "ric" => Unit.Ric,
                "vi" => Unit.Vi,
                "vb" => Unit.Vb,
                "svw" => Unit.Svw,
                "svh" => Unit.Svh,
                "svi" => Unit.Svi,
                "svb" => Unit.Svb,
                "svmin" => Unit.Svmin,
                "svmax" => Unit.Svmax,
                "lvw" => Unit.Lvw,
                "lvh" => Unit.Lvh,
                "lvi" => Unit.Lvi,
                "lvb" => Unit.Lvb,
                "lvmin" => Unit.Lvmin,
                "lvmax" => Unit.Lvmax,
                "dvw" => Unit.Dvw,
                "dvh" => Unit.Dvh,
                "dvi" => Unit.Dvi,
                "dvb" => Unit.Dvb,
                "dvmin" => Unit.Dvmin,
                "dvmax" => Unit.Dvmax,
                "cqw" => Unit.Cqw,
                "cqh" => Unit.Cqh,
                "cqi" => Unit.Cqi,
                "cqb" => Unit.Cqb,
                "cqmin" => Unit.Cqmin,
                "cqmax" => Unit.Cqmax,
                "%" => Unit.Percent,
                _ => Unit.None
            };
        }

        public float ToPixel()
        {
            return Type switch
            {
                Unit.In => // 1 in = 2.54 cm
                    Value * 96f,
                Unit.Mm => // 1 mm = 0.1 cm
                    Value * 5f * 96f / 127f,
                Unit.Pc => // 1 pc = 12 pt
                    Value * 12f * 96f / 72f,
                Unit.Pt => // 1 pt = 1/72 in
                    Value * 96f / 72f,
                Unit.Cm => // 1 cm = 50/127 in
                    Value * 50f * 96f / 127f,
                Unit.Q => // 1 Q = 1/40 cm
                    Value * 50f * 96f / (127f * 40f),
                Unit.Px => // 1 px = 1/96 in
                    Value,
                _ => throw new InvalidOperationException("A relative unit cannot be converted.")
            };
        }

        /// <summary>
        ///     Resolves this length to pixels. Other relative units than em, rem, ex and % resolve to 0.
        /// </summary>
        /// <param name="emFactor">Pixels per 1em, i.e. the relevant font size in pixels.</param>
        /// <param name="remFactor">Pixels per 1rem, i.e. the root element's font size in pixels.</param>
        /// <param name="hundredPercent">The pixel value equivalent to 100%.</param>
        /// <param name="fontAdjust">If the length is in pixels and font related, apply the 72/96 factor.</param>
        internal double ToPixels(double emFactor, double remFactor, double hundredPercent, bool fontAdjust = false)
        {
            // CSS Values 4 §6.2, keep in sync with CssValueParser.ParseLength
            switch (Type)
            {
                case Unit.Em: return emFactor * Value;
                case Unit.Rem: return remFactor * Value;
                case Unit.Ex: return emFactor / 2 * Value;
                case Unit.Px: return (fontAdjust ? 72d / 96d : 1d) * Value;
                case Unit.Percent: return hundredPercent / 100d * Value;
                default: return IsAbsolute ? ToPixel() : 0d;
            }
        }

        public float To(Unit unit)
        {
            var value = ToPixel();

            return unit switch
            {
                Unit.In => // 1 in = 2.54 cm
                    value / 96f,
                Unit.Mm => // 1 mm = 0.1 cm
                    value * 127f / (5f * 96f),
                Unit.Pc => // 1 pc = 12 pt
                    value * 72f / (12f * 96f),
                Unit.Pt => // 1 pt = 1/72 in
                    value * 72f / 96f,
                Unit.Cm => // 1 cm = 50/127 in
                    value * 127f / (50f * 96f),
                Unit.Q => // 1 Q = 1/40 cm
                    value * 127f * 40f / (50f * 96f),
                Unit.Px => // 1 px = 1/96 in
                    value,
                _ => throw new InvalidOperationException("An absolute unit cannot be converted to a relative one.")
            };
        }

        public bool Equals(Length other)
        {
            return Value == other.Value && Type == other.Type;
        }

        public enum Unit : byte
        {
            None,
            Px,
            Em,
            Ex,
            Cm,
            Mm,
            In,
            Pt,
            Pc,
            Ch,
            Rem,
            Vw,
            Vh,
            Vmin,
            Vmax,
            Percent,
            Q,
            Cap,
            Ic,
            Lh,
            Rlh,
            Rex,
            Rch,
            Rcap,
            Ric,
            Vi,
            Vb,
            Svw,
            Svh,
            Svi,
            Svb,
            Svmin,
            Svmax,
            Lvw,
            Lvh,
            Lvi,
            Lvb,
            Lvmin,
            Lvmax,
            Dvw,
            Dvh,
            Dvi,
            Dvb,
            Dvmin,
            Dvmax,
            Cqw,
            Cqh,
            Cqi,
            Cqb,
            Cqmin,
            Cqmax
        }

        /// <summary>
        ///     Checks the equality of the two given lengths.
        /// </summary>
        /// <param name="a">The left length.</param>
        /// <param name="b">The right length.</param>
        /// <returns>True if both lengths are equal, otherwise false.</returns>
        public static bool operator ==(Length a, Length b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Checks the inequality of the two given lengths.
        /// </summary>
        /// <param name="a">The left length.</param>
        /// <param name="b">The right length.</param>
        /// <returns>True if both lengths are not equal, otherwise false.</returns>
        public static bool operator !=(Length a, Length b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        ///     Tests if another object is equal to this object.
        /// </summary>
        /// <param name="obj">The object to test with.</param>
        /// <returns>True if the two objects are equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as Length?;

            if (other != null)
                return Equals(other.Value);

            return false;
        }

        /// <summary>
        ///     Returns a hash code that defines the current length.
        /// </summary>
        /// <returns>The integer value of the hashcode.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            var unit = Value == 0f ? string.Empty : UnitString;
            return string.Concat(Value.ToString(), unit);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var unit = Value == 0f ? string.Empty : UnitString;
            return string.Concat(Value.ToString(format, formatProvider), unit);
        }
    }
}