using System;

// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Angle : IEquatable<Angle>, IComparable<Angle>, IFormattable
    {
        public static readonly Angle Zero = new Angle(0f, Unit.Rad);
        public static readonly Angle HalfQuarter = new Angle(45f, Unit.Deg);
        public static readonly Angle Quarter = new Angle(90f, Unit.Deg);
        public static readonly Angle TripleHalfQuarter = new Angle(135f, Unit.Deg);
        public static readonly Angle Half = new Angle(180f, Unit.Deg);

        public Angle(float value, Unit unit)
        {
            Value = value;
            Type = unit;
        }

        public float Value { get; }
        public Unit Type { get; }

        public string UnitString
        {
            get
            {
                switch (Type)
                {
                    case Unit.Deg:
                        return UnitNames.Deg;

                    case Unit.Grad:
                        return UnitNames.Grad;

                    case Unit.Turn:
                        return UnitNames.Turn;

                    case Unit.Rad:
                        return UnitNames.Rad;

                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        ///     Compares the magnitude of two angles.
        /// </summary>
        public static bool operator >=(Angle a, Angle b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == 1;
        }

        /// <summary>
        ///     Compares the magnitude of two angles.
        /// </summary>
        public static bool operator >(Angle a, Angle b)
        {
            return a.CompareTo(b) == 1;
        }

        /// <summary>
        ///     Compares the magnitude of two angles.
        /// </summary>
        public static bool operator <=(Angle a, Angle b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == -1;
        }

        /// <summary>
        ///     Compares the magnitude of two angles.
        /// </summary>
        public static bool operator <(Angle a, Angle b)
        {
            return a.CompareTo(b) == -1;
        }

        /// <summary>
        ///     Compares the current angle against the given one.
        /// </summary>
        /// <param name="other">The angle to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public int CompareTo(Angle other)
        {
            return ToRadian().CompareTo(other.ToRadian());
        }

        public static bool TryParse(string s, out Angle result)
        {
            var unit = GetUnit(s.StylesheetUnit(out var value));

            if (unit != Unit.None)
            {
                result = new Angle(value, unit);
                return true;
            }

            result = default;
            return false;
        }

        public static Unit GetUnit(string s)
        {
            switch (s)
            {
                case "deg":
                    return Unit.Deg;
                case "grad":
                    return Unit.Grad;
                case "turn":
                    return Unit.Turn;
                case "rad":
                    return Unit.Rad;
                default:
                    return Unit.None;
            }
        }

        public float ToRadian()
        {
            switch (Type)
            {
                case Unit.Deg:
                    return (float)(Math.PI / 180.0 * Value);
                case Unit.Grad:
                    return (float)(Math.PI / 200.0 * Value);
                case Unit.Turn:
                    return (float)(2.0 * Math.PI * Value);
                default:
                    return Value;
            }
        }

        public float ToTurns()
        {
            switch (Type)
            {
                case Unit.Deg:
                    return (float)(Value / 360.0);
                case Unit.Grad:
                    return (float)(Value / 400.0);
                case Unit.Rad:
                    return (float)(Value / (2.0 * Math.PI));
                default:
                    return Value;
            }
        }

        public bool Equals(Angle other)
        {
            return ToRadian() == other.ToRadian();
        }

        /// <summary>
        ///     An enumeration of angle representations.
        /// </summary>
        internal enum Unit : byte
        {
            None,
            Deg,
            Rad,
            Grad,
            Turn
        }

        /// <summary>
        ///     Checks for equality of two angles.
        /// </summary>
        public static bool operator ==(Angle a, Angle b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Checks for inequality of two angles.
        /// </summary>
        public static bool operator !=(Angle a, Angle b)
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
            var other = obj as Angle?;

            return other != null && Equals(other.Value);
        }

        /// <summary>
        ///     Returns a hash code that defines the current angle.
        /// </summary>
        /// <returns>The integer value of the hashcode.</returns>
        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return string.Concat(Value.ToString(), UnitString);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(Value.ToString(format, formatProvider), UnitString);
        }
    }
}