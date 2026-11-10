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
    /// Turns the <c>content</c> value of a <c>::before</c>/<c>::after</c> box into its <see cref="CssBox.Text"/>
    /// (CSS 2.1 §12.2, CSS Generated Content 3 §2). Components it doesn't know are skipped.
    /// </summary>
    internal static class CssContentEngine
    {
        /// <summary>
        /// Sets the text of a pseudo-element box from its <c>content</c>, other boxes are left alone.
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

            // The CSS lexer decodes string escapes such as "\A"
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
                else if (token is FunctionToken counterFunctionToken &&
                         counterFunctionToken.Data.Equals(FunctionNames.Counter, StringComparison.OrdinalIgnoreCase))
                {
                    // ValueExtensions.ToList, not LINQ's: one token list per comma-separated argument
                    var counterArgs = counterFunctionToken.ArgumentTokens.ToList();
                    if (counterArgs.Count > 0 && counterArgs[0].FirstOrDefault() is KeywordToken counterNameToken)
                    {
                        var style = counterArgs.Count > 1 && counterArgs[1].FirstOrDefault() is KeywordToken styleToken
                            ? styleToken.Data
                            : CssConstants.Decimal;

                        // The pseudo-element has its own counter snapshot, a counter not in scope reads as 0
                        // (CSS Lists 3 §4.7)
                        int counterValue;
                        if (!box.Counters.TryGetValue(counterNameToken.Data, out counterValue))
                        {
                            counterValue = 0;
                        }
                        sb.Append(CssCounterEngine.FormatCounterValue(counterValue, style));
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
                }
            }

            return sb.ToString();
        }
    }
}
