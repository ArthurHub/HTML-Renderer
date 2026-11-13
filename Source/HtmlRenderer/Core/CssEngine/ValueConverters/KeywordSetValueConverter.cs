using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a value that is a single identifier from a fixed keyword set (matched case-insensitively)
    /// and preserves the authored text verbatim. Used by the plain keyword properties that have no
    /// dedicated value model (<c>isolation</c>, <c>pointer-events</c>, <c>user-select</c>, ...).
    /// </summary>
    internal sealed class KeywordSetValueConverter : TextValueConverter
    {
        private readonly string[] _keywords;

        public KeywordSetValueConverter(params string[] keywords)
        {
            _keywords = keywords;
        }

        protected override bool IsValid(Token[] tokens)
        {
            return tokens.Length == 1 && tokens[0].Type == TokenType.Ident &&
                   _keywords.Any(k => tokens[0].Data.Isi(k));
        }
    }

    /// <summary>
    /// <c>touch-action</c> (Pointer Events 3): <c>auto | none | manipulation |
    /// [ pan-x | pan-left | pan-right ] || [ pan-y | pan-up | pan-down ] || pinch-zoom</c>.
    /// </summary>
    internal sealed class TouchActionValueConverter : TextValueConverter
    {
        private static readonly string[] Horizontal = {"pan-x", "pan-left", "pan-right"};
        private static readonly string[] Vertical = {"pan-y", "pan-up", "pan-down"};

        protected override bool IsValid(Token[] tokens)
        {
            if (tokens.Length == 0 || tokens.Any(t => t.Type != TokenType.Ident)) return false;

            if (tokens.Length == 1 &&
                (tokens[0].Data.Isi(Keywords.Auto) || tokens[0].Data.Isi(Keywords.None) ||
                 tokens[0].Data.Isi("manipulation")))
            {
                return true;
            }

            if (tokens.Length > 3) return false;

            var horizontal = false;
            var vertical = false;
            var pinch = false;

            foreach (var token in tokens)
            {
                if (Horizontal.Any(k => token.Data.Isi(k)))
                {
                    if (horizontal) return false;
                    horizontal = true;
                }
                else if (Vertical.Any(k => token.Data.Isi(k)))
                {
                    if (vertical) return false;
                    vertical = true;
                }
                else if (token.Data.Isi("pinch-zoom"))
                {
                    if (pinch) return false;
                    pinch = true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// <c>math-depth</c> (MathML Core): <c>auto-add | add(&lt;integer&gt;) | &lt;integer&gt;</c>.
    /// </summary>
    internal sealed class MathDepthValueConverter : TextValueConverter
    {
        protected override bool IsValid(Token[] tokens)
        {
            if (tokens.Length != 1) return false;

            var token = tokens[0];

            switch (token.Type)
            {
                case TokenType.Ident:
                    return token.Data.Isi("auto-add");
                case TokenType.Number:
                    return IsInteger(token);
                case TokenType.Function:
                    var arguments = ((FunctionToken) token).ArgumentTokens
                        .Where(t => t.Type != TokenType.Whitespace).ToArray();
                    return token.Data.Isi("add") && arguments.Length == 1 && IsInteger(arguments[0]);
                default:
                    return false;
            }
        }

        private static bool IsInteger(Token token)
        {
            return token.Type == TokenType.Number && ((NumberToken) token).IsInteger;
        }
    }

    /// <summary>Base for converters that validate a token sequence and keep the authored text.</summary>
    internal abstract class TextValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            return IsValid(tokens) ? new TextValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<TextValue>();
        }

        protected abstract bool IsValid(Token[] tokens);

        private sealed class TextValue : IPropertyValue
        {
            public TextValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
