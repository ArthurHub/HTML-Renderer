using System;

// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Frequency : IEquatable<Frequency>, IComparable<Frequency>, IFormattable
    {
        public Frequency(float value, Unit unit)
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
                    case Unit.Khz:
                        return UnitNames.Khz;

                    case Unit.Hz:
                        return UnitNames.Hz;

                    default:
                        return string.Empty;
                }
            }
        }

        public static bool operator >=(Frequency a, Frequency b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == 1;
        }

        public static bool operator >(Frequency a, Frequency b)
        {
            return a.CompareTo(b) == 1;
        }

        public static bool operator <=(Frequency a, Frequency b)
        {
            var result = a.CompareTo(b);
            return result == 0 || result == -1;
        }

        public static bool operator <(Frequency a, Frequency b)
        {
            return a.CompareTo(b) == -1;
        }

        public int CompareTo(Frequency other)
        {
            return ToHertz().CompareTo(other.ToHertz());
        }

        public static bool TryParse(string s, out Frequency result)
        {
            var unit = GetUnit(s.StylesheetUnit(out var value));

            if (unit != Unit.None)
            {
                result = new Frequency(value, unit);
                return true;
            }

            result = default;
            return false;
        }

        public static Unit GetUnit(string s)
        {
            switch (s)
            {
                case "hz":
                    return Unit.Hz;
                case "khz":
                    return Unit.Khz;
                default:
                    return Unit.None;
            }
        }

        public float ToHertz()
        {
            return Type == Unit.Khz ? Value * 1000f : Value;
        }

        public bool Equals(Frequency other)
        {
            return Value == other.Value && Type == other.Type;
        }

        internal enum Unit : byte
        {
            None,
            Hz,
            Khz
        }

        public static bool operator ==(Frequency a, Frequency b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Frequency a, Frequency b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Frequency?;

            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
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