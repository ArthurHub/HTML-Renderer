using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The punctuation a <c>hanging-punctuation</c> value lets hang outside the line box.</summary>
    [Flags]
    internal enum HangingPunctuationFlags
    {
        None = 0,
        First = 1,
        Last = 2,
        AllowEnd = 4,
        ForceEnd = 8
    }

    /// <summary>
    /// The grammar for <c>hanging-punctuation</c> (CSS Text 3 8):
    /// <c>none | [ first || [ force-end | allow-end ] || last ]</c>.
    /// </summary>
    internal static class HangingPunctuationGrammar
    {
        internal static bool TryParse(IEnumerable<Token> tokens, out HangingPunctuationFlags flags)
        {
            flags = HangingPunctuationFlags.None;

            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (toks.Length == 0) return false;

            if (toks.Length == 1 && toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.None)) return true;

            foreach (var token in toks)
            {
                if (token.Type != TokenType.Ident) return false;

                var flag = token.Data.Isi("first") ? HangingPunctuationFlags.First
                    : token.Data.Isi("last") ? HangingPunctuationFlags.Last
                    : token.Data.Isi("allow-end") ? HangingPunctuationFlags.AllowEnd
                    : token.Data.Isi("force-end") ? HangingPunctuationFlags.ForceEnd
                    : HangingPunctuationFlags.None;

                // "||" permits each component once; allow-end and force-end are exclusive alternatives.
                if (flag == HangingPunctuationFlags.None || (flags & flag) != 0) return false;

                flags |= flag;
            }

            const HangingPunctuationFlags both = HangingPunctuationFlags.AllowEnd | HangingPunctuationFlags.ForceEnd;
            return (flags & both) != both;
        }

        internal static bool IsValid(IReadOnlyList<Token> tokens) => TryParse(tokens, out _);
    }
}
