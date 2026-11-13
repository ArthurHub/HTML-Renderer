using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>color-scheme</c> grammar (CSS Color Adjustment 1 2):
    /// <c>normal | [ light | dark | &lt;custom-ident&gt; ]+ &amp;&amp; only?</c>.
    /// </summary>
    internal static class ColorSchemeGrammar
    {
        /// <summary>Validates whitespace-free tokens against the grammar.</summary>
        internal static bool TryParse(IReadOnlyList<Token> tokens)
        {
            if (tokens == null || tokens.Count == 0) return false;

            if (tokens.Count == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.Normal))
                return true;

            var only = false;
            var schemes = 0;

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.Type != TokenType.Ident) return false;

                var name = token.Data;

                if (name.Isi(Keywords.Only))
                {
                    // "only" is written once, at either end of the list.
                    if (only || (i != 0 && i != tokens.Count - 1)) return false;
                    only = true;
                    continue;
                }

                if (IsReserved(name)) return false;

                schemes++;
            }

            return schemes > 0;
        }

        // "normal", the CSS-wide keywords and "default" are not <custom-ident>s (CSS Values 4 4.2).
        private static bool IsReserved(string name) =>
            name.Isi(Keywords.Normal) || name.Isi(Keywords.Initial) || name.Isi(Keywords.Inherit) ||
            name.Isi(Keywords.Unset) || name.Isi("revert") || name.Isi("revert-layer") || name.Isi("default");
    }
}
