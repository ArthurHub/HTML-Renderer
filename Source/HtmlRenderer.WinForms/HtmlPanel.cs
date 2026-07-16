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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms
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
    /// <para>
    /// <h4>Zoom and pan:</h4>
    /// Ctrl+MouseWheel zooms the content with anchor-to-cursor behavior (also Ctrl+/− and Ctrl+0).<br/>
    /// Middle-mouse button drag pans the canvas via scroll position.<br/>
    /// See <see cref="Zoom"/> for programmatic control.
    /// </para>
    /// </summary>
    public class HtmlPanel : ScrollableControl
    {
        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected HtmlContainer _htmlContainer;

        /// <summary>
        /// The current border style of the control
        /// </summary>
        protected BorderStyle _borderStyle;

        /// <summary>
        /// the raw base stylesheet data used in the control
        /// </summary>
        protected string? _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the control
        /// </summary>
        protected CssData? _baseCssData;

        /// <summary>
        /// the current html text set in the control
        /// </summary>
        protected string? _text;

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        protected bool _useSystemCursors;

        /// <summary>
        /// The text rendering hint to be used for text rendering.
        /// </summary>
        protected TextRenderingHint _textRenderingHint = TextRenderingHint.SystemDefault;

        /// <summary>
        /// The last position of the scrollbars to know if it has changed to update mouse
        /// </summary>
        protected Point _lastScrollOffset;

        /// <summary>
        /// The zoom factor of the rendered HTML (1.0 = 100%).
        /// </summary>
        protected float _zoom = 1f;

        /// <summary>
        /// True while middle-mouse button pan is active.
        /// </summary>
        private bool _isPanning;

        /// <summary>
        /// Last client mouse position during middle-mouse pan.
        /// </summary>
        private Point _panLastPoint;

        /// <summary>
        /// Cursor to restore when middle-mouse pan ends.
        /// </summary>
        private Cursor? _cursorBeforePan;

        /// <summary>
        /// Minimum allowed zoom factor.
        /// </summary>
        private const float MinZoom = 0.25f;

        /// <summary>
        /// Maximum allowed zoom factor.
        /// </summary>
        private const float MaxZoom = 5f;

        /// <summary>
        /// Zoom change per Ctrl+wheel / Ctrl+/- step.
        /// </summary>
        private const float ZoomStep = 0.1f;

        #endregion


        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel()
        {
            AutoScroll = true;
            BackColor = SystemColors.Window;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            _htmlContainer = new HtmlContainer();
            _htmlContainer.LoadComplete += OnLoadComplete;
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
        /// Raised when the <see cref="Zoom"/> property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler ZoomChanged;

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event EventHandler LoadComplete;

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
        /// Use GDI+ text rendering to measure/draw text.<br/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// GDI+ text rendering is less smooth than GDI text rendering but it natively supports alpha channel
        /// thus allows creating transparent images.
        /// </para>
        /// <para>
        /// While using GDI+ text rendering you can control the text rendering using <see cref="Graphics.TextRenderingHint"/>, note that
        /// using <see cref="System.Drawing.Text.TextRenderingHint.ClearTypeGridFit"/> doesn't work well with transparent background.
        /// </para>
        /// </remarks>
        [Category("Behavior")]
        [DefaultValue(false)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Description("If to use GDI+ text rendering to measure/draw text, false - use GDI")]
        public bool UseGdiPlusTextRendering
        {
            get { return _htmlContainer.UseGdiPlusTextRendering; }
            set { _htmlContainer.UseGdiPlusTextRendering = value; }
        }

        /// <summary>
        /// The text rendering hint to be used for text rendering.
        /// </summary>
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(TextRenderingHint.SystemDefault)]
        [Description("The text rendering hint to be used for text rendering.")]
        public TextRenderingHint TextRenderingHint
        {
            get { return _textRenderingHint; }
            set { _textRenderingHint = value; }
        }

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        [Category("Behavior")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
        [Description("If to use cursors defined by the operating system or .NET cursors")]
        public bool UseSystemCursors
        {
            get { return _useSystemCursors; }
            set { _useSystemCursors = value; }
        }

        /// <summary>
        /// Gets or sets the zoom factor of the rendered HTML (1.0 = 100%).<br/>
        /// Values are clamped to the range 0.25–5.0. Setting this property anchors zoom to the viewport center.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(1f)]
        [Description("The zoom factor of the rendered HTML (1.0 = 100%).")]
        public virtual float Zoom
        {
            get { return _zoom; }
            set { SetZoom(value, null); }
        }

        /// <summary>
        /// Gets or sets the border style.
        /// </summary>
        /// <value>The border style.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(BorderStyle), "None")]
        public virtual BorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set
            {
                if (BorderStyle != value)
                {
                    _borderStyle = value;
                    OnBorderStyleChanged(EventArgs.Empty);
                }
            }
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Windows.Forms.Design, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Drawing.Design.UITypeEditor, System.Windows.Forms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
        public virtual string BaseStylesheet
        {
            get { return _baseRawCssData; }
            set
            {
                _baseRawCssData = value;
                _baseCssData = HtmlRender.ParseStyleSheet(value);
                _htmlContainer.SetHtml(_text, _baseCssData);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container enables the user to scroll to any controls placed outside of its visible boundaries. 
        /// </summary>
        [Browsable(true)]
        [Description("Sets a value indicating whether the container enables the user to scroll to any controls placed outside of its visible boundaries.")]
        public override bool AutoScroll
        {
            get { return base.AutoScroll; }
            set { base.AutoScroll = value; }
        }

        /// <summary>
        /// Gets or sets the text of this panel
        /// </summary>
        [Browsable(true)]
        [Description("Sets the html of this control.")]
        public override string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                base.Text = value;
                if (!IsDisposed)
                {
                    VerticalScroll.Value = VerticalScroll.Minimum;
                    _htmlContainer.SetHtml(_text, _baseCssData);
                    PerformLayout();
                    Invalidate();
                    InvokeMouseMove();
                }
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
        public virtual RectangleF? GetElementRectangle(string elementId)
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
                    var location = rect.Value.Location;
                    UpdateScroll(Point.Round(new PointF(location.X * _zoom, location.Y * _zoom)));
                    InvokeMouseMove();
                }
            }
        }

        /// <summary>
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_htmlContainer != null)
                _htmlContainer.ClearSelection();
        }

        /// <summary>
        /// Set the zoom factor, optionally anchoring the scroll so the document point under
        /// <paramref name="anchorClientPoint"/> stays fixed on screen (browser-style zoom).
        /// </summary>
        /// <param name="zoom">desired zoom factor (clamped to 0.25–5.0)</param>
        /// <param name="anchorClientPoint">client-space anchor; null uses the viewport center</param>
        public virtual void SetZoom(float zoom, Point? anchorClientPoint)
        {
            float z1 = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));
            if (Math.Abs(z1 - _zoom) < 0.0001f)
                return;

            float z0 = _zoom;
            Point anchor = anchorClientPoint ?? new Point(ClientSize.Width / 2, ClientSize.Height / 2);

            // AutoScrollPosition is negative when scrolled; convert anchor to logical document coords
            double docX = (anchor.X - AutoScrollPosition.X) / z0;
            double docY = (anchor.Y - AutoScrollPosition.Y) / z0;

            _zoom = z1;
            PerformLayout();

            // Keep the same document point under the anchor after zoom
            int scrollX = (int)Math.Round(docX * z1 - anchor.X);
            int scrollY = (int)Math.Round(docY * z1 - anchor.Y);
            scrollX = ClampScroll(scrollX, AutoScrollMinSize.Width, ClientSize.Width);
            scrollY = ClampScroll(scrollY, AutoScrollMinSize.Height, ClientSize.Height);
            AutoScrollPosition = new Point(scrollX, scrollY);
            SyncHtmlScrollOffset();
            Invalidate();
            OnZoomChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Increase zoom by one step, anchored at the given client point (or viewport center).
        /// </summary>
        public void ZoomIn(Point? anchorClientPoint = null)
        {
            SetZoom(_zoom + ZoomStep, anchorClientPoint);
        }

        /// <summary>
        /// Decrease zoom by one step, anchored at the given client point (or viewport center).
        /// </summary>
        public void ZoomOut(Point? anchorClientPoint = null)
        {
            SetZoom(_zoom - ZoomStep, anchorClientPoint);
        }

        #region Private methods

        /// <summary>
        /// Override to support border for the control.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;

                switch (_borderStyle)
                {
                    case BorderStyle.FixedSingle:
                        createParams.Style |= Win32Utils.WsBorder;
                        break;

                    case BorderStyle.Fixed3D:
                        createParams.ExStyle |= Win32Utils.WsExClientEdge;
                        break;
                }

                return createParams;
            }
        }

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            PerformHtmlLayout();

            base.OnLayout(levent);

            // to handle if vertical scrollbar is appearing or disappearing
            float layoutWidth = (ClientSize.Width - Padding.Horizontal) / _zoom;
            if (_htmlContainer != null && Math.Abs(_htmlContainer.MaxSize.Width - layoutWidth) > 0.1)
            {
                PerformHtmlLayout();
                base.OnLayout(levent);
            }
        }

        /// <summary>
        /// Perform html container layout by the current panel client size and zoom.
        /// </summary>
        protected void PerformHtmlLayout()
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.MaxSize = new SizeF((ClientSize.Width - Padding.Horizontal) / _zoom, 0);

                using (var g = CreateGraphics())
                {
                    _htmlContainer.PerformLayout(g);
                }

                AutoScrollMinSize = Size.Round(new SizeF(
                    (_htmlContainer.ActualSize.Width + Padding.Horizontal) * _zoom,
                    _htmlContainer.ActualSize.Height * _zoom));
            }
        }

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_htmlContainer != null)
            {
                e.Graphics.TextRenderingHint = _textRenderingHint;
                e.Graphics.SetClip(ClientRectangle);
                e.Graphics.ScaleTransform(_zoom, _zoom);
                _htmlContainer.Location = new PointF(Padding.Left / _zoom, Padding.Top / _zoom);
                SyncHtmlScrollOffset();
                _htmlContainer.PerformPaint(e.Graphics);

                if (!_lastScrollOffset.Equals(_htmlContainer.ScrollOffset))
                {
                    _lastScrollOffset = _htmlContainer.ScrollOffset;
                    InvokeMouseMove();
                }
            }
        }

        /// <summary>
        /// Set focus on the control for keyboard scrollbars handling.
        /// </summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
        }

        /// <summary>
        /// Handle mouse move for middle-mouse pan, hover cursor and text selection.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isPanning)
            {
                int dx = e.X - _panLastPoint.X;
                int dy = e.Y - _panLastPoint.Y;
                int scrollX = -AutoScrollPosition.X - dx;
                int scrollY = -AutoScrollPosition.Y - dy;
                scrollX = ClampScroll(scrollX, AutoScrollMinSize.Width, ClientSize.Width);
                scrollY = ClampScroll(scrollY, AutoScrollMinSize.Height, ClientSize.Height);
                AutoScrollPosition = new Point(scrollX, scrollY);
                SyncHtmlScrollOffset();
                _panLastPoint = e.Location;
                Invalidate();
                return;
            }

            base.OnMouseMove(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseMove(this, CreateHtmlMouseEventArgs(e));
        }

        /// <summary>
        /// Handle mouse leave to handle cursor change.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_isPanning && _htmlContainer != null)
                _htmlContainer.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle mouse down for middle-mouse pan and selection.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                StartPan(e.Location);
                return;
            }

            base.OnMouseDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDown(this, CreateHtmlMouseEventArgs(e));
        }

        /// <summary>
        /// Handle mouse up for middle-mouse pan, selection and link click.
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_isPanning && e.Button == MouseButtons.Middle)
            {
                EndPan();
                return;
            }

            base.OnMouseUp(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseUp(this, CreateHtmlMouseEventArgs(e));
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse.
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDoubleClick(this, CreateHtmlMouseEventArgs(e));
        }

        /// <summary>
        /// Ctrl+MouseWheel zooms (anchor-to-cursor); otherwise normal scroll.
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                    ZoomIn(e.Location);
                else if (e.Delta < 0)
                    ZoomOut(e.Location);

                if (e is HandledMouseEventArgs handled)
                    handled.Handled = true;
                return;
            }

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// End middle-mouse pan if mouse capture is lost.
        /// </summary>
        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (_isPanning && !Capture)
                EndPan();
        }

        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleKeyDown(this, e);
            if (e.KeyCode == Keys.Up)
            {
                VerticalScroll.Value = Math.Max(VerticalScroll.Value - 70, VerticalScroll.Minimum);
                PerformLayout();
            }
            else if (e.KeyCode == Keys.Down)
            {
                VerticalScroll.Value = Math.Min(VerticalScroll.Value + 70, VerticalScroll.Maximum);
                PerformLayout();
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                VerticalScroll.Value = Math.Min(VerticalScroll.Value + 400, VerticalScroll.Maximum);
                PerformLayout();
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                VerticalScroll.Value = Math.Max(VerticalScroll.Value - 400, VerticalScroll.Minimum);
                PerformLayout();
            }
            else if (e.KeyCode == Keys.End)
            {
                VerticalScroll.Value = VerticalScroll.Maximum;
                PerformLayout();
            }
            else if (e.KeyCode == Keys.Home)
            {
                VerticalScroll.Value = VerticalScroll.Minimum;
                PerformLayout();
            }
        }

        /// <summary>
        /// Handle Ctrl+/-, Ctrl+=, and Ctrl+0 zoom shortcuts.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.Control) == Keys.Control)
            {
                Keys key = keyData & ~(Keys.Control | Keys.Shift);
                Point anchor = GetZoomAnchorFromCursor();
                if (key == Keys.Oemplus || key == Keys.Add)
                {
                    ZoomIn(anchor);
                    return true;
                }
                if (key == Keys.OemMinus || key == Keys.Subtract)
                {
                    ZoomOut(anchor);
                    return true;
                }
                if (key == Keys.D0 || key == Keys.NumPad0)
                {
                    SetZoom(1f, anchor);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        ///   Raises the <see cref="BorderStyleChanged" /> event.
        /// </summary>
        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            UpdateStyles();

            var handler = BorderStyleChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ZoomChanged"/> event.
        /// </summary>
        protected virtual void OnZoomChanged(EventArgs e)
        {
            var handler = ZoomChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Propagate the LoadComplete event from root container.
        /// </summary>
        protected virtual void OnLoadComplete(EventArgs e)
        {
            var handler = LoadComplete;
            if (handler != null)
                handler(this, e);
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
                PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// On html renderer scroll request adjust the scrolling of the panel to the requested location.
        /// </summary>
        protected virtual void OnScrollChange(HtmlScrollEventArgs e)
        {
            UpdateScroll(new Point((int)e.X, (int)e.Y));
        }

        /// <summary>
        /// Adjust the scrolling of the panel to the requested location (client / zoomed space).
        /// </summary>
        /// <param name="location">the location to adjust the scroll to</param>
        protected virtual void UpdateScroll(Point location)
        {
            AutoScrollPosition = location;
            SyncHtmlScrollOffset();
        }

        /// <summary>
        /// call mouse move to handle paint after scroll or html change affecting mouse cursor.
        /// </summary>
        protected virtual void InvokeMouseMove()
        {
            if (_isPanning || _htmlContainer == null)
                return;

            try
            {
                var mp = PointToHtml(PointToClient(MousePosition));
                _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Convert client-space point to HTML logical coordinates (accounts for zoom).
        /// </summary>
        private Point PointToHtml(Point clientPoint)
        {
            if (Math.Abs(_zoom - 1f) < 0.0001f)
                return clientPoint;
            return new Point(
                (int)Math.Round(clientPoint.X / _zoom),
                (int)Math.Round(clientPoint.Y / _zoom));
        }

        /// <summary>
        /// Create mouse args with location converted to HTML logical coordinates.
        /// </summary>
        private MouseEventArgs CreateHtmlMouseEventArgs(MouseEventArgs e)
        {
            var p = PointToHtml(e.Location);
            return new MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta);
        }

        /// <summary>
        /// Sync html container scroll offset from AutoScrollPosition in logical units.
        /// </summary>
        private void SyncHtmlScrollOffset()
        {
            if (_htmlContainer == null)
                return;
            var sp = AutoScrollPosition;
            _htmlContainer.ScrollOffset = new Point(
                (int)Math.Round(sp.X / _zoom),
                (int)Math.Round(sp.Y / _zoom));
        }

        /// <summary>
        /// Clamp a positive scroll offset to the valid range for the current content size.
        /// </summary>
        private static int ClampScroll(int offset, int contentSize, int viewportSize)
        {
            int max = Math.Max(0, contentSize - viewportSize);
            if (offset < 0)
                return 0;
            if (offset > max)
                return max;
            return offset;
        }

        /// <summary>
        /// Start middle-mouse pan at the given client point.
        /// </summary>
        private void StartPan(Point clientPoint)
        {
            _isPanning = true;
            _panLastPoint = clientPoint;
            _cursorBeforePan = Cursor;
            Cursor = Cursors.SizeAll;
            Capture = true;
            Focus();
        }

        /// <summary>
        /// End middle-mouse pan and restore cursor/capture.
        /// </summary>
        private void EndPan()
        {
            if (!_isPanning)
                return;
            _isPanning = false;
            Capture = false;
            Cursor = _cursorBeforePan ?? Cursors.Default;
            InvokeMouseMove();
        }

        /// <summary>
        /// Client point under the cursor if it is over this control; otherwise viewport center.
        /// </summary>
        private Point GetZoomAnchorFromCursor()
        {
            var client = PointToClient(MousePosition);
            if (ClientRectangle.Contains(client))
                return client;
            return new Point(ClientSize.Width / 2, ClientSize.Height / 2);
        }

        /// <summary>
        /// Used to add arrow keys to the handled keys in <see cref="OnKeyDown"/>.
        /// </summary>
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        /// <summary>
        /// Override the proc processing method to set OS specific hand cursor.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process. </param>
        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            if (_useSystemCursors && m.Msg == Win32Utils.WmSetCursor && Cursor == Cursors.Hand)
            {
                try
                {
                    // Replace .NET's hand cursor with the OS cursor
                    Win32Utils.SetCursor(Win32Utils.LoadCursor(IntPtr.Zero, Win32Utils.IdcHand));
                    m.Result = IntPtr.Zero;
                    return;
                }
                catch (Exception ex)
                {
                    OnRenderError(this, new HtmlRenderErrorEventArgs(HtmlRenderErrorType.General, "Failed to set OS hand cursor", ex));
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Release the html container resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.LoadComplete -= OnLoadComplete;
                _htmlContainer.LinkClicked -= OnLinkClicked;
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.Refresh -= OnRefresh;
                _htmlContainer.ScrollChange -= OnScrollChange;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }
            base.Dispose(disposing);
        }


        #region Private event handlers

        private void OnLoadComplete(object sender, EventArgs e)
        {
            OnLoadComplete(e);
        }

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            OnLinkClicked(e);
        }

        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => OnRenderError(e)));
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
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => OnRefresh(e)));
            else
                OnRefresh(e);
        }

        private void OnScrollChange(object sender, HtmlScrollEventArgs e)
        {
            OnScrollChange(e);
        }

        #endregion


        #region Hide not relevant properties from designer

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Font Font
        {
            get { return base.Font; }
            set { base.Font = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override bool AllowDrop
        {
            get { return base.AllowDrop; }
            set { base.AllowDrop = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override RightToLeft RightToLeft
        {
            get { return base.RightToLeft; }
            set { base.RightToLeft = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        public override Cursor Cursor
        {
            get { return base.Cursor; }
            set { base.Cursor = value; }
        }

        /// <summary>
        /// Not applicable.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseWaitCursor
        {
            get { return base.UseWaitCursor; }
            set { base.UseWaitCursor = value; }
        }

        #endregion


        #endregion
    }
}