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

        // Data holds the range without its "U+" prefix, so serializing it bare would emit "41-5A" - not a
        // valid <urange> and not what was authored.
        public override string ToValue()
        {
            return string.Concat("U+", Data);
        }

        public string Start { get; }
        public string End { get; }
    }
}
