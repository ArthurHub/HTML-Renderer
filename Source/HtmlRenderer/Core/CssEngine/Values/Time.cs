using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Time : IEquatable<Time>, IComparable<Time>, IFormattable
    {
        public static readonly Time Zero = new Time(0f, Unit.Ms);

        public Time(float value, Unit unit)
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
                    case Unit.Ms:
                        return UnitNames.Ms;
                    case Unit.S:
                        return UnitNames.S;
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        ///     Compares the magnitude of two times.
        /// </summary>
        public static bool operator >=(Time a, Time b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == 1;
        }

        /// <summary>
        ///     Compares the magnitude of two times.
        /// </summary>
        public static bool operator >(Time a, Time b)
        {
            return a.CompareTo(b) == 1;
        }

        /// <summary>
        ///     Compares the magnitude of two times.
        /// </summary>
        public static bool operator <=(Time a, Time b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == -1;
        }

        /// <summary>
        ///     Compares the magnitude of two times.
        /// </summary>
        public static bool operator <(Time a, Time b)
        {
            return a.CompareTo(b) == -1;
        }

        public int CompareTo(Time other)
        {
            return ToMilliseconds().CompareTo(other.ToMilliseconds());
        }


        public static Unit GetUnit(string s)
        {
            switch (s)
            {
                case "s":
                    return Unit.S;
                case "ms":
                    return Unit.Ms;
                default:
                    return Unit.None;
            }
        }

        public float ToMilliseconds()
        {
            return Type == Unit.S ? Value * 1000f : Value;
        }

        public bool Equals(Time other)
        {
            return ToMilliseconds() == other.ToMilliseconds();
        }

        internal enum Unit : byte
        {
            None,
            Ms,
            S
        }

        /// <summary>
        ///     Checks for equality of two times.
        /// </summary>
        public static bool operator ==(Time a, Time b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Checks for inequality of two times.
        /// </summary>
        public static bool operator !=(Time a, Time b)
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
            if (obj is Time other) return Equals(other);

            return false;
        }

        /// <summary>
        ///     Returns a hash code that defines the current time.
        /// </summary>
        /// <returns>The integer value of the hashcode.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        ///     Returns a string representing the time.
        /// </summary>
        /// <returns>The unit string.</returns>
        public override string ToString()
        {
            return string.Concat(Value.ToString(), UnitString);
        }

        /// <summary>
        ///     Returns a formatted string representing the time.
        /// </summary>
        /// <param name="format">The format of the number.</param>
        /// <param name="formatProvider">The provider to use.</param>
        /// <returns>The unit string.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(Value.ToString(format, formatProvider), UnitString);
        }
    }
}