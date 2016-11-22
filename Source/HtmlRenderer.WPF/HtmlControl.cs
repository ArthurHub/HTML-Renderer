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
using System.Windows.Input;
using System.Windows.Media;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;

namespace TheArtOfDev.HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
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
    public class HtmlControl : Control
    {
        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected readonly HtmlContainer _htmlContainer;

        /// <summary>
        /// the base stylesheet data used in the control
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// The last position of the scrollbars to know if it has changed to update mouse
        /// </summary>
        protected Point _lastScrollOffset;

        #endregion


        #region Dependency properties / routed events

        public static readonly DependencyProperty AvoidImagesLateLoadingProperty = DependencyProperty.Register("AvoidImagesLateLoading", typeof(bool), typeof(HtmlControl), new PropertyMetadata(false, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register("IsSelectionEnabled", typeof(bool), typeof(HtmlControl), new PropertyMetadata(true, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty IsContextMenuEnabledProperty = DependencyProperty.Register("IsContextMenuEnabled", typeof(bool), typeof(HtmlControl), new PropertyMetadata(true, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty BaseStylesheetProperty = DependencyProperty.Register("BaseStylesheet", typeof(string), typeof(HtmlControl), new PropertyMetadata(null, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HtmlControl), new PropertyMetadata(null, OnDependencyProperty_valueChanged));

        public static readonly RoutedEvent LoadCompleteEvent = EventManager.RegisterRoutedEvent("LoadComplete", RoutingStrategy.Bubble, typeof(RoutedEventHandler<EventArgs>), typeof(HtmlControl));
        public static readonly RoutedEvent LinkClickedEvent = EventManager.RegisterRoutedEvent("LinkClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler<HtmlLinkClickedEventArgs>), typeof(HtmlControl));
        public static readonly RoutedEvent RenderErrorEvent = EventManager.RegisterRoutedEvent("RenderError", RoutingStrategy.Bubble, typeof(RoutedEventHandler<HtmlRenderErrorEventArgs>), typeof(HtmlControl));
        public static readonly RoutedEvent RefreshEvent = EventManager.RegisterRoutedEvent("Refresh", RoutingStrategy.Bubble, typeof(RoutedEventHandler<HtmlRefreshEventArgs>), typeof(HtmlControl));
        public static readonly RoutedEvent StylesheetLoadEvent = EventManager.RegisterRoutedEvent("StylesheetLoad", RoutingStrategy.Bubble, typeof(RoutedEventHandler<HtmlStylesheetLoadEventArgs>), typeof(HtmlControl));
        public static readonly RoutedEvent ImageLoadEvent = EventManager.RegisterRoutedEvent("ImageLoad", RoutingStrategy.Bubble, typeof(RoutedEventHandler<HtmlImageLoadEventArgs>), typeof(HtmlControl));

        #endregion


        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        protected HtmlControl()
        {
            // shitty WPF rendering, have no idea why this actually makes everything sharper =/
            SnapsToDevicePixels = false;

            _htmlContainer = new HtmlContainer();
            _htmlContainer.LoadComplete += OnLoadComplete;
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;
        }

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event RoutedEventHandler LoadComplete
        {
            add { AddHandler(LoadCompleteEvent, value); }
            remove { RemoveHandler(LoadCompleteEvent, value); }
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event RoutedEventHandler<HtmlLinkClickedEventArgs> LinkClicked
        {
            add { AddHandler(LinkClickedEvent, value); }
            remove { RemoveHandler(LinkClickedEvent, value); }
        }

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        public event RoutedEventHandler<HtmlRenderErrorEventArgs> RenderError
        {
            add { AddHandler(RenderErrorEvent, value); }
            remove { RemoveHandler(RenderErrorEvent, value); }
        }

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event RoutedEventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad
        {
            add { AddHandler(StylesheetLoadEvent, value); }
            remove { RemoveHandler(StylesheetLoadEvent, value); }
        }

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event RoutedEventHandler<HtmlImageLoadEventArgs> ImageLoad
        {
            add { AddHandler(ImageLoadEvent, value); }
            remove { RemoveHandler(ImageLoadEvent, value); }
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
        [Description("If image loading only when visible should be avoided")]
        public bool AvoidImagesLateLoading
        {
            get { return (bool)GetValue(AvoidImagesLateLoadingProperty); }
            set { SetValue(AvoidImagesLateLoadingProperty, value); }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        [Category("Behavior")]
        [Description("Is content selection is enabled for the rendered html.")]
        public bool IsSelectionEnabled
        {
            get { return (bool)GetValue(IsSelectionEnabledProperty); }
            set { SetValue(IsSelectionEnabledProperty, value); }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        [Category("Behavior")]
        [Description("Is the build-in context menu enabled and will be shown on mouse right click.")]
        public bool IsContextMenuEnabled
        {
            get { return (bool)GetValue(IsContextMenuEnabledProperty); }
            set { SetValue(IsContextMenuEnabledProperty, value); }
        }

        /// <summary>
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        [Category("Appearance")]
        [Description("Set base stylesheet to be used by html rendered in the control.")]
        public string BaseStylesheet
        {
            get { return (string)GetValue(BaseStylesheetProperty); }
            set { SetValue(BaseStylesheetProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text of this panel
        /// </summary>
        [Description("Sets the html of this control.")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
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
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_htmlContainer != null)
                _htmlContainer.ClearSelection();
        }


        #region Private methods

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnRender(DrawingContext context)
        {
            if (Background.Opacity > 0)
                context.DrawRectangle(Background, null, new Rect(RenderSize));

            if (BorderThickness != new Thickness(0))
            {
                var brush = BorderBrush ?? SystemColors.ControlDarkBrush;
                if (BorderThickness.Top > 0)
                    context.DrawRectangle(brush, null, new Rect(0, 0, RenderSize.Width, BorderThickness.Top));
                if (BorderThickness.Bottom > 0)
                    context.DrawRectangle(brush, null, new Rect(0, RenderSize.Height - BorderThickness.Bottom, RenderSize.Width, BorderThickness.Bottom));
                if (BorderThickness.Left > 0)
                    context.DrawRectangle(brush, null, new Rect(0, 0, BorderThickness.Left, RenderSize.Height));
                if (BorderThickness.Right > 0)
                    context.DrawRectangle(brush, null, new Rect(RenderSize.Width - BorderThickness.Right, 0, BorderThickness.Right, RenderSize.Height));
            }

            var htmlWidth = HtmlWidth(RenderSize);
            var htmlHeight = HtmlHeight(RenderSize);
            if (_htmlContainer != null && htmlWidth > 0 && htmlHeight > 0)
            {
                var windows = Window.GetWindow(this);
                if (windows != null)
                {
                    // adjust render location to round point so we won't get anti-alias smugness
                    var wPoint = TranslatePoint(new Point(0, 0), windows);
                    wPoint.Offset(-(int)wPoint.X, -(int)wPoint.Y);
                    var xTrans = wPoint.X < .5 ? -wPoint.X : 1 - wPoint.X;
                    var yTrans = wPoint.Y < .5 ? -wPoint.Y : 1 - wPoint.Y;
                    context.PushTransform(new TranslateTransform(xTrans, yTrans));
                }

                context.PushClip(new RectangleGeometry(new Rect(Padding.Left + BorderThickness.Left, Padding.Top + BorderThickness.Top, htmlWidth, (int)htmlHeight)));
                _htmlContainer.Location = new Point(Padding.Left + BorderThickness.Left, Padding.Top + BorderThickness.Top);
                _htmlContainer.PerformPaint(context, new Rect(Padding.Left + BorderThickness.Left, Padding.Top + BorderThickness.Top, htmlWidth, htmlHeight));
                context.Pop();

                if (!_lastScrollOffset.Equals(_htmlContainer.ScrollOffset))
                {
                    _lastScrollOffset = _htmlContainer.ScrollOffset;
                    InvokeMouseMove();
                }
            }
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection. 
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_htmlContainer != null)
                _htmlContainer.HandleMouseMove(this, e.GetPosition(this));
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
        }

        /// <summary>
        /// Propagate the LoadComplete event from root container.
        /// </summary>
        protected virtual void OnLoadComplete(EventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEvenArgs<EventArgs>(LoadCompleteEvent, this, e);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEvenArgs<HtmlLinkClickedEventArgs>(LinkClickedEvent, this, e);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        protected virtual void OnRenderError(HtmlRenderErrorEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEvenArgs<HtmlRenderErrorEventArgs>(RenderErrorEvent, this, e);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        protected virtual void OnStylesheetLoad(HtmlStylesheetLoadEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEvenArgs<HtmlStylesheetLoadEventArgs>(StylesheetLoadEvent, this, e);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        protected virtual void OnImageLoad(HtmlImageLoadEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEvenArgs<HtmlImageLoadEventArgs>(ImageLoadEvent, this, e);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        protected virtual void OnRefresh(HtmlRefreshEventArgs e)
        {
            if (e.Layout)
                InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected virtual double HtmlWidth(Size size)
        {
            return size.Width - Padding.Left - Padding.Right - BorderThickness.Left - BorderThickness.Right;
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected virtual double HtmlHeight(Size size)
        {
            return size.Height - Padding.Top - Padding.Bottom - BorderThickness.Top - BorderThickness.Bottom;
        }

        /// <summary>
        /// call mouse move to handle paint after scroll or html change affecting mouse cursor.
        /// </summary>
        protected virtual void InvokeMouseMove()
        {
            _htmlContainer.HandleMouseMove(this, Mouse.GetPosition(this));
        }

        /// <summary>
        /// Handle when dependency property value changes to update the underline HtmlContainer with the new value.
        /// </summary>
        private static void OnDependencyProperty_valueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = dependencyObject as HtmlControl;
            if (control != null)
            {
                var htmlContainer = control._htmlContainer;
                if (e.Property == AvoidImagesLateLoadingProperty)
                {
                    htmlContainer.AvoidImagesLateLoading = (bool)e.NewValue;
                }
                else if (e.Property == IsSelectionEnabledProperty)
                {
                    htmlContainer.IsSelectionEnabled = (bool)e.NewValue;
                }
                else if (e.Property == IsContextMenuEnabledProperty)
                {
                    htmlContainer.IsContextMenuEnabled = (bool)e.NewValue;
                }
                else if (e.Property == BaseStylesheetProperty)
                {
                    var baseCssData = HtmlRender.ParseStyleSheet((string)e.NewValue);
                    control._baseCssData = baseCssData;
                    htmlContainer.SetHtml(control.Text, baseCssData);
                }
                else if (e.Property == TextProperty)
                {
                    htmlContainer.ScrollOffset = new Point(0, 0);
                    htmlContainer.SetHtml((string)e.NewValue, control._baseCssData);
                    control.InvalidateMeasure();
                    control.InvalidateVisual();
                    control.InvokeMouseMove();
                }
            }
        }


        #region Private event handlers

        private void OnLoadComplete(object sender, EventArgs e)
        {
            if (CheckAccess())
                OnLoadComplete(e);
            else
                Dispatcher.Invoke(new Action<HtmlLinkClickedEventArgs>(OnLinkClicked), e);
        }

        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            if (CheckAccess())
                OnLinkClicked(e);
            else
                Dispatcher.Invoke(new Action<HtmlLinkClickedEventArgs>(OnLinkClicked), e);
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
            if (CheckAccess())
                OnStylesheetLoad(e);
            else
                Dispatcher.Invoke(new Action<HtmlStylesheetLoadEventArgs>(OnStylesheetLoad), e);
        }

        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            if (CheckAccess())
                OnImageLoad(e);
            else
                Dispatcher.Invoke(new Action<HtmlImageLoadEventArgs>(OnImageLoad), e);
        }

        private void OnRefresh(object sender, HtmlRefreshEventArgs e)
        {
            if (CheckAccess())
                OnRefresh(e);
            else
                Dispatcher.Invoke(new Action<HtmlRefreshEventArgs>(OnRefresh), e);
        }

        #endregion


        #endregion
    }
}