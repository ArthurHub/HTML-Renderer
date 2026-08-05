namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal class TokenizerError
    {
        private readonly ParseError _code;

        public TokenizerError(ParseError code, TextPosition position)
        {
            _code = code;
            Position = position;
        }

        public TextPosition Position { get; }
        public int Code => _code.GetCode();
        public string Message => "An unknown error occurred.";
    }
}