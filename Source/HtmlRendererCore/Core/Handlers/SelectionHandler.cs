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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Handler for text selection in the html.
    /// </summary>
    internal sealed class SelectionHandler : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// the root of the handled html tree
        /// </summary>
        private readonly CssBox _root;

        /// <summary>
        /// handler for showing context menu on right click
        /// </summary>
        private readonly ContextMenuHandler _contextMenuHandler;

        /// <summary>
        /// the mouse location when selection started used to ignore small selections
        /// </summary>
        private RPoint _selectionStartPoint;

        /// <summary>
        /// the starting word of html selection<br/>
        /// where the user started the selection, if the selection is backwards then it will be the last selected word.
        /// </summary>
        private CssRect _selectionStart;

        /// <summary>
        /// the ending word of html selection<br/>
        /// where the user ended the selection, if the selection is backwards then it will be the first selected word.
        /// </summary>
        private CssRect _selectionEnd;

        /// <summary>
        /// the selection start index if the first selected word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        private int _selectionStartIndex = -1;

        /// <summary>
        /// the selection end index if the last selected word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        private int _selectionEndIndex = -1;

        /// <summary>
        /// the selection start offset if the first selected word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        private double _selectionStartOffset = -1;

        /// <summary>
        /// the selection end offset if the last selected word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        private double _selectionEndOffset = -1;

        /// <summary>
        /// is the selection goes backward in the html, the starting word comes after the ending word in DFS traversing.<br/>
        /// </summary>
        private bool _backwardSelection;

        /// <summary>
        /// used to ignore mouse up after selection
        /// </summary>
        private bool _inSelection;

        /// <summary>
        /// current selection process is after double click (full word selection)
        /// </summary>
        private bool _isDoubleClickSelect;

        /// <summary>
        /// used to know if selection is in the control or started outside so it needs to be ignored
        /// </summary>
        private bool _mouseDownInControl;

        /// <summary>
        /// used to handle drag & drop
        /// </summary>
        private bool _mouseDownOnSelectedWord;

        /// <summary>
        /// is the cursor on the control has been changed by the selection handler
        /// </summary>
        private bool _cursorChanged;

        /// <summary>
        /// used to know if double click selection is requested
        /// </summary>
        private DateTime _lastMouseDown;

        /// <summary>
        /// used to know if drag & drop was already started not to execute the same operation over
        /// </summary>
        private object _dragDropData;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="root">the root of the handled html tree</param>
        public SelectionHandler(CssBox root)
        {
            ArgChecker.AssertArgNotNull(root, "root");

            _root = root;
            _contextMenuHandler = new ContextMenuHandler(this, root.HtmlContainer);
        }

        /// <summary>
        /// Select all the words in the html.
        /// </summary>
        /// <param name="control">the control hosting the html to invalidate</param>
        public void SelectAll(RControl control)
        {
            if (_root.HtmlContainer.IsSelectionEnabled)
            {
                ClearSelection();
                SelectAllWords(_root);
                control.Invalidate();
            }
        }

        /// <summary>
        /// Select the word at the given location if found.
        /// </summary>
        /// <param name="control">the control hosting the html to invalidate</param>
        /// <param name="loc">the location to select word at</param>
        public void SelectWord(RControl control, RPoint loc)
        {
            if (_root.HtmlContainer.IsSelectionEnabled)
            {
                var word = DomUtils.GetCssBoxWord(_root, loc);
                if (word != null)
                {
                    word.Selection = this;
                    _selectionStartPoint = loc;
                    _selectionStart = _selectionEnd = word;
                    control.Invalidate();
                }
            }
        }

        /// <summary>
        /// Handle mouse down to handle selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="loc">the location of the mouse on the html</param>
        /// <param name="isMouseInContainer"> </param>
        public void HandleMouseDown(RControl parent, RPoint loc, bool isMouseInContainer)
        {
            bool clear = !isMouseInContainer;
            if (isMouseInContainer)
            {
                _mouseDownInControl = true;
                _isDoubleClickSelect = (DateTime.Now - _lastMouseDown).TotalMilliseconds < 400;
                _lastMouseDown = DateTime.Now;
                _mouseDownOnSelectedWord = false;

                if (_root.HtmlContainer.IsSelectionEnabled && parent.LeftMouseButton)
                {
                    var word = DomUtils.GetCssBoxWord(_root, loc);
                    if (word != null && word.Selected)
                    {
                        _mouseDownOnSelectedWord = true;
                    }
                    else
                    {
                        clear = true;
                    }
                }
                else if (parent.RightMouseButton)
                {
                    var rect = DomUtils.GetCssBoxWord(_root, loc);
                    var link = DomUtils.GetLinkBox(_root, loc);
                    if (_root.HtmlContainer.IsContextMenuEnabled)
                    {
                        _contextMenuHandler.ShowContextMenu(parent, rect, link);
                    }
                    clear = rect == null || !rect.Selected;
                }
            }

            if (clear)
            {
                ClearSelection();
                parent.Invalidate();
            }
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="leftMouseButton">is the left mouse button has been released</param>
        /// <returns>is the mouse up should be ignored</returns>
        public bool HandleMouseUp(RControl parent, bool leftMouseButton)
        {
            bool ignore = false;
            _mouseDownInControl = false;
            if (_root.HtmlContainer.IsSelectionEnabled)
            {
                ignore = _inSelection;
                if (!_inSelection && leftMouseButton && _mouseDownOnSelectedWord)
                {
                    ClearSelection();
                    parent.Invalidate();
                }

                _mouseDownOnSelectedWord = false;
                _inSelection = false;
            }
            ignore = ignore || (DateTime.Now - _lastMouseDown > TimeSpan.FromSeconds(1));
            return ignore;
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="loc">the location of the mouse on the html</param>
        public void HandleMouseMove(RControl parent, RPoint loc)
        {
            if (_root.HtmlContainer.IsSelectionEnabled && _mouseDownInControl && parent.LeftMouseButton)
            {
                if (_mouseDownOnSelectedWord)
                {
                    // make sure not to start drag-drop on click but when it actually moves as it fucks mouse-up
                    if ((DateTime.Now - _lastMouseDown).TotalMilliseconds > 200)
                        StartDragDrop(parent);
                }
                else
                {
                    HandleSelection(parent, loc, !_isDoubleClickSelect);
                    _inSelection = _selectionStart != null && _selectionEnd != null && (_selectionStart != _selectionEnd || _selectionStartIndex != _selectionEndIndex);
                }
            }
            else
            {
                // Handle mouse hover over the html to change the cursor depending if hovering word, link of other.
                var link = DomUtils.GetLinkBox(_root, loc);
                if (link != null)
                {
                    _cursorChanged = true;
                    parent.SetCursorHand();
                }
                else if (_root.HtmlContainer.IsSelectionEnabled)
                {
                    var word = DomUtils.GetCssBoxWord(_root, loc);
                    _cursorChanged = word != null && !word.IsImage && !(word.Selected && (word.SelectedStartIndex < 0 || word.Left + word.SelectedStartOffset <= loc.X) && (word.SelectedEndOffset < 0 || word.Left + word.SelectedEndOffset >= loc.X));
                    if (_cursorChanged)
                        parent.SetCursorIBeam();
                    else
                        parent.SetCursorDefault();
                }
                else if (_cursorChanged)
                {
                    parent.SetCursorDefault();
                }
            }
        }

        /// <summary>
        /// On mouse leave change the cursor back to default.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        public void HandleMouseLeave(RControl parent)
        {
            if (_cursorChanged)
            {
                _cursorChanged = false;
                parent.SetCursorDefault();
            }
        }

        /// <summary>
        /// Copy the currently selected html segment to clipboard.<br/>
        /// Copy rich html text and plain text.
        /// </summary>
        public void CopySelectedHtml()
        {
            if (_root.HtmlContainer.IsSelectionEnabled)
            {
                var html = DomUtils.GenerateHtml(_root, HtmlGenerationStyle.Inline, true);
                var plainText = DomUtils.GetSelectedPlainText(_root);
                if (!string.IsNullOrEmpty(plainText))
                    _root.HtmlContainer.Adapter.SetToClipboard(html, plainText);
            }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.<br/>
        /// </summary>
        public string GetSelectedText()
        {
            return _root.HtmlContainer.IsSelectionEnabled ? DomUtils.GetSelectedPlainText(_root) : null;
        }

        /// <summary>
        /// Copy the currently selected html segment with style.<br/>
        /// </summary>
        public string GetSelectedHtml()
        {
            return _root.HtmlContainer.IsSelectionEnabled ? DomUtils.GenerateHtml(_root, HtmlGenerationStyle.Inline, true) : null;
        }

        /// <summary>
        /// The selection start index if the first selected word is partially selected (-1 if not selected or fully selected)<br/>
        /// if the given word is not starting or ending selection word -1 is returned as full word selection is in place.
        /// </summary>
        /// <remarks>
        /// Handles backward selecting by returning the selection end data instead of start.
        /// </remarks>
        /// <param name="word">the word to return the selection start index for</param>
        /// <returns>data value or -1 if not applicable</returns>
        public int GetSelectingStartIndex(CssRect word)
        {
            return word == (_backwardSelection ? _selectionEnd : _selectionStart) ? (_backwardSelection ? _selectionEndIndex : _selectionStartIndex) : -1;
        }

        /// <summary>
        /// The selection end index if the last selected word is partially selected (-1 if not selected or fully selected)<br/>
        /// if the given word is not starting or ending selection word -1 is returned as full word selection is in place.
        /// </summary>
        /// <remarks>
        /// Handles backward selecting by returning the selection end data instead of start.
        /// </remarks>
        /// <param name="word">the word to return the selection end index for</param>
        public int GetSelectedEndIndexOffset(CssRect word)
        {
            return word == (_backwardSelection ? _selectionStart : _selectionEnd) ? (_backwardSelection ? _selectionStartIndex : _selectionEndIndex) : -1;
        }

        /// <summary>
        /// The selection start offset if the first selected word is partially selected (-1 if not selected or fully selected)<br/>
        /// if the given word is not starting or ending selection word -1 is returned as full word selection is in place.
        /// </summary>
        /// <remarks>
        /// Handles backward selecting by returning the selection end data instead of start.
        /// </remarks>
        /// <param name="word">the word to return the selection start offset for</param>
        public double GetSelectedStartOffset(CssRect word)
        {
            return word == (_backwardSelection ? _selectionEnd : _selectionStart) ? (_backwardSelection ? _selectionEndOffset : _selectionStartOffset) : -1;
        }

        /// <summary>
        /// The selection end offset if the last selected word is partially selected (-1 if not selected or fully selected)<br/>
        /// if the given word is not starting or ending selection word -1 is returned as full word selection is in place.
        /// </summary>
        /// <remarks>
        /// Handles backward selecting by returning the selection end data instead of start.
        /// </remarks>
        /// <param name="word">the word to return the selection end offset for</param>
        public double GetSelectedEndOffset(CssRect word)
        {
            return word == (_backwardSelection ? _selectionStart : _selectionEnd) ? (_backwardSelection ? _selectionStartOffset : _selectionEndOffset) : -1;
        }

        /// <summary>
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            // clear drag and drop
            _dragDropData = null;

            ClearSelection(_root);

            _selectionStartOffset = -1;
            _selectionStartIndex = -1;
            _selectionEndOffset = -1;
            _selectionEndIndex = -1;

            _selectionStartPoint = RPoint.Empty;
            _selectionStart = null;
            _selectionEnd = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _contextMenuHandler.Dispose();
        }


        #region Private methods

        /// <summary>
        /// Handle html text selection by mouse move over the html with left mouse button pressed.<br/>
        /// Calculate the words in the selected range and set their selected property.
        /// </summary>
        /// <param name="control">the control hosting the html to invalidate</param>
        /// <param name="loc">the mouse location</param>
        /// <param name="allowPartialSelect">true - partial word selection allowed, false - only full words selection</param>
        private void HandleSelection(RControl control, RPoint loc, bool allowPartialSelect)
        {
            // get the line under the mouse or nearest from the top
            var lineBox = DomUtils.GetCssLineBox(_root, loc);
            if (lineBox != null)
            {
                // get the word under the mouse
                var word = DomUtils.GetCssBoxWord(lineBox, loc);

                // if no word found under the mouse use the last or the first word in the line
                if (word == null && lineBox.Words.Count > 0)
                {
                    if (loc.Y > lineBox.LineBottom)
                    {
                        // under the line
                        word = lineBox.Words[lineBox.Words.Count - 1];
                    }
                    else if (loc.X < lineBox.Words[0].Left)
                    {
                        // before the line
                        word = lineBox.Words[0];
                    }
                    else if (loc.X > lineBox.Words[lineBox.Words.Count - 1].Right)
                    {
                        // at the end of the line
                        word = lineBox.Words[lineBox.Words.Count - 1];
                    }
                }

                // if there is matching word
                if (word != null)
                {
                    if (_selectionStart == null)
                    {
                        // on start set the selection start word
                        _selectionStartPoint = loc;
                        _selectionStart = word;
                        if (allowPartialSelect)
                            CalculateWordCharIndexAndOffset(control, word, loc, true);
                    }

                    // always set selection end word
                    _selectionEnd = word;
                    if (allowPartialSelect)
                        CalculateWordCharIndexAndOffset(control, word, loc, false);

                    ClearSelection(_root);
                    if (CheckNonEmptySelection(loc, allowPartialSelect))
                    {
                        CheckSelectionDirection();
                        SelectWordsInRange(_root, _backwardSelection ? _selectionEnd : _selectionStart, _backwardSelection ? _selectionStart : _selectionEnd);
                    }
                    else
                    {
                        _selectionEnd = null;
                    }

                    _cursorChanged = true;
                    control.SetCursorIBeam();
                    control.Invalidate();
                }
            }
        }

        /// <summary>
        /// Clear the selection from all the words in the css box recursively.
        /// </summary>
        /// <param name="box">the css box to selectionStart clear at</param>
        private static void ClearSelection(CssBox box)
        {
            foreach (var word in box.Words)
            {
                word.Selection = null;
            }
            foreach (var childBox in box.Boxes)
            {
                ClearSelection(childBox);
            }
        }

        /// <summary>
        /// Start drag & drop operation on the currently selected html segment.
        /// </summary>
        /// <param name="control">the control to start the drag & drop on</param>
        private void StartDragDrop(RControl control)
        {
            if (_dragDropData == null)
            {
                var html = DomUtils.GenerateHtml(_root, HtmlGenerationStyle.Inline, true);
                var plainText = DomUtils.GetSelectedPlainText(_root);
                _dragDropData = control.Adapter.GetClipboardDataObject(html, plainText);
            }
            control.DoDragDropCopy(_dragDropData);
        }

        /// <summary>
        /// Select all the words that are under <paramref name="box"/> DOM hierarchy.<br/>
        /// </summary>
        /// <param name="box">the box to start select all at</param>
        public void SelectAllWords(CssBox box)
        {
            foreach (var word in box.Words)
            {
                word.Selection = this;
            }

            foreach (var childBox in box.Boxes)
            {
                SelectAllWords(childBox);
            }
        }

        /// <summary>
        /// Check if the current selection is non empty, has some selection data.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="allowPartialSelect">true - partial word selection allowed, false - only full words selection</param>
        /// <returns>true - is non empty selection, false - empty selection</returns>
        private bool CheckNonEmptySelection(RPoint loc, bool allowPartialSelect)
        {
            // full word selection is never empty
            if (!allowPartialSelect)
                return true;

            // if end selection location is near starting location then the selection is empty
            if (Math.Abs(_selectionStartPoint.X - loc.X) <= 1 && Math.Abs(_selectionStartPoint.Y - loc.Y) < 5)
                return false;

            // selection is empty if on same word and same index
            return _selectionStart != _selectionEnd || _selectionStartIndex != _selectionEndIndex;
        }

        /// <summary>
        /// Select all the words that are between <paramref name="selectionStart"/> word and <paramref name="selectionEnd"/> word in the DOM hierarchy.<br/>
        /// </summary>
        /// <param name="root">the root of the DOM sub-tree the selection is in</param>
        /// <param name="selectionStart">selection start word limit</param>
        /// <param name="selectionEnd">selection end word limit</param>
        private void SelectWordsInRange(CssBox root, CssRect selectionStart, CssRect selectionEnd)
        {
            bool inSelection = false;
            SelectWordsInRange(root, selectionStart, selectionEnd, ref inSelection);
        }

        /// <summary>
        /// Select all the words that are between <paramref name="selectionStart"/> word and <paramref name="selectionEnd"/> word in the DOM hierarchy.
        /// </summary>
        /// <param name="box">the current traversal node</param>
        /// <param name="selectionStart">selection start word limit</param>
        /// <param name="selectionEnd">selection end word limit</param>
        /// <param name="inSelection">used to know the traversal is currently in selected range</param>
        /// <returns></returns>
        private bool SelectWordsInRange(CssBox box, CssRect selectionStart, CssRect selectionEnd, ref bool inSelection)
        {
            foreach (var boxWord in box.Words)
            {
                if (!inSelection && boxWord == selectionStart)
                {
                    inSelection = true;
                }
                if (inSelection)
                {
                    boxWord.Selection = this;

                    if (selectionStart == selectionEnd || boxWord == selectionEnd)
                    {
                        return true;
                    }
                }
            }

            foreach (var childBox in box.Boxes)
            {
                if (SelectWordsInRange(childBox, selectionStart, selectionEnd, ref inSelection))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculate the character index and offset by characters for the given word and given offset.<br/>
        /// <seealso cref="CalculateWordCharIndexAndOffset(RControl,HtmlRenderer.Core.Dom.CssRect,RPoint,bool)"/>.
        /// </summary>
        /// <param name="control">used to create graphics to measure string</param>
        /// <param name="word">the word to calculate its index and offset</param>
        /// <param name="loc">the location to calculate for</param>
        /// <param name="selectionStart">to set the starting or ending char and offset data</param>
        private void CalculateWordCharIndexAndOffset(RControl control, CssRect word, RPoint loc, bool selectionStart)
        {
            int selectionIndex;
            double selectionOffset;
            CalculateWordCharIndexAndOffset(control, word, loc, selectionStart, out selectionIndex, out selectionOffset);

            if (selectionStart)
            {
                _selectionStartIndex = selectionIndex;
                _selectionStartOffset = selectionOffset;
            }
            else
            {
                _selectionEndIndex = selectionIndex;
                _selectionEndOffset = selectionOffset;
            }
        }

        /// <summary>
        /// Calculate the character index and offset by characters for the given word and given offset.<br/>
        /// If the location is below the word line then set the selection to the end.<br/>
        /// If the location is to the right of the word then set the selection to the end.<br/>
        /// If the offset is to the left of the word set the selection to the beginning.<br/>
        /// Otherwise calculate the width of each substring to find the char the location is on.
        /// </summary>
        /// <param name="control">used to create graphics to measure string</param>
        /// <param name="word">the word to calculate its index and offset</param>
        /// <param name="loc">the location to calculate for</param>
        /// <param name="inclusive">is to include the first character in the calculation</param>
        /// <param name="selectionIndex">return the index of the char under the location</param>
        /// <param name="selectionOffset">return the offset of the char under the location</param>
        private static void CalculateWordCharIndexAndOffset(RControl control, CssRect word, RPoint loc, bool inclusive, out int selectionIndex, out double selectionOffset)
        {
            selectionIndex = 0;
            selectionOffset = 0f;
            var offset = loc.X - word.Left;
            if (word.Text == null)
            {
                // not a text word - set full selection
                selectionIndex = -1;
                selectionOffset = -1;
            }
            else if (offset > word.Width - word.OwnerBox.ActualWordSpacing || loc.Y > DomUtils.GetCssLineBoxByWord(word).LineBottom)
            {
                // mouse under the line, to the right of the word - set to the end of the word
                selectionIndex = word.Text.Length;
                selectionOffset = word.Width;
            }
            else if (offset > 0)
            {
                // calculate partial word selection
                int charFit;
                double charFitWidth;
                var maxWidth = offset + (inclusive ? 0 : 1.5f * word.LeftGlyphPadding);
                control.MeasureString(word.Text, word.OwnerBox.ActualFont, maxWidth, out charFit, out charFitWidth);

                selectionIndex = charFit;
                selectionOffset = charFitWidth;
            }
        }

        /// <summary>
        /// Check if the selection direction is forward or backward.<br/>
        /// Is the selection start word is before the selection end word in DFS traversal.
        /// </summary>
        private void CheckSelectionDirection()
        {
            if (_selectionStart == _selectionEnd)
            {
                _backwardSelection = _selectionStartIndex > _selectionEndIndex;
            }
            else if (DomUtils.GetCssLineBoxByWord(_selectionStart) == DomUtils.GetCssLineBoxByWord(_selectionEnd))
            {
                _backwardSelection = _selectionStart.Left > _selectionEnd.Left;
            }
            else
            {
                _backwardSelection = _selectionStart.Top >= _selectionEnd.Bottom;
            }
        }

        #endregion
    }
}