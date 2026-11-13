using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>text-underline-position</c> (CSS Text Decoration 4 2.5):
    /// <c>auto | [ from-font | under ] || [ left | right ]</c>. <c>auto</c> stands alone.
    /// </summary>
    internal static class TextUnderlinePositionGrammar
    {
        public static bool TryParse(IReadOnlyList<Token> tokens)
        {
            var significant = TokenGrammarValueConverter.Significant(tokens);

            if (significant.Count is 0 or > 2) return false;

            if (significant.Count == 1 && IsIdent(significant[0], Keywords.Auto)) return true;

            var hasPosition = false;
            var hasSide = false;

            foreach (var token in significant)
            {
                if (IsIdent(token, "from-font") || IsIdent(token, "under"))
                {
                    if (hasPosition) return false;
                    hasPosition = true;
                }
                else if (IsIdent(token, Keywords.Left) || IsIdent(token, Keywords.Right))
                {
                    if (hasSide) return false;
                    hasSide = true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdent(Token token, string keyword) =>
            token.Type == TokenType.Ident && token.Data.Isi(keyword);
    }
}
