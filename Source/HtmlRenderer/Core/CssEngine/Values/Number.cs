using System;

// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Number : IEquatable<Number>, IComparable<Number>, IFormattable
    {
        /// <summary>
        ///     Gets a zero value.
        /// </summary>
        public static readonly Number Zero = new Number(0f, Unit.Integer);

        /// <summary>
        ///     Gets the positive infinite value.
        /// </summary>
        public static readonly Number Infinite = new Number(float.PositiveInfinity, Unit.Float);

        /// <summary>
        ///     Gets the neutral element.
        /// </summary>
        public static readonly Number One = new Number(1f, Unit.Integer);

        private readonly Unit _unit;

        public Number(float value, Unit unit)
        {
            Value = value;
            _unit = unit;
        }

        public float Value { get; }
        public bool IsInteger => _unit == Unit.Integer;

        public static bool operator >=(Number a, Number b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator >(Number a, Number b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <=(Number a, Number b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator <(Number a, Number b)
        {
            return a.Value < b.Value;
        }

        public int CompareTo(Number other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(Number other)
        {
            return Value == other.Value && _unit == other._unit;
        }

        internal enum Unit : byte
        {
            Integer,
            Float,
            Percent
        }

        public static bool operator ==(Number a, Number b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Number a, Number b)
        {
            return a.Value != b.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Number other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString() + (_unit == Unit.Percent ? "%" : string.Empty);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider) + (_unit == Unit.Percent ? "%" : string.Empty);
        }
    }
}