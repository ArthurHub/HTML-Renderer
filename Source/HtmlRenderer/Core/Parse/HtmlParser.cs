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
using System.Linq;
using System.Text;
using HtmlKit;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using HtmlUtils = TheArtOfDev.HtmlRenderer.Core.Utils.HtmlUtils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
    /// <summary>
    /// Builds the box tree from HtmlKit's tokens (HTML §13.2.5). Tree construction (HTML §13.2.6) is a
    /// simplified version: the boxes form the stack of open elements, there are no insertion modes and no
    /// list of active formatting elements, see Spec/html.yml for what's missing.
    /// </summary>
    internal static class HtmlParser
    {
        /// <summary>
        /// Parses <paramref name="source"/> into boxes below <paramref name="root"/>, a new anonymous block if null.
        /// </summary>
        public static CssBox ParseDocument(string source, CssBox root = null)
        {
            if (root == null)
                root = CssBox.CreateBlock();

            var curBox = root;

            // HtmlKit tokenizes noscript content as raw text, split into data tokens at every '<'. Without
            // scripting it's markup (HTML 4.12.2), so it's collected and parsed with the noscript box as root.
            StringBuilder noscriptContent = null;

            using (var sourceReader = new StringReader(source))
            {
                var tokenizer = new HtmlTokenizer(sourceReader);

                HtmlToken token;
                while (tokenizer.ReadNextToken(out token))
                {
                    if (noscriptContent != null && token.Kind != HtmlTokenKind.Data)
                    {
                        ParseDocument(noscriptContent.ToString(), curBox);
                        noscriptContent = null;
                    }

                    switch (token.Kind)
                    {
                        case HtmlTokenKind.Tag:
                            {
                                var tag = (HtmlTagToken)token;
                                ParseHtmlTag(tag, ref curBox);
                                break;
                            }
                        case HtmlTokenKind.Data:
                            {
                                var text = (HtmlDataToken)token;

                                if (curBox != root && curBox.HtmlTag != null && curBox.HtmlTag.Name.Equals(HtmlConstants.NoScript, StringComparison.OrdinalIgnoreCase))
                                {
                                    noscriptContent ??= new StringBuilder();
                                    noscriptContent.Append(text.Data);
                                }
                                else
                                {
                                    AddTextBox(text, ref curBox);
                                }

                                break;
                            }
                        case HtmlTokenKind.CData:
                        case HtmlTokenKind.Comment:
                        case HtmlTokenKind.DocType:
                        case HtmlTokenKind.ScriptData:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (noscriptContent != null)
                    ParseDocument(noscriptContent.ToString(), curBox);
            }

            return root;
        }


        #region Private methods

        /// <summary>
        /// Adds the text as an anonymous child box of the current box. Every data token gets its own box, also
        /// a whitespace-only one, white space is only collapsed later, once 'white-space' is known.
        /// </summary>
        private static void AddTextBox(HtmlDataToken token, ref CssBox curBox)
        {
            var text = token.Data;

            if (text == null) return;

            // HTML §13.2.6.4.7: a newline right after the pre, listing or textarea start tag is dropped.
            // CRLF is checked as well, the input stream isn't normalized (HTML §13.2.3.5).
            if (curBox.Boxes.Count == 0 && curBox.HtmlTag != null && IsNewlineStrippingElement(curBox.HtmlTag.Name))
            {
                if (text.StartsWith("\r\n", StringComparison.Ordinal))
                    text = text.Substring(2);
                else if (text.StartsWith("\n", StringComparison.Ordinal))
                    text = text.Substring(1);

                if (text.Length == 0) return;
            }

            // HtmlKit splits text into several data tokens, e.g. at each '<' in a style element,
            // HTML appends them to the preceding Text node (parsing.html#insert-a-char)
            var lastChild = curBox.Boxes.Count > 0 ? curBox.Boxes[curBox.Boxes.Count - 1] : null;
            if (lastChild != null && lastChild.HtmlTag == null && lastChild.Text != null)
            {
                lastChild.Text += text;
                return;
            }

            var box = CssBox.CreateBox(curBox);
            box.Text = text;
        }


        private static bool IsNewlineStrippingElement(string tagName)
        {
            return tagName.Equals("pre", StringComparison.OrdinalIgnoreCase)
                   || tagName.Equals("listing", StringComparison.OrdinalIgnoreCase)
                   || tagName.Equals("textarea", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles a start or end tag: an end tag closes the nearest open element with its name, a start tag
        /// first closes the elements whose end tag it implies and then opens its element, unless it's void.
        /// </summary>
        private static void ParseHtmlTag(HtmlTagToken token, ref CssBox curBox)
        {
            string tagName;
            Dictionary<string, string> tagAttributes;

            if (ParseHtmlTag(token, out tagName, out tagAttributes))
            {
                if (!HtmlUtils.IsSingleTag(tagName.ToLowerInvariant()) && curBox.ParentBox != null)
                {
                    curBox = CloseElement(curBox, tagName);
                }
            }
            else if (!string.IsNullOrEmpty(tagName))
            {
                while (true)
                {
                    if (curBox.HtmlTag != null && HtmlUtils.CanEndTagBeOmitted(curBox.HtmlTag.Name.ToLowerInvariant(), tagName.ToLowerInvariant()))
                    {
                        curBox = CloseElement(curBox, curBox.HtmlTag.Name);
                    }
                    else
                    {
                        break;
                    }
                }

                // HTML §13.2.6.4.7 ignores the self-closing flag on non-void elements, this doesn't.
                var isSingle = HtmlUtils.IsSingleTag(tagName.ToLowerInvariant()) || token.IsEmptyElement;
                var tag = new HtmlTag(tagName, isSingle, tagAttributes);

                if (isSingle)
                {
                    CssBox.CreateBox(tag, curBox);
                }
                else
                {
                    curBox = CssBox.CreateBox(tag, curBox);
                }
            }
        }

        private static CssBox CloseElement(CssBox cssBox, string tagName)
        {
            return DomUtils.FindParent(cssBox.ParentBox, tagName, cssBox);
        }

        /// <summary>
        /// Reads the tag's name and, for a start tag, its attributes. Returns whether it's an end tag.
        /// </summary>
        /// <remarks>
        /// Names keep the case of the source, HTML §13.2.5.33 lowercases them. Of attributes with the same
        /// name only the first is kept.
        /// </remarks>
        private static bool ParseHtmlTag(HtmlTagToken token, out string name, out Dictionary<string, string> attributes)
        {
            var isClosing = token.IsEndTag;

            name = token.Name;

            attributes = null;

            if (!isClosing)
            {
                attributes = token.Attributes
                    .GroupBy(x => x.Name)
                    .ToDictionary(x => x.Key, x => x.First().Value);
            }

            return isClosing;
        }

        #endregion
    }
}
