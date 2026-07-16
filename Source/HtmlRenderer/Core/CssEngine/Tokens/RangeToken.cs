using System.Collections.Generic;
using System.Globalization;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RangeToken : Token
    {
        private string[] GetRange()
        {
            var index = int.Parse(Start, NumberStyles.HexNumber);

            if (index > Symbols.MaximumCodepoint) return null;

            if (End == null) return new[] { index.ConvertFromUtf32() };

            var list = new List<string>();
            var f = int.Parse(End, NumberStyles.HexNumber);

            if (f > Symbols.MaximumCodepoint) f = Symbols.MaximumCodepoint;

            while (index <= f)
            {
                list.Add(index.ConvertFromUtf32());
                index++;
            }

            return list.ToArray();
        }

        public RangeToken(string range, TextPosition position)
            : base(TokenType.Range, range, position)
        {
            Start = range.Replace(Symbols.QuestionMark, '0');
            End = range.Replace(Symbols.QuestionMark, 'F');
            SelectedRange = GetRange();
        }

        public RangeToken(string start, string end, TextPosition position)
            : base(TokenType.Range, string.Concat(start, "-", end), position)
        {
            Start = start;
            End = end;
            SelectedRange = GetRange();
        }

        //public bool IsEmpty => (SelectedRange == null) || (SelectedRange.Length == 0);
        public string Start { get; }
        public string End { get; }
        public string[] SelectedRange { get; }
    }
}