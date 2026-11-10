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
using System.IO;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Network;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Adapters
{
    /// <summary>
    /// Platform adapter to bridge platform specific objects to HTML Renderer core library.<br/>
    /// Core uses abstract renderer objects (RAdapter/RControl/REtc...) to access platform specific functionality, the concrete platforms 
    /// implements those objects to provide concrete platform implementation. Those allowing the core library to be platform agnostic.
    /// <para>
    /// Platforms: WinForms, WPF, Metro, PDF renders, etc.<br/>
    /// Objects: UI elements(Controls), Graphics(Render context), Colors, Brushes, Pens, Fonts, Images, Clipboard, etc.<br/>
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is best to have a singleton instance of this class for concrete implementation!<br/>
    /// This is because it holds caches of default CssData, Images, Fonts and Brushes.
    /// </remarks>
    public abstract class RAdapter
    {
        #region Fields/Consts

        /// <summary>
        /// cache of brush color to brush instance
        /// </summary>
        private readonly Dictionary<RColor, RBrush> _brushesCache = new Dictionary<RColor, RBrush>();

        /// <summary>
        /// cache of pen color to pen instance
        /// </summary>
        private readonly Dictionary<RColor, RPen> _penCache = new Dictionary<RColor, RPen>();

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private readonly FontsHandler _fontsHandler;

        /// <summary>
        /// Dedup cache for <see cref="AddFontFace"/>, keyed by the resolved resource's absolute URI - since
        /// the orchestrator re-runs <c>@font-face</c> registration on every <c>SetHtml</c> and this adapter
        /// is a process-wide singleton, re-registering the same face's bytes with the platform text engine
        /// on every call would leak native font handles (WinForms' <c>PrivateFontCollection.AddMemoryFont</c>/
        /// WPF's <c>AddFontMemResourceEx</c> both register a *new* resource each call, even for identical
        /// bytes - neither is idempotent).
        /// </summary>
        private readonly Dictionary<string, RFontFamily> _fontFaceCache = new Dictionary<string, RFontFamily>();

        /// <summary>
        /// default CSS parsed data singleton
        /// </summary>
        private CssData _defaultCssData;

        /// <summary>
        /// image used to draw loading image icon
        /// </summary>
        private RImage _loadImage;

        /// <summary>
        /// image used to draw error image icon
        /// </summary>
        private RImage _errorImage;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        protected RAdapter()
        {
            _fontsHandler = new FontsHandler(this);
        }

        /// <summary>
        /// The CSS media type this adapter renders for (CSS 2.1 §7.3), used to evaluate <c>@media</c> rules.
        /// <c>"screen"</c> for on-screen adapters (the default), <c>"print"</c> for paged output.
        /// </summary>
        public virtual string DefaultMediaType
        {
            get { return "screen"; }
        }

        /// <summary>
        /// The colour scheme the rendering surface presents, answering the <c>prefers-color-scheme</c>
        /// media feature. Defaults to <see cref="RColorScheme.Light"/>; an adapter that renders onto a
        /// themed surface should report the system setting instead, and raise
        /// <see cref="ColorSchemeChanged"/> when it changes.
        /// </summary>
        public virtual RColorScheme SystemColorScheme
        {
            get { return RColorScheme.Light; }
        }

        /// <summary>
        /// Test-only seam: when set, an adapter that would otherwise report a live system theme (WinForms/
        /// WPF) returns this value from <see cref="SystemColorScheme"/> instead of querying the OS. Image-
        /// regression tests render against a fixed baseline and must not depend on the host machine's own
        /// theme setting - without this, a CI runner whose theme differs from whatever machine captured the
        /// baseline fails a <c>prefers-color-scheme</c> sample for reasons unrelated to any real rendering
        /// regression. Production code must never set this.
        /// </summary>
        internal static RColorScheme? SystemColorSchemeOverride { get; set; }

        /// <summary>
        /// Raised when <see cref="SystemColorScheme"/> has changed, so anything rendered against it can
        /// re-evaluate its <c>prefers-color-scheme</c> rules and repaint. Never raised by an adapter
        /// whose scheme is fixed.
        /// </summary>
        /// <remarks>
        /// Handlers are held for the lifetime of the adapter, which is typically a process-wide
        /// singleton, so a subscriber must unsubscribe when it is disposed.
        /// </remarks>
        public event EventHandler ColorSchemeChanged;

        /// <summary>
        /// Raises <see cref="ColorSchemeChanged"/>. For adapters that track a system theme.
        /// </summary>
        protected void OnColorSchemeChanged()
        {
            var handler = ColorSchemeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Controls how the root HTML document and every external resource it references (stylesheets,
        /// images, <c>@font-face</c> fonts) is loaded. Defaults to <see cref="DataUriNetworkLoader"/>,
        /// which only resolves <c>data:</c> URIs — set this to a <see cref="FileUriNetworkLoader"/> or
        /// <see cref="HttpClientNetworkLoader"/> (or a custom <see cref="Network.RNetworkLoader"/>) to
        /// enable loading from local files or over HTTP(S).
        /// </summary>
        public RNetworkLoader NetworkLoader { get; set; } = new DataUriNetworkLoader();

        /// <summary>
        /// Whether <c>file:</c> resource requests are honored. Checked ahead of, and independently from,
        /// whether <see cref="NetworkLoader"/> happens to be a <see cref="FileUriNetworkLoader"/> — a
        /// <c>false</c> value refuses local file access even then. Defaults to <c>true</c>.
        /// </summary>
        public bool AllowLocalFileAccess { get; set; } = true;

        // Serves file: URIs (and supplies the default working-directory base URI) whenever the configured
        // NetworkLoader isn't itself a FileUriNetworkLoader - mirroring how data: URIs are always handled
        // internally regardless of which loader is configured. Lazily created so its
        // Directory.GetCurrentDirectory() snapshot isn't taken until a file: resource (or the fallback base
        // URI) is actually needed.
        private FileUriNetworkLoader _internalFileLoader;
        private FileUriNetworkLoader InternalFileLoader => _internalFileLoader ?? (_internalFileLoader = new FileUriNetworkLoader());

        // Serves embedded: URIs unconditionally, the same way data:/file: are always handled internally
        // regardless of which loader is configured - stateless (the target assembly is named in the URI
        // itself, see EmbeddedResourceNetworkLoader), so a single shared instance needs no lazy init.
        private static readonly EmbeddedResourceNetworkLoader InternalEmbeddedResourceLoader = new EmbeddedResourceNetworkLoader();

        /// <summary>
        /// The document's base URL, used to resolve relative <c>href</c>/<c>src</c>/CSS <c>url()</c>
        /// references that have no closer <c>&lt;base href&gt;</c> element to resolve against. Sourced from
        /// <see cref="NetworkLoader"/>'s own base URI when it has one; otherwise (e.g. the default
        /// <see cref="DataUriNetworkLoader"/>, which has none) falls back to the current working directory
        /// as a <c>file:</c> URI when local file access is allowed, or <c>null</c> when it is not.
        /// </summary>
        public RUri BaseUri => NetworkLoader.BaseUri ?? (AllowLocalFileAccess ? InternalFileLoader.BaseUri : null);

        /// <summary>
        /// Resolve an external resource (a stylesheet, image, or <c>@font-face</c> font) referenced by the
        /// document, dispatching by URI scheme: <c>data:</c>, <c>file:</c>, and <c>embedded:</c> always
        /// resolve internally (<c>file:</c> refused outright when <see cref="AllowLocalFileAccess"/> is
        /// <c>false</c>), every other scheme goes to the configured <see cref="NetworkLoader"/>.
        /// </summary>
        /// <param name="uri">the resource URI, already resolved to absolute against <see cref="BaseUri"/> or a <c>&lt;base href&gt;</c> element</param>
        /// <returns>the resolved resource, or null if it could not be resolved</returns>
        public Task<RNetworkResponse> GetResourceStream(RUri uri)
        {
            // BaseUri is normally never null, so every reference resolves to an absolute URI and loaders
            // only ever see those - RUri.Scheme throws on a relative URI, and neither DataUriNetworkLoader
            // nor HttpClientNetworkLoader checks. Denying local file access is what makes BaseUri nullable,
            // so a relative reference can now survive resolution; answer "unresolved" for it here rather
            // than handing a loader a URI it cannot inspect.
            if (!uri.IsAbsoluteUri)
            {
                return Task.FromResult<RNetworkResponse>(null);
            }

            if (uri.Scheme == "data")
            {
                var dataLoader = NetworkLoader as DataUriNetworkLoader ?? new DataUriNetworkLoader();
                return dataLoader.GetResourceStream(uri);
            }

            if (uri.Scheme == "file")
            {
                // Checked ahead of the configured loader, so a deny holds even when that loader is itself a
                // FileUriNetworkLoader - the two settings contradict each other, and refusing is the safe read.
                if (!AllowLocalFileAccess)
                {
                    return Task.FromResult<RNetworkResponse>(null);
                }

                var fileLoader = NetworkLoader as FileUriNetworkLoader ?? InternalFileLoader;
                return fileLoader.GetResourceStream(uri);
            }

            if (uri.Scheme == EmbeddedResourceNetworkLoader.Scheme)
            {
                var embeddedLoader = NetworkLoader as EmbeddedResourceNetworkLoader ?? InternalEmbeddedResourceLoader;
                return embeddedLoader.GetResourceStream(uri);
            }

            return NetworkLoader.GetResourceStream(uri);
        }

        /// <summary>
        /// Get the default CSS stylesheet data.
        /// </summary>
        public CssData DefaultCssData
        {
            get
            {
                if (_defaultCssData == null)
                {
                    _defaultCssData = CssData.Parse(this, CssDefaults.DefaultStyleSheet, false);
                    foreach (var stylesheet in _defaultCssData.Stylesheets)
                        stylesheet.IsUserAgent = true;
                }
                return _defaultCssData;
            }
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public RColor GetColor(string colorName)
        {
            ArgChecker.AssertArgNotNullOrEmpty(colorName, "colorName");
            return GetColorInt(colorName);
        }

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        public RPen GetPen(RColor color)
        {
            RPen pen;
            if (!_penCache.TryGetValue(color, out pen))
            {
                _penCache[color] = pen = CreatePen(color);
            }
            return pen;
        }

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        public RBrush GetSolidBrush(RColor color)
        {
            RBrush brush;
            if (!_brushesCache.TryGetValue(color, out brush))
            {
                _brushesCache[color] = brush = CreateSolidBrush(color);
            }
            return brush;
        }

        /// <summary>
        /// Get a multi-stop linear gradient brush along the line from <paramref name="p1"/> to <paramref name="p2"/>.
        /// </summary>
        /// <param name="p1">the gradient line's start point</param>
        /// <param name="p2">the gradient line's end point</param>
        /// <param name="stops">color stops, each with a position in [0,1] along the gradient line</param>
        /// <returns>linear gradient color brush instance</returns>
        public RBrush GetLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            return CreateLinearGradientBrush(p1, p2, stops);
        }

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="RImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public RImage ConvertImage(object image)
        {
            // TODO:a remove this by creating better API.
            return ConvertImageInt(image);
        }

        /// <summary>
        /// Create an <see cref="RImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public RImage ImageFromStream(Stream memoryStream)
        {
            return ImageFromStreamInt(memoryStream);
        }

        /// <summary>
        /// Check if the given font exists in the system by font family name, including the families
        /// registered by <see cref="AddFontFace"/>. <see cref="Core.Parse.CssParser.ParseFontFamily"/> uses
        /// it to pick the first available family of a <c>font-family</c> list.
        /// </summary>
        /// <param name="font">the font name to check</param>
        /// <returns>true - font exists by given family name, false - otherwise</returns>
        /// <remarks>
        /// Virtual for the PdfSharp backend, which keeps its <c>@font-face</c> faces in its own font resolver.
        /// </remarks>
        public virtual bool IsFontExists(string font)
        {
            return _fontsHandler.IsFontExists(font);
        }

        /// <summary>
        /// Adds a font family to be used.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public void AddFontFamily(RFontFamily fontFamily)
        {
            _fontsHandler.AddFontFamily(fontFamily);
        }

        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered html and is not found in existing 
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            _fontsHandler.AddFontFamilyMapping(fromFamily, toFamily);
        }

        /// <summary>
        /// Loads one <c>url()</c> item of an <c>@font-face</c> <c>src</c> (CSS Fonts 4 §4.3.2) through
        /// <see cref="GetResourceStream"/> and registers it as a face of <paramref name="familyName"/>.
        /// </summary>
        /// <param name="familyName">the family name of the <c>@font-face</c> rule</param>
        /// <param name="uri">the absolute URI of the item</param>
        /// <param name="weight">numeric weight (1-1000)</param>
        /// <param name="isItalic">whether the face is italic or oblique</param>
        /// <param name="stretch">width on the 1-9 scale of <see cref="Core.Utils.FontStretchResolver"/></param>
        /// <param name="ranges">the <c>unicode-range</c>, or null for any character</param>
        /// <returns>false if the resource couldn't be loaded, the caller then tries the next item</returns>
        /// <remarks>
        /// Virtual for the PdfSharp backend, which needs the font bytes for embedding and keeps the faces
        /// in its own font resolver.
        /// </remarks>
        public virtual async Task<bool> AddFontFace(string familyName, RUri uri, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            RFontFamily fontFamily;
            if (!_fontFaceCache.TryGetValue(uri.AbsoluteUri, out fontFamily))
            {
                var networkResponse = await GetResourceStream(uri).ConfigureAwait(false);
                if (networkResponse == null || networkResponse.ResourceStream == null)
                {
                    return false;
                }

                byte[] fontBytes;
                using (var memoryStream = new MemoryStream())
                {
                    using (networkResponse.ResourceStream)
                    {
                        networkResponse.ResourceStream.CopyTo(memoryStream);
                    }
                    fontBytes = memoryStream.ToArray();
                }

                try
                {
                    fontFamily = LoadFontFaceFontInt(fontBytes, uri.AbsoluteUri);
                }
                catch
                {
                    return false;
                }

                if (fontFamily == null)
                {
                    return false;
                }

                _fontFaceCache[uri.AbsoluteUri] = fontFamily;
            }

            _fontsHandler.AddFontFace(familyName, fontFamily, weight, isItalic, stretch, ranges);
            return true;
        }

        /// <summary>
        /// Registers the family named by a <c>local()</c> item of an <c>@font-face</c> <c>src</c>
        /// (CSS Fonts 4 §4.3.3.1) as a face of <paramref name="familyName"/>. The name is matched against
        /// the registered families, not against full font names.
        /// </summary>
        /// <returns>false if there's no such family, the caller then tries the next item</returns>
        /// <remarks>Virtual for the PdfSharp backend, see <see cref="AddFontFace"/>.</remarks>
        public virtual bool AddFontFaceFromLocalFamily(string familyName, string localFamilyName, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            var localFamily = _fontsHandler.TryGetExistingFamily(localFamilyName);
            if (localFamily == null)
            {
                return false;
            }

            _fontsHandler.AddFontFace(familyName, localFamily, weight, isItalic, stretch, ranges);
            return true;
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        public RFont GetFont(string family, double size, RFontStyle style)
        {
            return _fontsHandler.GetCachedFont(family, size, style);
        }

        /// <summary>
        /// Get font instance by given font family name, size, style, and CSS Fonts Level 4 numeric
        /// weight/stretch - matches against any <c>@font-face</c> faces registered for <paramref name="family"/>
        /// (see <see cref="FontsHandler.GetCachedFont(string,double,RFontStyle,int,int,int?)"/>), falling
        /// back to the legacy family-name lookup when none are registered.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style (Italic/Underline/Strikeout are honored regardless of which face is chosen; Bold is superseded by <paramref name="weight"/>)</param>
        /// <param name="weight">CSS Fonts Level 4 numeric weight (1-1000)</param>
        /// <param name="stretch">CSS Fonts Level 3 stretch (1-9)</param>
        /// <param name="codepoint">the box's first non-whitespace character's codepoint, for <c>unicode-range</c> face disambiguation, or null to skip it</param>
        /// <returns>font instance, or null when <paramref name="codepoint"/> is given and no registered face covers it</returns>
        /// <remarks>
        /// Virtual so the PdfSharp backend can override it to bypass <see cref="FontsHandler"/>'s shared
        /// registry entirely, for the same reason as <see cref="AddFontFace"/> - see its doc comment.
        /// </remarks>
        public virtual RFont GetFont(string family, double size, RFontStyle style, int weight, int stretch, int? codepoint)
        {
            return _fontsHandler.GetCachedFont(family, size, style, weight, stretch, codepoint);
        }

        /// <summary>
        /// Get image to be used while HTML image is loading.
        /// </summary>
        public RImage GetLoadingImage()
        {
            if (_loadImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Core.Utils.ImageLoad.png");
                if (stream != null)
                    _loadImage = ImageFromStream(stream);
            }
            return _loadImage;
        }

        /// <summary>
        /// Get image to be used if HTML image load failed.
        /// </summary>
        public RImage GetLoadingFailedImage()
        {
            if (_errorImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Core.Utils.ImageError.png");
                if (stream != null)
                    _errorImage = ImageFromStream(stream);
            }
            return _errorImage;
        }

        /// <summary>
        /// Get data object for the given html and plain text data.<br />
        /// The data object can be used for clipboard or drag-drop operation.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        /// <returns>drag-drop data object</returns>
        public object GetClipboardDataObject(string html, string plainText)
        {
            return GetClipboardDataObjectInt(html, plainText);
        }

        /// <summary>
        /// Set the given text to the clipboard<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="text">the text to set</param>
        public void SetToClipboard(string text)
        {
            SetToClipboardInt(text);
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        public void SetToClipboard(string html, string plainText)
        {
            SetToClipboardInt(html, plainText);
        }

        /// <summary>
        /// Set the given image to clipboard.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="image">the image object to set to clipboard</param>
        public void SetToClipboard(RImage image)
        {
            SetToClipboardInt(image);
        }

        /// <summary>
        /// Create a context menu that can be used on the control<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <returns>new context menu</returns>
        public RContextMenu GetContextMenu()
        {
            return CreateContextMenuInt();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        public void SaveToFile(RImage image, string name, string extension, RControl control = null)
        {
            SaveToFileInt(image, name, extension, control);
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        internal RFont CreateFont(string family, double size, RFontStyle style)
        {
            return CreateFontInt(family, size, style);
        }

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        internal RFont CreateFont(RFontFamily family, double size, RFontStyle style)
        {
            return CreateFontInt(family, size, style);
        }


        #region Private/Protected methods

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        protected abstract RColor GetColorInt(string colorName);

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        protected abstract RPen CreatePen(RColor color);

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        protected abstract RBrush CreateSolidBrush(RColor color);

        /// <summary>
        /// Get a multi-stop linear gradient brush along the line from <paramref name="p1"/> to <paramref name="p2"/>.
        /// </summary>
        /// <param name="p1">the gradient line's start point</param>
        /// <param name="p2">the gradient line's end point</param>
        /// <param name="stops">color stops, each with a position in [0,1] along the gradient line</param>
        /// <returns>linear gradient color brush instance</returns>
        protected abstract RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops);

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="RImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        protected abstract RImage ConvertImageInt(object image);

        /// <summary>
        /// Create an <see cref="RImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        protected abstract RImage ImageFromStreamInt(Stream memoryStream);

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected abstract RFont CreateFontInt(string family, double size, RFontStyle style);

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected abstract RFont CreateFontInt(RFontFamily family, double size, RFontStyle style);

        /// <summary>
        /// Registers the bytes of an <c>@font-face</c> font file with the platform text engine and returns
        /// the family of the loaded face. Not called when <see cref="AddFontFace"/> is overridden.
        /// </summary>
        /// <param name="fontBytes">the font file (TTF/OTF)</param>
        /// <param name="filePath">where the bytes came from, for diagnostics</param>
        /// <returns>the family of the face, or null if the bytes aren't a font</returns>
        protected abstract RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath);

        /// <summary>
        /// Get data object for the given html and plain text data.<br />
        /// The data object can be used for clipboard or drag-drop operation.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        /// <returns>drag-drop data object</returns>
        protected virtual object GetClipboardDataObjectInt(string html, string plainText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        protected virtual void SetToClipboardInt(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        protected virtual void SetToClipboardInt(string html, string plainText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        protected virtual void SetToClipboardInt(RImage image)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        protected virtual RContextMenu CreateContextMenuInt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        protected virtual void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}