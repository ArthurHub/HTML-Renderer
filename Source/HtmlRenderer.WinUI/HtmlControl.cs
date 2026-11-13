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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WinUI.Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace TheArtOfDev.HtmlRenderer.WinUI
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WinUI 3 control that will render html content in it's client rectangle.<br/>
    /// The control will handle pointer and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
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
    /// <para>
    /// All events are raised on the UI thread.
    /// </para>
    /// </summary>
    public class HtmlControl : UserControl
    {
        internal enum CursorShape
        {
            Default,
            Hand,
            IBeam,
        }

        #region Fields and Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        protected readonly HtmlContainer _htmlContainer;

        /// <summary>
        /// The canvas the html is drawn on.
        /// </summary>
        protected readonly CanvasControl _canvas;

        /// <summary>
        /// The content of the control, holds <see cref="_canvas"/> and any additional chrome like scrollbars.
        /// </summary>
        protected readonly Grid _rootPanel;

        /// <summary>
        /// the base stylesheet data used in the control
        /// </summary>
        protected CssData _baseCssData;

        /// <summary>
        /// The last position of the scrollbars to know if it has changed to update mouse
        /// </summary>
        protected Point _lastScrollOffset;

        /// <summary>
        /// Used to raise the container events on the UI thread.
        /// </summary>
        private readonly DispatcherQueue _dispatcherQueue;

        /// <summary>
        /// Canceled when the UI thread's queue shuts down, the queued events then never run.
        /// </summary>
        private readonly CancellationToken _dispatcherShutdown;

        /// <summary>
        /// One per UI thread, the queue keeps its handler alive, so the handler can't reference a control.
        /// </summary>
        [ThreadStatic]
        private static CancellationTokenSource _threadDispatcherShutdown;

        /// <summary>
        /// Tracks the in-flight <see cref="SetTextAsync"/> call, if any, so a newer call can supersede an
        /// older one still awaiting <see cref="HtmlContainer.SetHtml"/>.
        /// </summary>
        private CancellationTokenSource _pendingLoad;

        /// <summary>
        /// The last pointer state seen by the control, there's no global mouse state to query in WinUI.
        /// </summary>
        private PointerPoint _lastPointerPoint;

        /// <summary>
        /// The data of the drag operation started by the selection, null if none is in progress.
        /// </summary>
        private ClipboardHelper.Content _dragData;

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty AvoidImagesLateLoadingProperty = DependencyProperty.Register(nameof(AvoidImagesLateLoading), typeof(bool), typeof(HtmlControl), new PropertyMetadata(false, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register(nameof(IsSelectionEnabled), typeof(bool), typeof(HtmlControl), new PropertyMetadata(true, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty IsContextMenuEnabledProperty = DependencyProperty.Register(nameof(IsContextMenuEnabled), typeof(bool), typeof(HtmlControl), new PropertyMetadata(true, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty BaseStylesheetProperty = DependencyProperty.Register(nameof(BaseStylesheet), typeof(string), typeof(HtmlControl), new PropertyMetadata(null, OnDependencyProperty_valueChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(HtmlControl), new PropertyMetadata(null, OnDependencyProperty_valueChanged));

        #endregion

        /// <summary>
        /// Creates a new HtmlControl and sets a basic css for it's styling.
        /// </summary>
        protected HtmlControl()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            if (_threadDispatcherShutdown == null)
            {
                var shutdown = new CancellationTokenSource();
                _dispatcherQueue.ShutdownStarting += (sender, args) => shutdown.Cancel();
                _threadDispatcherShutdown = shutdown;
            }
            _dispatcherShutdown = _threadDispatcherShutdown.Token;

            // A UserControl isn't a tab stop by default, without focus there are no key events for copy and scrolling
            IsTabStop = true;

            _canvas = new CanvasControl();
            _canvas.Draw += OnCanvasDraw;
            _rootPanel = new Grid();
            _rootPanel.Children.Add(_canvas);
            Content = _rootPanel;

            DragStarting += OnDragStarting;

            _htmlContainer = new HtmlContainer();
            _htmlContainer.LoadComplete += OnLoadCompleteInternal;
            _htmlContainer.LinkClicked += OnLinkClickedInternal;
            _htmlContainer.RenderError += OnRenderErrorInternal;
            _htmlContainer.Refresh += OnRefreshInternal;
            _htmlContainer.StylesheetLoad += OnStylesheetLoadInternal;
            _htmlContainer.ImageLoad += OnImageLoadInternal;
        }

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
        public bool AvoidImagesLateLoading
        {
            get { return (bool)GetValue(AvoidImagesLateLoadingProperty); }
            set { SetValue(AvoidImagesLateLoadingProperty, value); }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        public bool IsSelectionEnabled
        {
            get { return (bool)GetValue(IsSelectionEnabledProperty); }
            set { SetValue(IsSelectionEnabledProperty, value); }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        public bool IsContextMenuEnabled
        {
            get { return (bool)GetValue(IsContextMenuEnabledProperty); }
            set { SetValue(IsContextMenuEnabledProperty, value); }
        }

        /// <summary>
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        public string BaseStylesheet
        {
            get { return (string)GetValue(BaseStylesheetProperty); }
            set { SetValue(BaseStylesheetProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text of this panel
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        public virtual string SelectedText
        {
            get { return _htmlContainer.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        public virtual string SelectedHtml
        {
            get { return _htmlContainer.SelectedHtml; }
        }

        /// <summary>
        /// The last known pointer location relative to this control.
        /// </summary>
        internal RPoint LastPointerLocation
        {
            get { return _lastPointerPoint != null ? Utils.Convert(_lastPointerPoint.Position) : RPoint.Empty; }
        }

        /// <summary>
        /// Whether the left pointer button was down as of the last pointer event of this control.
        /// </summary>
        internal bool IsLeftButtonPressed
        {
            get { return _lastPointerPoint != null && _lastPointerPoint.Properties.IsLeftButtonPressed; }
        }

        /// <summary>
        /// Whether the right pointer button was down as of the last pointer event of this control.
        /// </summary>
        internal bool IsRightButtonPressed
        {
            get { return _lastPointerPoint != null && _lastPointerPoint.Properties.IsRightButtonPressed; }
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
            _htmlContainer?.ClearSelection();
        }

        /// <summary>
        /// Sets the html of this control and awaits the async load - the real entry point behind the
        /// <see cref="TextProperty"/>/<see cref="BaseStylesheetProperty"/> dependency-property callbacks,
        /// for callers that want to await completion or cancel an in-flight load.
        /// </summary>
        /// <remarks>
        /// A new call before a previous one finishes supersedes it: the previous call's
        /// <see cref="HtmlContainer.SetHtml"/> keeps running (it has no mid-flight cancellation point of
        /// its own) but its post-completion side effects - invalidate, error reporting - are discarded
        /// once it resumes, so only the newest call's results ever reach the control.
        /// </remarks>
        /// <param name="html">the html to set</param>
        /// <param name="cancellationToken">optional: cancel this specific call without affecting others</param>
        public async Task SetTextAsync(string html, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _pendingLoad?.Cancel();
            _pendingLoad = cts;

            try
            {
                await _htmlContainer.SetHtml(html, _baseCssData);
            }
            catch (Exception ex)
            {
                if (cts == _pendingLoad)
                {
                    OnRenderError(new HtmlRenderErrorEventArgs(HtmlRenderErrorType.General, "Failed to set html", ex));
                }
                return;
            }

            if (cts != _pendingLoad || cts.IsCancellationRequested)
            {
                return;
            }

            InvalidateMeasure();
            _canvas.Invalidate();
            InvokeMouseMove();
        }

        /// <summary>
        /// Set the cursor shown over the control, <see cref="UIElement.ProtectedCursor"/> can only be set from within the control.
        /// </summary>
        internal void SetCursorShape(CursorShape shape)
        {
            InputSystemCursorShape cursorShape;
            switch (shape)
            {
                case CursorShape.Hand:
                    cursorShape = InputSystemCursorShape.Hand;
                    break;
                case CursorShape.IBeam:
                    cursorShape = InputSystemCursorShape.IBeam;
                    break;
                default:
                    cursorShape = InputSystemCursorShape.Arrow;
                    break;
            }
            ProtectedCursor = InputSystemCursor.Create(cursorShape);
        }

        /// <summary>
        /// Invalidate the canvas, causing a repaint.
        /// </summary>
        internal void InvalidateCanvas()
        {
            _canvas.Invalidate();
        }

        /// <summary>
        /// Raise the <see cref="RenderError"/> event, for errors outside of the container.
        /// </summary>
        internal void ReportError(HtmlRenderErrorType type, string message, Exception exception)
        {
            OnRenderError(new HtmlRenderErrorEventArgs(type, message, exception));
        }

        /// <summary>
        /// Start dragging the given data with the pointer that is currently pressed on the control.
        /// </summary>
        internal void StartDragDrop(ClipboardHelper.Content data)
        {
            if (_dragData != null || _lastPointerPoint == null || !_lastPointerPoint.IsInContact)
                return;

            _dragData = data;
            _ = DragDropAsync(_lastPointerPoint);
        }

        #region Protected methods

        /// <summary>
        /// Handle pointer move to handle hover cursor and text selection.
        /// </summary>
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            _lastPointerPoint = e.GetCurrentPoint(this);
            _htmlContainer?.HandleMouseMove(this, _lastPointerPoint.Position);
        }

        /// <summary>
        /// Handle pointer exit to handle cursor change.
        /// </summary>
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            _htmlContainer?.HandleMouseLeave(this);
        }

        /// <summary>
        /// Handle pointer press to handle selection.
        /// </summary>
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            _lastPointerPoint = e.GetCurrentPoint(this);
            _htmlContainer?.HandleMouseDown(this, e);
        }

        /// <summary>
        /// Handle pointer release to handle selection and link click.
        /// </summary>
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            _lastPointerPoint = e.GetCurrentPoint(this);
            _htmlContainer?.HandleMouseUp(this, e);
        }

        /// <summary>
        /// Handle double tap to select word under the pointer.
        /// </summary>
        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            base.OnDoubleTapped(e);
            _htmlContainer?.HandleMouseDoubleClick(this, e);
        }

        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            _htmlContainer?.HandleKeyDown(this, e);
        }

        /// <summary>
        /// Propagate the LoadComplete event from root container.
        /// </summary>
        protected virtual void OnLoadComplete(EventArgs e)
        {
            LoadComplete?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        protected virtual void OnLinkClicked(HtmlLinkClickedEventArgs e)
        {
            LinkClicked?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        protected virtual void OnRenderError(HtmlRenderErrorEventArgs e)
        {
            RenderError?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        protected virtual void OnStylesheetLoad(HtmlStylesheetLoadEventArgs e)
        {
            StylesheetLoad?.Invoke(this, e);
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        protected virtual void OnImageLoad(HtmlImageLoadEventArgs e)
        {
            ImageLoad?.Invoke(this, e);
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        protected virtual void OnRefresh(HtmlRefreshEventArgs e)
        {
            if (e.Layout)
                InvalidateMeasure();
            _canvas.Invalidate();
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected virtual double HtmlWidth(Size size)
        {
            return size.Width - Padding.Left - Padding.Right - BorderThickness.Left - BorderThickness.Right;
        }

        /// <summary>
        /// Get the height the HTML has to render in (not including horizontal scroll iff it is visible)
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
            _htmlContainer.HandleMouseMove(this, Utils.Convert(LastPointerLocation));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var g = args.DrawingSession;
            var renderSize = new Size(sender.ActualWidth, sender.ActualHeight);

            if (Background is SolidColorBrush backgroundBrush && backgroundBrush.Opacity > 0)
                g.FillRectangle(new Rect(0, 0, renderSize.Width, renderSize.Height), backgroundBrush.Color);

            var borderThickness = BorderThickness;
            if (borderThickness != new Thickness(0) && BorderBrush is SolidColorBrush borderBrush)
            {
                var color = borderBrush.Color;
                if (borderThickness.Top > 0)
                    g.FillRectangle(new Rect(0, 0, renderSize.Width, borderThickness.Top), color);
                if (borderThickness.Bottom > 0)
                    g.FillRectangle(new Rect(0, renderSize.Height - borderThickness.Bottom, renderSize.Width, borderThickness.Bottom), color);
                if (borderThickness.Left > 0)
                    g.FillRectangle(new Rect(0, 0, borderThickness.Left, renderSize.Height), color);
                if (borderThickness.Right > 0)
                    g.FillRectangle(new Rect(renderSize.Width - borderThickness.Right, 0, borderThickness.Right, renderSize.Height), color);
            }

            var htmlWidth = HtmlWidth(renderSize);
            var htmlHeight = HtmlHeight(renderSize);
            if (_htmlContainer != null && htmlWidth > 0 && htmlHeight > 0)
            {
                var clip = new Rect(Padding.Left + borderThickness.Left, Padding.Top + borderThickness.Top, htmlWidth, htmlHeight);
                using (g.CreateLayer(1f, clip))
                {
                    _htmlContainer.Location = new Point(clip.X, clip.Y);
                    _htmlContainer.PerformPaint(g, clip);
                }

                if (!_lastScrollOffset.Equals(_htmlContainer.ScrollOffset))
                {
                    _lastScrollOffset = _htmlContainer.ScrollOffset;
                    InvokeMouseMove();
                }
            }
        }

        private async Task DragDropAsync(PointerPoint pointerPoint)
        {
            try
            {
                await StartDragAsync(pointerPoint);
            }
            catch (Exception ex)
            {
                OnRenderError(new HtmlRenderErrorEventArgs(HtmlRenderErrorType.KeyboardMouse, "Failed drag and drop", ex));
            }
            finally
            {
                _dragData = null;
            }
        }

        private void OnDragStarting(UIElement sender, DragStartingEventArgs args)
        {
            if (_dragData == null)
            {
                args.Cancel = true;
                return;
            }

            ClipboardHelper.Fill(args.Data, _dragData);
            args.AllowedOperations = DataPackageOperation.Copy;
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
                    _ = control.SetTextAsync(control.Text);
                }
                else if (e.Property == TextProperty)
                {
                    htmlContainer.ScrollOffset = new Point(0, 0);
                    _ = control.SetTextAsync((string)e.NewValue);
                }
            }
        }

        /// <summary>
        /// Run the given action on the UI thread and wait for it, the load events need the handler's changes to
        /// the event args before the container continues.
        /// </summary>
        private void InvokeOnUIThread(Action action)
        {
            if (_dispatcherQueue.HasThreadAccess)
            {
                action();
                return;
            }

            // Not disposed, a queued action may still set it after the queue shut down
            var done = new ManualResetEventSlim();
            ExceptionDispatchInfo error = null;
            var queued = _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ExceptionDispatchInfo.Capture(ex);
                }
                finally
                {
                    done.Set();
                }
            });

            // Once the queue shuts down, there's no one left to raise the event to
            if (!queued)
                return;

            try
            {
                done.Wait(_dispatcherShutdown);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            error?.Throw();
        }

        private void OnLoadCompleteInternal(object sender, EventArgs e)
        {
            InvokeOnUIThread(() => OnLoadComplete(e));
        }

        private void OnLinkClickedInternal(object sender, HtmlLinkClickedEventArgs e)
        {
            InvokeOnUIThread(() => OnLinkClicked(e));
        }

        private void OnRenderErrorInternal(object sender, HtmlRenderErrorEventArgs e)
        {
            InvokeOnUIThread(() => OnRenderError(e));
        }

        private void OnStylesheetLoadInternal(object sender, HtmlStylesheetLoadEventArgs e)
        {
            InvokeOnUIThread(() => OnStylesheetLoad(e));
        }

        private void OnImageLoadInternal(object sender, HtmlImageLoadEventArgs e)
        {
            InvokeOnUIThread(() => OnImageLoad(e));
        }

        private void OnRefreshInternal(object sender, HtmlRefreshEventArgs e)
        {
            InvokeOnUIThread(() => OnRefresh(e));
        }

        #endregion
    }
}
