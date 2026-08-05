// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Linq;
using System.Text;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Resolves a synthesized <c>::before</c>/<c>::after</c> pseudo-element box's <see cref="CssBox.Content"/>
    /// value into literal text, so it renders through the box's normal <see cref="CssBox.Text"/>/
    /// <see cref="CssBox.ParseToWords"/> machinery like any other text box.<br/>
    /// Deliberately a small, self-contained subset of the full content grammar: literal strings,
    /// <c>attr()</c>, and the four quote keywords are supported; <c>counter()</c>/<c>counters()</c>/
    /// <c>string()</c>/<c>content()</c> are not (CSS counters and named strings remain out of scope
    /// for this backport - an unrecognized component is simply skipped, the same graceful-ignore
    /// behavior this engine already applies to unsupported CSS elsewhere).<br/>
    /// Tokenizes via the shared CSS lexer (<see cref="CssValueParser.GetCssTokens"/>) rather than
    /// hand-parsing the CSS text, so string literals (e.g. <c>"\A"</c>, the standard UA-stylesheet
    /// line-break trick used by <c>br:before</c>) get the same correct escape decoding the lexer
    /// already applies when the stylesheet is first parsed - see CalcParser/BackgroundPositionGrammar
    /// for the same "don't write two independent parsers for the same grammar" precedent.
    /// </summary>
    internal static class CssContentEngine
    {
        /// <summary>
        /// If <paramref name="box"/> is a synthesized pseudo-element with a resolved <c>content</c> value,
        /// sets its <see cref="CssBox.Text"/> to the resolved literal text. No-op for any other box, or
        /// if content is <c>none</c>/<c>normal</c>/empty.
        /// </summary>
        public static void ApplyContent(CssBox box)
        {
            if (!box.IsPseudoElement) return;
            if (string.IsNullOrEmpty(box.Content)) return;

            var content = box.Content.Trim();
            if (content.Length == 0 || content == CssConstants.None || content == CssConstants.Normal) return;

            var text = Resolve(box, content);
            if (!string.IsNullOrEmpty(text))
            {
                box.Text = text;
            }
        }

        private static string Resolve(CssBox box, string content)
        {
            var sb = new StringBuilder();

            foreach (var token in CssValueParser.GetCssTokens(content))
            {
                if (token is StringToken stringToken)
                {
                    sb.Append(stringToken.Data);
                }
                else if (token is FunctionToken functionToken &&
                         functionToken.Data.Equals(FunctionNames.Attr, StringComparison.OrdinalIgnoreCase))
                {
                    if (functionToken.ArgumentTokens.FirstOrDefault() is KeywordToken attrNameToken)
                    {
                        var sourceBox = box.IsPseudoElement && box.ParentBox != null ? box.ParentBox : box;
                        var attrValue = sourceBox.GetAttribute(attrNameToken.Data, string.Empty);
                        sb.Append(attrValue);
                    }
                }
                else if (token is KeywordToken keywordToken)
                {
                    if (keywordToken.Data.Equals(Keywords.OpenQuote, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('“');
                    }
                    else if (keywordToken.Data.Equals(Keywords.CloseQuote, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('”');
                    }
                    // no-open-quote/no-close-quote produce no text, but still count as a quoting depth
                    // change per spec - depth tracking isn't implemented here (no "quotes" property
                    // support), so they (like any other unrecognized keyword) are a no-op.
                }
                // counter()/counters()/string()/content() and anything else unrecognized: skip.
            }

            return sb.ToString();
        }
    }
}
