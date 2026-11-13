using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Resolution : IEquatable<Resolution>, IComparable<Resolution>, IFormattable
    {
        public Resolution(float value, Unit unit)
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
                return Type switch
                {
                    Unit.Dpcm => UnitNames.Dpcm,
                    Unit.Dpi => UnitNames.Dpi,
                    Unit.Dppx => UnitNames.Dppx,
                    _ => string.Empty
                };
            }
        }

        public static bool TryParse(string s, out Resolution result)
        {
            var unit = GetUnit(s.StylesheetUnit(out var value));

            if (unit != Unit.None)
            {
                result = new Resolution(value, unit);
                return true;
            }

            result = default;
            return false;
        }

        public static Unit GetUnit(string s)
        {
            return s switch
            {
                "dpcm" => Unit.Dpcm,
                "dpi" => Unit.Dpi,
                "dppx" => Unit.Dppx,
                "x" => Unit.Dppx, // `x` is the canonical alias for `dppx` (CSS Values 4 7.4)
                _ => Unit.None
            };
        }

        public float ToDotsPerPixel()
        {
            if (Type == Unit.Dpi) return Value / 96f;
            if (Type == Unit.Dpcm) return Value * 127f / (50f * 96f);

            return Value;
        }

        public float To(Unit unit)
        {
            var value = ToDotsPerPixel();

            if (unit == Unit.Dpi) return value * 96f;
            if (unit == Unit.Dpcm) return value * 50f * 96f / 127f;

            return value;
        }

        public bool Equals(Resolution other)
        {
            return Value == other.Value && Type == other.Type;
        }

        public enum Unit : byte
        {
            None,
            Dpi,
            Dpcm,
            Dppx
        }

        public int CompareTo(Resolution other)
        {
            return ToDotsPerPixel().CompareTo(other.ToDotsPerPixel());
        }

        public override bool Equals(object obj)
        {
            if (obj is Resolution other) return Equals(other);

            return false;
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