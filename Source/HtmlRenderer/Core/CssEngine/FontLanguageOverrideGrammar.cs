using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value grammar of <c>font-language-override</c> (CSS Fonts 4 6.6): <c>normal | &lt;string&gt;</c>,
    /// the string being an OpenType language system tag of one to four printable ASCII characters.
    /// </summary>
    internal static class FontLanguageOverrideGrammar
    {
        public static readonly IValueConverter Converter = new FontGrammarValueConverter(TryParse);

        public static bool TryParse(IReadOnlyList<Token> tokens, out string text)
        {
            text = null;

            if (tokens.Count != 1) return false;

            if (FontGrammar.IsIdent(tokens[0], Keywords.Normal))
            {
                text = Keywords.Normal;
                return true;
            }

            if (tokens[0].Type != TokenType.String) return false;

            var data = tokens[0].Data;

            if (data.Length < 1 || data.Length > 4 || !data.All(FontGrammar.IsPrintableAscii)) return false;

            text = FontGrammar.Quote(data);
            return true;
        }
    }
}
