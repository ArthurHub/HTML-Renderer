using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>&lt;percentage [0,∞]&gt;</c> form of <c>font-stretch</c> (CSS Fonts 4 3.3), accepted next to
    /// the nine width keywords. The <c>font</c> shorthand takes the keywords only.
    /// </summary>
    internal static class FontStretchGrammar
    {
        public static readonly IValueConverter PercentageConverter = new FontGrammarValueConverter(TryParsePercentage);

        public static bool TryParsePercentage(IReadOnlyList<Token> tokens, out string text)
        {
            text = null;

            if (tokens.Count != 1 || !(tokens[0] is UnitToken percentage) ||
                percentage.Type != TokenType.Percentage || percentage.Value < 0f)
            {
                return false;
            }

            text = percentage.ToValue();
            return true;
        }
    }
}
