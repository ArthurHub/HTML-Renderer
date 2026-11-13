using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The grammar for <c>initial-letter</c> (CSS Inline 3 7.4):
    /// <c>normal | &lt;number [1,inf]&gt; &lt;integer [1,inf]&gt; | &lt;number [1,inf]&gt; [ drop | raise ]?</c>.
    /// </summary>
    internal static class InitialLetterGrammar
    {
        /// <summary>
        /// Parses the tokens. <paramref name="size"/> is the number of lines the letter's height spans and
        /// <paramref name="sink"/> the line its baseline sits on; both are 0 for <c>normal</c>.
        /// </summary>
        internal static bool TryParse(IEnumerable<Token> tokens, out double size, out int sink)
        {
            size = 0;
            sink = 0;

            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (toks.Length == 1 && toks[0].Type == TokenType.Ident && toks[0].Data.Isi(Keywords.Normal)) return true;
            if (toks.Length == 0 || toks.Length > 2) return false;

            if (!(toks[0] is NumberToken sizeToken) || sizeToken.Value < 1f) return false;

            var parsedSize = (double)sizeToken.Value;
            var parsedSink = (int)Math.Floor(parsedSize);

            if (toks.Length == 2)
            {
                if (toks[1].Type == TokenType.Ident && toks[1].Data.Isi("drop"))
                {
                }
                else if (toks[1].Type == TokenType.Ident && toks[1].Data.Isi("raise"))
                {
                    parsedSink = 1;
                }
                else if (toks[1] is NumberToken sinkToken && sinkToken.IsInteger && sinkToken.IntegerValue >= 1
                         && !sinkToken.Data.StartsWith("+", StringComparison.Ordinal))
                {
                    parsedSink = sinkToken.IntegerValue;
                }
                else
                {
                    return false;
                }
            }

            size = parsedSize;
            sink = parsedSink;
            return true;
        }

        internal static bool IsValid(IReadOnlyList<Token> tokens) => TryParse(tokens, out _, out _);
    }
}
