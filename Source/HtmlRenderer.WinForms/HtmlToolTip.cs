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
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms
{
    /// <summary>
    /// Provides HTML rendering on the tooltips.
    /// </summary>
    public class HtmlToolTip : ToolTip
    {
        #region Fields and Consts

        /// <summary>
        /// the container to render and handle the html shown in the tooltip
        /// </summary>
        protected HtmlContainer _htmlContainer;

        /// <summary>
        /// the raw base stylesheet data used in the control
        /// </summary>
        protected string _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the panel
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// The text rendering hint to be used for text rendering.
        /// </summary>
        protected TextRenderingHint _textRenderingHint = TextRenderingHint.SystemDefault;

        /// <summary>
        /// The CSS class used for tooltip html root div
        /// </summary>
        private string _tooltipCssClass = "htmltooltip";

#if !MONO
       
        /// <summary>
        /// the control that the tooltip is currently showing on.<br/>
        /// Used for link handling.
        /// </summary>
        private Control _associatedControl;

        /// <summary>
        /// timer used to handle mouse move events when mouse is over the tooltip.<br/>
        /// Used for link handling.
        /// </summary>
        private Timer _linkHandlingTimer;

        /// <summary>
        /// the handle of the actual tooltip window used to know when the tooltip is hidden<br/>
        /// Used for link handling.
        /// </summary>
        private IntPtr _tooltipHandle;

        /// <summary>
        /// If to handle links in the tooltip (default: false).<br/>
        /// When set to true the mouse pointer will change to hand when hovering over a tooltip and
        /// if clicked the <see cref="LinkClicked"/> event will be raised although the tooltip will be closed.
        /// </summary>
        private bool _allowLinksHandling = true;
#endif

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public HtmlToolTip()
        {
            OwnerDraw = true;

            _htmlContainer = new HtmlContainer();
            _htmlContainer.IsSelectionEnabled = false;
            _htmlContainer.IsContextMenuEnabled = false;
            _htmlContainer.AvoidGeometryAntialias = true;
            _htmlContainer.AvoidImagesLateLoading = true;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;

            Popup += OnToolTipPopup;
            Draw += OnToolTipDraw;
            Disposed += OnToolTipDisposed;

#if !MONO
            _linkHandlingTimer = new Timer();
            _linkHandlingTimer.Tick += OnLinkHandlingTimerTick;
            _linkHandlingTimer.Interval = 40;

            _htmlContainer.LinkClicked += OnLinkClicked;
#endif
        }

#if !MONO
        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;
#endif

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
        /// Use GDI+ text rendering to measure/draw text.<br/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// GDI+ text rendering is less smooth than GDI text rendering but it natively supports alpha channel
        /// thus allows creating transparent images.
        /// </para>
        /// <para>
        /// While using GDI+ text rendering you can control the text rendering using <see cref="Graphics.TextRenderingHint"/>, note that
        /// using <see cref="TextRenderingHint.ClearTypeGridFit"/> doesn't work well with transparent background.
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
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        [Browsable(true)]
        [Description("Set base stylesheet to be used by html rendered in the tooltip.")]
        [Category("Appearance")]
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
        /// The CSS class used for tooltip html root div (default: htmltooltip)<br/>
        /// Setting to 'null' clear base style on the tooltip.<br/>
        /// Set custom class found in <see cref="BaseStylesheet"/> to change the base style of the tooltip.
        /// </summary>
        [Browsable(true)]
        [Description("The CSS class used for tooltip html root div.")]
        [Category("Appearance")]
        public virtual string TooltipCssClass
        {
            get { return _tooltipCssClass; }
            set { _tooltipCssClass = value; }
        }

#if !MONO
        /// <summary>
        /// If to handle links in the tooltip (default: false).<br/>
        /// When set to true the mouse pointer will change to hand when hovering over a tooltip and
        /// if clicked the <see cref="LinkClicked"/> event will be raised although the tooltip will be closed.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Description("If to handle links in the tooltip.")]
        [Category("Behavior")]
        public virtual bool AllowLinksHandling
        {
            get { return _allowLinksHandling; }
            set { _allowLinksHandling = value; }
        }
#endif

        /// <summary>
        /// Gets or sets the max size the tooltip.
        /// </summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.</returns>
        [Browsable(true)]
        [Category("Layout")]
        [Description("Restrict the max size of the shown tooltip (0 is not restricted)")]
        public virtual Size MaximumSize
        {
            get { return Size.Round(_htmlContainer.MaxSize); }
            set { _htmlContainer.MaxSize = value; }
        }


        #region Private methods

        /// <summary>
        /// On tooltip appear set the html by the associated control, layout and set the tooltip size by the html size.
        /// </summary>
        protected virtual void OnToolTipPopup(PopupEventArgs e)
        {
            //Create fragment container
            var cssClass = string.IsNullOrEmpty(_tooltipCssClass) ? null : string.Format(" class=\"{0}\"", _tooltipCssClass);
            var toolipHtml = string.Format("<div{0}>{1}</div>", cssClass, GetToolTip(e.AssociatedControl));
            _htmlContainer.SetHtml(toolipHtml, _baseCssData);
            _htmlContainer.MaxSize = MaximumSize;

            //Measure size of the container
            using (var g = e.AssociatedControl.CreateGraphics())
            {
                g.TextRenderingHint = _textRenderingHint;
                _htmlContainer.PerformLayout(g);
            }

            //Set the size of the tooltip
            var desiredWidth = (int)Math.Ceiling(MaximumSize.Width > 0 ? Math.Min(_htmlContainer.ActualSize.Width, MaximumSize.Width) : _htmlContainer.ActualSize.Width);
            var desiredHeight = (int)Math.Ceiling(MaximumSize.Height > 0 ? Math.Min(_htmlContainer.ActualSize.Height, MaximumSize.Height) : _htmlContainer.ActualSize.Height);
            e.ToolTipSize = new Size(desiredWidth, desiredHeight);

#if !MONO
            // start mouse handle timer
            if (_allowLinksHandling)
            {
                _associatedControl = e.AssociatedControl;
                _linkHandlingTimer.Start();
            }
#endif
        }

        /// <summary>
        /// Draw the html using the tooltip graphics.
        /// </summary>
        protected virtual void OnToolTipDraw(DrawToolTipEventArgs e)
        {
#if !MONO
            if (_tooltipHandle == IntPtr.Zero)
            {
                // get the handle of the tooltip window using the graphics device context
                var hdc = e.Graphics.GetHdc();
                _tooltipHandle = Win32Utils.WindowFromDC(hdc);
                e.Graphics.ReleaseHdc(hdc);

                AdjustTooltipPosition(e.AssociatedControl, e.Bounds.Size);
            }
#endif

            e.Graphics.Clear(Color.White);
            e.Graphics.TextRenderingHint = _textRenderingHint;
            _htmlContainer.PerformPaint(e.Graphics);
        }

        /// <summary>
        /// Adjust the location of the tooltip window to the location of the mouse and handle
        /// if the tooltip window will try to appear outside the boundaries of the control.
        /// </summary>
        /// <param name="associatedControl">the control the tooltip is appearing on</param>
        /// <param name="size">the size of the tooltip window</param>
        protected virtual void AdjustTooltipPosition(Control associatedControl, Size size)
        {
            var mousePos = Control.MousePosition;
            var screenBounds = Screen.FromControl(associatedControl).WorkingArea;

            // adjust if tooltip is outside form bounds
            if (mousePos.X + size.Width > screenBounds.Right)
                mousePos.X = Math.Max(screenBounds.Right - size.Width - 5, screenBounds.Left + 3);

            const int yOffset = 20;
            if (mousePos.Y + size.Height + yOffset > screenBounds.Bottom)
                mousePos.Y = Math.Max(screenBounds.Bottom - size.Height - yOffset - 3, screenBounds.Top + 2);

#if !MONO
            // move the tooltip window to new location
            Win32Utils.MoveWindow(_tooltipHandle, mousePos.X, mousePos.Y + yOffset, size.Width, size.Height, false);
#endif
        }

#if !MONO
        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e)
        {
            var handler = LinkClicked;
            if (handler != null)
                handler(this, e);
        }
#endif

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

#if !MONO
        /// <summary>
        /// Raised on link handling timer tick, used for:
        /// 1. Know when the tooltip is hidden by checking the visibility of the tooltip window.
        /// 2. Call HandleMouseMove so the mouse cursor will react if over a link element.
        /// 3. Call HandleMouseDown and HandleMouseUp to simulate click on a link if one was clicked.
        /// </summary>
        protected virtual void OnLinkHandlingTimerTick(EventArgs e)
        {
            try
            {
                var handle = _tooltipHandle;
                if (handle != IntPtr.Zero && Win32Utils.IsWindowVisible(handle))
                {
                    var mPos = Control.MousePosition;
                    var mButtons = Control.MouseButtons;
                    var rect = Win32Utils.GetWindowRectangle(handle);
                    if (rect.Contains(mPos))
                    {
                        _htmlContainer.HandleMouseMove(_associatedControl, new MouseEventArgs(mButtons, 0, mPos.X - rect.X, mPos.Y - rect.Y, 0));
                    }
                }
                else
                {
                    _linkHandlingTimer.Stop();
                    _tooltipHandle = IntPtr.Zero;

                    var mPos = Control.MousePosition;
                    var mButtons = Control.MouseButtons;
                    var rect = Win32Utils.GetWindowRectangle(handle);
                    if (rect.Contains(mPos))
                    {
                        if (mButtons == MouseButtons.Left)
                        {
                            var args = new MouseEventArgs(mButtons, 1, mPos.X - rect.X, mPos.Y - rect.Y, 0);
                            _htmlContainer.HandleMouseDown(_associatedControl, args);
                            _htmlContainer.HandleMouseUp(_associatedControl, args);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnRenderError(this, new HtmlRenderErrorEventArgs(HtmlRenderErrorType.General, "Error in link handling for tooltip", ex));
            }
        }
#endif

        /// <summary>
        /// Unsubscribe from events and dispose of <see cref="_htmlContainer"/>.
        /// </summary>
        protected virtual void OnToolTipDisposed(EventArgs e)
        {
            Popup -= OnToolTipPopup;
            Draw -= OnToolTipDraw;
            Disposed -= OnToolTipDisposed;

            if (_htmlContainer != null)
            {
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }

#if !MONO
            if (_linkHandlingTimer != null)
            {
                _linkHandlingTimer.Dispose();
                _linkHandlingTimer = null;

                if (_htmlContainer != null)
                    _htmlContainer.LinkClicked -= OnLinkClicked;
            }
#endif
        }


        #region Private event handlers

        private void OnToolTipPopup(object sender, PopupEventArgs e)
        {
            OnToolTipPopup(e);
        }

        private void OnToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            OnToolTipDraw(e);
        }
        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
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

#if !MONO
        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            OnLinkClicked(e);
        }
        private void OnLinkHandlingTimerTick(object sender, EventArgs e)
        {
            OnLinkHandlingTimerTick(e);
        }
#endif

        private void OnToolTipDisposed(object sender, EventArgs e)
        {
            OnToolTipDisposed(e);
        }

        #endregion


        #endregion
    }
}