using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates the <c>system</c> descriptor of <c>@counter-style</c> (CSS Counter Styles Level 3 §2.1):
    /// <c>cyclic | numeric | alphabetic | symbolic | additive | [fixed &lt;integer&gt;?] | [extends &lt;counter-style-name&gt;]</c>.
    /// Anything else is dropped at parse time; the authored value text is preserved verbatim.
    /// </summary>
    internal sealed class CounterSystemValueConverter : IValueConverter
    {
        // Idents that can never be a counter-style-name.
        private static readonly string[] ReservedNames =
            { "none", "inherit", "initial", "unset", "revert", "revert-layer", "default" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length == 0 || tokens[0].Type != TokenType.Ident) return null;

            var keyword = tokens[0].Data;
            var valid = false;

            if (tokens.Length == 1)
            {
                valid = keyword.Isi("cyclic") || keyword.Isi("numeric") || keyword.Isi("alphabetic") ||
                        keyword.Isi("symbolic") || keyword.Isi("additive") || keyword.Isi("fixed");
            }
            else if (tokens.Length == 2)
            {
                if (keyword.Isi("fixed"))
                {
                    valid = tokens[1] is NumberToken { IsInteger: true };
                }
                else if (keyword.Isi("extends"))
                {
                    valid = tokens[1].Type == TokenType.Ident &&
                            !ReservedNames.Any(n => n.Isi(tokens[1].Data));
                }
            }

            return valid ? new CounterSystemValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<CounterSystemValue>();
        }

        private sealed class CounterSystemValue : IPropertyValue
        {
            public CounterSystemValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
