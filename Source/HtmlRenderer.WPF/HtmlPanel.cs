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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Utils;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
    /// If the layout of the html resulted in its content beyond the client bounds of the panel it will show scrollbars (horizontal/vertical) allowing to scroll the content.<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// </summary>
    /// <remarks>
    /// See <see cref="HtmlControlBase"/> for more info.
    /// </remarks>
    public class HtmlPanel : HtmlControlBase
    {
        #region Fields and Consts

        /// <summary>
        /// the vertical scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected ScrollBar _verticalScrollBar;

        /// <summary>
        /// the horizontal scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected ScrollBar _horizontalScrollBar;

        #endregion


        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel()
        {
            // VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            // HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            _verticalScrollBar = new ScrollBar();
            _verticalScrollBar.Orientation = Orientation.Vertical;
            _verticalScrollBar.Width = 18;
            _verticalScrollBar.Scroll += OnVerticalScrollBarScroll;
            AddVisualChild(_verticalScrollBar);
            AddLogicalChild(_verticalScrollBar);

            _horizontalScrollBar = new ScrollBar();
            _horizontalScrollBar.Visibility = Visibility.Hidden;
            //            _horizontalScrollBar.Orientation = Orientation.Horizontal;
            //            _horizontalScrollBar.Height = 18;
            //            AddVisualChild(_horizontalScrollBar);
            //            AddLogicalChild(_horizontalScrollBar);

            _htmlContainer.ScrollChange += OnScrollChange;
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
                    ScrollToPoint(rect.Value.Location.X, rect.Value.Location.Y);
                    _htmlContainer.HandleMouseMove(this, new MouseEventArgs(Mouse.PrimaryDevice, 0));
                }
            }
        }


        #region Private methods

        protected override int VisualChildrenCount
        {
            get { return _verticalScrollBar != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
                return _verticalScrollBar;
            return null;
        }

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = PerformHtmlLayout(constraint);

            // to handle if scrollbar is appearing or disappearing
            if ((_verticalScrollBar.Visibility == Visibility.Hidden && size.Height > constraint.Height) ||
                (_verticalScrollBar.Visibility == Visibility.Visible && size.Height < constraint.Height))
            {
                _verticalScrollBar.Visibility = _verticalScrollBar.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                PerformHtmlLayout(constraint);
            }

            return constraint;
        }

        /// <summary>
        /// After measurement arrange the scrollbars of the panel.
        /// </summary>
        protected override Size ArrangeOverride(Size bounds)
        {
            _verticalScrollBar.Arrange(new Rect(bounds.Width - _verticalScrollBar.Width, 0, _verticalScrollBar.Width, bounds.Height));

            if (_htmlContainer != null && _verticalScrollBar.Visibility == Visibility.Visible)
            {
                _verticalScrollBar.ViewportSize = HtmlHeight(bounds);
                _verticalScrollBar.SmallChange = 25;
                _verticalScrollBar.LargeChange = _verticalScrollBar.ViewportSize * .9;
                _verticalScrollBar.Maximum = _htmlContainer.ActualSize.Height - _verticalScrollBar.ViewportSize;
            }

            return bounds;
        }

        /// <summary>
        /// Perform html container layout by the current panel client size.
        /// </summary>
        protected Size PerformHtmlLayout(Size constraint)
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.MaxSize = new Size(HtmlWidth(constraint), 0);

                DrawingGroup dGroup = new DrawingGroup();
                using (var g = dGroup.Open())
                {
                    _htmlContainer.PerformLayout(g);
                }

                return _htmlContainer.ActualSize;
            }
            return Size.Empty;
        }

        private void OnVerticalScrollBarScroll(object sender, ScrollEventArgs e)
        {
            UpdateScrollOffsets();
        }

        private void UpdateScrollOffsets()
        {
            _htmlContainer.ScrollOffset = new Point(-_horizontalScrollBar.Value, -_verticalScrollBar.Value);
            InvalidateVisual();
        }

        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Key == Key.Home)
            {
                //                ScrollToTop();
            }
            else if (e.Key == Key.End)
            {
                //                ScrollToBottom();
            }
        }

        private void ScrollToPoint(double x, double y)
        {
            _horizontalScrollBar.Value = x;
            _verticalScrollBar.Value = y;
            UpdateScrollOffsets();
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected override double HtmlWidth(Size size)
        {
            return size.Width - (_verticalScrollBar.Visibility == Visibility.Visible ? _verticalScrollBar.Width : 0);
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected override double HtmlHeight(Size size)
        {
            return size.Height - (_horizontalScrollBar.Visibility == Visibility.Visible ? _horizontalScrollBar.Height : 0);
        }

        private void OnScrollChange(object sender, HtmlScrollEventArgs e)
        {
            ScrollToPoint(e.X, e.Y);
        }

        #endregion
    }
}