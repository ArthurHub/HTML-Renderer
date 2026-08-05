namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class KeywordToken : Token
    {
        public KeywordToken(TokenType type, string data, TextPosition position)
            : base(type, data, position)
        {
        }

        public override string ToValue()
        {
            switch (Type)
            {
                case TokenType.Hash:
                    return "#" + Data;
                case TokenType.AtKeyword:
                    return "@" + Data;
                case TokenType.Function:
                    return Data + "(";
                default:
                    return Data;
            }
        }
    }
}