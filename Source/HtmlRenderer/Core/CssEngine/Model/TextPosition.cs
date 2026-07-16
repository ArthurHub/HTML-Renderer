using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct TextPosition : IEquatable<TextPosition>, IComparable<TextPosition>
    {
        public static readonly TextPosition Empty = new TextPosition();

        private readonly ushort _line;
        private readonly ushort _column;

        public TextPosition(ushort line, ushort column, int position)
        {
            _line = line;
            _column = column;
            Position = position;
        }

        public int Line => _line;
        public int Column => _column;
        public int Position { get; }

        public TextPosition Shift(int columns)
        {
            return new TextPosition(_line, (ushort)(_column + columns), Position + columns);
        }

        public TextPosition After(char chr)
        {
            var line = _line;
            var column = _column;

            if (chr != Symbols.LineFeed) return new TextPosition(line, ++column, Position + 1);

            ++line;
            column = 0;

            return new TextPosition(line, ++column, Position + 1);
        }

        public TextPosition After(string str)
        {
            var line = _line;
            var column = _column;

            foreach (var chr in str)
            {
                if (chr == Symbols.LineFeed)
                {
                    ++line;
                    column = 0;
                }

                ++column;
            }

            return new TextPosition(line, column, Position + str.Length);
        }

        public override string ToString()
        {
            return $"Line {_line}, Column {_column}, Position {Position}";
        }

        public override int GetHashCode()
        {
            return Position ^ ((_line | _column) + _line);
        }

        public override bool Equals(object obj)
        {
            return obj is TextPosition other && Equals(other);
        }

        public bool Equals(TextPosition other)
        {
            return Position == other.Position &&
                   _column == other._column &&
                   _line == other._line;
        }

        public static bool operator >(TextPosition a, TextPosition b)
        {
            return a.Position > b.Position;
        }

        public static bool operator <(TextPosition a, TextPosition b)
        {
            return a.Position < b.Position;
        }

        public int CompareTo(TextPosition other)
        {
            return Equals(other) ? 0 : this > other ? 1 : -1;
        }
    }
}