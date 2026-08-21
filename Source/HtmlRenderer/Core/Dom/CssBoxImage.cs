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
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// CSS box for image element.
    /// </summary>
    internal sealed class CssBoxImage : CssBox
    {
        #region Fields and Consts

        /// <summary>
        /// the image word of this image box
        /// </summary>
        private readonly CssRectImage _imageWord;

        /// <summary>
        /// handler used for image loading by source
        /// </summary>
        private ImageLoadHandler _imageLoadHandler;

        /// <summary>
        /// is image load is finished, used to know if no image is found
        /// </summary>
        private bool _imageLoadingComplete;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxImage(CssBox parent, HtmlTag tag)
            : base(parent, tag)
        {
            _imageWord = new CssRectImage(this);
            Words.Add(_imageWord);
        }

        /// <summary>
        /// Get the image of this image box.
        /// </summary>
        public RImage Image
        {
            get { return _imageWord.Image; }
        }

        /// <summary>
        /// Paints the fragment
        /// </summary>
        /// <param name="g">the device to draw to</param>
        protected override void PaintImp(RGraphics g)
        {
            EnsureImageLoadStarted();

            var rect = CommonUtils.GetFirstValueOrDefault(Rectangles);
            RPoint offset = RPoint.Empty;

            if (!IsFixed)
                offset = HtmlContainer.ScrollOffset;

            rect.Offset(offset);

            var clipped = RenderUtils.ClipGraphicsByOverflow(g, this);

            PaintBackground(g, rect, true, true);
            BordersDrawHandler.DrawBoxBorders(g, this, rect, true, true);

            DrawImageContent(g, offset);

            if (clipped)
                g.PopClip();
        }

        /// <summary>
        /// Starts loading the image if it hasn't started already. This is the primary load trigger for
        /// the common async case (<see cref="HtmlContainerInt.AvoidAsyncImagesLoading"/>/
        /// <see cref="HtmlContainerInt.AvoidImagesLateLoading"/> both false) - <see cref="MeasureWordsSize"/>
        /// only starts loading when one of those flags is set, so paint is where loading normally begins.
        /// Shared by <see cref="PaintImp"/> and <see cref="Paint.Content.ImageFragmentPainter"/>.
        /// </summary>
        internal void EnsureImageLoadStarted()
        {
            if (_imageLoadHandler == null)
            {
                _imageLoadHandler = new ImageLoadHandler(HtmlContainer, OnLoadImageComplete);
                _imageLoadHandler.LoadImage(GetImageSource(), HtmlTag != null ? HtmlTag.Attributes : null);
            }
        }

        /// <summary>
        /// Draws the image itself (or its error/loading placeholder) at <paramref name="offset"/> - the
        /// part of <see cref="PaintImp"/> specific to this box's own image word, as opposed to the
        /// generic background/border painting shared with every other replaced element.
        /// </summary>
        internal void DrawImageContent(RGraphics g, RPoint offset)
        {
            RRect r = _imageWord.Rectangle;
            r.Offset(offset);
            r.Height -= ActualBorderTopWidth + ActualBorderBottomWidth + ActualPaddingTop + ActualPaddingBottom;
            r.Y += ActualBorderTopWidth + ActualPaddingTop;
            r.X = Math.Floor(r.X);
            r.Y = Math.Floor(r.Y);

            if (_imageWord.Image != null)
            {
                if (r.Width > 0 && r.Height > 0)
                {
                    if (_imageWord.ImageRectangle == RRect.Empty)
                        g.DrawImage(_imageWord.Image, r);
                    else
                        g.DrawImage(_imageWord.Image, r, _imageWord.ImageRectangle);

                    if (_imageWord.Selected)
                    {
                        g.DrawRectangle(GetSelectionBackBrush(g, true), _imageWord.Left + offset.X, _imageWord.Top + offset.Y, _imageWord.Width + 2, DomUtils.GetCssLineBoxByWord(_imageWord).LineHeight);
                    }
                }
            }
            else if (_imageLoadingComplete)
            {
                if (_imageLoadingComplete && r.Width > 19 && r.Height > 19)
                {
                    RenderUtils.DrawImageErrorIcon(g, HtmlContainer, r);
                }
            }
            else
            {
                RenderUtils.DrawImageLoadingIcon(g, HtmlContainer, r);
                if (r.Width > 19 && r.Height > 19)
                {
                    g.DrawRectangle(g.GetPen(RColor.LightGray), r.X, r.Y, r.Width, r.Height);
                }
            }
        }

        /// <summary>
        /// Assigns words its width and height
        /// </summary>
        /// <param name="g">the device to use</param>
        internal override void MeasureWordsSize(RGraphics g)
        {
            if (!_wordsSizeMeasured)
            {
                if (_imageLoadHandler == null && (HtmlContainer.AvoidAsyncImagesLoading || HtmlContainer.AvoidImagesLateLoading))
                {
                    _imageLoadHandler = new ImageLoadHandler(HtmlContainer, OnLoadImageComplete);
                    _imageLoadHandler.LoadImage(GetImageSource(), HtmlTag != null ? HtmlTag.Attributes : null);
                }

                MeasureWordSpacing(g);
                _wordsSizeMeasured = true;
            }

            CssLayoutEngine.MeasureImageSize(_imageWord);
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
        /// Get the image source to load: a resolved <c>content: url(...)</c> value takes precedence
        /// over the "src" attribute (CSS generated content replacing the element's own rendering, same
        /// as this engine already gives generated content priority for pseudo-elements), falling back
        /// to "src" when content is unset/none/normal or isn't a url().<br/>
        /// <see cref="CssBox.Content"/> holds the literal declared CSS text (e.g. <c>url('data:...')</c>,
        /// quotes and all) rather than an unwrapped source string, so the url() token's data has to be
        /// pulled out here - <see cref="ImageLoadHandler.LoadImage"/> only recognizes a bare path or
        /// "data:image..." URI, not CSS function syntax.
        /// </summary>
        private string GetImageSource()
        {
            if (!string.IsNullOrEmpty(Content) && Content != CssConstants.Normal && Content != CssConstants.None)
            {
                var token = CssValueParser.GetCssTokens(Content).FirstOrDefault();
                if (token is UrlToken urlToken)
                    return urlToken.Data;
            }

            return GetAttribute("src");
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

            if (!HtmlContainer.AvoidImagesLateLoading || async)
            {
                var width = new CssLength(Width);
                var height = new CssLength(Height);
                var layout = (width.Number <= 0 || width.Unit != CssUnit.Pixels) || (height.Number <= 0 || height.Unit != CssUnit.Pixels);
                HtmlContainer.RequestRefresh(layout);
            }
        }

        #endregion
    }
}