using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FunctionToken : Token, IEnumerable<Token>
    {
        private readonly List<Token> _arguments;

        public FunctionToken(string data, TextPosition position)
            : base(TokenType.Function, data, position)
        {
            _arguments = new List<Token>();
        }

        public IEnumerable<Token> ArgumentTokens
        {
            get
            {
                var final = _arguments.Count - 1;

                if (final >= 0 && _arguments[final].Type == TokenType.RoundBracketClose) final--;

                return _arguments.Take(1 + final);
            }
        }

        public override string ToValue()
        {
            return string.Concat(Data, "(", _arguments.ToText());
        }

        public void AddArgumentToken(Token token)
        {
            _arguments.Add(token);
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}