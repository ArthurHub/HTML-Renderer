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
using System.Globalization;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
    /// <summary>
    /// Handle css DOM tree generation from raw html and stylesheet.
    /// </summary>
    internal sealed class DomParser
    {
        #region Fields and Consts

        /// <summary>
        /// Parser for CSS
        /// </summary>
        private readonly CssParser _cssParser;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public DomParser(CssParser cssParser)
        {
            ArgChecker.AssertArgNotNull(cssParser, "cssParser");

            _cssParser = cssParser;
        }

        /// <summary>
        /// Generate css tree by parsing the given html and applying the given css style data on it.
        /// </summary>
        /// <param name="html">the html to parse</param>
        /// <param name="htmlContainer">the html container to use for reference resolve</param>
        /// <param name="cssData">the css data to use</param>
        /// <returns>the root of the generated tree</returns>
        public CssBox GenerateCssTree(string html, HtmlContainerInt htmlContainer, ref CssData cssData)
        {
            var root = HtmlParser.ParseDocument(html);
            if (root != null)
            {
                root.HtmlContainer = htmlContainer;

                bool cssDataChanged = false;
                CascadeParseStyles(root, htmlContainer, ref cssData, ref cssDataChanged);

                CascadeApplyStyles(root, cssData);

                SetTextSelectionStyle(htmlContainer, cssData);

                CorrectTextBoxes(root);

                CorrectImgBoxes(root);

                bool followingBlock = true;
                CorrectLineBreaksBlocks(root, ref followingBlock);

                CorrectInlineBoxesParent(root);

                CorrectBlockInsideInline(root);

                CorrectInlineBoxesParent(root);
            }
            return root;
        }


        #region Private methods

        /// <summary>
        /// Read styles defined inside the dom structure in links and style elements.<br/>
        /// If the html tag is "style" tag parse it content and add to the css data for all future tags parsing.<br/>
        /// If the html tag is "link" that point to style data parse it content and add to the css data for all future tags parsing.<br/>
        /// </summary>
        /// <param name="box">the box to parse style data in</param>
        /// <param name="htmlContainer">the html container to use for reference resolve</param>
        /// <param name="cssData">the style data to fill with found styles</param>
        /// <param name="cssDataChanged">check if the css data has been modified by the handled html not to change the base css data</param>
        private void CascadeParseStyles(CssBox box, HtmlContainerInt htmlContainer, ref CssData cssData, ref bool cssDataChanged)
        {
            if (box.HtmlTag != null)
            {
                // Check for the <link rel=stylesheet> tag
                if (box.HtmlTag.Name.Equals("link", StringComparison.CurrentCultureIgnoreCase) &&
                    box.GetAttribute("rel", string.Empty).Equals("stylesheet", StringComparison.CurrentCultureIgnoreCase))
                {
                    CloneCssData(ref cssData, ref cssDataChanged);
                    string stylesheet;
                    CssData stylesheetData;
                    StylesheetLoadHandler.LoadStylesheet(htmlContainer, box.GetAttribute("href", string.Empty), box.HtmlTag.Attributes, out stylesheet, out stylesheetData);
                    if (stylesheet != null)
                        _cssParser.ParseStyleSheet(cssData, stylesheet);
                    else if (stylesheetData != null)
                        cssData.Combine(stylesheetData);
                }

                // Check for the <style> tag
                if (box.HtmlTag.Name.Equals("style", StringComparison.CurrentCultureIgnoreCase) && box.Boxes.Count > 0)
                {
                    CloneCssData(ref cssData, ref cssDataChanged);
                    foreach (var child in box.Boxes)
                        _cssParser.ParseStyleSheet(cssData, child.Text.CutSubstring());
                }
            }

            foreach (var childBox in box.Boxes)
            {
                CascadeParseStyles(childBox, htmlContainer, ref cssData, ref cssDataChanged);
            }
        }


        /// <summary>
        /// Applies style to all boxes in the tree.<br/>
        /// If the html tag has style defined for each apply that style to the css box of the tag.<br/>
        /// If the html tag has "class" attribute and the class name has style defined apply that style on the tag css box.<br/>
        /// If the html tag has "style" attribute parse it and apply the parsed style on the tag css box.<br/>
        /// </summary>
        /// <param name="box">the box to apply the style to</param>
        /// <param name="cssData">the style data for the html</param>
        private void CascadeApplyStyles(CssBox box, CssData cssData)
        {
            box.InheritStyle();

            if (box.HtmlTag != null)
            {
                // try assign style using all wildcard
                AssignCssBlocks(box, cssData, "*");

                // try assign style using the html element tag
                AssignCssBlocks(box, cssData, box.HtmlTag.Name);

                // try assign style using the "class" attribute of the html element
                if (box.HtmlTag.HasAttribute("class"))
                {
                    AssignClassCssBlocks(box, cssData);
                }

                // try assign style using the "id" attribute of the html element
                if (box.HtmlTag.HasAttribute("id"))
                {
                    var id = box.HtmlTag.TryGetAttribute("id");
                    AssignCssBlocks(box, cssData, "#" + id);
                }

                TranslateAttributes(box.HtmlTag, box);

                // Check for the style="" attribute
                if (box.HtmlTag.HasAttribute("style"))
                {
                    var block = _cssParser.ParseCssBlock(box.HtmlTag.Name, box.HtmlTag.TryGetAttribute("style"));
                    if (block != null)
                        AssignCssBlock(box, block);
                }
            }

            // cascade text decoration only to boxes that actually have text so it will be handled correctly.
            if (box.TextDecoration != String.Empty && box.Text == null)
            {
                foreach (var childBox in box.Boxes)
                    childBox.TextDecoration = box.TextDecoration;
                box.TextDecoration = string.Empty;
            }

            foreach (var childBox in box.Boxes)
            {
                CascadeApplyStyles(childBox, cssData);
            }
        }

        /// <summary>
        /// Set the selected text style (selection text color and background color).
        /// </summary>
        /// <param name="htmlContainer"> </param>
        /// <param name="cssData">the style data</param>
        private void SetTextSelectionStyle(HtmlContainerInt htmlContainer, CssData cssData)
        {
            htmlContainer.SelectionForeColor = RColor.Empty;
            htmlContainer.SelectionBackColor = RColor.Empty;

            if (cssData.ContainsCssBlock("::selection"))
            {
                var blocks = cssData.GetCssBlock("::selection");
                foreach (var block in blocks)
                {
                    if (block.Properties.ContainsKey("color"))
                        htmlContainer.SelectionForeColor = _cssParser.ParseColor(block.Properties["color"]);
                    if (block.Properties.ContainsKey("background-color"))
                        htmlContainer.SelectionBackColor = _cssParser.ParseColor(block.Properties["background-color"]);
                }
            }
        }

        /// <summary>
        /// Assigns the given css classes to the given css box checking if matching.<br/>
        /// Support multiple classes in single attribute separated by whitespace.
        /// </summary>
        /// <param name="box">the css box to assign css to</param>
        /// <param name="cssData">the css data to use to get the matching css blocks</param>
        private static void AssignClassCssBlocks(CssBox box, CssData cssData)
        {
            var classes = box.HtmlTag.TryGetAttribute("class");

            var startIdx = 0;
            while (startIdx < classes.Length)
            {
                while (startIdx < classes.Length && classes[startIdx] == ' ')
                    startIdx++;

                if (startIdx < classes.Length)
                {
                    var endIdx = classes.IndexOf(' ', startIdx);

                    if (endIdx < 0)
                        endIdx = classes.Length;

                    var cls = "." + classes.Substring(startIdx, endIdx - startIdx);
                    AssignCssBlocks(box, cssData, cls);
                    AssignCssBlocks(box, cssData, box.HtmlTag.Name + cls);

                    startIdx = endIdx + 1;
                }
            }
        }

        /// <summary>
        /// Assigns the given css style blocks to the given css box checking if matching.
        /// </summary>
        /// <param name="box">the css box to assign css to</param>
        /// <param name="cssData">the css data to use to get the matching css blocks</param>
        /// <param name="className">the class selector to search for css blocks</param>
        private static void AssignCssBlocks(CssBox box, CssData cssData, string className)
        {
            var blocks = cssData.GetCssBlock(className);
            foreach (var block in blocks)
            {
                if (IsBlockAssignableToBox(box, block))
                {
                    AssignCssBlock(box, block);
                }
            }
        }

        /// <summary>
        /// Check if the given css block is assignable to the given css box.<br/>
        /// the block is assignable if it has no hierarchical selectors or if the hierarchy matches.<br/>
        /// Special handling for ":hover" pseudo-class.<br/>
        /// </summary>
        /// <param name="box">the box to check assign to</param>
        /// <param name="block">the block to check assign of</param>
        /// <returns>true - the block is assignable to the box, false - otherwise</returns>
        private static bool IsBlockAssignableToBox(CssBox box, CssBlock block)
        {
            bool assignable = true;
            if (block.Selectors != null)
            {
                assignable = IsBlockAssignableToBoxWithSelector(box, block);
            }
            else if (box.HtmlTag.Name.Equals("a", StringComparison.OrdinalIgnoreCase) && block.Class.Equals("a", StringComparison.OrdinalIgnoreCase) && !box.HtmlTag.HasAttribute("href"))
            {
                assignable = false;
            }

            if (assignable && block.Hover)
            {
                box.HtmlContainer.AddHoverBox(box, block);
                assignable = false;
            }

            return assignable;
        }

        /// <summary>
        /// Check if the given css block is assignable to the given css box by validating the selector.<br/>
        /// </summary>
        /// <param name="box">the box to check assign to</param>
        /// <param name="block">the block to check assign of</param>
        /// <returns>true - the block is assignable to the box, false - otherwise</returns>
        private static bool IsBlockAssignableToBoxWithSelector(CssBox box, CssBlock block)
        {
            foreach (var selector in block.Selectors)
            {
                bool matched = false;
                while (!matched)
                {
                    box = box.ParentBox;
                    while (box != null && box.HtmlTag == null)
                        box = box.ParentBox;

                    if (box == null)
                        return false;

                    if (box.HtmlTag.Name.Equals(selector.Class, StringComparison.InvariantCultureIgnoreCase))
                        matched = true;

                    if (!matched && box.HtmlTag.HasAttribute("class"))
                    {
                        var className = box.HtmlTag.TryGetAttribute("class");
                        if (selector.Class.Equals("." + className, StringComparison.InvariantCultureIgnoreCase) || selector.Class.Equals(box.HtmlTag.Name + "." + className, StringComparison.InvariantCultureIgnoreCase))
                            matched = true;
                    }

                    if (!matched && box.HtmlTag.HasAttribute("id"))
                    {
                        var id = box.HtmlTag.TryGetAttribute("id");
                        if (selector.Class.Equals("#" + id, StringComparison.InvariantCultureIgnoreCase))
                            matched = true;
                    }

                    if (!matched && selector.DirectParent)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Assigns the given css style block properties to the given css box.
        /// </summary>
        /// <param name="box">the css box to assign css to</param>
        /// <param name="block">the css block to assign</param>
        private static void AssignCssBlock(CssBox box, CssBlock block)
        {
            foreach (var prop in block.Properties)
            {
                var value = prop.Value;
                if (prop.Value == CssConstants.Inherit && box.ParentBox != null)
                {
                    value = CssUtils.GetPropertyValue(box.ParentBox, prop.Key);
                }
                if (IsStyleOnElementAllowed(box, prop.Key, value))
                {
                    CssUtils.SetPropertyValue(box, prop.Key, value);
                }
            }
        }

        /// <summary>
        /// Check if the given style is allowed to be set on the given css box.<br/>
        /// Used to prevent invalid CssBoxes creation like table with inline display style.
        /// </summary>
        /// <param name="box">the css box to assign css to</param>
        /// <param name="key">the style key to cehck</param>
        /// <param name="value">the style value to check</param>
        /// <returns>true - style allowed, false - not allowed</returns>
        private static bool IsStyleOnElementAllowed(CssBox box, string key, string value)
        {
            if (box.HtmlTag != null && key == HtmlConstants.Display)
            {
                switch (box.HtmlTag.Name)
                {
                    case HtmlConstants.Table:
                        return value == CssConstants.Table;
                    case HtmlConstants.Tr:
                        return value == CssConstants.TableRow;
                    case HtmlConstants.Tbody:
                        return value == CssConstants.TableRowGroup;
                    case HtmlConstants.Thead:
                        return value == CssConstants.TableHeaderGroup;
                    case HtmlConstants.Tfoot:
                        return value == CssConstants.TableFooterGroup;
                    case HtmlConstants.Col:
                        return value == CssConstants.TableColumn;
                    case HtmlConstants.Colgroup:
                        return value == CssConstants.TableColumnGroup;
                    case HtmlConstants.Td:
                    case HtmlConstants.Th:
                        return value == CssConstants.TableCell;
                    case HtmlConstants.Caption:
                        return value == CssConstants.TableCaption;
                }
            }
            return true;
        }

        /// <summary>
        /// Clone css data if it has not already been cloned.<br/>
        /// Used to preserve the base css data used when changed by style inside html.
        /// </summary>
        private static void CloneCssData(ref CssData cssData, ref bool cssDataChanged)
        {
            if (!cssDataChanged)
            {
                cssDataChanged = true;
                cssData = cssData.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="box"></param>
        private void TranslateAttributes(HtmlTag tag, CssBox box)
        {
            if (tag.HasAttributes())
            {
                foreach (string att in tag.Attributes.Keys)
                {
                    string value = tag.Attributes[att];

                    switch (att)
                    {
                        case HtmlConstants.Align:
                            if (value == HtmlConstants.Left || value == HtmlConstants.Center || value == HtmlConstants.Right || value == HtmlConstants.Justify)
                                box.TextAlign = value.ToLower();
                            else
                                box.VerticalAlign = value.ToLower();
                            break;
                        case HtmlConstants.Background:
                            box.BackgroundImage = value.ToLower();
                            break;
                        case HtmlConstants.Bgcolor:
                            box.BackgroundColor = value.ToLower();
                            break;
                        case HtmlConstants.Border:
                            if (!string.IsNullOrEmpty(value) && value != "0")
                                box.BorderLeftStyle = box.BorderTopStyle = box.BorderRightStyle = box.BorderBottomStyle = CssConstants.Solid;
                            box.BorderLeftWidth = box.BorderTopWidth = box.BorderRightWidth = box.BorderBottomWidth = TranslateLength(value);

                            if (tag.Name == HtmlConstants.Table)
                            {
                                if (value != "0")
                                    ApplyTableBorder(box, "1px");
                            }
                            else
                            {
                                box.BorderTopStyle = box.BorderLeftStyle = box.BorderRightStyle = box.BorderBottomStyle = CssConstants.Solid;
                            }
                            break;
                        case HtmlConstants.Bordercolor:
                            box.BorderLeftColor = box.BorderTopColor = box.BorderRightColor = box.BorderBottomColor = value.ToLower();
                            break;
                        case HtmlConstants.Cellspacing:
                            box.BorderSpacing = TranslateLength(value);
                            break;
                        case HtmlConstants.Cellpadding:
                            ApplyTablePadding(box, value);
                            break;
                        case HtmlConstants.Color:
                            box.Color = value.ToLower();
                            break;
                        case HtmlConstants.Dir:
                            box.Direction = value.ToLower();
                            break;
                        case HtmlConstants.Face:
                            box.FontFamily = _cssParser.ParseFontFamily(value);
                            break;
                        case HtmlConstants.Height:
                            box.Height = TranslateLength(value);
                            break;
                        case HtmlConstants.Hspace:
                            box.MarginRight = box.MarginLeft = TranslateLength(value);
                            break;
                        case HtmlConstants.Nowrap:
                            box.WhiteSpace = CssConstants.NoWrap;
                            break;
                        case HtmlConstants.Size:
                            if (tag.Name.Equals(HtmlConstants.Hr, StringComparison.OrdinalIgnoreCase))
                                box.Height = TranslateLength(value);
                            else if (tag.Name.Equals(HtmlConstants.Font, StringComparison.OrdinalIgnoreCase))
                                box.FontSize = value;
                            break;
                        case HtmlConstants.Valign:
                            box.VerticalAlign = value.ToLower();
                            break;
                        case HtmlConstants.Vspace:
                            box.MarginTop = box.MarginBottom = TranslateLength(value);
                            break;
                        case HtmlConstants.Width:
                            box.Width = TranslateLength(value);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts an HTML length into a Css length
        /// </summary>
        /// <param name="htmlLength"></param>
        /// <returns></returns>
        private static string TranslateLength(string htmlLength)
        {
            CssLength len = new CssLength(htmlLength);

            if (len.HasError)
            {
                return string.Format(NumberFormatInfo.InvariantInfo, "{0}px", htmlLength);
            }

            return htmlLength;
        }

        /// <summary>
        /// Cascades to the TD's the border spacified in the TABLE tag.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="border"></param>
        private static void ApplyTableBorder(CssBox table, string border)
        {
            SetForAllCells(table, cell =>
            {
                cell.BorderLeftStyle = cell.BorderTopStyle = cell.BorderRightStyle = cell.BorderBottomStyle = CssConstants.Solid;
                cell.BorderLeftWidth = cell.BorderTopWidth = cell.BorderRightWidth = cell.BorderBottomWidth = border;
            });
        }

        /// <summary>
        /// Cascades to the TD's the border spacified in the TABLE tag.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="padding"></param>
        private static void ApplyTablePadding(CssBox table, string padding)
        {
            var length = TranslateLength(padding);
            SetForAllCells(table, cell => cell.PaddingLeft = cell.PaddingTop = cell.PaddingRight = cell.PaddingBottom = length);
        }

        /// <summary>
        /// Execute action on all the "td" cells of the table.<br/>
        /// Handle if there is "theader" or "tbody" exists.
        /// </summary>
        /// <param name="table">the table element</param>
        /// <param name="action">the action to execute</param>
        private static void SetForAllCells(CssBox table, ActionInt<CssBox> action)
        {
            foreach (var l1 in table.Boxes)
            {
                foreach (var l2 in l1.Boxes)
                {
                    if (l2.HtmlTag != null && l2.HtmlTag.Name == "td")
                    {
                        action(l2);
                    }
                    else
                    {
                        foreach (var l3 in l2.Boxes)
                        {
                            action(l3);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Go over all the text boxes (boxes that have some text that will be rendered) and
        /// remove all boxes that have only white-spaces but are not 'preformatted' so they do not effect
        /// the rendered html.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectTextBoxes(CssBox box)
        {
            for (int i = box.Boxes.Count - 1; i >= 0; i--)
            {
                var childBox = box.Boxes[i];
                if (childBox.Text != null)
                {
                    // is the box has text
                    var keepBox = !childBox.Text.IsEmptyOrWhitespace();

                    // is the box is pre-formatted
                    keepBox = keepBox || childBox.WhiteSpace == CssConstants.Pre || childBox.WhiteSpace == CssConstants.PreWrap;

                    // is the box is only one in the parent
                    keepBox = keepBox || box.Boxes.Count == 1;

                    // is it a whitespace between two inline boxes
                    keepBox = keepBox || (i > 0 && i < box.Boxes.Count - 1 && box.Boxes[i - 1].IsInline && box.Boxes[i + 1].IsInline);

                    // is first/last box where is in inline box and it's next/previous box is inline
                    keepBox = keepBox || (i == 0 && box.Boxes.Count > 1 && box.Boxes[1].IsInline && box.IsInline) || (i == box.Boxes.Count - 1 && box.Boxes.Count > 1 && box.Boxes[i - 1].IsInline && box.IsInline);

                    if (keepBox)
                    {
                        // valid text box, parse it to words
                        childBox.ParseToWords();
                    }
                    else
                    {
                        // remove text box that has no 
                        childBox.ParentBox.Boxes.RemoveAt(i);
                    }
                }
                else
                {
                    // recursive
                    CorrectTextBoxes(childBox);
                }
            }
        }

        /// <summary>
        /// Go over all image boxes and if its display style is set to block, put it inside another block but set the image to inline.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectImgBoxes(CssBox box)
        {
            for (int i = box.Boxes.Count - 1; i >= 0; i--)
            {
                var childBox = box.Boxes[i];
                if (childBox is CssBoxImage && childBox.Display == CssConstants.Block)
                {
                    var block = CssBox.CreateBlock(childBox.ParentBox, null, childBox);
                    childBox.ParentBox = block;
                    childBox.Display = CssConstants.Inline;
                }
                else
                {
                    // recursive
                    CorrectImgBoxes(childBox);
                }
            }
        }

        /// <summary>
        /// Correct the DOM tree recursively by replacing  "br" html boxes with anonymous blocks that respect br spec.<br/>
        /// If the "br" tag is after inline box then the anon block will have zero height only acting as newline,
        /// but if it is after block box then it will have min-height of the font size so it will create empty line.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        /// <param name="followingBlock">used to know if the br is following a box so it should create an empty line or not so it only
        /// move to a new line</param>
        private static void CorrectLineBreaksBlocks(CssBox box, ref bool followingBlock)
        {
            followingBlock = followingBlock || box.IsBlock;
            foreach (var childBox in box.Boxes)
            {
                CorrectLineBreaksBlocks(childBox, ref followingBlock);
                followingBlock = childBox.Words.Count == 0 && (followingBlock || childBox.IsBlock);
            }

            int lastBr = -1;
            CssBox brBox;
            do
            {
                brBox = null;
                for (int i = 0; i < box.Boxes.Count && brBox == null; i++)
                {
                    if (i > lastBr && box.Boxes[i].IsBrElement)
                    {
                        brBox = box.Boxes[i];
                        lastBr = i;
                    }
                    else if (box.Boxes[i].Words.Count > 0)
                    {
                        followingBlock = false;
                    }
                    else if (box.Boxes[i].IsBlock)
                    {
                        followingBlock = true;
                    }
                }

                if (brBox != null)
                {
                    brBox.Display = CssConstants.Block;
                    if (followingBlock)
                        brBox.Height = ".95em"; // TODO:a check the height to min-height when it is supported
                }
            } while (brBox != null);
        }

        /// <summary>
        /// Correct DOM tree if there is block boxes that are inside inline blocks.<br/>
        /// Need to rearrange the tree so block box will be only the child of other block box.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectBlockInsideInline(CssBox box)
        {
            try
            {
                if (DomUtils.ContainsInlinesOnly(box) && !ContainsInlinesOnlyDeep(box))
                {
                    var tempRightBox = CorrectBlockInsideInlineImp(box);
                    while (tempRightBox != null)
                    {
                        // loop on the created temp right box for the fixed box until no more need (optimization remove recursion)
                        CssBox newTempRightBox = null;
                        if (DomUtils.ContainsInlinesOnly(tempRightBox) && !ContainsInlinesOnlyDeep(tempRightBox))
                            newTempRightBox = CorrectBlockInsideInlineImp(tempRightBox);

                        tempRightBox.ParentBox.SetAllBoxes(tempRightBox);
                        tempRightBox.ParentBox = null;
                        tempRightBox = newTempRightBox;
                    }
                }

                if (!DomUtils.ContainsInlinesOnly(box))
                {
                    foreach (var childBox in box.Boxes)
                    {
                        CorrectBlockInsideInline(childBox);
                    }
                }
            }
            catch (Exception ex)
            {
                box.HtmlContainer.ReportError(HtmlRenderErrorType.HtmlParsing, "Failed in block inside inline box correction", ex);
            }
        }

        /// <summary>
        /// Rearrange the DOM of the box to have block box with boxes before the inner block box and after.
        /// </summary>
        /// <param name="box">the box that has the problem</param>
        private static CssBox CorrectBlockInsideInlineImp(CssBox box)
        {
            if (box.Display == CssConstants.Inline)
                box.Display = CssConstants.Block;

            if (box.Boxes.Count > 1 || box.Boxes[0].Boxes.Count > 1)
            {
                var leftBlock = CssBox.CreateBlock(box);

                while (ContainsInlinesOnlyDeep(box.Boxes[0]))
                    box.Boxes[0].ParentBox = leftBlock;
                leftBlock.SetBeforeBox(box.Boxes[0]);

                var splitBox = box.Boxes[1];
                splitBox.ParentBox = null;

                CorrectBlockSplitBadBox(box, splitBox, leftBlock);

                // remove block that did not get any inner elements
                if (leftBlock.Boxes.Count < 1)
                    leftBlock.ParentBox = null;

                int minBoxes = leftBlock.ParentBox != null ? 2 : 1;
                if (box.Boxes.Count > minBoxes)
                {
                    // create temp box to handle the tail elements and then get them back so no deep hierarchy is created
                    var tempRightBox = CssBox.CreateBox(box, null, box.Boxes[minBoxes]);
                    while (box.Boxes.Count > minBoxes + 1)
                        box.Boxes[minBoxes + 1].ParentBox = tempRightBox;

                    return tempRightBox;
                }
            }
            else if (box.Boxes[0].Display == CssConstants.Inline)
            {
                box.Boxes[0].Display = CssConstants.Block;
            }

            return null;
        }

        /// <summary>
        /// Split bad box that has inline and block boxes into two parts, the left - before the block box
        /// and right - after the block box.
        /// </summary>
        /// <param name="parentBox">the parent box that has the problem</param>
        /// <param name="badBox">the box to split into different boxes</param>
        /// <param name="leftBlock">the left block box that is created for the split</param>
        private static void CorrectBlockSplitBadBox(CssBox parentBox, CssBox badBox, CssBox leftBlock)
        {
            CssBox leftbox = null;
            while (badBox.Boxes[0].IsInline && ContainsInlinesOnlyDeep(badBox.Boxes[0]))
            {
                if (leftbox == null)
                {
                    // if there is no elements in the left box there is no reason to keep it
                    leftbox = CssBox.CreateBox(leftBlock, badBox.HtmlTag);
                    leftbox.InheritStyle(badBox, true);
                }
                badBox.Boxes[0].ParentBox = leftbox;
            }

            var splitBox = badBox.Boxes[0];
            if (!ContainsInlinesOnlyDeep(splitBox))
            {
                CorrectBlockSplitBadBox(parentBox, splitBox, leftBlock);
                splitBox.ParentBox = null;
            }
            else
            {
                splitBox.ParentBox = parentBox;
            }

            if (badBox.Boxes.Count > 0)
            {
                CssBox rightBox;
                if (splitBox.ParentBox != null || parentBox.Boxes.Count < 3)
                {
                    rightBox = CssBox.CreateBox(parentBox, badBox.HtmlTag);
                    rightBox.InheritStyle(badBox, true);

                    if (parentBox.Boxes.Count > 2)
                        rightBox.SetBeforeBox(parentBox.Boxes[1]);

                    if (splitBox.ParentBox != null)
                        splitBox.SetBeforeBox(rightBox);
                }
                else
                {
                    rightBox = parentBox.Boxes[2];
                }

                rightBox.SetAllBoxes(badBox);
            }
            else if (splitBox.ParentBox != null && parentBox.Boxes.Count > 1)
            {
                splitBox.SetBeforeBox(parentBox.Boxes[1]);
                if (splitBox.HtmlTag != null && splitBox.HtmlTag.Name == "br" && (leftbox != null || leftBlock.Boxes.Count > 1))
                    splitBox.Display = CssConstants.Inline;
            }
        }

        /// <summary>
        /// Makes block boxes be among only block boxes and all inline boxes have block parent box.<br/>
        /// Inline boxes should live in a pool of Inline boxes only so they will define a single block.<br/>
        /// At the end of this process a block box will have only block siblings and inline box will have
        /// only inline siblings.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectInlineBoxesParent(CssBox box)
        {
            if (ContainsVariantBoxes(box))
            {
                for (int i = 0; i < box.Boxes.Count; i++)
                {
                    if (box.Boxes[i].IsInline)
                    {
                        var newbox = CssBox.CreateBlock(box, null, box.Boxes[i++]);
                        while (i < box.Boxes.Count && box.Boxes[i].IsInline)
                        {
                            box.Boxes[i].ParentBox = newbox;
                        }
                    }
                }
            }

            if (!DomUtils.ContainsInlinesOnly(box))
            {
                foreach (var childBox in box.Boxes)
                {
                    CorrectInlineBoxesParent(childBox);
                }
            }
        }

        /// <summary>
        /// Check if the given box contains only inline child boxes in all subtree.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - only inline child boxes, false - otherwise</returns>
        private static bool ContainsInlinesOnlyDeep(CssBox box)
        {
            foreach (var childBox in box.Boxes)
            {
                if (!childBox.IsInline || !ContainsInlinesOnlyDeep(childBox))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the given box contains inline and block child boxes.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - has variant child boxes, false - otherwise</returns>
        private static bool ContainsVariantBoxes(CssBox box)
        {
            bool hasBlock = false;
            bool hasInline = false;
            for (int i = 0; i < box.Boxes.Count && (!hasBlock || !hasInline); i++)
            {
                var isBlock = !box.Boxes[i].IsInline;
                hasBlock = hasBlock || isBlock;
                hasInline = hasInline || !isBlock;
            }

            return hasBlock && hasInline;
        }

        #endregion
    }
}