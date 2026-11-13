using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value grammar of <c>font-size-adjust</c> (CSS Fonts 5 3.2):
    /// <c>none | [ ex-height | cap-height | ch-width | ic-width | ic-height ]? [ from-font | &lt;number [0,∞]&gt; ]</c>.
    /// </summary>
    internal static class FontSizeAdjustGrammar
    {
        private static readonly string[] Metrics = {"ex-height", "cap-height", "ch-width", "ic-width", "ic-height"};

        public static readonly IValueConverter Converter = new FontGrammarValueConverter(TryParse);

        public static bool TryParse(IReadOnlyList<Token> tokens, out string text)
        {
            text = null;

            if (tokens.Count == 0 || tokens.Count > 2) return false;

            if (tokens.Count == 1 && FontGrammar.IsIdent(tokens[0], Keywords.None))
            {
                text = Keywords.None;
                return true;
            }

            var index = 0;
            var metric = string.Empty;

            if (tokens[0].Type == TokenType.Ident && FontGrammar.Contains(Metrics, tokens[0].Data))
            {
                metric = tokens[0].Data.ToLowerInvariant() + " ";
                index = 1;
            }

            if (index != tokens.Count - 1) return false;

            var factor = tokens[index];

            if (FontGrammar.IsIdent(factor, "from-font"))
            {
                text = metric + "from-font";
                return true;
            }

            if (factor is NumberToken number && number.Value >= 0f)
            {
                text = metric + number.Data;
                return true;
            }

            return false;
        }
    }
}
