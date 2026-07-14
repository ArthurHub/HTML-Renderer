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
using HtmlKit;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using HtmlUtils = TheArtOfDev.HtmlRenderer.Core.Utils.HtmlUtils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
    /// <summary>
    ///
    /// </summary>
    internal static class HtmlParser
    {
        /// <summary>
        /// Parses the source html to css boxes tree structure.
        /// </summary>
        /// <param name="source">the html source to parse</param>
        /// <param name="root">the root box (null for document root)</param>
        public static CssBox ParseDocument(string source, CssBox root = null)
        {
            if (root == null)
                root = CssBox.CreateBlock();

            var curBox = root;

            using (var sourceReader = new StringReader(source))
            {
                var tokenizer = new HtmlTokenizer(sourceReader);

                HtmlToken token;
                while (tokenizer.ReadNextToken(out token))
                {
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

                                if (curBox.HtmlTag != null && curBox.HtmlTag.Name.Equals(HtmlConstants.NoScript, StringComparison.OrdinalIgnoreCase))
                                {
                                    curBox = ParseDocument(text.Data, curBox);
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
            }

            return root;
        }


        #region Private methods

        /// <summary>
        /// Add html text anon box to the current box, this box will have the rendered text<br/>
        /// Adding box also for text that contains only whitespaces because we don't know yet if
        /// the box is preformatted. At later stage they will be removed if not relevant.
        /// </summary>
        /// <param name="token">the html token to parse</param>
        /// <param name="curBox">the current box in html tree parsing</param>
        private static void AddTextBox(HtmlDataToken token, ref CssBox curBox)
        {
            var text = token.Data;

            if (text == null) return;

            var box = CssBox.CreateBox(curBox);
            box.Text = text;
        }


        /// <summary>
        /// Parse the html part, the part from prev parsing index to the beginning of the next html tag.<br/>
        /// </summary>
        /// <param name="token">the html tag token</param>
        /// <param name="curBox">the current box in html tree parsing</param>
        private static void ParseHtmlTag(HtmlTagToken token, ref CssBox curBox)
        {
            string tagName;
            Dictionary<string, string> tagAttributes;

            if (ParseHtmlTag(token, out tagName, out tagAttributes))
            {
                if (!HtmlUtils.IsSingleTag(tagName.ToLowerInvariant()) && curBox.ParentBox != null)
                {
                    // need to find the parent tag to go one level up
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

                var isSingle = HtmlUtils.IsSingleTag(tagName.ToLowerInvariant()) || token.IsEmptyElement;
                var tag = new HtmlTag(tagName, isSingle, tagAttributes);

                if (isSingle)
                {
                    // the current box is not changed
                    CssBox.CreateBox(tag, curBox);
                }
                else
                {
                    // go one level down, make the new box the current box
                    curBox = CssBox.CreateBox(tag, curBox);
                }
            }
        }

        private static CssBox CloseElement(CssBox cssBox, string tagName)
        {
            return DomUtils.FindParent(cssBox.ParentBox, tagName, cssBox);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="token"></param>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
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
