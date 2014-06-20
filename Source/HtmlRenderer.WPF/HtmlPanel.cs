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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Utils;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
    /// If the layout of the html resulted in its content beyond the client bounds of the panel it will show scrollbars (horizontal/vertical) allowing to scroll the content.<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// <para>
    /// The major differential to use HtmlPanel or HtmlLabel is size and scrollbars.<br/>
    /// If the size of the control depends on the html content the HtmlLabel should be used.<br/>
    /// If the size is set by some kind of layout then HtmlPanel is more suitable, also shows scrollbars if the html contents is larger than the control client rectangle.<br/>
    /// </para>
    /// <para>
    /// <h4>LinkClicked event:</h4>
    /// Raised when the user clicks on a link in the html.<br/>
    /// Allows canceling the execution of the link.
    /// </para>
    /// <para>
    /// <h4>StylesheetLoad event:</h4>
    /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
    /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// <para>
    /// <h4>ImageLoad event:</h4>
    /// Raised when an image is about to be loaded by file path or URI.<br/>
    /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
    /// </para>
    /// <para>
    /// <h4>RenderError event:</h4>
    /// Raised when an error occurred during html rendering.<br/>
    /// </para>
    /// </summary>
    public class HtmlPanel : Control
    {
        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected readonly HtmlContainer _htmlContainer;

        /// <summary>
        /// the vertical scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected ScrollBar _verticalScrollBar;

        /// <summary>
        /// the horizontal scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected ScrollBar _horizontalScrollBar;

        /// <summary>
        /// the raw base stylesheet data used in the control
        /// </summary>
        protected string _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the control
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// the current html text set in the control
        /// </summary>
        protected string _html;

        #endregion


        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel()
        {
            Background = SystemColors.WindowBrush;
            // VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            // HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            _verticalScrollBar = new ScrollBar();
            _verticalScrollBar.Orientation = Orientation.Vertical;
            _verticalScrollBar.Width = 18;
            _verticalScrollBar.Scroll += VerticalScrollBarOnScroll;
            AddVisualChild(_verticalScrollBar);
            AddLogicalChild(_verticalScrollBar);

            _horizontalScrollBar = new ScrollBar();
            _horizontalScrollBar.Visibility = Visibility.Hidden;
            //            _horizontalScrollBar.Orientation = Orientation.Horizontal;
            //            _horizontalScrollBar.Height = 18;
            //            AddVisualChild(_horizontalScrollBar);
            //            AddLogicalChild(_horizontalScrollBar);

            _htmlContainer = new HtmlContainer();
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.ScrollChange += OnScrollChange;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).<br/>
        /// True - images are loaded as soon as the html is parsed.<br/>
        /// False - images that are not visible because of scroll location are not loaded until they are scrolled to.
        /// </summary>
        /// <remarks>
        /// Images late loading improve performance if the page contains image outside the visible scroll area, especially if there is large 
        /// amount of images, as all image loading is delayed (downloading and loading into memory).<br/>
        /// Late image loading may effect the layout and actual size as image without set size will not have actual size until they are loaded
        /// resulting in layout change during user scroll.<br/>
        /// Early image loading may also effect the layout if image without known size above the current scroll location are loaded as they
        /// will push the html elements down.
        /// </remarks>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If image loading only when visible should be avoided")]
        public virtual bool AvoidImagesLateLoading
        {
            get { return _htmlContainer.AvoidImagesLateLoading; }
            set { _htmlContainer.AvoidImagesLateLoading = value; }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Is content selection is enabled for the rendered html.")]
        public virtual bool IsSelectionEnabled
        {
            get { return _htmlContainer.IsSelectionEnabled; }
            set { _htmlContainer.IsSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Is the build-in context menu enabled and will be shown on mouse right click.")]
        public virtual bool IsContextMenuEnabled
        {
            get { return _htmlContainer.IsContextMenuEnabled; }
            set { _htmlContainer.IsContextMenuEnabled = value; }
        }

        /// <summary>
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set base stylesheet to be used by html rendered in the control.")]
        public virtual string BaseStylesheet
        {
            get { return _baseRawCssData; }
            set
            {
                _baseRawCssData = value;
                _baseCssData = HtmlRender.ParseStyleSheet(value);
            }
        }

        /// <summary>
        /// Gets or sets the text of this panel
        /// </summary>
        [Browsable(true)]
        [Description("Sets the html of this control.")]
        public virtual string Html
        {
            get { return _html; }
            set
            {
                _html = value;
                _verticalScrollBar.Value = 0;
                _horizontalScrollBar.Value = 0;
                _htmlContainer.ScrollOffset = new Point(0, 0);
                _htmlContainer.SetHtml(_html, _baseCssData);
                InvalidateMeasure();
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedText
        {
            get { return _htmlContainer.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        [Browsable(false)]
        public virtual string SelectedHtml
        {
            get { return _htmlContainer.SelectedHtml; }
        }

        /// <summary>
        /// Get html from the current DOM tree with inline style.
        /// </summary>
        /// <returns>generated html</returns>
        public virtual string GetHtml()
        {
            return _htmlContainer != null ? _htmlContainer.GetHtml() : null;
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public virtual Rect? GetElementRectangle(string elementId)
        {
            return _htmlContainer != null ? _htmlContainer.GetElementRectangle(elementId) : null;
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

        private void VerticalScrollBarOnScroll(object sender, ScrollEventArgs e)
        {
            _htmlContainer.ScrollOffset = new Point(_htmlContainer.ScrollOffset.X, -e.NewValue);
            InvalidateVisual();
        }

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnRender(DrawingContext context)
        {
            if (Background != null && Background.Opacity > 0)
                context.DrawRectangle(Background, null, new Rect(RenderSize));

            var htmlWidth = HtmlWidth(RenderSize);
            var htmlHeight = HtmlHeight(RenderSize);
            if (_htmlContainer != null && htmlWidth > 0 && htmlHeight > 0)
            {
                context.PushClip(new RectangleGeometry(new Rect(new Size(htmlWidth, htmlHeight))));
                _htmlContainer.PerformPaint(context, new Rect(new Size(htmlWidth, htmlHeight)));
                context.Pop();

                // TODO:a handle if we need to refresh the pointer here
                // call mouse move to handle paint after scroll or html change affecting mouse cursor.
                //                var mp = PointToClient(MousePosition);
                //                _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
            }
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection. 
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseMove(this, e);
        }

        /// <summary>
        /// Handle mouse leave to handle cursor change.
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle mouse down to handle selection. 
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDown(this, e);
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click. 
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseUp(this, e);
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse. 
        /// </summary>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDoubleClick(this, e);
        }

        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleKeyDown(this, e);

            if (e.Key == Key.Home)
            {
                //                ScrollToTop();
            }
            else if (e.Key == Key.End)
            {
                //                ScrollToBottom();
            }
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e)
        {
            var handler = LinkClicked;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        protected virtual void OnRenderError(HtmlRenderErrorEventArgs e)
        {
            var handler = RenderError;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        protected virtual void OnStylesheetLoad(HtmlStylesheetLoadEventArgs e)
        {
            var handler = StylesheetLoad;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        protected virtual void OnImageLoad(HtmlImageLoadEventArgs e)
        {
            var handler = ImageLoad;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        protected virtual void OnRefresh(HtmlRefreshEventArgs e)
        {
            if (e.Layout)
                InvalidateMeasure();
            else
                InvalidateVisual();
        }

        private void ScrollToPoint(double x, double y)
        {
            //            ScrollToHorizontalOffset(x);
            //            ScrollToVerticalOffset(y);
        }

        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected void Dispose(bool disposing)
        {
            // TODO:a handle dispose
            //            if (_htmlContainer != null)
            //            {
            //                _htmlContainer.LinkClicked -= OnLinkClicked;
            //                _htmlContainer.RenderError -= OnRenderError;
            //                _htmlContainer.Refresh -= OnRefresh;
            //                _htmlContainer.ScrollChange -= OnScrollChange;
            //                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
            //                _htmlContainer.ImageLoad -= OnImageLoad;
            //                _htmlContainer.Dispose();
            //                _htmlContainer = null;
            //            }
            //            base.Dispose(disposing);
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        private double HtmlWidth(Size size)
        {
            return size.Width - (_verticalScrollBar.Visibility == Visibility.Visible ? _verticalScrollBar.Width : 0);
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        private double HtmlHeight(Size size)
        {
            return size.Height - (_horizontalScrollBar.Visibility == Visibility.Visible ? _horizontalScrollBar.Height : 0);
        }

        #region Private event handlers

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            OnLinkClicked(e);
        }

        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            if (CheckAccess())
                OnRenderError(e);
            else
                Dispatcher.Invoke(new Action<HtmlRenderErrorEventArgs>(OnRenderError), e);
        }

        private void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            OnStylesheetLoad(e);
        }

        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            OnImageLoad(e);
        }

        private void OnRefresh(object sender, HtmlRefreshEventArgs e)
        {
            if (CheckAccess())
                OnRefresh(e);
            else
                Dispatcher.Invoke(new Action<HtmlRefreshEventArgs>(OnRefresh), e);
        }

        private void OnScrollChange(object sender, HtmlScrollEventArgs e)
        {
            ScrollToPoint(e.X, e.Y);
        }

        #endregion


        #endregion
    }
}