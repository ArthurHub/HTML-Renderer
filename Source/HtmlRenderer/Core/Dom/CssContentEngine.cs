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

using System.Text;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Resolves a synthesized <c>::before</c>/<c>::after</c> pseudo-element box's <see cref="CssBox.Content"/>
    /// value into literal text, so it renders through the box's normal <see cref="CssBox.Text"/>/
    /// <see cref="CssBox.ParseToWords"/> machinery like any other text box.<br/>
    /// Deliberately a small, self-contained subset of PeachPDF's CssContentEngine: literal strings,
    /// <c>attr()</c>, and the four quote keywords are supported; <c>counter()</c>/<c>counters()</c>/
    /// <c>string()</c>/<c>content()</c> are not (CSS counters and named strings remain out of scope
    /// for this backport - an unrecognized component is simply skipped, the same graceful-ignore
    /// behavior this engine already applies to unsupported CSS elsewhere).
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
            var i = 0;
            while (i < content.Length)
            {
                if (char.IsWhiteSpace(content[i]))
                {
                    i++;
                    continue;
                }

                if (content[i] == '"' || content[i] == '\'')
                {
                    var quote = content[i];
                    var start = i + 1;
                    var j = start;
                    while (j < content.Length && content[j] != quote)
                    {
                        if (content[j] == '\\' && j + 1 < content.Length)
                            j++;
                        j++;
                    }
                    sb.Append(Unescape(content.Substring(start, j - start)));
                    i = j + 1;
                    continue;
                }

                var tokenEnd = i;
                while (tokenEnd < content.Length && !char.IsWhiteSpace(content[tokenEnd]))
                    tokenEnd++;
                var token = content.Substring(i, tokenEnd - i);
                i = tokenEnd;

                if (token.StartsWith("attr(", System.StringComparison.OrdinalIgnoreCase) && token.EndsWith(")"))
                {
                    var attrName = token.Substring(5, token.Length - 6).Trim();
                    var sourceBox = box.IsPseudoElement && box.ParentBox != null ? box.ParentBox : box;
                    var attrValue = sourceBox.GetAttribute(attrName, string.Empty);
                    sb.Append(attrValue);
                }
                else if (token.Equals("open-quote", System.StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append('“');
                }
                else if (token.Equals("close-quote", System.StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append('”');
                }
                else if (token.Equals("no-open-quote", System.StringComparison.OrdinalIgnoreCase) ||
                         token.Equals("no-close-quote", System.StringComparison.OrdinalIgnoreCase))
                {
                    // produces no text, but still counts as a quoting depth change per spec - depth
                    // tracking isn't implemented here (no "quotes" property support), so this is a no-op.
                }
                // counter()/counters()/string()/content() and anything else unrecognized: skip.
            }

            return sb.ToString();
        }

        private static string Unescape(string s)
        {
            if (s.IndexOf('\\') < 0) return s;

            var sb = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    i++;
                    sb.Append(s[i]);
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }
    }
}
