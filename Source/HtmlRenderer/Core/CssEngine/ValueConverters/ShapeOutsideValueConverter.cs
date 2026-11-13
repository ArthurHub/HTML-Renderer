using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates <c>shape-outside</c> (CSS Shapes 1 §2.1):
    /// <c>none | [ &lt;basic-shape&gt; || &lt;shape-box&gt; ] | &lt;image&gt;</c>, where
    /// <c>&lt;shape-box&gt; = &lt;box&gt; | margin-box</c>. The basic shapes are those <c>clip-path</c> accepts
    /// (<see cref="ClipPathValueConverter"/>), and <c>&lt;image&gt;</c> is any image source. The authored text is preserved.
    /// </summary>
    internal sealed class ShapeOutsideValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return IsValid(value.Where(t => t.Type != TokenType.Whitespace).ToArray())
                ? new GrammarValue(value)
                : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GrammarValue>();
        }

        private static bool IsValid(Token[] tokens)
        {
            if (tokens.Length == 0) return false;

            if (tokens.Length == 1)
            {
                var token = tokens[0];

                if (token.Type == TokenType.Ident && (token.Data.Isi(Keywords.None) || IsShapeBox(token))) return true;
                if (ClipPathValueConverter.IsBasicShape(tokens)) return true;

                return Converters.ImageSourceConverter.Convert(tokens) != null;
            }

            // <basic-shape> || <shape-box>, in either order.
            if (tokens.Length == 2)
            {
                return (IsShapeBox(tokens[0]) && ClipPathValueConverter.IsBasicShape(new[] { tokens[1] })) ||
                       (IsShapeBox(tokens[1]) && ClipPathValueConverter.IsBasicShape(new[] { tokens[0] }));
            }

            return false;
        }

        private static bool IsShapeBox(Token token)
        {
            return token.Type == TokenType.Ident &&
                   (token.Data.Isi(Keywords.BorderBox) || token.Data.Isi(Keywords.PaddingBox) ||
                    token.Data.Isi(Keywords.ContentBox) || token.Data.Isi("margin-box"));
        }
    }
}
