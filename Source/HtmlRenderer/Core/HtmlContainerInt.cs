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
using System.Diagnostics;
using HtmlRenderer.Core.Dom;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Handlers;
using HtmlRenderer.Core.Parse;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;

namespace HtmlRenderer.Core
{
    /// <summary>
    /// Low level handling of Html Renderer logic.<br/>
    /// Allows html layout and rendering without association to actual control, those allowing to handle html rendering on any graphics object.<br/>
    /// Using this class will require the client to handle all propagations of mouse/keyboard events, layout/paint calls, scrolling offset, 
    /// location/size/rectangle handling and UI refresh requests.<br/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>MaxSize and ActualSize:</b><br/>
    /// The max width and height of the rendered html.<br/>
    /// The max width will effect the html layout wrapping lines, resize images and tables where possible.<br/>
    /// The max height does NOT effect layout, but will not render outside it (clip).<br/>
    /// <see cref="ActualSize"/> can exceed the max size by layout restrictions (unwrap-able line, set image size, etc.).<br/>
    /// Set zero for unlimited (width/height separately).<br/>
    /// </para>
    /// <para>
    /// <b>ScrollOffset:</b><br/>
    /// This will adjust the rendered html by the given offset so the content will be "scrolled".<br/>
    /// Element that is rendered at location (50,100) with offset of (0,200) will not be rendered 
    /// at -100, therefore outside the client rectangle.
    /// </para>
    /// <para>
    /// <b>LinkClicked event</b><br/>
    /// Raised when the user clicks on a link in the html.<br/>
    /// Allows canceling the execution of the link to overwrite by custom logic.<br/>
    /// If error occurred in event handler it will propagate up the stack.
    /// </para>
    /// <para>
    /// <b>StylesheetLoad event:</b><br/>
    /// Raised when a stylesheet is about to be loaded by file path or URL in 'link' element.<br/>
    /// Allows to overwrite the loaded stylesheet by providing the stylesheet data manually, or different source (file or URL) to load from.<br/>
    /// Example: The stylesheet 'href' can be non-valid URI string that is interpreted in the overwrite delegate by custom logic to pre-loaded stylesheet object<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// <para>
    /// <b>ImageLoad event:</b><br/>
    /// Raised when an image is about to be loaded by file path, URL or inline data in 'img' element or background-image CSS style.<br/>
    /// Allows to overwrite the loaded image by providing the image object manually, or different source (file or URL) to load from.<br/>
    /// Example: image 'src' can be non-valid string that is interpreted in the overwrite delegate by custom logic to resource image object<br/>
    /// Example: image 'src' in the html is relative - the overwrite intercepts the load and provide full source URL to load the image from<br/>
    /// Example: image download requires authentication - the overwrite intercepts the load, downloads the image to disk using custom code and provide 
    /// file path to load the image from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </para>
    /// <para>
    /// <b>Refresh event:</b><br/>
    /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).<br/>
    /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
    /// </para>
    /// <para>
    /// <b>RenderError event:</b><br/>
    /// Raised when an error occurred during html rendering.<br/>
    /// </para>
    /// </remarks>
    public sealed class HtmlContainerInt : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// 
        /// </summary>
        private readonly GlobalBase _global;

        /// <summary>
        /// parser for CSS data
        /// </summary>
        private readonly CssParser _cssParser;

        /// <summary>
        /// the root css box of the parsed html
        /// </summary>
        private CssBox _root;

        /// <summary>
        /// list of all css boxes that have ":hover" selector on them
        /// </summary>
        private List<HoverBoxBlock> _hoverBoxes;

        /// <summary>
        /// Handler for text selection in the html. 
        /// </summary>
        private SelectionHandler _selectionHandler;

        /// <summary>
        /// the text fore color use for selected text
        /// </summary>
        private RColor _selectionForeColor;

        /// <summary>
        /// the back-color to use for selected text
        /// </summary>
        private RColor _selectionBackColor;

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        private CssData _cssData;

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        private bool _isSelectionEnabled = true;

        /// <summary>
        /// Is the build-in context menu enabled (default - true)
        /// </summary>
        private bool _isContextMenuEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided 
        /// for geometry like backgrounds and borders
        /// </summary>
        private bool _avoidGeometryAntialias;

        /// <summary>
        /// Gets or sets a value indicating if image asynchronous loading should be avoided (default - false).<br/>
        /// </summary>
        private bool _avoidAsyncImagesLoading;

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).<br/>
        /// </summary>
        private bool _avoidImagesLateLoading;

        /// <summary>
        /// the top-left most location of the rendered html
        /// </summary>
        private RPoint _location;

        /// <summary>
        /// the max width and height of the rendered html, effects layout, actual size cannot exceed this values.<br/>
        /// Set zero for unlimited.<br/>
        /// </summary>
        private RSize _maxSize;

        /// <summary>
        /// Gets or sets the scroll offset of the document for scroll controls
        /// </summary>
        private RPoint _scrollOffset;

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        private RSize _actualSize;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public HtmlContainerInt(GlobalBase global)
        {
            ArgChecker.AssertArgNotNull(global, "global");

            _global = global;
            _cssParser = new CssParser(global);
        }

        /// <summary>
        /// 
        /// </summary>
        internal GlobalBase Global
        {
            get { return _global; }
        }

        /// <summary>
        /// parser for CSS data
        /// </summary>
        internal CssParser CssParser
        {
            get { return _cssParser; }
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRefreshEventArgs> Refresh;

        /// <summary>
        /// Raised when Html Renderer request scroll to specific location.<br/>
        /// This can occur on document anchor click.
        /// </summary>
        public event EventHandler<HtmlScrollEventArgs> ScrollChange;

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or Uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        public CssData CssData
        {
            get { return _cssData; }
        }

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        public bool AvoidGeometryAntialias
        {
            get { return _avoidGeometryAntialias; }
            set { _avoidGeometryAntialias = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image asynchronous loading should be avoided (default - false).<br/>
        /// True - images are loaded synchronously during html parsing.<br/>
        /// False - images are loaded asynchronously to html parsing when downloaded from URL or loaded from disk.<br/>
        /// </summary>
        /// <remarks>
        /// Asynchronously image loading allows to unblock html rendering while image is downloaded or loaded from disk using IO 
        /// ports to achieve better performance.<br/>
        /// Asynchronously image loading should be avoided when the full html content must be available during render, like render to image.
        /// </remarks>
        public bool AvoidAsyncImagesLoading
        {
            get { return _avoidAsyncImagesLoading; }
            set { _avoidAsyncImagesLoading = value; }
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
        public bool AvoidImagesLateLoading
        {
            get { return _avoidImagesLateLoading; }
            set { _avoidImagesLateLoading = value; }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set { _isSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        public bool IsContextMenuEnabled
        {
            get { return _isContextMenuEnabled; }
            set { _isContextMenuEnabled = value; }
        }

        /// <summary>
        /// The scroll offset of the html.<br/>
        /// This will adjust the rendered html by the given offset so the content will be "scrolled".<br/>
        /// </summary>
        /// <example>
        /// Element that is rendered at location (50,100) with offset of (0,200) will not be rendered as it
        /// will be at -100 therefore outside the client rectangle.
        /// </example>
        public RPoint ScrollOffset
        {
            get { return _scrollOffset; }
            set { _scrollOffset = value; }
        }

        /// <summary>
        /// The top-left most location of the rendered html.<br/>
        /// This will offset the top-left corner of the rendered html.
        /// </summary>
        public RPoint Location
        {
            get { return _location; }
            set { _location = value; }
        }

        /// <summary>
        /// The max width and height of the rendered html.<br/>
        /// The max width will effect the html layout wrapping lines, resize images and tables where possible.<br/>
        /// The max height does NOT effect layout, but will not render outside it (clip).<br/>
        /// <see cref="ActualSize"/> can be exceed the max size by layout restrictions (unwrappable line, set image size, etc.).<br/>
        /// Set zero for unlimited (width\height separately).<br/>
        /// </summary>
        public RSize MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = value; }
        }

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        public RSize ActualSize
        {
            get { return _actualSize; }
            set { _actualSize = value; }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        public string SelectedText
        {
            get { return _selectionHandler.GetSelectedText(); }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        public string SelectedHtml
        {
            get { return _selectionHandler.GetSelectedHtml(); }
        }

        /// <summary>
        /// the root css box of the parsed html
        /// </summary>
        internal CssBox Root
        {
            get { return _root; }
        }

        /// <summary>
        /// the text fore color use for selected text
        /// </summary>
        internal RColor SelectionForeColor
        {
            get { return _selectionForeColor; }
            set { _selectionForeColor = value; }
        }

        /// <summary>
        /// the back-color to use for selected text
        /// </summary>
        internal RColor SelectionBackColor
        {
            get { return _selectionBackColor; }
            set { _selectionBackColor = value; }
        }

        /// <summary>
        /// Init with optional document and stylesheet.
        /// </summary>
        /// <param name="htmlSource">the html to init with, init empty if not given</param>
        /// <param name="baseCssData">optional: the stylesheet to init with, init default if not given</param>
        public void SetHtml(string htmlSource, CssData baseCssData = null)
        {
            if (_root != null)
            {
                _root.Dispose();
                _root = null;
                if (_selectionHandler != null)
                    _selectionHandler.Dispose();
                _selectionHandler = null;
            }

            if (!string.IsNullOrEmpty(htmlSource))
            {
                _cssData = baseCssData ?? _global.DefaultCssData;

                DomParser parser = new DomParser(_cssParser);
                _root = parser.GenerateCssTree(htmlSource, this, ref _cssData);
                if (_root != null)
                {
                    _selectionHandler = new SelectionHandler(_root);
                }
            }
        }

        /// <summary>
        /// Get html from the current DOM tree with style if requested.
        /// </summary>
        /// <param name="styleGen">Optional: controls the way styles are generated when html is generated (default: <see cref="HtmlGenerationStyle.Inline"/>)</param>
        /// <returns>generated html</returns>
        public string GetHtml(HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline)
        {
            return DomUtils.GenerateHtml(_root, styleGen);
        }

        /// <summary>
        /// Get attribute value of element at the given x,y location by given key.<br/>
        /// If more than one element exist with the attribute at the location the inner most is returned.
        /// </summary>
        /// <param name="location">the location to find the attribute at</param>
        /// <param name="attribute">the attribute key to get value by</param>
        /// <returns>found attribute value or null if not found</returns>
        public string GetAttributeAt(RPoint location, string attribute)
        {
            ArgChecker.AssertArgNotNullOrEmpty(attribute, "attribute");

            var cssBox = DomUtils.GetCssBox(_root, OffsetByScroll(location));
            return cssBox != null ? DomUtils.GetAttribute(cssBox, attribute) : null;
        }

        /// <summary>
        /// Get css link href at the given x,y location.
        /// </summary>
        /// <param name="location">the location to find the link at</param>
        /// <returns>css link href if exists or null</returns>
        public string GetLinkAt(RPoint location)
        {
            var link = DomUtils.GetLinkBox(_root, OffsetByScroll(location));
            return link != null ? link.HrefLink : null;
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public RRect? GetElementRectangle(string elementId)
        {
            ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

            var box = DomUtils.GetBoxById(_root, elementId.ToLower());
            return box != null ? CommonUtils.GetFirstValueOrDefault(box.Rectangles, box.Bounds) : (RRect?)null;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.
        /// </summary>
        /// <param name="g">Device context to draw</param>
        public void PerformLayout(IGraphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            if (_root != null)
            {
                _actualSize = RSize.Empty;

                // if width is not restricted we set it to large value to get the actual later
                _root.Size = new RSize(_maxSize.Width > 0 ? _maxSize.Width : 99999, 0);
                _root.Location = _location;
                _root.PerformLayout(g);

                if (_maxSize.Width <= 0.1)
                {
                    // in case the width is not restricted we need to double layout, first will find the width so second can layout by it (center alignment)
                    _root.Size = new RSize((int)Math.Ceiling(_actualSize.Width), 0);
                    _actualSize = RSize.Empty;
                    _root.PerformLayout(g);
                }
            }
        }

        /// <summary>
        /// Render the html using the given device.
        /// </summary>
        /// <param name="g">the device to use to render</param>
        public void PerformPaint(IGraphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            RRect prevClip = RRect.Empty;
            if (MaxSize.Height > 0)
            {
                prevClip = g.GetClip();
                g.SetClipReplace(new RRect(_location, _maxSize));
            }

            if (_root != null)
            {
                _root.Paint(g);
            }

            if (prevClip != RRect.Empty)
            {
                g.SetClipReplace(prevClip);
            }
        }

        /// <summary>
        /// Handle mouse down to handle selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="location">the location of the mouse</param>
        public void HandleMouseDown(IControl parent, RPoint location)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            try
            {
                if (_selectionHandler != null)
                    _selectionHandler.HandleMouseDown(parent, OffsetByScroll(location), IsMouseInContainer(location));
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse down handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="location">the location of the mouse</param>
        /// <param name="e">the mouse event data</param>
        public void HandleMouseUp(IControl parent, RPoint location, RMouseEvent e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            try
            {
                if (_selectionHandler != null && IsMouseInContainer(location))
                {
                    var ignore = _selectionHandler.HandleMouseUp(parent, e.LeftButton);
                    if (!ignore && e.LeftButton)
                    {
                        var loc = OffsetByScroll(location);
                        var link = DomUtils.GetLinkBox(_root, loc);
                        if (link != null)
                        {
                            HandleLinkClicked(parent, location, link);
                        }
                    }
                }
            }
            catch (HtmlLinkClickedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse up handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="location">the location of the mouse</param>
        public void HandleMouseDoubleClick(IControl parent, RPoint location)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            try
            {
                if (_selectionHandler != null && IsMouseInContainer(location))
                    _selectionHandler.SelectWord(parent, OffsetByScroll(location));
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse double click handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="location">the location of the mouse</param>
        public void HandleMouseMove(IControl parent, RPoint location)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            try
            {
                var loc = OffsetByScroll(location);
                if (_selectionHandler != null && IsMouseInContainer(location))
                    _selectionHandler.HandleMouseMove(parent, loc);

                /*
                if( _hoverBoxes != null )
                {
                    bool refresh = false;
                    foreach(var hoverBox in _hoverBoxes)
                    {
                        foreach(var rect in hoverBox.Item1.Rectangles.Values)
                        {
                            if( rect.Contains(loc) )
                            {
                                //hoverBox.Item1.Color = "gold";
                                refresh = true;
                            }
                        }
                    }

                    if(refresh)
                        RequestRefresh(true);
                }
                 */
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse move handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse leave to handle hover cursor.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        public void HandleMouseLeave(IControl parent)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            try
            {
                if (_selectionHandler != null)
                    _selectionHandler.HandleMouseLeave(parent);
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse leave handle", ex);
            }
        }

        /// <summary>
        /// Handle key down event for selection and copy.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the pressed key</param>
        public void HandleKeyDown(IControl parent, RKeyEvent e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            try
            {
                if (e.Control && _selectionHandler != null)
                {
                    // select all
                    if (e.AKeyCode)
                    {
                        _selectionHandler.SelectAll(parent);
                    }

                    // copy currently selected text
                    if (e.CKeyCode)
                    {
                        _selectionHandler.CopySelectedHtml();
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed key down handle", ex);
            }
        }

        /// <summary>
        /// Raise the stylesheet load event with the given event args.
        /// </summary>
        /// <param name="args">the event args</param>
        internal void RaiseHtmlStylesheetLoadEvent(HtmlStylesheetLoadEventArgs args)
        {
            try
            {
                if (StylesheetLoad != null)
                {
                    StylesheetLoad(this, args);
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.CssParsing, "Failed stylesheet load event", ex);
            }
        }

        /// <summary>
        /// Raise the image load event with the given event args.
        /// </summary>
        /// <param name="args">the event args</param>
        internal void RaiseHtmlImageLoadEvent(HtmlImageLoadEventArgs args)
        {
            try
            {
                if (ImageLoad != null)
                {
                    ImageLoad(this, args);
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.Image, "Failed image load event", ex);
            }
        }

        /// <summary>
        /// Request invalidation and re-layout of the control hosting the renderer.
        /// </summary>
        /// <param name="layout">is re-layout is required for the refresh</param>
        public void RequestRefresh(bool layout)
        {
            try
            {
                if (Refresh != null)
                {
                    Refresh(this, new HtmlRefreshEventArgs(layout));
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.General, "Failed refresh request", ex);
            }
        }

        /// <summary>
        /// Report error in html render process.
        /// </summary>
        /// <param name="type">the type of error to report</param>
        /// <param name="message">the error message</param>
        /// <param name="exception">optional: the exception that occured</param>
        internal void ReportError(HtmlRenderErrorType type, string message, Exception exception = null)
        {
            try
            {
                if (RenderError != null)
                {
                    RenderError(this, new HtmlRenderErrorEventArgs(type, message, exception));
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Handle link clicked going over <see cref="LinkClicked"/> event and using <see cref="Process.Start()"/> if not canceled.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="location">the location of the mouse</param>
        /// <param name="link">the link that was clicked</param>
        internal void HandleLinkClicked(IControl parent, RPoint location, CssBox link)
        {
            if (LinkClicked != null)
            {
                var args = new HtmlLinkClickedEventArgs(link.HrefLink, link.HtmlTag.Attributes);
                try
                {
                    LinkClicked(this, args);
                }
                catch (Exception ex)
                {
                    throw new HtmlLinkClickedException("Error in link clicked intercept", ex);
                }
                if (args.Handled)
                    return;
            }

            if (!string.IsNullOrEmpty(link.HrefLink))
            {
                if (link.HrefLink.StartsWith("#") && link.HrefLink.Length > 1)
                {
                    if (ScrollChange != null)
                    {
                        var rect = GetElementRectangle(link.HrefLink.Substring(1));
                        if (rect.HasValue)
                        {
                            ScrollChange(this, new HtmlScrollEventArgs(rect.Value.Location));
                            HandleMouseMove(parent, location);
                        }
                    }
                }
                else
                {
                    var nfo = new ProcessStartInfo(link.HrefLink);
                    nfo.UseShellExecute = true;
                    Process.Start(nfo);
                }
            }
        }

        /// <summary>
        /// Add css box that has ":hover" selector to be handled on mouse hover.
        /// </summary>
        /// <param name="box">the box that has the hover selector</param>
        /// <param name="block">the css block with the css data with the selector</param>
        internal void AddHoverBox(CssBox box, CssBlock block)
        {
            ArgChecker.AssertArgNotNull(box, "box");
            ArgChecker.AssertArgNotNull(block, "block");

            if (_hoverBoxes == null)
                _hoverBoxes = new List<HoverBoxBlock>();

            _hoverBoxes.Add(new HoverBoxBlock(box, block));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }


        #region Private methods

        /// <summary>
        /// Adjust the offset of the given location by the current scroll offset.
        /// </summary>
        /// <param name="location">the location to adjust</param>
        /// <returns>the adjusted location</returns>
        private RPoint OffsetByScroll(RPoint location)
        {
            return new RPoint(location.X - ScrollOffset.X, location.Y - ScrollOffset.Y);
        }

        /// <summary>
        /// Check if the mouse is currently on the html container.<br/>
        /// Relevant if the html container is not filled in the hosted control (location is not zero and the size is not the full size of the control).
        /// </summary>
        private bool IsMouseInContainer(RPoint location)
        {
            return location.X >= _location.X && location.X <= _location.X + _actualSize.Width && location.Y >= _location.Y + ScrollOffset.Y && location.Y <= _location.Y + ScrollOffset.Y + _actualSize.Height;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        private void Dispose(bool all)
        {
            try
            {
                if (all)
                {
                    LinkClicked = null;
                    Refresh = null;
                    RenderError = null;
                    StylesheetLoad = null;
                    ImageLoad = null;
                }

                _cssData = null;
                if (_root != null)
                    _root.Dispose();
                _root = null;
                if (_selectionHandler != null)
                    _selectionHandler.Dispose();
                _selectionHandler = null;
            }
            catch
            { }
        }

        #endregion
    }
}