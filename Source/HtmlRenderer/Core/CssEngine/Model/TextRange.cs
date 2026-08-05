using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct TextRange : IEquatable<TextRange>, IComparable<TextRange>
    {
        public TextRange(TextPosition start, TextPosition end)
        {
            Start = start;
            End = end;
        }

        public TextPosition Start { get; }
        public TextPosition End { get; }

        public override string ToString()
        {
            return $"({Start}) -- ({End})";
        }

        public override int GetHashCode()
        {
            return End.GetHashCode() ^ Start.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TextRange other && Equals(other);
        }

        public bool Equals(TextRange other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public static bool operator >(TextRange a, TextRange b)
        {
            return a.Start > b.End;
        }

        public static bool operator <(TextRange a, TextRange b)
        {
            return a.End < b.Start;
        }

        public int CompareTo(TextRange other)
        {
            if (this > other) return 1;

            if (other > this) return -1;

            return 0;
        }
    }
}