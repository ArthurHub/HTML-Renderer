using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Grammar of <c>image-resolution</c> (CSS Images 4 §6.1): <c>[ from-image || &lt;resolution&gt; ] &amp;&amp; snap?</c>.
    /// The resolution must be positive.
    /// </summary>
    internal static class ImageResolutionGrammar
    {
        private const string FromImage = "from-image";
        private const string Snap = "snap";

        internal static bool IsValid(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length < 1 || tokens.Length > 3) return false;

            var fromImage = false;
            var snap = false;
            var resolution = false;

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Ident && token.Data.Isi(FromImage) && !fromImage)
                {
                    fromImage = true;
                }
                else if (token.Type == TokenType.Ident && token.Data.Isi(Snap) && !snap)
                {
                    snap = true;
                }
                else if (!resolution && token.Type == TokenType.Dimension && token is UnitToken unit &&
                         unit.Value > 0f && Resolution.GetUnit(unit.Unit.ToLowerInvariant()) != Resolution.Unit.None)
                {
                    resolution = true;
                }
                else
                {
                    return false;
                }
            }

            // "snap" only modifies a resolution or from-image; it is not a value on its own.
            return fromImage || resolution;
        }
    }
}
