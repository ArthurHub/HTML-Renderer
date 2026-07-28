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
using System.Collections.Generic;
using System.IO;
using System.Text;
using HtmlKit;

namespace TheArtOfDev.HtmlRenderer.Demo.Common
{
    /// <summary>
    /// Utility to format HTML into a readable representation while preserving semantic structure.
    /// </summary>
    public static class HtmlPrettyPrinter
    {
        /// <summary>
        /// html tags that should be written with indentation on separate lines
        /// </summary>
        private static readonly HashSet<string> _blockTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "article", "aside", "blockquote", "body", "caption", "colgroup", "dd", "div", "dl", "dt", "fieldset", "figcaption",
            "figure", "footer", "form", "head", "header", "html", "li", "main", "nav", "ol", "p", "section", "table", "tbody",
            "td", "tfoot", "th", "thead", "tr", "ul", "title"
        };

        /// <summary>
        /// html tags that don't have a closing tag and should not change indentation depth
        /// </summary>
        private static readonly HashSet<string> _voidTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
        };

        /// <summary>
        /// Format the given html into a readable left-to-right comparable layout.
        /// </summary>
        public static string Format(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var builder = new StringBuilder(html.Length * 2);
            using (var reader = new StringReader(html))
            using (var writer = new StringWriter(builder))
            {
                var tokenizer = new HtmlTokenizer(reader);
                HtmlToken token;
                int indent = 0;

                while (tokenizer.ReadNextToken(out token))
                {
                    switch (token.Kind)
                    {
                        case HtmlTokenKind.DocType:
                        case HtmlTokenKind.Comment:
                            AppendTokenLine(builder, writer, token, indent);
                            break;

                        case HtmlTokenKind.Tag:
                            var tag = (HtmlTagToken)token;
                            var isBlockTag = _blockTags.Contains(tag.Name);
                            var isVoidTag = tag.IsEmptyElement || _voidTags.Contains(tag.Name);

                            if (tag.IsEndTag)
                            {
                                if (isBlockTag)
                                {
                                    indent = Math.Max(0, indent - 1);
                                    AppendTokenLine(builder, writer, token, indent);
                                }
                                else
                                {
                                    token.WriteTo(writer);
                                }
                            }
                            else if (isBlockTag)
                            {
                                AppendTokenLine(builder, writer, token, indent);
                                if (!isVoidTag)
                                    indent++;
                            }
                            else
                            {
                                token.WriteTo(writer);
                            }
                            break;

                        case HtmlTokenKind.CData:
                        case HtmlTokenKind.Data:
                        case HtmlTokenKind.ScriptData:
                            var data = ((HtmlDataToken)token).Data;
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                if (IsAtLineStart(builder))
                                    AppendIndent(builder, indent);
                                builder.Append(data);
                            }
                            break;
                    }
                }
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Append the given html token on its own line using the given indentation level.
        /// </summary>
        private static void AppendTokenLine(StringBuilder builder, StringWriter writer, HtmlToken token, int indent)
        {
            if (!IsAtLineStart(builder))
                builder.AppendLine();

            AppendIndent(builder, indent);
            token.WriteTo(writer);
            builder.AppendLine();
        }

        /// <summary>
        /// Append indentation spaces to the string builder.
        /// </summary>
        private static void AppendIndent(StringBuilder builder, int indent)
        {
            builder.Append(' ', indent * 2);
        }

        /// <summary>
        /// Return if the builder currently points at the start of a new line.
        /// </summary>
        private static bool IsAtLineStart(StringBuilder builder)
        {
            return builder.Length == 0 || builder[builder.Length - 1] == '\n';
        }
    }
}
