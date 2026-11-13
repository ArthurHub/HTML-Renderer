using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Shared length checks for the properties whose grammar takes a non-negative <c>&lt;length&gt;</c>
    /// (<c>line-height-step</c>, <c>block-step-size</c>, <c>tab-size</c>).
    /// </summary>
    internal static class NonNegativeLengthGrammar
    {
        /// <summary>
        /// A <c>&lt;length [0,inf]&gt;</c>: a dimension or bare zero that is not negative, or a calc() expression
        /// (whose sign can't be checked at parse time). Percentages are not lengths.
        /// </summary>
        internal static bool IsNonNegativeLength(IEnumerable<Token> tokens)
        {
            var all = tokens.ToArray();
            var significant = all.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (significant.Length == 0 || Converters.LengthConverter.Convert(all) == null) return false;

            if (significant.Length == 1)
            {
                if (significant[0] is UnitToken unit) return unit.Value >= 0f;
                if (significant[0] is NumberToken number) return number.Value == 0f;
            }

            return true;
        }

        /// <summary><c>tab-size</c> (CSS Text 4 3.6): <c>&lt;number [0,inf]&gt; | &lt;length [0,inf]&gt;</c>.</summary>
        internal static bool IsTabSize(IReadOnlyList<Token> tokens)
        {
            var significant = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (significant.Length == 1 && significant[0] is NumberToken number) return number.Value >= 0f;

            return IsNonNegativeLength(tokens);
        }
    }

    /// <summary>The over or under edge <c>text-box-edge</c> trims to (CSS Inline 3 5.2).</summary>
    internal enum TextEdgeMetric
    {
        Text,
        Cap,
        Ex,
        Alphabetic,
        Ideographic,
        IdeographicInk
    }

    /// <summary>
    /// The grammars of the CSS Rhythmic Sizing and CSS Inline 3 line-box properties (<c>line-height-step</c>,
    /// <c>block-step-*</c>, <c>text-box-trim</c>, <c>text-box-edge</c>).
    /// </summary>
    internal static class RhythmGrammar
    {
        private static readonly string[] OverKeywords = {"text", "cap", "ex", "ideographic", "ideographic-ink"};
        private static readonly string[] UnderKeywords = {"text", "alphabetic", "ideographic", "ideographic-ink"};

        /// <summary><c>none | &lt;length [0,inf]&gt;</c>.</summary>
        internal static bool IsNoneOrLength(IReadOnlyList<Token> tokens)
        {
            var significant = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (significant.Length == 1 && significant[0].Type == TokenType.Ident)
            {
                return significant[0].Data.Isi(Keywords.None);
            }

            return NonNegativeLengthGrammar.IsNonNegativeLength(tokens);
        }

        /// <summary>
        /// <c>auto | [ text | cap | ex | ideographic | ideographic-ink ] [ text | alphabetic | ideographic |
        /// ideographic-ink ]?</c> (CSS Inline 3 5.2).
        /// </summary>
        internal static bool IsTextBoxEdge(IReadOnlyList<Token> tokens)
        {
            var toks = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (toks.Length == 0 || toks.Length > 2 || toks.Any(t => t.Type != TokenType.Ident)) return false;
            if (toks.Length == 1 && toks[0].Data.Isi(Keywords.Auto)) return true;

            return OverKeywords.Any(k => toks[0].Data.Isi(k)) &&
                   (toks.Length == 1 || UnderKeywords.Any(k => toks[1].Data.Isi(k)));
        }

        /// <summary>The over and under metrics of a valid, non-<c>auto</c> <c>text-box-edge</c>.</summary>
        internal static bool TryParseEdgeMetrics(IEnumerable<Token> tokens, out TextEdgeMetric over, out TextEdgeMetric under)
        {
            over = TextEdgeMetric.Text;
            under = TextEdgeMetric.Text;

            var list = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
            if (!IsTextBoxEdge(list) || list[0].Data.Isi(Keywords.Auto)) return false;

            over = MetricOf(list[0].Data);

            // One value: both edges use it, except cap and ex, whose under edge is the alphabetic baseline.
            under = list.Length == 2
                ? MetricOf(list[1].Data)
                : over == TextEdgeMetric.Cap || over == TextEdgeMetric.Ex ? TextEdgeMetric.Alphabetic : over;

            return true;
        }

        private static TextEdgeMetric MetricOf(string keyword)
        {
            if (keyword.Isi("cap")) return TextEdgeMetric.Cap;
            if (keyword.Isi("ex")) return TextEdgeMetric.Ex;
            if (keyword.Isi("alphabetic")) return TextEdgeMetric.Alphabetic;
            if (keyword.Isi("ideographic")) return TextEdgeMetric.Ideographic;
            if (keyword.Isi("ideographic-ink")) return TextEdgeMetric.IdeographicInk;

            return TextEdgeMetric.Text;
        }
    }
}
