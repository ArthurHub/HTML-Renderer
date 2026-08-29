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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Helps on CSS Layout.
    /// </summary>
    internal static class CssLayoutEngine
    {
        /// <summary>
        /// Measure image box size by the width\height set on the box and the actual rendered image size.<br/>
        /// If no image exists for the box error icon will be set.
        /// </summary>
        /// <param name="imageWord">the image word to measure</param>
        public static void MeasureImageSize(CssRectImage imageWord)
        {
            ArgChecker.AssertArgNotNull(imageWord, "imageWord");
            ArgChecker.AssertArgNotNull(imageWord.OwnerBox, "imageWord.OwnerBox");

            var width = new CssLength(imageWord.OwnerBox.Width);
            var height = new CssLength(imageWord.OwnerBox.Height);

            bool hasImageTagWidth = width.Number > 0 && width.Unit == CssUnit.Pixels;
            bool hasImageTagHeight = height.Number > 0 && height.Unit == CssUnit.Pixels;
            bool scaleImageHeight = false;

            if (hasImageTagWidth)
            {
                imageWord.Width = width.Number;
            }
            else if (width.Number > 0 && width.IsPercentage)
            {
                imageWord.Width = width.Number * imageWord.OwnerBox.ContainingBlock.Size.Width;
                scaleImageHeight = true;
            }
            else if (imageWord.Image != null)
            {
                imageWord.Width = imageWord.ImageRectangle == RRect.Empty ? imageWord.Image.Width : imageWord.ImageRectangle.Width;
            }
            else
            {
                imageWord.Width = hasImageTagHeight ? height.Number / 1.14f : 20;
            }

            var maxWidth = new CssLength(imageWord.OwnerBox.MaxWidth);
            if (maxWidth.Number > 0)
            {
                double maxWidthVal = -1;
                if (maxWidth.Unit == CssUnit.Pixels)
                {
                    maxWidthVal = maxWidth.Number;
                }
                else if (maxWidth.IsPercentage)
                {
                    maxWidthVal = maxWidth.Number * imageWord.OwnerBox.ContainingBlock.Size.Width;
                }

                if (maxWidthVal > -1 && imageWord.Width > maxWidthVal)
                {
                    imageWord.Width = maxWidthVal;
                    scaleImageHeight = !hasImageTagHeight;
                }
            }

            if (hasImageTagHeight)
            {
                imageWord.Height = height.Number;
            }
            else if (imageWord.Image != null)
            {
                imageWord.Height = imageWord.ImageRectangle == RRect.Empty ? imageWord.Image.Height : imageWord.ImageRectangle.Height;
            }
            else
            {
                imageWord.Height = imageWord.Width > 0 ? imageWord.Width * 1.14f : 22.8f;
            }

            if (imageWord.Image != null)
            {
                // If only the width was set in the html tag, ratio the height.
                if ((hasImageTagWidth && !hasImageTagHeight) || scaleImageHeight)
                {
                    // Divide the given tag width with the actual image width, to get the ratio.
                    double ratio = imageWord.Width / imageWord.Image.Width;
                    imageWord.Height = imageWord.Image.Height * ratio;
                }
                // If only the height was set in the html tag, ratio the width.
                else if (hasImageTagHeight && !hasImageTagWidth)
                {
                    // Divide the given tag height with the actual image height, to get the ratio.
                    double ratio = imageWord.Height / imageWord.Image.Height;
                    imageWord.Width = imageWord.Image.Width * ratio;
                }
            }

            imageWord.Height += imageWord.OwnerBox.ActualBorderBottomWidth + imageWord.OwnerBox.ActualBorderTopWidth + imageWord.OwnerBox.ActualPaddingTop + imageWord.OwnerBox.ActualPaddingBottom;
        }

        /// <summary>
        /// Creates line boxes for the specified blockbox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="blockBox"></param>
        public static void CreateLineBoxes(RGraphics g, CssBox blockBox)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            ArgChecker.AssertArgNotNull(blockBox, "blockBox");

            blockBox.LineBoxes.Clear();

            double limitRight = blockBox.ActualRight - blockBox.ActualPaddingRight - blockBox.ActualBorderRightWidth;

            //Get the start x and y of the blockBox
            double startx = blockBox.Location.X + blockBox.ActualPaddingLeft - 0 + blockBox.ActualBorderLeftWidth;
            double starty = blockBox.Location.Y + blockBox.ActualPaddingTop - 0 + blockBox.ActualBorderTopWidth;
            double curx = startx + blockBox.ActualTextIndent;
            double cury = starty;

            //Reminds the maximum bottom reached
            double maxRight = startx;
            double maxBottom = starty;

            //First line box
            CssLineBox line = new CssLineBox(blockBox);

            //Flow words and boxes
            FlowBox(g, blockBox, blockBox, limitRight, 0, startx, ref line, ref curx, ref cury, ref maxRight, ref maxBottom);

            // if width is not restricted we need to lower it to the actual width
            if (blockBox.ActualRight >= 90999)
            {
                blockBox.ActualRight = maxRight + blockBox.ActualPaddingRight + blockBox.ActualBorderRightWidth;
            }

            //Gets the rectangles for each line-box
            foreach (var linebox in blockBox.LineBoxes)
            {
                ApplyHorizontalAlignment(g, linebox);
                ApplyRightToLeft(blockBox, linebox);
                BubbleRectangles(blockBox, linebox);
                ApplyVerticalAlignment(g, linebox);
                linebox.AssignRectanglesToBoxes();
            }

            blockBox.ActualBottom = maxBottom + blockBox.ActualPaddingBottom + blockBox.ActualBorderBottomWidth;

            // handle limiting block height when overflow is hidden
            if (blockBox.Height != null && blockBox.Height != CssConstants.Auto && blockBox.Overflow == CssConstants.Hidden && blockBox.ActualBottom - blockBox.Location.Y > blockBox.ActualHeight)
            {
                blockBox.ActualBottom = blockBox.Location.Y + blockBox.ActualHeight;
            }
        }

        /// <summary>
        /// Applies float positioning (float: left/right) and clearance (clear) to a box that has already
        /// had its static in-flow position committed by the caller (<see cref="CssBox.PerformLayoutImp"/>) -
        /// overwrites <see cref="CssBox.Location"/> using that static position as input. No-op if the box
        /// is neither floated nor has clear set. Ported from PeachPDF.
        /// </summary>
        /// <param name="box">the box to float/clear - its Location.Y must already hold its static position</param>
        public static void FloatBox(CssBox box)
        {
            if (box.Float == CssConstants.None && box.Clear == CssConstants.None)
                return;

            var containingBox = box.ContainingBlock;
            var limitRight = containingBox.ClientRight;
            var startX = containingBox.ClientLeft;
            var startY = box.Location.Y;
            var currentBoxIdx = containingBox.Boxes.IndexOf(box);

            switch (box.Float)
            {
                case CssConstants.Left:
                    FloatBoxLeft(box, startX, startY, limitRight);
                    break;
                case CssConstants.Right:
                    FloatBoxRight(box, startX, startY, limitRight);
                    break;
            }

            if (box.Clear != CssConstants.None)
            {
                ClearBox(box, currentBoxIdx, containingBox);
            }
        }

        /// <summary>
        /// Applies special vertical alignment for table-cells
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cell"></param>
        public static void ApplyCellVerticalAlignment(RGraphics g, CssBox cell)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            ArgChecker.AssertArgNotNull(cell, "cell");

            if (cell.VerticalAlign == CssConstants.Top || cell.VerticalAlign == CssConstants.Baseline)
                return;

            double cellbot = cell.ClientBottom;
            double bottom = cell.GetMaximumBottom(cell, 0f);
            double dist = 0f;

            if (cell.VerticalAlign == CssConstants.Bottom)
            {
                dist = cellbot - bottom;
            }
            else if (cell.VerticalAlign == CssConstants.Middle)
            {
                dist = (cellbot - bottom) / 2;
            }

            foreach (CssBox b in cell.Boxes)
            {
                b.OffsetTop(dist);
            }

            //float top = cell.ClientTop;
            //float bottom = cell.ClientBottom;
            //bool middle = cell.VerticalAlign == CssConstants.Middle;

            //foreach (LineBox line in cell.LineBoxes)
            //{
            //    for (int i = 0; i < line.RelatedBoxes.Count; i++)
            //    {

            //        double diff = bottom - line.RelatedBoxes[i].Rectangles[line].Bottom;
            //        if (middle) diff /= 2f;
            //        RectangleF r = line.RelatedBoxes[i].Rectangles[line];
            //        line.RelatedBoxes[i].Rectangles[line] = new RectangleF(r.X, r.Y + diff, r.Width, r.Height);

            //    }

            //    foreach (BoxWord word in line.Words)
            //    {
            //        double gap = word.Top - top;
            //        word.Top = bottom - gap - word.Height;
            //    }
            //}
        }


        #region Private methods

        /// <summary>
        /// Pushes <paramref name="box"/> down past any floated preceding sibling (at any depth - a float
        /// nested inside a plain non-floated wrapper still needs clearing against, CSS 2.1 9.5.2) that
        /// matches <paramref name="box"/>'s <c>clear</c> direction. Only ever changes Y, never X - X is
        /// left exactly as <see cref="FloatBox"/>/normal static-position assignment already set it.
        /// </summary>
        private static void ClearBox(CssBox box, int currentBoxIdx, CssBox containingBox)
        {
            var clearance = Math.Max(containingBox.ClientTop, box.Location.Y);

            for (int i = 0; i < currentBoxIdx; i++)
            {
                clearance = Math.Max(clearance, GetClearance(containingBox.Boxes[i], box.Clear));
            }

            box.Location = new RPoint(box.Location.X, clearance);
        }

        /// <summary>
        /// Recursively finds the outer (margin) bottom edge of every floated box in <paramref name="sibling"/>'s
        /// subtree that matches <paramref name="clearDirection"/>, testing <paramref name="sibling"/> itself
        /// and every descendant at every depth (not just direct children) - so a float directly inside another
        /// float (e.g. ACID1's <c>dd</c> float:right containing <c>blockquote</c>/<c>h1</c> float:left as direct
        /// children) is never missed.
        /// </summary>
        private static double GetClearance(CssBox sibling, string clearDirection)
        {
            double clearance = 0;

            if (sibling.IsFloated &&
                (clearDirection == CssConstants.Both ||
                 (clearDirection == CssConstants.Left && sibling.Float == CssConstants.Left) ||
                 (clearDirection == CssConstants.Right && sibling.Float == CssConstants.Right)))
            {
                clearance = sibling.ActualBottom + sibling.ActualMarginBottom;
            }

            foreach (var child in sibling.Boxes)
            {
                clearance = Math.Max(clearance, GetClearance(child, clearDirection));
            }

            return clearance;
        }

        private static void FloatBoxLeft(CssBox box, double startX, double startY, double limitRight)
        {
            var coordinates = new DomUtils.CssFloatCoordinates
            {
                Left = startX + box.ActualMarginLeft,
                Right = limitRight,
                Top = startY,
                MaxBottom = startY,
                MarginLeft = box.ActualMarginLeft,
                MarginRight = box.ActualMarginRight,
                // FloatBox (this method's only caller) runs from PerformLayoutImp's block-width-
                // resolution branch, after Size.Width is already resolved to this box's border-box
                // width - use it directly rather than ActualWidth, which re-parses the Width CSS
                // string against Size.Width as a percentage base (correct for the inline-block callers
                // it otherwise serves, but not what a border-box reference width means here: for a
                // fixed-unit Width like "34em" it returns the bare content-only value, ignoring
                // Size.Width entirely, understating a padded/bordered float's true footprint).
                ReferenceWidth = box.Size.Width
            };

            while (true)
            {
                var intersectingFloat = DomUtils.GetFirstIntersectingFloatBox(box, coordinates, box.Float);
                if (intersectingFloat == null)
                    break;

                switch (intersectingFloat.Float)
                {
                    case CssConstants.Left:
                        coordinates.Left = intersectingFloat.ActualRight + intersectingFloat.ActualMarginRight + box.ActualMarginLeft;
                        break;
                    case CssConstants.Right:
                        coordinates.Right = intersectingFloat.Location.X - intersectingFloat.ActualMarginLeft;
                        break;
                }

                if (intersectingFloat.ActualBottom > coordinates.MaxBottom)
                {
                    coordinates.MaxBottom = intersectingFloat.ActualBottom;
                }

                if (coordinates.Left + box.Size.Width > coordinates.Right)
                {
                    coordinates.Top = coordinates.MaxBottom + box.ActualMarginTop;
                    coordinates.Left = startX + box.ActualMarginLeft;
                }
            }

            box.Location = new RPoint(coordinates.Left, coordinates.Top);
        }

        private static void FloatBoxRight(CssBox box, double startX, double startY, double limitRight)
        {
            var coordinates = new DomUtils.CssFloatCoordinates
            {
                Left = startX,
                Right = limitRight - box.ActualMarginRight,
                Top = startY,
                MaxBottom = startY,
                MarginLeft = box.ActualMarginLeft,
                MarginRight = box.ActualMarginRight,
                // See FloatBoxLeft's matching comment - Size.Width is this box's own already-resolved
                // border-box width, which is what ReferenceWidth means here.
                ReferenceWidth = box.Size.Width
            };

            while (true)
            {
                var intersectingFloat = DomUtils.GetFirstIntersectingFloatBox(box, coordinates, box.Float);
                if (intersectingFloat == null)
                    break;

                switch (intersectingFloat.Float)
                {
                    case CssConstants.Left:
                        coordinates.Left = intersectingFloat.ActualRight;
                        break;
                    case CssConstants.Right:
                        coordinates.Right = intersectingFloat.Location.X;
                        break;
                }

                if (intersectingFloat.ActualBottom > coordinates.MaxBottom)
                {
                    coordinates.MaxBottom = intersectingFloat.ActualBottom;
                }

                if (coordinates.Left > coordinates.FloatRightStartX)
                {
                    coordinates.Right = limitRight - box.ActualMarginRight;
                    coordinates.Top = coordinates.MaxBottom;
                }
            }

            box.Location = new RPoint(coordinates.FloatRightStartX, coordinates.Top);
        }

        /// <summary>
        /// Recursively flows the content of the box using the inline model
        /// </summary>
        /// <param name="g">Device Info</param>
        /// <param name="blockbox">Blockbox that contains the text flow</param>
        /// <param name="box">Current box to flow its content</param>
        /// <param name="limitRight">Maximum reached right</param>
        /// <param name="linespacing">Space to use between rows of text</param>
        /// <param name="startx">x starting coordinate for when breaking lines of text</param>
        /// <param name="line">Current linebox being used</param>
        /// <param name="curx">Current x coordinate that will be the left of the next word</param>
        /// <param name="cury">Current y coordinate that will be the top of the next word</param>
        /// <param name="maxRight">Maximum right reached so far</param>
        /// <param name="maxbottom">Maximum bottom reached so far</param>
        private static void FlowBox(RGraphics g, CssBox blockbox, CssBox box, double limitRight, double linespacing, double startx, ref CssLineBox line, ref double curx, ref double cury, ref double maxRight, ref double maxbottom)
        {
            var startX = curx;
            var startY = cury;
            box.FirstHostingLineBox = line;
            var localCurx = curx;
            var localMaxRight = maxRight;
            var localmaxbottom = maxbottom;

            foreach (CssBox b in box.Boxes)
            {
                double leftspacing = (b.Position != CssConstants.Absolute && b.Position != CssConstants.Fixed) ? b.ActualMarginLeft + b.ActualBorderLeftWidth + b.ActualPaddingLeft : 0;
                double rightspacing = (b.Position != CssConstants.Absolute && b.Position != CssConstants.Fixed) ? b.ActualMarginRight + b.ActualBorderRightWidth + b.ActualPaddingRight : 0;

                b.RectanglesReset();
                b.MeasureWordsSize(g);

                curx += leftspacing;

                var leadingLeftFloat = DomUtils.GetLastLeftIntersectingFloatBox(box, curx, cury, maxRight, maxbottom);
                if (leadingLeftFloat != null)
                {
                    curx = leadingLeftFloat.ActualRight + leadingLeftFloat.ActualMarginRight + leftspacing;
                }

                if (b.Words.Count > 0)
                {
                    bool wrapNoWrapBox = false;
                    if (b.WhiteSpace == CssConstants.NoWrap && curx > startx)
                    {
                        var boxRight = curx;
                        foreach (var word in b.Words)
                            boxRight += word.FullWidth;
                        if (boxRight > limitRight)
                            wrapNoWrapBox = true;
                    }

                    if (DomUtils.IsBoxHasWhitespace(b))
                        curx += box.ActualWordSpacing;

                    foreach (var word in b.Words)
                    {
                        if (maxbottom - cury < box.ActualLineHeight)
                            maxbottom += box.ActualLineHeight - (maxbottom - cury);

                        double actualLimitRight = limitRight;
                        var rightObstructingFloat = DomUtils.GetLastRightIntersectingFloatBox(box, cury);
                        if (rightObstructingFloat != null)
                        {
                            actualLimitRight = rightObstructingFloat.Location.X - rightObstructingFloat.ActualMarginLeft - rightspacing;
                        }

                        if ((b.WhiteSpace != CssConstants.NoWrap && b.WhiteSpace != CssConstants.Pre && curx + word.Width + rightspacing > actualLimitRight
                             && (b.WhiteSpace != CssConstants.PreWrap || !word.IsSpaces))
                            || word.IsLineBreak || wrapNoWrapBox)
                        {
                            wrapNoWrapBox = false;
                            curx = startx;

                            // handle if line is wrapped for the first text element where parent has left margin\padding
                            if (b == box.Boxes[0] && !word.IsLineBreak && (word == b.Words[0] || (box.ParentBox != null && box.ParentBox.IsBlock)))
                                curx += box.ActualMarginLeft + box.ActualBorderLeftWidth + box.ActualPaddingLeft;

                            cury = maxbottom + linespacing;

                            var wrapLeftFloat = DomUtils.GetLastLeftIntersectingFloatBox(b, curx, cury, maxRight, maxbottom);
                            if (wrapLeftFloat != null)
                            {
                                curx = wrapLeftFloat.ActualRight + wrapLeftFloat.ActualMarginRight + leftspacing;
                            }

                            line = new CssLineBox(blockbox);

                            if (word.IsImage || word.Equals(b.FirstWord))
                            {
                                curx += leftspacing;
                            }
                        }

                        line.ReportExistanceOf(word);

                        var placementLeftFloat = DomUtils.GetLastLeftIntersectingFloatBox(box, curx, cury, maxRight, maxbottom);
                        if (placementLeftFloat != null)
                        {
                            curx = placementLeftFloat.ActualRight + placementLeftFloat.ActualMarginRight + leftspacing;
                        }

                        word.Left = curx;
                        word.Top = cury;

                        curx = word.Left + word.FullWidth;

                        maxRight = Math.Max(maxRight, word.Right);
                        maxbottom = Math.Max(maxbottom, word.Bottom);

                        if (b.Position == CssConstants.Absolute)
                        {
                            word.Left += box.ActualMarginLeft;
                            word.Top += box.ActualMarginTop;
                        }
                    }
                }
                else
                {
                    FlowBox(g, blockbox, b, limitRight, linespacing, startx, ref line, ref curx, ref cury, ref maxRight, ref maxbottom);
                }

                curx += rightspacing;
            }

            // handle height setting
            if (maxbottom - startY < box.ActualHeight)
            {
                maxbottom += box.ActualHeight - (maxbottom - startY);
            }

            // handle width setting
            if (box.IsInline && 0 <= curx - startX && curx - startX < box.ActualWidth)
            {
                // hack for actual width handling
                curx += box.ActualWidth - (curx - startX);
                line.Rectangles.Add(box, new RRect(startX, startY, box.ActualWidth, box.ActualHeight));
            }

            // handle box that is only a whitespace
            if (box.Text != null && CommonUtils.IsNonEmptyWhitespace(box.Text) && !box.IsImage && box.IsInline && box.Boxes.Count == 0 && box.Words.Count == 0)
            {
                curx += box.ActualWordSpacing;
            }

            // hack to support specific absolute position elements
            if (box.Position == CssConstants.Absolute)
            {
                curx = localCurx;
                maxRight = localMaxRight;
                maxbottom = localmaxbottom;
                AdjustAbsolutePosition(box, 0, 0);
            }

            box.LastHostingLineBox = line;
        }

        /// <summary>
        /// Adjust the position of absolute elements by letf and top margins.
        /// </summary>
        private static void AdjustAbsolutePosition(CssBox box, double left, double top)
        {
            left += box.ActualMarginLeft;
            top += box.ActualMarginTop;
            if (box.Words.Count > 0)
            {
                foreach (var word in box.Words)
                {
                    word.Left += left;
                    word.Top += top;
                }
            }
            else
            {
                foreach (var b in box.Boxes)
                    AdjustAbsolutePosition(b, left, top);
            }
        }

        /// <summary>
        /// Recursively creates the rectangles of the blockBox, by bubbling from deep to outside of the boxes 
        /// in the rectangle structure
        /// </summary>
        private static void BubbleRectangles(CssBox box, CssLineBox line)
        {
            if (box.Words.Count > 0)
            {
                double x = Single.MaxValue, y = Single.MaxValue, r = Single.MinValue, b = Single.MinValue;
                List<CssRect> words = line.WordsOf(box);

                if (words.Count > 0)
                {
                    foreach (CssRect word in words)
                    {
                        // handle if line is wrapped for the first text element where parent has left margin\padding
                        var left = word.Left;

                        if (box == box.ParentBox.Boxes[0] && word == box.Words[0] && word == line.Words[0] && line != line.OwnerBox.LineBoxes[0] && !word.IsLineBreak)
                            left -= box.ParentBox.ActualMarginLeft + box.ParentBox.ActualBorderLeftWidth + box.ParentBox.ActualPaddingLeft;


                        x = Math.Min(x, left);
                        r = Math.Max(r, word.Right);
                        y = Math.Min(y, word.Top);
                        b = Math.Max(b, word.Bottom);
                    }
                    line.UpdateRectangle(box, x, y, r, b);
                }
            }
            else
            {
                foreach (CssBox b in box.Boxes)
                {
                    BubbleRectangles(b, line);
                }
            }
        }

        /// <summary>
        /// Applies vertical and horizontal alignment to words in lineboxes
        /// </summary>
        /// <param name="g"></param>
        /// <param name="lineBox"></param>
        private static void ApplyHorizontalAlignment(RGraphics g, CssLineBox lineBox)
        {
            switch (lineBox.OwnerBox.TextAlign)
            {
                case CssConstants.Right:
                    ApplyRightAlignment(g, lineBox);
                    break;
                case CssConstants.Center:
                    ApplyCenterAlignment(g, lineBox);
                    break;
                case CssConstants.Justify:
                    ApplyJustifyAlignment(g, lineBox);
                    break;
                default:
                    ApplyLeftAlignment(g, lineBox);
                    break;
            }
        }

        /// <summary>
        /// Applies right to left direction to words
        /// </summary>
        /// <param name="blockBox"></param>
        /// <param name="lineBox"></param>
        private static void ApplyRightToLeft(CssBox blockBox, CssLineBox lineBox)
        {
            if (blockBox.Direction == CssConstants.Rtl)
            {
                ApplyRightToLeftOnLine(lineBox);
            }
            else
            {
                foreach (var box in lineBox.RelatedBoxes)
                {
                    if (box.Direction == CssConstants.Rtl)
                    {
                        ApplyRightToLeftOnSingleBox(lineBox, box);
                    }
                }
            }
        }

        /// <summary>
        /// Applies RTL direction to all the words on the line.
        /// </summary>
        /// <param name="line">the line to apply RTL to</param>
        private static void ApplyRightToLeftOnLine(CssLineBox line)
        {
            if (line.Words.Count > 0)
            {
                double left = line.Words[0].Left;
                double right = line.Words[line.Words.Count - 1].Right;

                foreach (CssRect word in line.Words)
                {
                    double diff = word.Left - left;
                    double wright = right - diff;
                    word.Left = wright - word.Width;
                }
            }
        }

        /// <summary>
        /// Applies RTL direction to specific box words on the line.
        /// </summary>
        /// <param name="lineBox"></param>
        /// <param name="box"></param>
        private static void ApplyRightToLeftOnSingleBox(CssLineBox lineBox, CssBox box)
        {
            int leftWordIdx = -1;
            int rightWordIdx = -1;
            for (int i = 0; i < lineBox.Words.Count; i++)
            {
                if (lineBox.Words[i].OwnerBox == box)
                {
                    if (leftWordIdx < 0)
                        leftWordIdx = i;
                    rightWordIdx = i;
                }
            }

            if (leftWordIdx > -1 && rightWordIdx > leftWordIdx)
            {
                double left = lineBox.Words[leftWordIdx].Left;
                double right = lineBox.Words[rightWordIdx].Right;

                for (int i = leftWordIdx; i <= rightWordIdx; i++)
                {
                    double diff = lineBox.Words[i].Left - left;
                    double wright = right - diff;
                    lineBox.Words[i].Left = wright - lineBox.Words[i].Width;
                }
            }
        }

        /// <summary>
        /// Applies vertical alignment to the linebox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="lineBox"></param>
        private static void ApplyVerticalAlignment(RGraphics g, CssLineBox lineBox)
        {
            double baseline = Single.MinValue;
            foreach (var box in lineBox.Rectangles.Keys)
            {
                baseline = Math.Max(baseline, lineBox.Rectangles[box].Top);
            }

            var boxes = new List<CssBox>(lineBox.Rectangles.Keys);
            foreach (CssBox box in boxes)
            {
                //Important notes on http://www.w3.org/TR/CSS21/tables.html#height-layout
                switch (box.VerticalAlign)
                {
                    case CssConstants.Sub:
                        lineBox.SetBaseLine(g, box, baseline + lineBox.Rectangles[box].Height * .5f);
                        break;
                    case CssConstants.Super:
                        lineBox.SetBaseLine(g, box, baseline - lineBox.Rectangles[box].Height * .2f);
                        break;
                    case CssConstants.TextTop:

                        break;
                    case CssConstants.TextBottom:

                        break;
                    case CssConstants.Top:

                        break;
                    case CssConstants.Bottom:

                        break;
                    case CssConstants.Middle:

                        break;
                    default:
                        //case: baseline
                        lineBox.SetBaseLine(g, box, baseline);
                        break;
                }
            }
        }

        /// <summary>
        /// Applies centered alignment to the text on the linebox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="lineBox"></param>
        private static void ApplyJustifyAlignment(RGraphics g, CssLineBox lineBox)
        {
            if (lineBox.Equals(lineBox.OwnerBox.LineBoxes[lineBox.OwnerBox.LineBoxes.Count - 1]))
                return;

            double indent = lineBox.Equals(lineBox.OwnerBox.LineBoxes[0]) ? lineBox.OwnerBox.ActualTextIndent : 0f;
            double textSum = 0f;
            double words = 0f;
            double availWidth = lineBox.OwnerBox.ClientRectangle.Width - indent;

            // Gather text sum
            foreach (CssRect w in lineBox.Words)
            {
                textSum += w.Width;
                words += 1f;
            }

            if (words <= 0f)
                return; //Avoid Zero division
            double spacing = (availWidth - textSum) / words; //Spacing that will be used
            double curx = lineBox.OwnerBox.ClientLeft + indent;

            foreach (CssRect word in lineBox.Words)
            {
                word.Left = curx;
                curx = word.Right + spacing;

                if (word == lineBox.Words[lineBox.Words.Count - 1])
                {
                    word.Left = lineBox.OwnerBox.ClientRight - word.Width;
                }
            }
        }

        /// <summary>
        /// Applies centered alignment to the text on the linebox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="line"></param>
        private static void ApplyCenterAlignment(RGraphics g, CssLineBox line)
        {
            if (line.Words.Count == 0)
                return;

            CssRect lastWord = line.Words[line.Words.Count - 1];
            double right = line.OwnerBox.ActualRight - line.OwnerBox.ActualPaddingRight - line.OwnerBox.ActualBorderRightWidth;
            double diff = right - lastWord.Right - lastWord.OwnerBox.ActualBorderRightWidth - lastWord.OwnerBox.ActualPaddingRight;
            diff /= 2;

            if (diff > 0)
            {
                foreach (CssRect word in line.Words)
                {
                    word.Left += diff;
                }

                if (line.Rectangles.Count > 0)
                {
                    foreach (CssBox b in ToList(line.Rectangles.Keys))
                    {
                        RRect r = line.Rectangles[b];
                        line.Rectangles[b] = new RRect(r.X + diff, r.Y, r.Width, r.Height);
                    }
                }
            }
        }

        /// <summary>
        /// Applies right alignment to the text on the linebox
        /// </summary>
        /// <param name="g"></param>
        /// <param name="line"></param>
        private static void ApplyRightAlignment(RGraphics g, CssLineBox line)
        {
            if (line.Words.Count == 0)
                return;


            CssRect lastWord = line.Words[line.Words.Count - 1];
            double right = line.OwnerBox.ActualRight - line.OwnerBox.ActualPaddingRight - line.OwnerBox.ActualBorderRightWidth;
            double diff = right - lastWord.Right - lastWord.OwnerBox.ActualBorderRightWidth - lastWord.OwnerBox.ActualPaddingRight;

            if (diff > 0)
            {
                foreach (CssRect word in line.Words)
                {
                    word.Left += diff;
                }

                if (line.Rectangles.Count > 0)
                {
                    foreach (CssBox b in ToList(line.Rectangles.Keys))
                    {
                        RRect r = line.Rectangles[b];
                        line.Rectangles[b] = new RRect(r.X + diff, r.Y, r.Width, r.Height);
                    }
                }
            }
        }

        /// <summary>
        /// Simplest alignment, just arrange words.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="line"></param>
        private static void ApplyLeftAlignment(RGraphics g, CssLineBox line)
        {
            //No alignment needed.

            //foreach (LineBoxRectangle r in line.Rectangles)
            //{
            //    double curx = r.Left + (r.Index == 0 ? r.OwnerBox.ActualPaddingLeft + r.OwnerBox.ActualBorderLeftWidth / 2 : 0);

            //    if (r.SpaceBefore) curx += r.OwnerBox.ActualWordSpacing;

            //    foreach (BoxWord word in r.Words)
            //    {
            //        word.Left = curx;
            //        word.Top = r.Top;// +r.OwnerBox.ActualPaddingTop + r.OwnerBox.ActualBorderTopWidth / 2;

            //        curx = word.Right + r.OwnerBox.ActualWordSpacing;
            //    }
            //}
        }

        /// <summary>
        /// todo: optimizate, not creating a list each time
        /// </summary>
        private static List<T> ToList<T>(IEnumerable<T> collection)
        {
            List<T> result = new List<T>();
            foreach (T item in collection)
            {
                result.Add(item);
            }
            return result;
        }

        #endregion
    }
}