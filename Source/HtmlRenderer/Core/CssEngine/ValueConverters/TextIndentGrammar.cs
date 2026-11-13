using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>text-indent</c> (CSS Text 3 3): <c>&lt;length-percentage&gt; &amp;&amp; hanging? &amp;&amp; each-line?</c>.
    /// The length-percentage (including <c>calc()</c>) is required; <c>hanging</c> and <c>each-line</c> may each
    /// appear once, in any order around it.
    /// </summary>
    internal static class TextIndentGrammar
    {
        public static bool TryParse(IReadOnlyList<Token> tokens)
        {
            var hasLength = false;
            var hasHanging = false;
            var hasEachLine = false;

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Whitespace) continue;

                if (token.Type == TokenType.Ident && token.Data.Isi("hanging"))
                {
                    if (hasHanging) return false;
                    hasHanging = true;
                }
                else if (token.Type == TokenType.Ident && token.Data.Isi("each-line"))
                {
                    if (hasEachLine) return false;
                    hasEachLine = true;
                }
                else
                {
                    if (hasLength) return false;
                    if (Converters.LengthOrPercentConverter.Convert(new[] { token }) == null) return false;
                    hasLength = true;
                }
            }

            return hasLength;
        }
    }
}
