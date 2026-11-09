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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Handler for all loading image logic.<br/>
    /// <p>
    /// Loading by <see cref="HtmlImageLoadEventArgs"/>.<br/>
    /// Loading by file path.<br/>
    /// Loading by URI.<br/>
    /// </p>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports sync and async image loading.
    /// </para>
    /// <para>
    /// If the image object is created by the handler on calling dispose of the handler the image will be released, this
    /// makes release of unused images faster as they can be large.<br/>
    /// Disposing image load handler will also cancel download of image from the web.
    /// </para>
    /// </remarks>
    internal sealed class ImageLoadHandler : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// the container of the html to handle load image for
        /// </summary>
        private readonly HtmlContainerInt _htmlContainer;

        /// <summary>
        /// callback raised when image load process is complete with image or without
        /// </summary>
        private readonly ActionInt<RImage, RRect, bool> _loadCompleteCallback;

        /// <summary>
        /// the resource stream the image was decoded from; kept open as long as the image is in use
        /// </summary>
        private Stream _imageStream;

        /// <summary>
        /// the image instance of the loaded image
        /// </summary>
        private RImage _image;

        /// <summary>
        /// the image rectangle restriction as returned from image load event
        /// </summary>
        private RRect _imageRectangle;

        /// <summary>
        /// flag to indicate if to release the image object on box dispose (only if image was loaded by the box)
        /// </summary>
        private bool _releaseImageObject;

        /// <summary>
        /// is the handler has been disposed
        /// </summary>
        private bool _disposed;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load image for</param>
        /// <param name="loadCompleteCallback">callback raised when image load process is complete with image or without</param>
        public ImageLoadHandler(HtmlContainerInt htmlContainer, ActionInt<RImage, RRect, bool> loadCompleteCallback)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");
            ArgChecker.AssertArgNotNull(loadCompleteCallback, "loadCompleteCallback");

            _htmlContainer = htmlContainer;
            _loadCompleteCallback = loadCompleteCallback;
        }

        /// <summary>
        /// the image instance of the loaded image
        /// </summary>
        public RImage Image
        {
            get { return _image; }
        }

        /// <summary>
        /// the image rectangle restriction as returned from image load event
        /// </summary>
        public RRect Rectangle
        {
            get { return _imageRectangle; }
        }

        /// <summary>
        /// Set image of this image box by analyzing the src attribute.<br/>
        /// Load the image from inline base64 encoded string.<br/>
        /// Or from calling property/method on the bridge object that returns image or URL to image.<br/>
        /// Or from file path<br/>
        /// Or from URI.
        /// </summary>
        /// <remarks>
        /// HTML §4.8.4.3.5. A path or URI is resolved against the document base URL and fetched through
        /// <see cref="RAdapter.GetResourceStream"/>. With <see cref="HtmlContainerInt.AvoidAsyncImagesLoading"/>
        /// the fetch blocks until the image is loaded, otherwise it runs in the background and the
        /// completion callback requests a re-layout.
        /// </remarks>
        /// <param name="src">the source of the image to load</param>
        /// <param name="attributes">the collection of attributes on the element to use in event</param>
        public void LoadImage(string src, Dictionary<string, string> attributes)
        {
            try
            {
                var args = new HtmlImageLoadEventArgs(src, attributes, OnHtmlImageLoadEventCallback);
                _htmlContainer.RaiseHtmlImageLoadEvent(args);
                var async = !_htmlContainer.AvoidAsyncImagesLoading;

                if (!args.Handled)
                {
                    if (!string.IsNullOrEmpty(src))
                    {
                        if (async)
                        {
                            _ = LoadImageFromPathAsync(src, true);
                        }
                        else
                        {
                            LoadImageFromPathAsync(src, false).GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        ImageLoadComplete(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.Image, "Exception in handling image source", ex);
                ImageLoadComplete(false);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            ReleaseObjects();
        }


        #region Private methods

        /// <summary>
        /// Set the image using callback from load image event, use the given data.
        /// </summary>
        /// <param name="path">the path to the image to load (file path or uri)</param>
        /// <param name="image">the image to load</param>
        /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
        private void OnHtmlImageLoadEventCallback(string path, object image, RRect imageRectangle)
        {
            if (!_disposed)
            {
                _imageRectangle = imageRectangle;

                if (image != null)
                {
                    _image = _htmlContainer.Adapter.ConvertImage(image);
                    ImageLoadComplete(!_htmlContainer.AvoidAsyncImagesLoading);
                }
                else if (!string.IsNullOrEmpty(path))
                {
                    var async = !_htmlContainer.AvoidAsyncImagesLoading;
                    if (async)
                        _ = LoadImageFromPathAsync(path, true);
                    else
                        LoadImageFromPathAsync(path, false).GetAwaiter().GetResult();
                }
                else
                {
                    ImageLoadComplete(!_htmlContainer.AvoidAsyncImagesLoading);
                }
            }
        }

        /// <summary>
        /// Resolve <paramref name="path"/> (a file path, URI or <c>data:</c> URI) against the document base
        /// and fetch it through <see cref="RAdapter.GetResourceStream"/>.
        /// </summary>
        private async Task LoadImageFromPathAsync(string path, bool async)
        {
            try
            {
                var uri = CommonUtils.ResolveAgainstDocumentBase(_htmlContainer, path);
                if (uri == null)
                {
                    _htmlContainer.ReportError(HtmlRenderErrorType.Image, "Failed load image, invalid source: " + path);
                    ImageLoadComplete(async);
                    return;
                }

                var networkResponse = await _htmlContainer.Adapter.GetResourceStream(uri).ConfigureAwait(false);

                if (!_disposed && networkResponse != null && networkResponse.ResourceStream != null)
                {
                    _imageStream = networkResponse.ResourceStream;
                    _image = _htmlContainer.Adapter.ImageFromStream(_imageStream);
                    _releaseImageObject = true;
                }

                ImageLoadComplete(async);
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.Image, "Failed to load image from source: " + path, ex);
                ImageLoadComplete(async);
            }
        }

        /// <summary>
        /// Flag image load complete and request refresh for re-layout and invalidate.
        /// </summary>
        private void ImageLoadComplete(bool async = true)
        {
            // can happen if some operation return after the handler was disposed
            if (_disposed)
                ReleaseObjects();
            else
                _loadCompleteCallback(_image, _imageRectangle, async);
        }

        /// <summary>
        /// Release the image and client objects.
        /// </summary>
        private void ReleaseObjects()
        {
            if (_releaseImageObject && _image != null)
            {
                _image.Dispose();
                _image = null;
            }
            if (_imageStream != null)
            {
                _imageStream.Dispose();
                _imageStream = null;
            }
        }

        #endregion
    }
}
