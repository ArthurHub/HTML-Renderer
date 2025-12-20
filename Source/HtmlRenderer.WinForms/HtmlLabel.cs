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
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WinForms.Adapters;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WinForms control that will render html content in it's client rectangle.<br/>
    /// Using <see cref="AutoSize"/> and <see cref="AutoSizeHeightOnly"/> client can control how the html content effects the
    /// size of the label. Either case scrollbars are never shown and html content outside of client bounds will be clipped.
    /// <see cref="MaximumSize"/> and <see cref="MinimumSize"/> with AutoSize can limit the max/min size of the control<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// <para>
    /// The major differential to use HtmlPanel or HtmlLabel is size and scrollbars.<br/>
    /// If the size of the control depends on the html content the HtmlLabel should be used.<br/>
    /// If the size is set by some kind of layout then HtmlPanel is more suitable, also shows scrollbars if the html contents is larger than the control client rectangle.<br/>
    /// </para>
    /// <para>
    /// <h4>AutoSize:</h4>
    /// <u>AutoSize = AutoSizeHeightOnly = false</u><br/>
    /// The label size will not change by the html content. MaximumSize and MinimumSize are ignored.<br/>
    /// <br/>
    /// <u>AutoSize = true</u><br/>
    /// The width and height is adjustable by the html content, the width will be longest line in the html, MaximumSize.Width will restrict it but it can be lower than that.<br/>
    /// <br/>
    /// <u>AutoSizeHeightOnly = true</u><br/>
    /// The width of the label is set and will not change by the content, the height is adjustable by the html content with restrictions to the MaximumSize.Height and MinimumSize.Height values.<br/>
    /// </para>
    /// <para>
    /// <h4>LinkClicked event</h4>
    /// Raised when the user clicks on a link in the html.<br/>
    /// Allows canceling the execution of the link.
    /// </para>
    /// <para>
    /// <h4>StylesheetLoad event:</h4>
    /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
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
    public class HtmlLabel : Control
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
        protected string _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the panel
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// the current html text set in the control
        /// </summary>
        protected string _text;

        /// <summary>
        /// is to handle auto size of the control height only
        /// </summary>
        protected bool _autoSizeHight;

        /// <summary>
        /// If to use cursors defined by the operating system or .NET cursors
        /// </summary>
        protected bool _useSystemCursors;

        /// <summary>
        /// The text rendering hint to be used for text rendering.
        /// </summary>
        protected TextRenderingHint _textRenderingHint = TextRenderingHint.SystemDefault;

        #endregion


        /// <summary>
        /// Creates a new HTML Label
        /// </summary>
        public HtmlLabel()
        {
            SuspendLayout();

            AutoSize = true;
            BackColor = SystemColors.Window;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);

            _htmlContainer = new HtmlContainer();
            _htmlContainer.AvoidImagesLateLoading = true;
            _htmlContainer.MaxSize = MaximumSize;
            _htmlContainer.LoadComplete += OnLoadComplete;
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;

            ResumeLayout(false);
        }

        /// <summary>
        ///   Raised when the BorderStyle property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler BorderStyleChanged;

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
        /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
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
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(false)]
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
        [Description("Set base stylesheet to be used by html rendered in the control.")]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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
        /// Automatically sets the size of the label by content size
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Automatically sets the size of the label by content size.")]
        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set
            {
                base.AutoSize = value;
                if (value)
                {
                    _autoSizeHight = false;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Automatically sets the height of the label by content height (width is not effected).
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category("Layout")]
        [Description("Automatically sets the height of the label by content height (width is not effected)")]
        public virtual bool AutoSizeHeightOnly
        {
            get { return _autoSizeHight; }
            set
            {
                _autoSizeHight = value;
                if (value)
                {
                    AutoSize = false;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the max size the control get be set by <see cref="AutoSize"/> or <see cref="AutoSizeHeightOnly"/>.
        /// </summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.</returns>
        [Description("If AutoSize or AutoSizeHeightOnly is set this will restrict the max size of the control (0 is not restricted)")]
        public override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set
            {
                base.MaximumSize = value;
                if (_htmlContainer != null)
                {
                    _htmlContainer.MaxSize = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the min size the control get be set by <see cref="AutoSize"/> or <see cref="AutoSizeHeightOnly"/>.
        /// </summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.</returns>
        [Description("If AutoSize or AutoSizeHeightOnly is set this will restrict the min size of the control (0 is not restricted)")]
        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = value; }
        }

        /// <summary>
        /// Gets or sets the html of this control.
        /// </summary>
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
                    _htmlContainer.SetHtml(_text, _baseCssData);
                    PerformLayout();
                    Invalidate();
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
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_htmlContainer != null)
                _htmlContainer.ClearSelection();
        }


        #region Private methods

#if !MONO
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
#endif

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (_htmlContainer != null)
            {
                Graphics g = CreateGraphics();
                if (g != null)
                {
                    using (g)
                    using (var ig = new GraphicsAdapter(g, _htmlContainer.UseGdiPlusTextRendering))
                    {
                        var newSize = HtmlRendererUtils.Layout(ig,
                            _htmlContainer.HtmlContainerInt,
                            new RSize(ClientSize.Width - Padding.Horizontal, ClientSize.Height - Padding.Vertical),
                            new RSize(MinimumSize.Width - Padding.Horizontal, MinimumSize.Height - Padding.Vertical),
                            new RSize(MaximumSize.Width - Padding.Horizontal, MaximumSize.Height - Padding.Vertical),
                            AutoSize,
                            AutoSizeHeightOnly);
                        ClientSize = Utils.ConvertRound(new RSize(newSize.Width + Padding.Horizontal, newSize.Height + Padding.Vertical));
                    }
                }
            }
            base.OnLayout(levent);
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

                _htmlContainer.Location = new PointF(Padding.Left, Padding.Top);
                _htmlContainer.PerformPaint(e.Graphics);
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
        /// Handle mouse down to handle selection. 
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDown(this, e);
        }

        /// <summary>
        /// Handle mouse leave to handle cursor change.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click. 
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseUp(this, e);
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse. 
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseDoubleClick(this, e);
        }

        /// <summary>
        ///   Raises the <see cref="BorderStyleChanged" /> event.
        /// </summary>
        protected virtual void OnBorderStyleChanged(EventArgs e)
        {
            UpdateStyles();

            var handler = BorderStyleChanged;
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

#if !MONO
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
                    Win32Utils.SetCursor(Win32Utils.LoadCursor(0, Win32Utils.IdcHand));
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
#endif

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