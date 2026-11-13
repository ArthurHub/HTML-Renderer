using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The grammar of the <c>overflow-clip-margin</c> properties (CSS Overflow 4 §3.2):
    /// <c>&lt;visual-box&gt; || &lt;length [0,∞]&gt;</c>.
    /// </summary>
    internal static class OverflowClipMarginGrammar
    {
        /// <summary>One <c>&lt;visual-box&gt;</c> keyword, one length, or both in either order; each at most once.</summary>
        internal static bool TryParse(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length is 0 or > 2) return false;

            var boxes = 0;
            var lengths = 0;

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Ident)
                {
                    if (!IsVisualBox(token.Data)) return false;

                    boxes++;
                }
                else if (ContainGrammar.IsNonNegativeLength(token))
                {
                    lengths++;
                }
                else
                {
                    return false;
                }
            }

            return boxes <= 1 && lengths <= 1;
        }

        private static bool IsVisualBox(string keyword) =>
            keyword.Isi(Keywords.ContentBox) || keyword.Isi(Keywords.PaddingBox) || keyword.Isi(Keywords.BorderBox);
    }
}
