using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates the CSS Borders 4 <c>&lt;corner-shape-value&gt;</c>:
    /// <c>round | scoop | bevel | notch | square | squircle | superellipse(&lt;number&gt; | infinity | -infinity)</c>.
    /// The value is stored in its canonical form (keyword lower-cased, <c>superellipse()</c> argument normalized).
    /// </summary>
    internal sealed class CornerShapeValueConverter : IValueConverter
    {
        private static readonly string[] ShapeKeywords =
        {
            Keywords.Round, "scoop", Keywords.Bevel, "notch", Keywords.Square, "squircle"
        };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var text = Normalize(value);

            return text == null ? null : new CornerShapeValue(text, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<CornerShapeValue>();
        }

        private static string Normalize(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length != 1) return null;

            var token = tokens[0];

            if (token.Type == TokenType.Ident)
            {
                return ShapeKeywords.FirstOrDefault(keyword => keyword.Isi(token.Data));
            }

            if (token is FunctionToken function && function.Data.Isi("superellipse"))
            {
                var arguments = function.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

                if (arguments.Length != 1) return null;

                var argument = arguments[0];

                if (argument is NumberToken number)
                {
                    var parsed = double.Parse(number.Data, NumberStyles.Float, CultureInfo.InvariantCulture);

                    return "superellipse(" + parsed.ToString("R", CultureInfo.InvariantCulture) + ")";
                }

                if (argument.Type == TokenType.Ident)
                {
                    if (argument.Data.Isi("infinity")) return "superellipse(infinity)";
                    if (argument.Data.Isi("-infinity")) return "superellipse(-infinity)";
                }
            }

            return null;
        }

        private sealed class CornerShapeValue : IPropertyValue
        {
            public CornerShapeValue(string text, IEnumerable<Token> tokens)
            {
                CssText = text;
                Original = new TokenValue(tokens);
            }

            public string CssText { get; }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
