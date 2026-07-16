using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ValueBuilder
    {
        public ValueBuilder()
        {
            _values = new List<Token>();
            Reset();
        }

        private readonly List<Token> _values;
        private Token _buffer;
        private bool _valid;
        private int _open;

        public bool IsReady => _open == 0 && _values.Count > 0;

        public bool IsValid => _valid && IsReady;
        public bool IsImportant { get; private set; }

        public TokenValue GetResult()
        {
            return new TokenValue(_values);
        }

        public void Apply(Token token)
        {
            switch (token.Type)
            {
                case TokenType.RoundBracketOpen:
                    _open++;
                    Add(token);
                    break;
                case TokenType.Function:
                    Add(token);
                    break;
                case TokenType.Ident:
                    IsImportant = CheckImportant(token);
                    break;
                case TokenType.RoundBracketClose:
                    _open--;
                    Add(token);
                    break;
                case TokenType.Whitespace:
                    if (_values.Count > 0 && IsSlash(_values[_values.Count - 1]) == false)
                        _buffer = token;
                    break;
                case TokenType.Dimension:
                case TokenType.Percentage:
                case TokenType.Color:
                case TokenType.Delim:
                case TokenType.String:
                case TokenType.Url:
                case TokenType.Number:
                case TokenType.Comma:
                case TokenType.GreaterThan:
                case TokenType.GreaterThanOrEqual:
                case TokenType.LessThan:
                case TokenType.LessThanOrEqual:
                    Add(token);
                    break;
                case TokenType.Comment:
                    break;
                default:
                    _valid = false;
                    Add(token);
                    break;
            }
        }

        public ValueBuilder Reset()
        {
            _open = 0;
            _valid = true;
            _buffer = null;
            IsImportant = false;
            _values.Clear();
            return this;
        }

        private bool CheckImportant(Token token)
        {
            if (_values.Count != 0 && token.Data == Keywords.Important)
            {
                var previous = _values[_values.Count - 1];
                if (IsExclamationMark(previous))
                {
                    do
                    {
                        _values.RemoveAt(_values.Count - 1);
                    } while (_values.Count > 0 && _values[_values.Count - 1].Type == TokenType.Whitespace);

                    return true;
                }
            }

            Add(token);
            return IsImportant;
        }

        private void Add(Token token)
        {
            if (_buffer != null && !IsCommaOrSlash(token))
                _values.Add(_buffer);
            else if (_values.Count != 0 && !IsComma(token) && IsComma(_values[_values.Count - 1]))
                _values.Add(Token.Whitespace);

            _buffer = null;

            if (IsImportant) _valid = false;
            _values.Add(token);
        }

        private static bool IsCommaOrSlash(Token token)
        {
            return IsComma(token) || IsSlash(token);
        }

        private static bool IsComma(Token token)
        {
            return token.Type == TokenType.Comma;
        }

        private static bool IsExclamationMark(Token token)
        {
            return token.Type == TokenType.Delim && token.Data.Has(Symbols.ExclamationMark);
        }

        private static bool IsSlash(Token token)
        {
            return token.Type == TokenType.Delim && token.Data.Has(Symbols.Solidus);
        }
    }
}