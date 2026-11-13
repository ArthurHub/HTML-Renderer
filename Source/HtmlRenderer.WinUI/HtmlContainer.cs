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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinUI.Adapters;
using TheArtOfDev.HtmlRenderer.WinUI.Utilities;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

namespace TheArtOfDev.HtmlRenderer.WinUI
{
    /// <summary>
    /// Low level handling of Html Renderer logic, this class is used by <see cref="HtmlControl"/>,
    /// <see cref="HtmlPanel"/>, <see cref="HtmlLabel"/> and <see cref="HtmlRender"/>.<br/>
    /// </summary>
    /// <seealso cref="HtmlContainerInt"/>
    public sealed class HtmlContainer : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// The internal core html container
        /// </summary>
        private readonly HtmlContainerInt _htmlContainerInt;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public HtmlContainer()
        {
            _htmlContainerInt = new HtmlContainerInt(WinUIAdapter.Instance);
            _htmlContainerInt.PageSize = new RSize(99999, 99999);
        }

        /// <summary>
        /// Raised when the set html document has been fully loaded.<br/>
        /// Allows manipulation of the html dom, scroll position, etc.
        /// </summary>
        public event EventHandler LoadComplete
        {
            add { _htmlContainerInt.LoadComplete += value; }
            remove { _htmlContainerInt.LoadComplete -= value; }
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked
        {
            add { _htmlContainerInt.LinkClicked += value; }
            remove { _htmlContainerInt.LinkClicked -= value; }
        }

        /// <summary>
        /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRefreshEventArgs> Refresh
        {
            add { _htmlContainerInt.Refresh += value; }
            remove { _htmlContainerInt.Refresh -= value; }
        }

        /// <summary>
        /// Raised when Html Renderer request scroll to specific location.<br/>
        /// This can occur on document anchor click.
        /// </summary>
        public event EventHandler<HtmlScrollEventArgs> ScrollChange
        {
            add { _htmlContainerInt.ScrollChange += value; }
            remove { _htmlContainerInt.ScrollChange -= value; }
        }

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError
        {
            add { _htmlContainerInt.RenderError += value; }
            remove { _htmlContainerInt.RenderError -= value; }
        }

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or Uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad
        {
            add { _htmlContainerInt.StylesheetLoad += value; }
            remove { _htmlContainerInt.StylesheetLoad -= value; }
        }

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad
        {
            add { _htmlContainerInt.ImageLoad += value; }
            remove { _htmlContainerInt.ImageLoad -= value; }
        }

        /// <summary>
        /// The internal core html container
        /// </summary>
        internal HtmlContainerInt HtmlContainerInt
        {
            get { return _htmlContainerInt; }
        }

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        public CssData CssData
        {
            get { return _htmlContainerInt.CssData; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image asynchronous loading should be avoided (default - false).
        /// </summary>
        public bool AvoidAsyncImagesLoading
        {
            get { return _htmlContainerInt.AvoidAsyncImagesLoading; }
            set { _htmlContainerInt.AvoidAsyncImagesLoading = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).
        /// </summary>
        public bool AvoidImagesLateLoading
        {
            get { return _htmlContainerInt.AvoidImagesLateLoading; }
            set { _htmlContainerInt.AvoidImagesLateLoading = value; }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).
        /// </summary>
        public bool IsSelectionEnabled
        {
            get { return _htmlContainerInt.IsSelectionEnabled; }
            set { _htmlContainerInt.IsSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        public bool IsContextMenuEnabled
        {
            get { return _htmlContainerInt.IsContextMenuEnabled; }
            set { _htmlContainerInt.IsContextMenuEnabled = value; }
        }

        /// <summary>
        /// The scroll offset of the html.
        /// </summary>
        public Point ScrollOffset
        {
            get { return Utils.Convert(_htmlContainerInt.ScrollOffset); }
            set { _htmlContainerInt.ScrollOffset = Utils.Convert(value); }
        }

        /// <summary>
        /// The top-left most location of the rendered html.
        /// </summary>
        public Point Location
        {
            get { return Utils.Convert(_htmlContainerInt.Location); }
            set { _htmlContainerInt.Location = Utils.Convert(value); }
        }

        /// <summary>
        /// The max width and height of the rendered html.
        /// </summary>
        public Size MaxSize
        {
            get { return Utils.Convert(_htmlContainerInt.MaxSize); }
            set { _htmlContainerInt.MaxSize = Utils.Convert(value); }
        }

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        public Size ActualSize
        {
            get { return Utils.Convert(_htmlContainerInt.ActualSize); }
            internal set { _htmlContainerInt.ActualSize = Utils.Convert(value); }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        public string SelectedText
        {
            get { return _htmlContainerInt.SelectedText; }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        public string SelectedHtml
        {
            get { return _htmlContainerInt.SelectedHtml; }
        }

        /// <summary>
        /// Clear the current selection.
        /// </summary>
        public void ClearSelection()
        {
            HtmlContainerInt.ClearSelection();
        }

        /// <summary>
        /// Init with optional document and stylesheet.
        /// </summary>
        public Task SetHtml(string htmlSource, CssData baseCssData = null)
        {
            return _htmlContainerInt.SetHtml(htmlSource, baseCssData);
        }

        /// <summary>
        /// Clear the content of the HTML container releasing any resources used to render previously existing content.
        /// </summary>
        public void Clear()
        {
            _htmlContainerInt.Clear();
        }

        /// <summary>
        /// Get html from the current DOM tree with style if requested.
        /// </summary>
        public string GetHtml(HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline)
        {
            return _htmlContainerInt.GetHtml(styleGen);
        }

        /// <summary>
        /// Get attribute value of element at the given x,y location by given key.
        /// </summary>
        public string GetAttributeAt(Point location, string attribute)
        {
            return _htmlContainerInt.GetAttributeAt(Utils.Convert(location), attribute);
        }

        /// <summary>
        /// Get all the links in the HTML with the element Rect and href data.
        /// </summary>
        public List<LinkElementData<Rect>> GetLinks()
        {
            var linkElements = new List<LinkElementData<Rect>>();
            foreach (var link in HtmlContainerInt.GetLinks())
            {
                linkElements.Add(new LinkElementData<Rect>(link.Id, link.Href, Utils.Convert(link.Rectangle)));
            }
            return linkElements;
        }

        /// <summary>
        /// Get css link href at the given x,y location.
        /// </summary>
        public string GetLinkAt(Point location)
        {
            return _htmlContainerInt.GetLinkAt(Utils.Convert(location));
        }

        /// <summary>
        /// Get the Rect of html element as calculated by html layout.
        /// </summary>
        public Rect? GetElementRectangle(string elementId)
        {
            var r = _htmlContainerInt.GetElementRectangle(elementId);
            return r.HasValue ? Utils.Convert(r.Value) : (Rect?)null;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.
        /// </summary>
        public void PerformLayout()
        {
            using (var ig = new GraphicsAdapter())
            {
                _htmlContainerInt.PerformLayout(ig);
            }
        }

        /// <summary>
        /// Render the html using the given device.
        /// </summary>
        /// <param name="g">the device to use to render</param>
        /// <param name="clip">the clip rectangle of the html container</param>
        public void PerformPaint(CanvasDrawingSession g, Rect clip)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            using (var ig = new GraphicsAdapter(g, Utils.Convert(clip), g.Device))
            {
                _htmlContainerInt.PerformPaint(ig);
            }
        }

        /// <summary>
        /// Handle mouse down to handle selection.
        /// </summary>
        public void HandleMouseDown(Control parent, PointerRoutedEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            var point = e.GetCurrentPoint(parent);
            _htmlContainerInt.HandleMouseDown(new ControlAdapter(parent), Utils.Convert(point.Position));
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click.
        /// </summary>
        public void HandleMouseUp(Control parent, PointerRoutedEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            var point = e.GetCurrentPoint(parent);
            var mouseEvent = new RMouseEvent(point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased);
            _htmlContainerInt.HandleMouseUp(new ControlAdapter(parent), Utils.Convert(point.Position), mouseEvent);
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse.
        /// </summary>
        public void HandleMouseDoubleClick(Control parent, DoubleTappedRoutedEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            _htmlContainerInt.HandleMouseDoubleClick(new ControlAdapter(parent), Utils.Convert(e.GetPosition(parent)));
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection.
        /// </summary>
        public void HandleMouseMove(Control parent, Point mousePos)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            _htmlContainerInt.HandleMouseMove(new ControlAdapter(parent), Utils.Convert(mousePos));
        }

        /// <summary>
        /// Handle mouse leave to handle hover cursor.
        /// </summary>
        public void HandleMouseLeave(Control parent)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            _htmlContainerInt.HandleMouseLeave(new ControlAdapter(parent));
        }

        /// <summary>
        /// Handle key down event for selection and copy.
        /// </summary>
        public void HandleKeyDown(Control parent, KeyRoutedEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            _htmlContainerInt.HandleKeyDown(new ControlAdapter(parent), CreateKeyEvent(e));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _htmlContainerInt.Dispose();
        }

        #region Private methods

        /// <summary>
        /// Create HtmlRenderer key event from WinUI key event.
        /// </summary>
        private static RKeyEvent CreateKeyEvent(KeyRoutedEventArgs e)
        {
            var controlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            var control = (controlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            return new RKeyEvent(control, e.Key == VirtualKey.A, e.Key == VirtualKey.C);
        }

        #endregion
    }
}
