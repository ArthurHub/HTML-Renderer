using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Grammars of the CSS Text Decoration 4 text-emphasis and decoration-skip/inset properties.
    /// </summary>
    internal static class TextEmphasisGrammar
    {
        private static bool IsIdent(Token token, string keyword) =>
            token.Type == TokenType.Ident && token.Data.Isi(keyword);

        /// <summary>
        /// <c>text-emphasis-style</c> (4.1):
        /// <c>none | [ [ filled | open ] || [ dot | circle | double-circle | triangle | sesame ] ] | &lt;string&gt;</c>.
        /// </summary>
        public static bool TryParseStyle(IReadOnlyList<Token> tokens)
        {
            var significant = TokenGrammarValueConverter.Significant(tokens);

            if (significant.Count == 0) return false;

            if (significant.Count == 1)
            {
                if (IsIdent(significant[0], Keywords.None)) return true;

                if (significant[0].Type == TokenType.String) return significant[0].Data.Length > 0;
            }

            var hasFill = false;
            var hasShape = false;

            foreach (var token in significant)
            {
                if (token.Type != TokenType.Ident) return false;

                if (IsIdent(token, "filled") || IsIdent(token, "open"))
                {
                    if (hasFill) return false;
                    hasFill = true;
                }
                else if (IsIdent(token, "dot") || IsIdent(token, "circle") || IsIdent(token, "double-circle") ||
                         IsIdent(token, "triangle") || IsIdent(token, "sesame"))
                {
                    if (hasShape) return false;
                    hasShape = true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary><c>text-emphasis-position</c> (4.3): <c>[ over | under ] &amp;&amp; [ right | left ]?</c>.</summary>
        public static bool TryParsePosition(IReadOnlyList<Token> tokens)
        {
            var significant = TokenGrammarValueConverter.Significant(tokens);

            if (significant.Count is < 1 or > 2) return false;

            var hasLine = false;
            var hasSide = false;

            foreach (var token in significant)
            {
                if (IsIdent(token, "over") || IsIdent(token, "under"))
                {
                    if (hasLine) return false;
                    hasLine = true;
                }
                else if (IsIdent(token, Keywords.Right) || IsIdent(token, Keywords.Left))
                {
                    if (hasSide) return false;
                    hasSide = true;
                }
                else
                {
                    return false;
                }
            }

            // over/under is required: a side alone is not a value.
            return hasLine;
        }

        /// <summary>
        /// <c>text-decoration-skip</c> (Text Decoration 3 2.4, with the level 4 <c>ink</c>):
        /// <c>none | auto | [ objects || [ spaces | [ leading-spaces || trailing-spaces ] ] || edges || box-decoration || ink ]</c>.
        /// </summary>
        public static bool TryParseSkip(IReadOnlyList<Token> tokens)
        {
            var significant = TokenGrammarValueConverter.Significant(tokens);

            if (significant.Count == 0) return false;

            if (significant.Count == 1 && (IsIdent(significant[0], Keywords.None) || IsIdent(significant[0], Keywords.Auto)))
                return true;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var token in significant)
            {
                if (token.Type != TokenType.Ident) return false;

                var name = token.Data;

                if (!(name.Isi("objects") || name.Isi("spaces") || name.Isi("leading-spaces") ||
                      name.Isi("trailing-spaces") || name.Isi("edges") || name.Isi("box-decoration") ||
                      name.Isi("ink")))
                    return false;

                if (!seen.Add(name)) return false;
            }

            // spaces is an alternative to the two partial-space keywords, not a companion.
            return !(seen.Contains("spaces") && (seen.Contains("leading-spaces") || seen.Contains("trailing-spaces")));
        }

        /// <summary><c>text-decoration-inset</c> (2.7): <c>auto | &lt;length&gt;{1,2}</c>.</summary>
        public static bool TryParseInset(IReadOnlyList<Token> tokens)
        {
            var significant = TokenGrammarValueConverter.Significant(tokens);

            if (significant.Count is 0 or > 2) return false;

            if (significant.Count == 1 && IsIdent(significant[0], Keywords.Auto)) return true;

            foreach (var token in significant)
            {
                if (Converters.LengthConverter.Convert(new[] { token }) == null) return false;
            }

            return true;
        }
    }
}
