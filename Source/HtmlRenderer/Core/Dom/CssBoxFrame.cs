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
using System.IO;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Network;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// CSS box for iframe element.<br/>
    /// If the iframe is of embedded YouTube or Vimeo video it will show image with play.
    /// </summary>
    internal sealed class CssBoxFrame : CssBox
    {
        #region Fields and Consts

        /// <summary>
        /// the image word of this image box
        /// </summary>
        private readonly CssRectImage _imageWord;

        /// <summary>
        /// is the iframe is of embeded video
        /// </summary>
        private readonly bool _isVideo;

        /// <summary>
        /// the title of the video
        /// </summary>
        private string _videoTitle;

        /// <summary>
        /// the url of the video thumbnail image
        /// </summary>
        private string _videoImageUrl;

        /// <summary>
        /// link to the video on the site
        /// </summary>
        private string _videoLinkUrl;

        /// <summary>
        /// handler used for image loading by source
        /// </summary>
        private ImageLoadHandler _imageLoadHandler;

        /// <summary>
        /// is image load is finished, used to know if no image is found
        /// </summary>
        private bool _imageLoadingComplete;

        /// <summary>
        /// the resolved YouTube/Vimeo oEmbed API URI to fetch, set in the constructor but not fetched
        /// until <see cref="MeasureWordsSize"/> - see its own doc comment for why the fetch can't start
        /// in the constructor.
        /// </summary>
        private RUri _videoApiUri;

        /// <summary>"YouTube" or "Vimeo", for <see cref="FetchVideoApiDataAsync"/>'s error reporting.</summary>
        private string _videoApiSource;

        /// <summary><see cref="OnDownloadYoutubeApiCompleted"/> or <see cref="OnDownloadVimeoApiCompleted"/>.</summary>
        private Action<string> _videoApiParseResult;

        /// <summary>Whether <see cref="FetchVideoApiDataAsync"/> has already been kicked off - guards <see cref="MeasureWordsSize"/> so it only starts once.</summary>
        private bool _videoDataRequested;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxFrame(CssBox parent, HtmlTag tag)
            : base(parent, tag)
        {
            _imageWord = new CssRectImage(this);
            Words.Add(_imageWord);

            Uri uri;
            if (Uri.TryCreate(GetAttribute("src"), UriKind.Absolute, out uri))
            {
                if (uri.Host.IndexOf("youtube.com", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    _isVideo = true;
                    _videoApiUri = new RUri(string.Format("https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={0}&format=json", uri.Segments[2]));
                    _videoApiSource = "YouTube";
                    _videoApiParseResult = OnDownloadYoutubeApiCompleted;
                }
                else if (uri.Host.IndexOf("vimeo.com", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    _isVideo = true;
                    _videoApiUri = new RUri(string.Format("https://vimeo.com/api/v2/video/{0}.json", uri.Segments[2]));
                    _videoApiSource = "Vimeo";
                    _videoApiParseResult = OnDownloadVimeoApiCompleted;
                }
            }

            if (!_isVideo)
            {
                SetErrorBorder();
            }
        }

        /// <summary>
        /// Is the css box clickable ("a" element is clickable)
        /// </summary>
        public override bool IsClickable
        {
            get { return true; }
        }

        /// <summary>
        /// Get the href link of the box (by default get "href" attribute)
        /// </summary>
        public override string HrefLink
        {
            get { return _videoLinkUrl ?? GetAttribute("src"); }
        }

        /// <summary>
        /// is the iframe is of embeded video
        /// </summary>
        public bool IsVideo
        {
            get { return _isVideo; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (_imageLoadHandler != null)
                _imageLoadHandler.Dispose();
            base.Dispose();
        }


        #region Private methods

        /// <summary>
        /// Fetch and read <paramref name="apiUri"/> as text, fire-and-forget (matching
        /// <see cref="ImageLoadHandler"/>'s pattern - nothing here blocks layout/paint), and hand the
        /// result (or null on failure) to <paramref name="parseResult"/>. A null/unresolvable response is
        /// reported the same way as any other "video not found" outcome - <see cref="RNetworkResponse"/>
        /// has no HTTP status code to distinguish a 404 from another failure, matching every other
        /// resource kind's fail-soft contract through this funnel.
        /// </summary>
        private async Task FetchVideoApiDataAsync(RUri apiUri, string source, Action<string> parseResult)
        {
            try
            {
                var networkResponse = await HtmlContainer.Adapter.GetResourceStream(apiUri).ConfigureAwait(false);

                if (networkResponse == null || networkResponse.ResourceStream == null)
                {
                    _videoTitle = "The video is not found, possibly removed by the user.";
                }
                else
                {
                    string result;
                    using (var reader = new StreamReader(networkResponse.ResourceStream))
                    {
                        result = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }

                    parseResult(result);
                }
            }
            catch (Exception ex)
            {
                HtmlContainer.ReportError(HtmlRenderErrorType.Iframe, "Failed to get " + source + " video data: " + apiUri.AbsoluteUri, ex);
            }

            HandlePostApiCall();
        }

        /// <summary>
        /// Parse YouTube API response to get video data (title, image, link).
        /// </summary>
        private void OnDownloadYoutubeApiCompleted(string result)
        {
            try
            {
                var idx = result.IndexOf("\"title\"", StringComparison.Ordinal);
                if (idx > -1)
                {
                    idx = result.IndexOf('"', idx + 7);
                    if (idx > -1)
                    {
                        var endIdx = result.IndexOf('"', idx + 1);
                        while (endIdx > 0 && result[endIdx - 1] == '\\')
                            endIdx = result.IndexOf('"', endIdx + 1);
                        if (endIdx > -1)
                        {
                            _videoTitle = result.Substring(idx + 1, endIdx - idx - 1).Replace("\\\"", "\"");
                        }
                    }
                }

                idx = result.IndexOf("\"thumbnail_url\"", StringComparison.Ordinal);
                if (idx > -1)
                {
                    idx = result.IndexOf('"', idx + 15);
                    if (idx > -1)
                    {
                        var endIdx = result.IndexOf('"', idx + 1);
                        while (endIdx > 0 && result[endIdx - 1] == '\\')
                            endIdx = result.IndexOf('"', endIdx + 1);
                        if (endIdx > -1)
                        {
                            _videoImageUrl = result.Substring(idx + 1, endIdx - idx - 1).Replace("\\\"", "\"");
                        }
                    }

                    idx = result.IndexOf("\"thumbnail_width\"", StringComparison.Ordinal);
                    if (idx > -1)
                    {
                        idx = result.IndexOf(':', idx);
                        if (idx > -1)
                        {
                            var endIdx = result.IndexOf(',', idx);
                            if (endIdx > -1)
                            {
                                var widthStr = result.Substring(idx + 1, endIdx - idx - 1).Trim();
                                if (int.TryParse(widthStr, out int width))
                                {
                                    if (string.IsNullOrEmpty(Width))
                                        Width = width + "px";
                                }
                            }
                        }
                    }

                    idx = result.IndexOf("\"thumbnail_height\"", StringComparison.Ordinal);
                    if (idx > -1)
                    {
                        idx = result.IndexOf(':', idx);
                        if (idx > -1)
                        {
                            var endIdx = result.IndexOf(',', idx);
                            if (endIdx == -1)
                                endIdx = result.IndexOf('}', idx);
                            if (endIdx > -1)
                            {
                                var heightStr = result.Substring(idx + 1, endIdx - idx - 1).Trim();
                                if (int.TryParse(heightStr, out int height))
                                {
                                    if (string.IsNullOrEmpty(Height))
                                        Height = height + "px";
                                }
                            }
                        }
                    }
                }

                idx = result.IndexOf("\"html\"", StringComparison.Ordinal);
                if (idx > -1)
                {
                    idx = result.IndexOf("src=", idx);
                    if (idx > -1)
                    {
                        idx = result.IndexOf("embed/", idx);
                        if (idx > -1)
                        {
                            var endIdx = result.IndexOf('?', idx);
                            if (endIdx > -1)
                            {
                                var videoId = result.Substring(idx + 6, endIdx - idx - 6);
                                _videoLinkUrl = "https://www.youtube.com/watch?v=" + videoId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HtmlContainer.ReportError(HtmlRenderErrorType.Iframe, "Failed to parse YouTube video response", ex);
            }
        }

        /// <summary>
        /// Parse Vimeo API response to get video data (title, image, link).
        /// </summary>
        private void OnDownloadVimeoApiCompleted(string result)
        {
            try
            {
                var idx = result.IndexOf("\"title\"", StringComparison.Ordinal);
                if (idx > -1)
                {
                    idx = result.IndexOf('"', idx + 7);
                    if (idx > -1)
                    {
                        var endIdx = result.IndexOf('"', idx + 1);
                        while (endIdx > 0 && result[endIdx - 1] == '\\')
                            endIdx = result.IndexOf('"', endIdx + 1);
                        if (endIdx > -1)
                        {
                            _videoTitle = result.Substring(idx + 1, endIdx - idx - 1).Replace("\\\"", "\"");
                        }
                    }
                }

                idx = result.IndexOf("\"thumbnail_large\"", StringComparison.Ordinal);
                if (idx > -1)
                {
                    if (string.IsNullOrEmpty(Width))
                        Width = "640";
                    if (string.IsNullOrEmpty(Height))
                        Height = "360";
                    var urlIdx = result.IndexOf("\"https:\\/\\/", idx);
                    if (urlIdx != -1)
                        idx = urlIdx;
                }
                else
                {
                    idx = result.IndexOf("\"thumbnail_medium\"", StringComparison.Ordinal);
                    if (idx > -1)
                    {
                        if (string.IsNullOrEmpty(Width))
                            Width = "200";
                        if (string.IsNullOrEmpty(Height))
                            Height = "150";
                        var urlIdx = result.IndexOf("\"https:\\/\\/", idx);
                        if (urlIdx != -1)
                            idx = urlIdx;
                    }
                    else
                    {
                        idx = result.IndexOf("\"thumbnail_small\"", StringComparison.Ordinal);
                        if (idx > -1)
                        {
                            if (string.IsNullOrEmpty(Width))
                                Width = "100";
                            if (string.IsNullOrEmpty(Height))
                                Height = "75";
                            var urlIdx = result.IndexOf("\"https:\\/\\/", idx);
                            if (urlIdx != -1)
                                idx = urlIdx;
                        }
                    }
                }
                if (idx > -1)
                {
                    idx = idx + 1;
                    var endIdx = result.IndexOf('"', idx);
                    if (endIdx > -1)
                    {
                        _videoImageUrl = result.Substring(idx, endIdx - idx).Replace("\\/", "/");
                    }
                }

                idx = result.IndexOf("\"url\"", StringComparison.Ordinal);
                if (idx > -1)
                {
                    idx = result.IndexOf("\"https:\\/\\/", idx);
                    if (idx > -1)
                    {
                        idx = idx + 1;
                        var endIdx = result.IndexOf('"', idx);
                        if (endIdx > -1)
                        {
                            _videoLinkUrl = result.Substring(idx, endIdx - idx).Replace("\\/", "/");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HtmlContainer.ReportError(HtmlRenderErrorType.Iframe, "Failed to parse Vimeo video response", ex);
            }
        }

        /// <summary>
        /// Update error-border state once the video API call has finished (success, failure, or parse
        /// error) and request a refresh.
        /// </summary>
        private void HandlePostApiCall()
        {
            if (_videoImageUrl == null)
            {
                _imageLoadingComplete = true;
                SetErrorBorder();
            }

            HtmlContainer.RequestRefresh(IsLayoutRequired());
        }

        /// <summary>
        /// Starts loading the video thumbnail if the video API call resolved a thumbnail URL and loading
        /// hasn't started already - the same paint-time trigger pattern as <see cref="CssBoxImage"/>, see
        /// its <see cref="CssBoxImage.EnsureImageLoadStarted"/> for why this can't move to measure time.
        /// Called by <see cref="Paint.Content.FrameFragmentPainter"/>.
        /// </summary>
        internal void EnsureVideoImageLoadStarted()
        {
            if (_videoImageUrl != null && _imageLoadHandler == null)
            {
                _imageLoadHandler = new ImageLoadHandler(HtmlContainer, OnLoadImageComplete);
                _imageLoadHandler.LoadImage(_videoImageUrl, HtmlTag != null ? HtmlTag.Attributes : null);
            }
        }

        /// <summary>
        /// Draws the video thumbnail/title/play-button chrome at <paramref name="offset"/>, leaving
        /// background/border painting to the caller (<see cref="Paint.Content.FrameFragmentPainter"/>).
        /// </summary>
        internal void DrawFrameContent(RGraphics g, RPoint offset)
        {
            var word = Words[0];
            var tmpRect = word.Rectangle;
            tmpRect.Offset(offset);
            tmpRect.Height -= ActualBorderTopWidth + ActualBorderBottomWidth + ActualPaddingTop + ActualPaddingBottom;
            tmpRect.Y += ActualBorderTopWidth + ActualPaddingTop;
            tmpRect.X = Math.Floor(tmpRect.X);
            tmpRect.Y = Math.Floor(tmpRect.Y);
            var rect = tmpRect;

            DrawImage(g, offset, rect);

            DrawTitle(g, rect);

            DrawPlay(g, rect);
        }

        /// <summary>
        /// Draw video image over the iframe if found.
        /// </summary>
        private void DrawImage(RGraphics g, RPoint offset, RRect rect)
        {
            if (_imageWord.Image != null)
            {
                if (rect.Width > 0 && rect.Height > 0)
                {
                    if (_imageWord.ImageRectangle == RRect.Empty)
                        g.DrawImage(_imageWord.Image, rect);
                    else
                        g.DrawImage(_imageWord.Image, rect, _imageWord.ImageRectangle);

                    if (_imageWord.Selected)
                    {
                        g.DrawRectangle(GetSelectionBackBrush(g, true), _imageWord.Left + offset.X, _imageWord.Top + offset.Y, _imageWord.Width + 2, DomUtils.GetCssLineBoxByWord(_imageWord).LineHeight);
                    }
                }
            }
            else if (_isVideo && !_imageLoadingComplete)
            {
                RenderUtils.DrawImageLoadingIcon(g, HtmlContainer, rect);
                if (rect.Width > 19 && rect.Height > 19)
                {
                    g.DrawRectangle(g.GetPen(RColor.LightGray), rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
        }

        /// <summary>
        /// Draw video title on top of the iframe if found.
        /// </summary>
        private void DrawTitle(RGraphics g, RRect rect)
        {
            if (_videoTitle != null && _imageWord.Width > 40 && _imageWord.Height > 40)
            {
                var font = HtmlContainer.Adapter.GetFont("Arial", 9f, RFontStyle.Regular);
                g.DrawRectangle(g.GetSolidBrush(RColor.FromArgb(160, 0, 0, 0)), rect.Left, rect.Top, rect.Width, ActualFont.Height + 7);

                var titleRect = new RRect(rect.Left + 3, rect.Top + 3, rect.Width - 6, rect.Height - 6);
                g.DrawString(_videoTitle, font, RColor.WhiteSmoke, titleRect.Location, RSize.Empty, false);
            }
        }

        /// <summary>
        /// Draw play over the iframe if we found link url.
        /// </summary>
        private void DrawPlay(RGraphics g, RRect rect)
        {
            if (_isVideo && _imageWord.Width > 70 && _imageWord.Height > 50)
            {
                var prevMode = g.SetAntiAliasSmoothingMode();

                var size = new RSize(60, 40);
                var left = rect.Left + (rect.Width - size.Width) / 2;
                var top = rect.Top + (rect.Height - size.Height) / 2;
                g.DrawRectangle(g.GetSolidBrush(RColor.FromArgb(160, 0, 0, 0)), left, top, size.Width, size.Height);

                RPoint[] points =
                {
                    new RPoint(left + size.Width / 3f + 1,top + 3 * size.Height / 4f),
                    new RPoint(left + size.Width / 3f + 1, top + size.Height / 4f),
                    new RPoint(left + 2 * size.Width / 3f + 1, top + size.Height / 2f)
                };
                g.DrawPolygon(g.GetSolidBrush(RColor.White), points);
                
                g.ReturnPreviousSmoothingMode(prevMode);
            }
        }

        /// <summary>
        /// Assigns words its width and height
        /// </summary>
        /// <param name="g">the device to use</param>
        internal override void MeasureWordsSize(RGraphics g)
        {
            // Kicks off the YouTube/Vimeo oEmbed API fetch (through the configured
            // RAdapter.NetworkLoader - the same funnel used for images/stylesheets/fonts, so a consumer's
            // custom loader applies here too) here rather than from the constructor: HtmlContainer walks
            // up to the root box's own HtmlContainer, which DomParser.GenerateCssTree only assigns AFTER
            // the whole box tree (including this one) has already been constructed - so at construction
            // time it's always null. FetchVideoApiDataAsync dereferences it immediately (before its first
            // genuine await), so starting the fetch from the constructor throws a NullReferenceException
            // synchronously out of box-tree construction itself. MeasureWordsSize runs during layout, well
            // after HtmlContainer is assigned, so it's a safe place to start this instead.
            if (_isVideo && !_videoDataRequested)
            {
                _videoDataRequested = true;
                _ = FetchVideoApiDataAsync(_videoApiUri, _videoApiSource, _videoApiParseResult);
            }

            if (!_wordsSizeMeasured)
            {
                MeasureWordSpacing(g);
                _wordsSizeMeasured = true;
            }
            CssLayoutEngine.MeasureImageSize(_imageWord);
        }

        /// <summary>
        /// Set error image border on the image box.
        /// </summary>
        private void SetErrorBorder()
        {
            SetAllBorders(CssConstants.Solid, "2px", "#A0A0A0");
            BorderRightColor = BorderBottomColor = "#E3E3E3";
        }

        /// <summary>
        /// On image load process is complete with image or without update the image box.
        /// </summary>
        /// <param name="image">the image loaded or null if failed</param>
        /// <param name="rectangle">the source rectangle to draw in the image (empty - draw everything)</param>
        /// <param name="async">is the callback was called async to load image call</param>
        private void OnLoadImageComplete(RImage image, RRect rectangle, bool async)
        {
            _imageWord.Image = image;
            _imageWord.ImageRectangle = rectangle;
            _imageLoadingComplete = true;
            _wordsSizeMeasured = false;

            if (_imageLoadingComplete && image == null)
            {
                SetErrorBorder();
            }

            if (async)
            {
                HtmlContainer.RequestRefresh(IsLayoutRequired());
            }
        }

        private bool IsLayoutRequired()
        {
            var width = new CssLength(Width);
            var height = new CssLength(Height);
            return (width.Number <= 0 || width.Unit != CssUnit.Pixels) || (height.Number <= 0 || height.Unit != CssUnit.Pixels);
        }

        #endregion
    }
}