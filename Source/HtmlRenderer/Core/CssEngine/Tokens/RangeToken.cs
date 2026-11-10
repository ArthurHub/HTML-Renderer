namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RangeToken : Token
    {
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

        // Data is the range without "U+".
        public override string ToValue()
        {
            return string.Concat("U+", Data);
        }

        public string Start { get; }
        public string End { get; }
    }
}
