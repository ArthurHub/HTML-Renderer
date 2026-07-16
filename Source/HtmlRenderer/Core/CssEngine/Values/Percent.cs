using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Percent : IEquatable<Percent>, IComparable<Percent>, IFormattable
    {
        /// <summary>
        ///     Gets a zero percent value.
        /// </summary>
        public static readonly Percent Zero = new Percent(0f);

        /// <summary>
        ///     Gets a fifty percent value.
        /// </summary>
        public static readonly Percent Fifty = new Percent(50f);

        /// <summary>
        ///     Gets a hundred percent value.
        /// </summary>
        public static readonly Percent Hundred = new Percent(100f);

        public Percent(float value)
        {
            Value = value;
        }

        public float NormalizedValue => Value * 0.01f;
        public float Value { get; }

        /// <summary>
        ///     Compares the magnitude of two percents.
        /// </summary>
        public static bool operator >=(Percent a, Percent b)
        {
            return a.Value >= b.Value;
        }

        /// <summary>
        ///     Compares the magnitude of two percents.
        /// </summary>
        public static bool operator >(Percent a, Percent b)
        {
            return a.Value > b.Value;
        }

        /// <summary>
        ///     Compares the magnitude of two percents.
        /// </summary>
        public static bool operator <=(Percent a, Percent b)
        {
            return a.Value <= b.Value;
        }

        /// <summary>
        ///     Compares the magnitude of two percents.
        /// </summary>
        public static bool operator <(Percent a, Percent b)
        {
            return a.Value < b.Value;
        }

        /// <summary>
        ///     Compares the current percentage against the given one.
        /// </summary>
        /// <param name="other">The percentage to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public int CompareTo(Percent other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        ///     Checks if the given percent value is equal to the current one.
        /// </summary>
        /// <param name="other">The other percent value.</param>
        /// <returns>True if both have the same value.</returns>
        public bool Equals(Percent other)
        {
            return Value == other.Value;
        }

        /// <summary>
        ///     Checks for equality of two percents.
        /// </summary>
        public static bool operator ==(Percent a, Percent b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Checks for inequality of two percents.
        /// </summary>
        public static bool operator !=(Percent a, Percent b)
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
            if (obj is Percent other) return Equals(other);

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value + "%";
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider) + "%";
        }
    }
}