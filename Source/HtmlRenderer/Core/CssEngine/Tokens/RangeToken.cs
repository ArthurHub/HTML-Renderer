using System.Collections.Generic;
using System.Globalization;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RangeToken : Token
    {
        private string[] _selectedRange;
        private bool _selectedRangeComputed;

        private string[] GetRange()
        {
            var index = int.Parse(Start, NumberStyles.HexNumber);

            if (index > Symbols.MaximumCodepoint) return null;

            if (End == null) return new[] {index.ConvertFromUtf32()};

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
        }

        public RangeToken(string start, string end, TextPosition position)
            : base(TokenType.Range, string.Concat(start, "-", end), position)
        {
            Start = start;
            End = end;
        }

        // Data holds the range without its "U+" prefix, so serializing it bare would emit "41-5A" - not a
        // valid <urange> and not what was authored.
        public override string ToValue()
        {
            return string.Concat("U+", Data);
        }

        //public bool IsEmpty => (SelectedRange == null) || (SelectedRange.Length == 0);
        public string Start { get; }
        public string End { get; }

        // Computed on demand: a legal range such as U+0-10FFFF expands to over a million strings, which is
        // far too much to materialize eagerly in a token constructor for a value nothing may ever read.
        public string[] SelectedRange
        {
            get
            {
                if (!_selectedRangeComputed)
                {
                    _selectedRange = GetRange();
                    _selectedRangeComputed = true;
                }

                return _selectedRange;
            }
        }
    }
}