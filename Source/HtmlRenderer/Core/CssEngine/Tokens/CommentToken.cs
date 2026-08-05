namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CommentToken : Token
    {
        public CommentToken(string data, bool valid, TextPosition position)
            : base(TokenType.Comment, data, position)
        {
            IsValid = valid;
        }

        public bool IsValid { get; }

        public override string ToValue()
        {
            var trailing = IsValid ? string.Empty : "*/";
            return string.Concat("/*", Data, trailing);
        }
    }
}