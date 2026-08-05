using System;

// ReSharper disable UnusedMember.Global


namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Length : IEquatable<Length>, IComparable<Length>, IFormattable
    {
        /// <summary>
        ///     Gets a zero pixel length value.
        /// </summary>
        public static readonly Length Zero = new Length(0f, Unit.Px);

        /// <summary>
        ///     Gets the half relative length, i.e. 50%.
        /// </summary>
        public static readonly Length Half = new Length(50f, Unit.Percent);

        /// <summary>
        ///     Gets the full relative length, i.e. 100%.
        /// </summary>
        public static readonly Length Full = new Length(100f, Unit.Percent);

        /// <summary>
        ///     Gets a thin length value.
        /// </summary>
        public static readonly Length Thin = new Length(1f, Unit.Px);

        /// <summary>
        ///     Gets a medium length value.
        /// </summary>
        public static readonly Length Medium = new Length(3f, Unit.Px);

        /// <summary>
        ///     Gets a thick length value.
        /// </summary>
        public static readonly Length Thick = new Length(5f, Unit.Px);

        /// <summary>
        ///     Gets the missing value.
        /// </summary>
        public static readonly Length Missing = new Length(-1f, Unit.Ch);

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
            Type == Unit.Pt || Type == Unit.Cm;

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

            // Only a genuinely parsed bare "0" may omit its unit (CSS Values & Units §5.1) -
            // StylesheetUnit returns an empty unit string for a plain number but null when it
            // couldn't tokenize a number at all, and both leave value at 0, so the unit string
            // must be checked to avoid accepting arbitrary non-length input as zero.
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
            switch (s)
            {
                case "ch": return Unit.Ch;
                case "cm": return Unit.Cm;
                case "em": return Unit.Em;
                case "ex": return Unit.Ex;
                case "in": return Unit.In;
                case "mm": return Unit.Mm;
                case "pc": return Unit.Pc;
                case "pt": return Unit.Pt;
                case "px": return Unit.Px;
                case "rem": return Unit.Rem;
                case "vh": return Unit.Vh;
                case "vmax": return Unit.Vmax;
                case "vmin": return Unit.Vmin;
                case "vw": return Unit.Vw;
                case "%": return Unit.Percent;
                default: return Unit.None;
            }
        }

        public float ToPixel()
        {
            if (IsRelative)
                throw new InvalidOperationException("A relative unit cannot be converted.");

            return (float)ToPixels(0, 0, 0);
        }

        /// <summary>
        ///     Resolves this length to pixels, given the context needed for relative units
        ///     (<c>em</c>/<c>rem</c>/<c>ex</c>/<c>%</c>). This is the single source of truth for
        ///     "unit + context -> pixels" — <see cref="ToPixel"/> delegates to it for absolute units,
        ///     and layout-time consumers (e.g. calc() evaluation) use it directly for the relative case.
        /// </summary>
        /// <param name="emFactor">Pixels per 1em, i.e. the relevant font size in pixels.</param>
        /// <param name="remFactor">Pixels per 1rem, i.e. the root element's font size in pixels.</param>
        /// <param name="hundredPercent">The pixel value equivalent to 100%.</param>
        /// <param name="fontAdjust">If the length is in pixels and font related, apply the 72/96 factor.</param>
        internal double ToPixels(double emFactor, double remFactor, double hundredPercent, bool fontAdjust = false)
        {
            // The layout unit here is the CSS pixel, so absolute units resolve against 96dpi
            // (CSS Values & Units §5.2: 1in = 96px, 1pt = 1/72in, 1pc = 12pt). These factors must
            // stay identical to the ones CssValueParser.ParseLength applies to non-calc() lengths,
            // otherwise `calc(1in)` and `width: 1in` would disagree.
            switch (Type)
            {
                case Unit.Em: return emFactor * Value;
                case Unit.Rem: return remFactor * Value;
                case Unit.Ex: return emFactor / 2 * Value;
                case Unit.Px: return (fontAdjust ? 72d / 96d : 1d) * Value; //TODO: a check support for hi dpi
                case Unit.In: // 1 in = 96 px
                    return 96d * Value;
                case Unit.Mm: // 1 mm = 96/25.4 px
                    return (96d / 25.4d) * Value;
                case Unit.Pc: // 1 pc = 12 pt = 16 px
                    return 16d * Value;
                case Unit.Pt: // 1 pt = 1/72 in = 96/72 px
                    return (96d / 72d) * Value;
                case Unit.Cm: // 1 cm = 96/2.54 px
                    return (96d / 2.54d) * Value;
                case Unit.Percent: return hundredPercent / 100d * Value;
                default: return 0d;
            }
        }

        public float To(Unit unit)
        {
            var value = ToPixel();

            // Inverse of ToPixels' absolute-unit factors: `value` is in CSS pixels.
            switch (unit)
            {
                case Unit.In: // 1 in = 96 px
                    return value / 96f;
                case Unit.Mm: // 1 mm = 96/25.4 px
                    return value * 25.4f / 96f;
                case Unit.Pc: // 1 pc = 16 px
                    return value / 16f;
                case Unit.Pt: // 1 pt = 96/72 px
                    return value * 72f / 96f;
                case Unit.Cm: // 1 cm = 96/2.54 px
                    return value * 2.54f / 96f;
                case Unit.Px: // px is the layout unit
                    return value;
                default:
                    throw new InvalidOperationException("An absolute unit cannot be converted to a relative one.");
            }
        }

        public bool Equals(Length other)
        {
            return Value == other.Value && Type == other.Type;
        }

        internal enum Unit : byte
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
            Percent
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