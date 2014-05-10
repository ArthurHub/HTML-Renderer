// // "Therefore those skilled at the unorthodox
// // are infinite as heaven and earth,
// // inexhaustible as the great rivers.
// // When they come to an end,
// // they begin again,
// // like the days and months;
// // they die and are reborn,
// // like the four seasons."
// // 
// // - Sun Tsu,
// // "The Art of War"

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Utils;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WinForms control that will render html content in it's client rectangle.<br/>
    /// If <see cref="AutoScroll"/> is true and the layout of the html resulted in its content beyond the client bounds 
    /// of the panel it will show scrollbars (horizontal/vertical) allowing to scroll the content.<br/>
    /// If <see cref="AutoScroll"/> is false html content outside the client bounds will be clipped.<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// <para>
    /// The major differential to use HtmlPanel or HtmlLabel is size and scrollbars.<br/>
    /// If the size of the control depends on the html content the HtmlLabel should be used.<br/>
    /// If the size is set by some kind of layout then HtmlPanel is more suitable, also shows scrollbars if the html contents is larger than the control client rectangle.<br/>
    /// </para>
    /// <para>
    /// <h4>AutoScroll:</h4>
    /// Allows showing scrollbars if html content is placed outside the visible boundaries of the panel.
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
    public class HtmlPanel : ScrollViewer
    {
        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected HtmlContainer _htmlContainer;

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
            //AutoScroll = true;
            //BackColor = SystemColors.Window;

            _htmlContainer = new HtmlContainer();
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.ScrollChange += OnScrollChange;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;
        }

        /// <summary>
        ///   Raised when the BorderStyle property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler BorderStyleChanged;

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
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("If anti-aliasing should be avoided for geometry like backgrounds and borders")]
        public virtual bool AvoidGeometryAntialias
        {
            get { return _htmlContainer.AvoidGeometryAntialias; }
            set { _htmlContainer.AvoidGeometryAntialias = value; }
        }

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
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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
                //ScrollInfo.SetVerticalOffset(0);
                _htmlContainer.SetHtml(_html, _baseCssData);
                InvalidateArrange();
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
                    UpdateScroll(rect.Value.Location);
                    _htmlContainer.HandleMouseMove(this, new MouseEventArgs(Mouse.PrimaryDevice, 0));
                }
            }
        }


        #region Private methods

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            PerformHtmlLayout();

            base.OnRenderSizeChanged(sizeInfo);

            // to handle if vertical scrollbar is appearing or disappearing
            if (_htmlContainer != null && Math.Abs(_htmlContainer.MaxSize.Width - Width) > 0.1)
            {
                PerformHtmlLayout();
                base.OnRenderSizeChanged(sizeInfo);
            }
        }

        /// <summary>
        /// Perform html container layout by the current panel client size.
        /// </summary>
        protected void PerformHtmlLayout()
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.MaxSize = new Size(Width, 0);

                DrawingGroup dGroup = new DrawingGroup();
                using (var g = dGroup.Open())
                {
                    _htmlContainer.PerformLayout(g);
                }

                // TODO:a handle scroll
                // AutoScrollMinSize = Size.Round(_htmlContainer.ActualSize);
            }
        }

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            if (_htmlContainer != null)
            {
                //_htmlContainer.ScrollOffset = new Point(ScrollInfo.HorizontalOffset, ScrollInfo.VerticalOffset);
                _htmlContainer.PerformPaint(context);

                // call mouse move to handle paint after scroll or html change affecting mouse cursor.
                //                var mp = PointToClient(MousePosition);
                //                _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
            }
        }
        /*
        /// <summary>
        /// Set focus on the control for keyboard scrollbars handling.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }
        */
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

            // TODO:a handle scroll
            //            if (e.Key == Key.Up)
            //            {
            //                VerticalScroll.Value = Math.Max(VerticalScroll.Value - 70, VerticalScroll.Minimum);
            //                PerformLayout();
            //            }
            //            else if (e.Key == Key.Down)
            //            {
            //                VerticalScroll.Value = Math.Min(VerticalScroll.Value + 70, VerticalScroll.Maximum);
            //                PerformLayout();
            //            }
            //            else if (e.Key == Key.PageDown)
            //            {
            //                VerticalScroll.Value = Math.Min(VerticalScroll.Value + 400, VerticalScroll.Maximum);
            //                PerformLayout();
            //            }
            //            else if (e.Key == Key.PageUp)
            //            {
            //                VerticalScroll.Value = Math.Max(VerticalScroll.Value - 400, VerticalScroll.Minimum);
            //                PerformLayout();
            //            }
            //            else if (e.Key == Key.End)
            //            {
            //                VerticalScroll.Value = VerticalScroll.Maximum;
            //                PerformLayout();
            //            }
            //            else if (e.Key == Key.Home)
            //            {
            //                VerticalScroll.Value = VerticalScroll.Minimum;
            //                PerformLayout();
            //            }
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
                InvalidateArrange();
            else
                InvalidateVisual();
        }

        /// <summary>
        /// On html renderer scroll request adjust the scrolling of the panel to the requested location.
        /// </summary>
        protected virtual void OnScrollChange(HtmlScrollEventArgs e)
        {
            UpdateScroll(new Point((int)e.X, (int)e.Y));
        }

        /// <summary>
        /// Adjust the scrolling of the panel to the requested location.
        /// </summary>
        /// <param name="location">the location to adjust the scroll to</param>
        protected virtual void UpdateScroll(Point location)
        {
            // TODO:a handle scroll
            //            AutoScrollPosition = location;
            //            _htmlContainer.ScrollOffset = AutoScrollPosition;
        }


        /*
        /// <summary>
        /// Used to add arrow keys to the handled keys in <see cref="OnKeyDown"/>.
        /// </summary>
        protected override bool IsInputKey(Key keyData)
        {
            switch (keyData)
            {
                case Key.Right:
                case Key.Left:
                case Key.Up:
                case Key.Down:
                    return true;
                case Key.Shift | Key.Right:
                case Key.Shift | Key.Left:
                case Key.Shift | Key.Up:
                case Key.Shift | Key.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
        */


        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected void Dispose(bool disposing)
        {
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


        #region Private event handlers

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            OnLinkClicked(e);
        }

        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            if (CheckAccess())
                Dispatcher.Invoke(new Action<HtmlRenderErrorEventArgs>(OnRenderError), e);
            else
                OnRenderError(e);
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
                Dispatcher.Invoke(new Action<HtmlRefreshEventArgs>(OnRefresh), e);
            else
                OnRefresh(e);
        }

        private void OnScrollChange(object sender, HtmlScrollEventArgs e)
        {
            OnScrollChange(e);
        }

        #endregion

        #endregion
    }
}