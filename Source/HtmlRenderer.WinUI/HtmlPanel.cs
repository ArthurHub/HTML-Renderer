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
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using Windows.Foundation;
using Windows.System;

namespace TheArtOfDev.HtmlRenderer.WinUI
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WinUI 3 control that will render html content in it's client rectangle.<br/>
    /// If the layout of the html resulted in its content beyond the client bounds of the panel it will show scrollbars (horizontal/vertical) allowing to scroll the content.<br/>
    /// The control will handle pointer and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// </summary>
    /// <remarks>
    /// See <see cref="HtmlControl"/> for more info.
    /// </remarks>
    public class HtmlPanel : HtmlControl
    {
        #region Fields and Consts

        /// <summary>
        /// the vertical scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected readonly ScrollBar _verticalScrollBar;

        /// <summary>
        /// the horizontal scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected readonly ScrollBar _horizontalScrollBar;

        #endregion

        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel()
        {
            Background = new SolidColorBrush(Colors.White);

            _verticalScrollBar = new ScrollBar
            {
                Orientation = Orientation.Vertical,
                // A standalone ScrollBar shows nothing with the default IndicatorMode of None
                IndicatorMode = ScrollingIndicatorMode.MouseIndicator,
                Width = 18,
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            _verticalScrollBar.Scroll += OnScrollBarScroll;
            _rootPanel.Children.Add(_verticalScrollBar);

            _horizontalScrollBar = new ScrollBar
            {
                Orientation = Orientation.Horizontal,
                IndicatorMode = ScrollingIndicatorMode.MouseIndicator,
                Height = 18,
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            _horizontalScrollBar.Scroll += OnScrollBarScroll;
            _rootPanel.Children.Add(_horizontalScrollBar);

            _htmlContainer.ScrollChange += OnScrollChange;

            RegisterPropertyChangedCallback(TextProperty, (d, dp) =>
            {
                _horizontalScrollBar.Value = _verticalScrollBar.Value = 0;
            });
        }

        /// <summary>
        /// Adjust the scrollbar of the panel on html element by the given id.<br/>
        /// The top of the html element rectangle will be at the top of the panel, if there
        /// is not enough height to scroll to the top the scroll will be at maximum.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to scroll to</param>
        public virtual void ScrollToElement(string elementId)
        {
            ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

            if (_htmlContainer != null)
            {
                var rect = _htmlContainer.GetElementRectangle(elementId);
                if (rect.HasValue)
                {
                    ScrollToPoint(rect.Value.X, rect.Value.Y);
                    InvokeMouseMove();
                }
            }
        }

        #region Protected methods

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            var size = PerformHtmlLayout(availableSize);

            // to handle if scrollbar is appearing or disappearing
            bool relayout = false;
            var htmlHeight = HtmlHeight(availableSize);
            var verticalNeeded = size.Height > htmlHeight;
            if ((_verticalScrollBar.Visibility == Visibility.Collapsed && verticalNeeded) ||
                (_verticalScrollBar.Visibility == Visibility.Visible && !verticalNeeded))
            {
                _verticalScrollBar.Visibility = verticalNeeded ? Visibility.Visible : Visibility.Collapsed;
                relayout = true;
            }

            var htmlWidth = HtmlWidth(availableSize);
            var horizontalNeeded = size.Width > htmlWidth;
            if ((_horizontalScrollBar.Visibility == Visibility.Collapsed && horizontalNeeded) ||
                (_horizontalScrollBar.Visibility == Visibility.Visible && !horizontalNeeded))
            {
                _horizontalScrollBar.Visibility = horizontalNeeded ? Visibility.Visible : Visibility.Collapsed;
                relayout = true;
            }

            if (relayout)
                size = PerformHtmlLayout(availableSize);

            UpdateScrollBarRanges(availableSize);

            base.MeasureOverride(availableSize);

            if (double.IsPositiveInfinity(availableSize.Width) || double.IsPositiveInfinity(availableSize.Height))
                return size;

            return availableSize;
        }

        /// <summary>
        /// After measurement update the scrollbar ranges to match the arranged size.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateScrollBarRanges(finalSize);
            UpdateScrollOffsets();
            return result;
        }

        /// <summary>
        /// Perform html container layout by the current panel client size.
        /// </summary>
        protected Size PerformHtmlLayout(Size constraint)
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.MaxSize = new Size(HtmlWidth(constraint), 0);
                _htmlContainer.PerformLayout();
                return _htmlContainer.ActualSize;
            }
            return new Size(0, 0);
        }

        /// <summary>
        /// Handle pointer press to set focus on the control.
        /// </summary>
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            Focus(FocusState.Pointer);
            base.OnPointerPressed(e);

            // Otherwise the root ScrollViewer takes the focus
            e.Handled = true;
        }

        /// <summary>
        /// Handle mouse wheel for scrolling.
        /// </summary>
        protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (_verticalScrollBar.Visibility == Visibility.Visible)
            {
                var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
                _verticalScrollBar.Value -= delta;
                UpdateScrollOffsets();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);

            if (_verticalScrollBar.Visibility == Visibility.Visible)
            {
                if (e.Key == VirtualKey.Up)
                {
                    _verticalScrollBar.Value -= _verticalScrollBar.SmallChange;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.Down)
                {
                    _verticalScrollBar.Value += _verticalScrollBar.SmallChange;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.PageUp)
                {
                    _verticalScrollBar.Value -= _verticalScrollBar.LargeChange;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.PageDown)
                {
                    _verticalScrollBar.Value += _verticalScrollBar.LargeChange;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.Home)
                {
                    _verticalScrollBar.Value = 0;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.End)
                {
                    _verticalScrollBar.Value = _verticalScrollBar.Maximum;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
            }

            if (_horizontalScrollBar.Visibility == Visibility.Visible)
            {
                if (e.Key == VirtualKey.Left)
                {
                    _horizontalScrollBar.Value -= _horizontalScrollBar.SmallChange;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.Right)
                {
                    _horizontalScrollBar.Value += _horizontalScrollBar.SmallChange;
                    UpdateScrollOffsets();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected override double HtmlWidth(Size size)
        {
            var width = base.HtmlWidth(size) - (_verticalScrollBar.Visibility == Visibility.Visible ? _verticalScrollBar.Width : 0);
            return width > 1 ? width : 1;
        }

        /// <summary>
        /// Get the height the HTML has to render in (not including horizontal scroll iff it is visible)
        /// </summary>
        protected override double HtmlHeight(Size size)
        {
            var height = base.HtmlHeight(size) - (_horizontalScrollBar.Visibility == Visibility.Visible ? _horizontalScrollBar.Height : 0);
            return height > 1 ? height : 1;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// On HTML container scroll change request scroll to the requested location.
        /// </summary>
        private void OnScrollChange(object sender, Core.Entities.HtmlScrollEventArgs e)
        {
            ScrollToPoint(e.X, e.Y);
        }

        /// <summary>
        /// Set the control scroll offset to the given values.
        /// </summary>
        private void ScrollToPoint(double x, double y)
        {
            _horizontalScrollBar.Value = x;
            _verticalScrollBar.Value = y;
            UpdateScrollOffsets();
        }

        /// <summary>
        /// On scrollbar scroll update the scroll offsets and invalidate.
        /// </summary>
        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            UpdateScrollOffsets();
        }

        /// <summary>
        /// Update the scroll range/viewport of the scrollbars to match the given bounds.
        /// </summary>
        private void UpdateScrollBarRanges(Size bounds)
        {
            if (_htmlContainer == null)
                return;

            if (_verticalScrollBar.Visibility == Visibility.Visible)
            {
                _verticalScrollBar.ViewportSize = HtmlHeight(bounds);
                _verticalScrollBar.SmallChange = 25;
                _verticalScrollBar.LargeChange = _verticalScrollBar.ViewportSize * .9;
                _verticalScrollBar.Maximum = Math.Max(0, _htmlContainer.ActualSize.Height - _verticalScrollBar.ViewportSize);
            }

            if (_horizontalScrollBar.Visibility == Visibility.Visible)
            {
                _horizontalScrollBar.ViewportSize = HtmlWidth(bounds);
                _horizontalScrollBar.SmallChange = 25;
                _horizontalScrollBar.LargeChange = _horizontalScrollBar.ViewportSize * .9;
                _horizontalScrollBar.Maximum = Math.Max(0, _htmlContainer.ActualSize.Width - _horizontalScrollBar.ViewportSize);
            }
        }

        /// <summary>
        /// Update the scroll offset of the HTML container and invalidate visual to re-render.
        /// </summary>
        private void UpdateScrollOffsets()
        {
            var newScrollOffset = new Point(-_horizontalScrollBar.Value, -_verticalScrollBar.Value);
            if (!newScrollOffset.Equals(_htmlContainer.ScrollOffset))
            {
                _htmlContainer.ScrollOffset = newScrollOffset;
                InvalidateCanvas();
            }
        }

        #endregion
    }
}
