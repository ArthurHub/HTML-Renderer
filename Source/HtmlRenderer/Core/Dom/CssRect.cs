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

using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Represents a word inside an inline box
    /// </summary>
    /// <remarks>
    /// Because of performance, words of text are the most atomic 
    /// element in the project. It should be characters, but come on,
    /// imagine the performance when drawing char by char on the device.<br/>
    /// It may change for future versions of the library.
    /// </remarks>
    internal abstract class CssRect
    {
        #region Fields and Consts

        /// <summary>
        /// the CSS box owner of the word
        /// </summary>
        private readonly CssBox _ownerBox;

        /// <summary>
        /// Rectangle
        /// </summary>
        private RRect _rect;

        /// <summary>
        /// If the word is selected this points to the selection handler for more data
        /// </summary>
        private SelectionHandler _selection;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="owner">the CSS box owner of the word</param>
        protected CssRect(CssBox owner)
        {
            _ownerBox = owner;
        }

        /// <summary>
        /// Gets the Box where this word belongs.
        /// </summary>
        public CssBox OwnerBox
        {
            get { return _ownerBox; }
        }

        /// <summary>
        /// Gets or sets the bounds of the rectangle
        /// </summary>
        public RRect Rectangle
        {
            get { return _rect; }
            set { _rect = value; }
        }

        /// <summary>
        /// Left of the rectangle
        /// </summary>
        public double Left
        {
            get { return _rect.X; }
            set { _rect.X = value; }
        }

        /// <summary>
        /// Top of the rectangle
        /// </summary>
        public double Top
        {
            get { return _rect.Y; }
            set { _rect.Y = value; }
        }

        /// <summary>
        /// Width of the rectangle
        /// </summary>
        public double Width
        {
            get { return _rect.Width; }
            set { _rect.Width = value; }
        }

        /// <summary>
        /// Get the full width of the word including the spacing.
        /// </summary>
        public double FullWidth
        {
            get { return _rect.Width + ActualWordSpacing; }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public double ActualWordSpacing
        {
            get { return (OwnerBox != null ? (HasSpaceAfter ? OwnerBox.ActualWordSpacing : 0) + (IsImage ? OwnerBox.ActualWordSpacing : 0) : 0); }
        }

        /// <summary>
        /// Height of the rectangle
        /// </summary>
        public double Height
        {
            get { return _rect.Height; }
            set { _rect.Height = value; }
        }

        /// <summary>
        /// Gets or sets the right of the rectangle. When setting, it only affects the Width of the rectangle.
        /// </summary>
        public double Right
        {
            get { return Rectangle.Right; }
            set { Width = value - Left; }
        }

        /// <summary>
        /// Gets or sets the bottom of the rectangle. When setting, it only affects the Height of the rectangle.
        /// </summary>
        public double Bottom
        {
            get { return Rectangle.Bottom; }
            set { Height = value - Top; }
        }

        /// <summary>
        /// If the word is selected this points to the selection handler for more data
        /// </summary>
        public SelectionHandler Selection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        /// <summary>
        /// was there a whitespace before the word chars (before trim)
        /// </summary>
        public virtual bool HasSpaceBefore
        {
            get { return false; }
        }

        /// <summary>
        /// was there a whitespace after the word chars (before trim)
        /// </summary>
        public virtual bool HasSpaceAfter
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the image this words represents (if one exists)
        /// </summary>
        public virtual RImage Image
        {
            get { return null; }
            // ReSharper disable ValueParameterNotUsed
            set { }
            // ReSharper restore ValueParameterNotUsed
        }

        /// <summary>
        /// Gets if the word represents an image.
        /// </summary>
        public virtual bool IsImage
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a bool indicating if this word is composed only by spaces.
        /// Spaces include tabs and line breaks
        /// </summary>
        public virtual bool IsSpaces
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if the word is composed by only a line break
        /// </summary>
        public virtual bool IsLineBreak
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the text of the word
        /// </summary>
        public virtual string Text
        {
            get { return null; }
        }

        /// <summary>
        /// is the word is currently selected
        /// </summary>
        public bool Selected
        {
            get { return _selection != null; }
        }

        /// <summary>
        /// the selection start index if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public int SelectedStartIndex
        {
            get { return _selection != null ? _selection.GetSelectingStartIndex(this) : -1; }
        }

        /// <summary>
        /// the selection end index if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public int SelectedEndIndexOffset
        {
            get { return _selection != null ? _selection.GetSelectedEndIndexOffset(this) : -1; }
        }

        /// <summary>
        /// the selection start offset if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public double SelectedStartOffset
        {
            get { return _selection != null ? _selection.GetSelectedStartOffset(this) : -1; }
        }

        /// <summary>
        /// the selection end offset if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public double SelectedEndOffset
        {
            get { return _selection != null ? _selection.GetSelectedEndOffset(this) : -1; }
        }

        /// <summary>
        /// Gets or sets an offset to be considered in measurements
        /// </summary>
        internal double LeftGlyphPadding
        {
            get { return OwnerBox != null ? OwnerBox.ActualFont.LeftPadding : 0; }
        }

        /// <summary>
        /// Represents this word for debugging purposes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ({1} char{2})", Text.Replace(' ', '-').Replace("\n", "\\n"), Text.Length, Text.Length != 1 ? "s" : string.Empty);
        }
    }
}