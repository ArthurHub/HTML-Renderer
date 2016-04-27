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
using System.Text;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Utility class for traversing DOM structure and execution stuff on it.
    /// </summary>
    internal sealed class DomUtils
    {
        /// <summary>
        /// Check if the given location is inside the given box deep.<br/>
        /// Check inner boxes and all lines that the given box spans to.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <param name="location">the location to check</param>
        /// <returns>true - location inside the box, false - otherwise</returns>
        public static bool IsInBox(CssBox box, RPoint location)
        {
            foreach (var line in box.Rectangles)
            {
                if (line.Value.Contains(location))
                    return true;
            }
            foreach (var childBox in box.Boxes)
            {
                if (IsInBox(childBox, location))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the given box contains only inline child boxes.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - only inline child boxes, false - otherwise</returns>
        public static bool ContainsInlinesOnly(CssBox box)
        {
            foreach (CssBox b in box.Boxes)
            {
                if (!b.IsInline)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Recursively searches for the parent with the specified HTML Tag name
        /// </summary>
        /// <param name="root"></param>
        /// <param name="tagName"></param>
        /// <param name="box"></param>
        public static CssBox FindParent(CssBox root, string tagName, CssBox box)
        {
            if (box == null)
            {
                return root;
            }
            else if (box.HtmlTag != null && box.HtmlTag.Name.Equals(tagName, StringComparison.CurrentCultureIgnoreCase))
            {
                return box.ParentBox ?? root;
            }
            else
            {
                return FindParent(root, tagName, box.ParentBox);
            }
        }

        /// <summary>
        /// Gets the previous sibling of this box.
        /// </summary>
        /// <returns>Box before this one on the tree. Null if its the first</returns>
        public static CssBox GetPreviousSibling(CssBox b)
        {
            if (b.ParentBox != null)
            {
                int index = b.ParentBox.Boxes.IndexOf(b);
                if (index > 0)
                {
                    int diff = 1;
                    CssBox sib = b.ParentBox.Boxes[index - diff];

                    while ((sib.Display == CssConstants.None || sib.Position == CssConstants.Absolute || sib.Position == CssConstants.Fixed) && index - diff - 1 >= 0)
                    {
                        sib = b.ParentBox.Boxes[index - ++diff];
                    }

                    return (sib.Display == CssConstants.None || sib.Position == CssConstants.Fixed) ? null : sib;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the previous sibling of this box.
        /// </summary>
        /// <returns>Box before this one on the tree. Null if its the first</returns>
        public static CssBox GetPreviousContainingBlockSibling(CssBox b)
        {
            var conBlock = b;
            int index = conBlock.ParentBox.Boxes.IndexOf(conBlock);
            while (conBlock.ParentBox != null && index < 1 && conBlock.Display != CssConstants.Block && conBlock.Display != CssConstants.Table && conBlock.Display != CssConstants.TableCell && conBlock.Display != CssConstants.ListItem)
            {
                conBlock = conBlock.ParentBox;
                index = conBlock.ParentBox != null ? conBlock.ParentBox.Boxes.IndexOf(conBlock) : -1;
            }
            conBlock = conBlock.ParentBox;
            if (conBlock != null && index > 0)
            {
                int diff = 1;
                CssBox sib = conBlock.Boxes[index - diff];

                while ((sib.Display == CssConstants.None || sib.Position == CssConstants.Absolute || sib.Position == CssConstants.Fixed) && index - diff - 1 >= 0)
                {
                    sib = conBlock.Boxes[index - ++diff];
                }

                return sib.Display == CssConstants.None ? null : sib;
            }
            return null;
        }

        /// <summary>
        /// fix word space for first word in inline tag.
        /// </summary>
        /// <param name="box">the box to check</param>
        public static bool IsBoxHasWhitespace(CssBox box)
        {
            if (!box.Words[0].IsImage && box.Words[0].HasSpaceBefore && box.IsInline)
            {
                var sib = GetPreviousContainingBlockSibling(box);
                if (sib != null && sib.IsInline)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the next sibling of this box.
        /// </summary>
        /// <returns>Box before this one on the tree. Null if its the first</returns>
        public static CssBox GetNextSibling(CssBox b)
        {
            CssBox sib = null;
            if (b.ParentBox != null)
            {
                var index = b.ParentBox.Boxes.IndexOf(b) + 1;
                while (index <= b.ParentBox.Boxes.Count - 1)
                {
                    var pSib = b.ParentBox.Boxes[index];
                    if (pSib.Display != CssConstants.None && pSib.Position != CssConstants.Absolute && pSib.Position != CssConstants.Fixed)
                    {
                        sib = pSib;
                        break;
                    }
                    index++;
                }
            }
            return sib;
        }

        /// <summary>
        /// Get attribute value by given key starting search from given box, search up the tree until
        /// attribute found or root.
        /// </summary>
        /// <param name="box">the box to start lookup at</param>
        /// <param name="attribute">the attribute to get</param>
        /// <returns>the value of the attribute or null if not found</returns>
        public static string GetAttribute(CssBox box, string attribute)
        {
            string value = null;
            while (box != null && value == null)
            {
                value = box.GetAttribute(attribute, null);
                box = box.ParentBox;
            }
            return value;
        }

        /// <summary>
        /// Get css box under the given sub-tree at the given x,y location, get the inner most.<br/>
        /// the location must be in correct scroll offset.
        /// </summary>
        /// <param name="box">the box to start search from</param>
        /// <param name="location">the location to find the box by</param>
        /// <param name="visible">Optional: if to get only visible boxes (default - true)</param>
        /// <returns>css link box if exists or null</returns>
        public static CssBox GetCssBox(CssBox box, RPoint location, bool visible = true)
        {
            if (box != null)
            {
                if ((!visible || box.Visibility == CssConstants.Visible) && (box.Bounds.IsEmpty || box.Bounds.Contains(location)))
                {
                    foreach (var childBox in box.Boxes)
                    {
                        if (CommonUtils.GetFirstValueOrDefault(box.Rectangles, box.Bounds).Contains(location))
                        {
                            return GetCssBox(childBox, location) ?? childBox;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Collect all link boxes found in the HTML tree.
        /// </summary>
        /// <param name="box">the box to start search from</param>
        /// <param name="linkBoxes">collection to add all link boxes to</param>
        public static void GetAllLinkBoxes(CssBox box, List<CssBox> linkBoxes)
        {
            if (box != null)
            {
                if (box.IsClickable && box.Visibility == CssConstants.Visible)
                {
                    linkBoxes.Add(box);
                }

                foreach (var childBox in box.Boxes)
                {
                    GetAllLinkBoxes(childBox, linkBoxes);
                }
            }
        }

        /// <summary>
        /// Get css link box under the given sub-tree at the given x,y location.<br/>
        /// the location must be in correct scroll offset.
        /// </summary>
        /// <param name="box">the box to start search from</param>
        /// <param name="location">the location to find the box by</param>
        /// <returns>css link box if exists or null</returns>
        public static CssBox GetLinkBox(CssBox box, RPoint location)
        {
            if (box != null)
            {
                if (box.IsClickable && box.Visibility == CssConstants.Visible)
                {
                    if (IsInBox(box, location))
                        return box;
                }

                if (box.ClientRectangle.IsEmpty || box.ClientRectangle.Contains(location))
                {
                    foreach (var childBox in box.Boxes)
                    {
                        var foundBox = GetLinkBox(childBox, location);
                        if (foundBox != null)
                            return foundBox;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get css box under the given sub-tree with the given id.<br/>
        /// </summary>
        /// <param name="box">the box to start search from</param>
        /// <param name="id">the id to find the box by</param>
        /// <returns>css box if exists or null</returns>
        public static CssBox GetBoxById(CssBox box, string id)
        {
            if (box != null && !string.IsNullOrEmpty(id))
            {
                if (box.HtmlTag != null && id.Equals(box.HtmlTag.TryGetAttribute("id"), StringComparison.OrdinalIgnoreCase))
                {
                    return box;
                }

                foreach (var childBox in box.Boxes)
                {
                    var foundBox = GetBoxById(childBox, id);
                    if (foundBox != null)
                        return foundBox;
                }
            }

            return null;
        }

        /// <summary>
        /// Get css line box under the given sub-tree at the given y location or the nearest line from the top.<br/>
        /// the location must be in correct scroll offset.
        /// </summary>
        /// <param name="box">the box to start search from</param>
        /// <param name="location">the location to find the box at</param>
        /// <returns>css word box if exists or null</returns>
        public static CssLineBox GetCssLineBox(CssBox box, RPoint location)
        {
            CssLineBox line = null;
            if (box != null)
            {
                if (box.LineBoxes.Count > 0)
                {
                    if (box.HtmlTag == null || box.HtmlTag.Name != "td" || box.Bounds.Contains(location))
                    {
                        foreach (var lineBox in box.LineBoxes)
                        {
                            foreach (var rect in lineBox.Rectangles)
                            {
                                if (rect.Value.Top <= location.Y)
                                {
                                    line = lineBox;
                                }

                                if (rect.Value.Top > location.Y)
                                {
                                    return line;
                                }
                            }
                        }
                    }
                }

                foreach (var childBox in box.Boxes)
                {
                    line = GetCssLineBox(childBox, location) ?? line;
                }
            }

            return line;
        }

        /// <summary>
        /// Get css word box under the given sub-tree at the given x,y location.<br/>
        /// the location must be in correct scroll offset.
        /// </summary>
        /// <param name="box">the box to start search from</param>
        /// <param name="location">the location to find the box at</param>
        /// <returns>css word box if exists or null</returns>
        public static CssRect GetCssBoxWord(CssBox box, RPoint location)
        {
            if (box != null && box.Visibility == CssConstants.Visible)
            {
                if (box.LineBoxes.Count > 0)
                {
                    foreach (var lineBox in box.LineBoxes)
                    {
                        var wordBox = GetCssBoxWord(lineBox, location);
                        if (wordBox != null)
                            return wordBox;
                    }
                }

                if (box.ClientRectangle.IsEmpty || box.ClientRectangle.Contains(location))
                {
                    foreach (var childBox in box.Boxes)
                    {
                        var foundWord = GetCssBoxWord(childBox, location);
                        if (foundWord != null)
                        {
                            return foundWord;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get css word box under the given sub-tree at the given x,y location.<br/>
        /// the location must be in correct scroll offset.
        /// </summary>
        /// <param name="lineBox">the line box to search in</param>
        /// <param name="location">the location to find the box at</param>
        /// <returns>css word box if exists or null</returns>
        public static CssRect GetCssBoxWord(CssLineBox lineBox, RPoint location)
        {
            foreach (var rects in lineBox.Rectangles)
            {
                foreach (var word in rects.Key.Words)
                {
                    // add word spacing to word width so sentence won't have hols in it when moving the mouse
                    var rect = word.Rectangle;
                    rect.Width += word.OwnerBox.ActualWordSpacing;
                    if (rect.Contains(location))
                    {
                        return word;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the css line box that the given word is in.
        /// </summary>
        /// <param name="word">the word to search for it's line box</param>
        /// <returns>line box that the word is in</returns>
        public static CssLineBox GetCssLineBoxByWord(CssRect word)
        {
            var box = word.OwnerBox;
            while (box.LineBoxes.Count == 0)
            {
                box = box.ParentBox;
            }
            foreach (var lineBox in box.LineBoxes)
            {
                foreach (var lineWord in lineBox.Words)
                {
                    if (lineWord == word)
                    {
                        return lineBox;
                    }
                }
            }
            return box.LineBoxes[0];
        }

        /// <summary>
        /// Get selected plain text of the given html sub-tree.
        /// </summary>
        /// <param name="root">the DOM box to get selected text from its sub-tree</param>
        /// <returns>the selected plain text string</returns>
        public static string GetSelectedPlainText(CssBox root)
        {
            var sb = new StringBuilder();
            var lastWordIndex = GetSelectedPlainText(sb, root);
            return sb.ToString(0, lastWordIndex).Trim();
        }

        /// <summary>
        /// Generate html from the given DOM tree.<br/>
        /// Generate all the style inside the html, in header or for every tag depending on <paramref name="styleGen"/> value.
        /// </summary>
        /// <param name="root">the box of the html generate html from</param>
        /// <param name="styleGen">Optional: controls the way styles are generated when html is generated</param>
        /// <param name="onlySelected">Optional: true - generate only selected html subset, false - generate all (default - false)</param>
        /// <returns>generated html</returns>
        public static string GenerateHtml(CssBox root, HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline, bool onlySelected = false)
        {
            var sb = new StringBuilder();
            if (root != null)
            {
                var selectedBoxes = onlySelected ? CollectSelectedBoxes(root) : null;
                var selectionRoot = onlySelected ? GetSelectionRoot(root, selectedBoxes) : null;
                WriteHtml(root.HtmlContainer.CssParser, sb, root, styleGen, selectedBoxes, selectionRoot);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generate textual tree representation of the css boxes tree starting from the given root.<br/>
        /// Used for debugging html parsing.
        /// </summary>
        /// <param name="root">the root to generate tree from</param>
        /// <returns>generated tree</returns>
        public static string GenerateBoxTree(CssBox root)
        {
            var sb = new StringBuilder();
            GenerateBoxTree(root, sb, 0);
            return sb.ToString();
        }


        #region Private methods

        /// <summary>
        /// Get selected plain text of the given html sub-tree.<br/>
        /// Append all the selected words.
        /// </summary>
        /// <param name="sb">the builder to append the selected text to</param>
        /// <param name="box">the DOM box to get selected text from its sub-tree</param>
        /// <returns>the index of the last word appended</returns>
        private static int GetSelectedPlainText(StringBuilder sb, CssBox box)
        {
            int lastWordIndex = 0;
            foreach (var boxWord in box.Words)
            {
                // append the text of selected word (handle partial selected words)
                if (boxWord.Selected)
                {
                    sb.Append(GetSelectedWord(boxWord, true));
                    lastWordIndex = sb.Length;
                }
            }

            // empty span box
            if (box.Boxes.Count < 1 && box.Text != null && box.Text.IsWhitespace())
            {
                sb.Append(' ');
            }

            // deep traversal
            if (box.Visibility != CssConstants.Hidden && box.Display != CssConstants.None)
            {
                foreach (var childBox in box.Boxes)
                {
                    var innerLastWordIdx = GetSelectedPlainText(sb, childBox);
                    lastWordIndex = Math.Max(lastWordIndex, innerLastWordIdx);
                }
            }

            if (sb.Length > 0)
            {
                // convert hr to line of dashes
                if (box.HtmlTag != null && box.HtmlTag.Name == "hr")
                {
                    if (sb.Length > 1 && sb[sb.Length - 1] != '\n')
                        sb.AppendLine();
                    sb.AppendLine(new string('-', 80));
                }

                // new line for css block
                if (box.Display == CssConstants.Block || box.Display == CssConstants.ListItem || box.Display == CssConstants.TableRow)
                {
                    if (!(box.IsBrElement && sb.Length > 1 && sb[sb.Length - 1] == '\n'))
                        sb.AppendLine();
                }

                // space between table cells
                if (box.Display == CssConstants.TableCell)
                {
                    sb.Append(' ');
                }

                // paragraphs has additional newline for nice formatting
                if (box.HtmlTag != null && box.HtmlTag.Name == "p")
                {
                    int newlines = 0;
                    for (int i = sb.Length - 1; i >= 0 && char.IsWhiteSpace(sb[i]); i--)
                        newlines += sb[i] == '\n' ? 1 : 0;
                    if (newlines < 2)
                        sb.AppendLine();
                }
            }

            return lastWordIndex;
        }

        /// <summary>
        /// Collect the boxes that have at least one word down the hierarchy that is selected recursively.<br/>
        /// </summary>
        /// <param name="root">the box to check its sub-tree</param>
        /// <returns>the collection to add the selected tags to</returns>
        private static Dictionary<CssBox, bool> CollectSelectedBoxes(CssBox root)
        {
            var selectedBoxes = new Dictionary<CssBox, bool>();
            var maybeBoxes = new Dictionary<CssBox, bool>();
            CollectSelectedBoxes(root, selectedBoxes, maybeBoxes);
            return selectedBoxes;
        }

        /// <summary>
        /// Collect the boxes that have at least one word down the hierarchy that is selected recursively.<br/>
        /// Use <paramref name="maybeBoxes"/> to handle boxes that are between selected words but don't have selected word inside.<br/>
        /// </summary>
        /// <param name="box">the box to check its sub-tree</param>
        /// <param name="selectedBoxes">the hash to add the selected boxes to</param>
        /// <param name="maybeBoxes">used to handle boxes that are between selected words but don't have selected word inside</param>
        /// <returns>is the current box is in selected sub-tree</returns>
        private static bool CollectSelectedBoxes(CssBox box, Dictionary<CssBox, bool> selectedBoxes, Dictionary<CssBox, bool> maybeBoxes)
        {
            bool isInSelection = false;
            foreach (var word in box.Words)
            {
                if (word.Selected)
                {
                    selectedBoxes[box] = true;
                    foreach (var maybeTag in maybeBoxes)
                        selectedBoxes[maybeTag.Key] = maybeTag.Value;
                    maybeBoxes.Clear();
                    isInSelection = true;
                }
            }

            foreach (var childBox in box.Boxes)
            {
                var childInSelection = CollectSelectedBoxes(childBox, selectedBoxes, maybeBoxes);
                if (childInSelection)
                {
                    selectedBoxes[box] = true;
                    isInSelection = true;
                }
            }

            if (box.HtmlTag != null && selectedBoxes.Count > 0)
            {
                maybeBoxes[box] = true;
            }

            return isInSelection;
        }

        /// <summary>
        /// find the box the is the root of selected boxes (the first box to contain multiple selected boxes)
        /// </summary>
        /// <param name="root">the root of the boxes tree</param>
        /// <param name="selectedBoxes">the selected boxes to find selection root in</param>
        /// <returns>the box that is the root of selected boxes</returns>
        private static CssBox GetSelectionRoot(CssBox root, Dictionary<CssBox, bool> selectedBoxes)
        {
            var selectionRoot = root;
            var selectionRootRun = root;
            while (true)
            {
                bool foundRoot = false;
                CssBox selectedChild = null;
                foreach (var childBox in selectionRootRun.Boxes)
                {
                    if (selectedBoxes.ContainsKey(childBox))
                    {
                        if (selectedChild != null)
                        {
                            foundRoot = true;
                            break;
                        }
                        selectedChild = childBox;
                    }
                }

                if (foundRoot || selectedChild == null)
                    break;

                selectionRootRun = selectedChild;

                // the actual selection root must be a box with html tag
                if (selectionRootRun.HtmlTag != null)
                    selectionRoot = selectionRootRun;
            }

            // if the selection root doesn't contained any named boxes in it then we must go one level up, otherwise we will miss the selection root box formatting
            if (!ContainsNamedBox(selectionRoot))
            {
                selectionRootRun = selectionRoot.ParentBox;
                while (selectionRootRun.ParentBox != null && selectionRootRun.HtmlTag == null)
                    selectionRootRun = selectionRootRun.ParentBox;

                if (selectionRootRun.HtmlTag != null)
                    selectionRoot = selectionRootRun;
            }

            return selectionRoot;
        }

        /// <summary>
        /// Check if the given box has a names child box (has html tag) recursively.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - in sub-tree there is a named box, false - otherwise</returns>
        private static bool ContainsNamedBox(CssBox box)
        {
            foreach (var childBox in box.Boxes)
            {
                if (childBox.HtmlTag != null || ContainsNamedBox(childBox))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Write the given html DOM sub-tree into the given string builder.<br/>
        /// If <paramref name="selectedBoxes"/> are given write html only from those tags.
        /// </summary>
        /// <param name="cssParser">used to parse CSS data</param>
        /// <param name="sb">the string builder to write html into</param>
        /// <param name="box">the html sub-tree to write</param>
        /// <param name="styleGen">Controls the way styles are generated when html is generated</param>
        /// <param name="selectedBoxes">Control if to generate only selected boxes, if given only boxes found in hash will be generated</param>
        /// <param name="selectionRoot">the box the is the root of selected boxes (the first box to contain multiple selected boxes)</param>
        private static void WriteHtml(CssParser cssParser, StringBuilder sb, CssBox box, HtmlGenerationStyle styleGen, Dictionary<CssBox, bool> selectedBoxes, CssBox selectionRoot)
        {
            if (box.HtmlTag == null || selectedBoxes == null || selectedBoxes.ContainsKey(box))
            {
                if (box.HtmlTag != null)
                {
                    if (box.HtmlTag.Name != "link" || !box.HtmlTag.Attributes.ContainsKey("href") ||
                        (!box.HtmlTag.Attributes["href"].StartsWith("property") && !box.HtmlTag.Attributes["href"].StartsWith("method")))
                    {
                        WriteHtmlTag(cssParser, sb, box, styleGen);
                        if (box == selectionRoot)
                            sb.Append("<!--StartFragment-->");
                    }

                    if (styleGen == HtmlGenerationStyle.InHeader && box.HtmlTag.Name == "html" && box.HtmlContainer.CssData != null)
                    {
                        sb.AppendLine("<head>");
                        WriteStylesheet(sb, box.HtmlContainer.CssData);
                        sb.AppendLine("</head>");
                    }
                }

                if (box.Words.Count > 0)
                {
                    foreach (var word in box.Words)
                    {
                        if (selectedBoxes == null || word.Selected)
                        {
                            var wordText = GetSelectedWord(word, selectedBoxes != null);
                            sb.Append(HtmlUtils.EncodeHtml(wordText));
                        }
                    }
                }

                foreach (var childBox in box.Boxes)
                {
                    WriteHtml(cssParser, sb, childBox, styleGen, selectedBoxes, selectionRoot);
                }

                if (box.HtmlTag != null && !box.HtmlTag.IsSingle)
                {
                    if (box == selectionRoot)
                        sb.Append("<!--EndFragment-->");
                    sb.AppendFormat("</{0}>", box.HtmlTag.Name);
                }
            }
        }

        /// <summary>
        /// Write the given html tag with all its attributes and styles.
        /// </summary>
        /// <param name="cssParser">used to parse CSS data</param>
        /// <param name="sb">the string builder to write html into</param>
        /// <param name="box">the css box with the html tag to write</param>
        /// <param name="styleGen">Controls the way styles are generated when html is generated</param>
        private static void WriteHtmlTag(CssParser cssParser, StringBuilder sb, CssBox box, HtmlGenerationStyle styleGen)
        {
            sb.AppendFormat("<{0}", box.HtmlTag.Name);

            // collect all element style properties including from stylesheet
            var tagStyles = new Dictionary<string, string>();
            var tagCssBlock = box.HtmlContainer.CssData.GetCssBlock(box.HtmlTag.Name);
            if (tagCssBlock != null)
            {
                // TODO:a handle selectors
                foreach (var cssBlock in tagCssBlock)
                    foreach (var prop in cssBlock.Properties)
                        tagStyles[prop.Key] = prop.Value;
            }

            if (box.HtmlTag.HasAttributes())
            {
                sb.Append(" ");
                foreach (var att in box.HtmlTag.Attributes)
                {
                    // handle image tags by inserting the image using base64 data
                    if (styleGen == HtmlGenerationStyle.Inline && att.Key == HtmlConstants.Style)
                    {
                        // if inline style add the styles to the collection
                        var block = cssParser.ParseCssBlock(box.HtmlTag.Name, box.HtmlTag.TryGetAttribute("style"));
                        foreach (var prop in block.Properties)
                            tagStyles[prop.Key] = prop.Value;
                    }
                    else if (styleGen == HtmlGenerationStyle.Inline && att.Key == HtmlConstants.Class)
                    {
                        // if inline style convert the style class to actual properties and add to collection
                        var cssBlocks = box.HtmlContainer.CssData.GetCssBlock("." + att.Value);
                        if (cssBlocks != null)
                        {
                            // TODO:a handle selectors
                            foreach (var cssBlock in cssBlocks)
                                foreach (var prop in cssBlock.Properties)
                                    tagStyles[prop.Key] = prop.Value;
                        }
                    }
                    else
                    {
                        sb.AppendFormat("{0}=\"{1}\" ", att.Key, att.Value);
                    }
                }

                sb.Remove(sb.Length - 1, 1);
            }

            // if inline style insert the style tag with all collected style properties
            if (styleGen == HtmlGenerationStyle.Inline && tagStyles.Count > 0)
            {
                var cleanTagStyles = StripDefaultStyles(box, tagStyles);
                if (cleanTagStyles.Count > 0)
                {
                    sb.Append(" style=\"");
                    foreach (var style in cleanTagStyles)
                        sb.AppendFormat("{0}: {1}; ", style.Key, style.Value);
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\"");
                }
            }

            sb.AppendFormat("{0}>", box.HtmlTag.IsSingle ? "/" : "");
        }

        /// <summary>
        /// Clean the given style collection by removing default styles so only custom styles remain.<br/>
        /// Return new collection where the old remains unchanged.
        /// </summary>
        /// <param name="box">the box the styles apply to, used to know the default style</param>
        /// <param name="tagStyles">the collection of styles to clean</param>
        /// <returns>new cleaned styles collection</returns>
        private static Dictionary<string, string> StripDefaultStyles(CssBox box, Dictionary<string, string> tagStyles)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var cleanTagStyles = new Dictionary<string, string>();
            var defaultBlocks = box.HtmlContainer.Adapter.DefaultCssData.GetCssBlock(box.HtmlTag.Name);
            foreach (var style in tagStyles)
            {
                bool isDefault = false;
                foreach (var defaultBlock in defaultBlocks)
                {
                    string value;
                    if (defaultBlock.Properties.TryGetValue(style.Key, out value) && value.Equals(style.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        isDefault = true;
                        break;
                    }
                }

                if (!isDefault)
                    cleanTagStyles[style.Key] = style.Value;
            }
            return cleanTagStyles;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        /// Write stylesheet data inline into the html.
        /// </summary>
        /// <param name="sb">the string builder to write stylesheet into</param>
        /// <param name="cssData">the css data to write to the head</param>
        private static void WriteStylesheet(StringBuilder sb, CssData cssData)
        {
            sb.AppendLine("<style type=\"text/css\">");
            foreach (var cssBlocks in cssData.MediaBlocks["all"])
            {
                sb.Append(cssBlocks.Key);
                sb.Append(" { ");
                foreach (var cssBlock in cssBlocks.Value)
                {
                    foreach (var property in cssBlock.Properties)
                    {
                        // TODO:a handle selectors
                        sb.AppendFormat("{0}: {1};", property.Key, property.Value);
                    }
                }
                sb.Append(" }");
                sb.AppendLine();
            }
            sb.AppendLine("</style>");
        }

        /// <summary>
        /// Get the selected word with respect to partial selected words.
        /// </summary>
        /// <param name="rect">the word to append</param>
        /// <param name="selectedText">is to get selected text or all the text in the word</param>
        private static string GetSelectedWord(CssRect rect, bool selectedText)
        {
            if (selectedText && rect.SelectedStartIndex > -1 && rect.SelectedEndIndexOffset > -1)
            {
                return rect.Text.Substring(rect.SelectedStartIndex, rect.SelectedEndIndexOffset - rect.SelectedStartIndex);
            }
            else if (selectedText && rect.SelectedStartIndex > -1)
            {
                return rect.Text.Substring(rect.SelectedStartIndex) + (rect.HasSpaceAfter ? " " : "");
            }
            else if (selectedText && rect.SelectedEndIndexOffset > -1)
            {
                return rect.Text.Substring(0, rect.SelectedEndIndexOffset);
            }
            else
            {
                var whitespaceBefore = rect.OwnerBox.Words[0] == rect ? IsBoxHasWhitespace(rect.OwnerBox) : rect.HasSpaceBefore;
                return (whitespaceBefore ? " " : "") + rect.Text + (rect.HasSpaceAfter ? " " : "");
            }
        }

        /// <summary>
        /// Generate textual tree representation of the css boxes tree starting from the given root.<br/>
        /// Used for debugging html parsing.
        /// </summary>
        /// <param name="box">the box to generate for</param>
        /// <param name="builder">the string builder to generate to</param>
        /// <param name="indent">the current indent level to set indent of generated text</param>
        private static void GenerateBoxTree(CssBox box, StringBuilder builder, int indent)
        {
            builder.AppendFormat("{0}<{1}", new string(' ', 2 * indent), box.Display);
            if (box.HtmlTag != null)
                builder.AppendFormat(" element=\"{0}\"", box.HtmlTag != null ? box.HtmlTag.Name : string.Empty);
            if (box.Words.Count > 0)
                builder.AppendFormat(" words=\"{0}\"", box.Words.Count);
            builder.AppendFormat("{0}>\r\n", box.Boxes.Count > 0 ? "" : "/");
            if (box.Boxes.Count > 0)
            {
                foreach (var childBox in box.Boxes)
                {
                    GenerateBoxTree(childBox, builder, indent + 1);
                }
                builder.AppendFormat("{0}</{1}>\r\n", new string(' ', 2 * indent), box.Display);
            }
        }

        #endregion
    }
}