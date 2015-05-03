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

using PdfSharp;
using PdfSharp.Drawing;

namespace TheArtOfDev.HtmlRenderer.PdfSharp
{
    /// <summary>
    /// The settings for generating PDF using <see cref="PdfGenerator"/>
    /// </summary>
    public sealed class PdfGenerateConfig
    {
        #region Fields/Consts

        /// <summary>
        /// the page size to use for each page in the generated pdf
        /// </summary>
        private PageSize _pageSize;

        /// <summary>
        /// if the page size is undefined this allow you to set manually the page size
        /// </summary>
        private XSize _xsize;

        /// <summary>
        /// the orientation of each page of the generated pdf
        /// </summary>
        private PageOrientation _pageOrientation;

        /// <summary>
        /// the top margin between the page start and the text
        /// </summary>
        private int _marginTop;

        /// <summary>
        /// the bottom margin between the page end and the text
        /// </summary>
        private int _marginBottom;

        /// <summary>
        /// the left margin between the page start and the text
        /// </summary>
        private int _marginLeft;

        /// <summary>
        /// the right margin between the page end and the text
        /// </summary>
        private int _marginRight;

        #endregion


        /// <summary>
        /// the page size to use for each page in the generated pdf
        /// </summary>
        public PageSize PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        /// <summary>
        /// if the page size is undefined this allow you to set manually the page size
        /// </summary>
        public XSize ManualPageSize {
            get { return _xsize; }
            set { _xsize = value; }
        }

        /// <summary>
        /// the orientation of each page of the generated pdf
        /// </summary>
        public PageOrientation PageOrientation
        {
            get { return _pageOrientation; }
            set { _pageOrientation = value; }
        }

        /// <summary>
        /// the top margin between the page start and the text
        /// </summary>
        public int MarginTop
        {
            get { return _marginTop; }
            set
            {
                if (value > -1)
                    _marginTop = value;
            }
        }

        /// <summary>
        /// the bottom margin between the page end and the text
        /// </summary>
        public int MarginBottom
        {
            get { return _marginBottom; }
            set
            {
                if (value > -1)
                    _marginBottom = value;
            }
        }

        /// <summary>
        /// the left margin between the page start and the text
        /// </summary>
        public int MarginLeft
        {
            get { return _marginLeft; }
            set
            {
                if (value > -1)
                    _marginLeft = value;
            }
        }

        /// <summary>
        /// the right margin between the page end and the text
        /// </summary>
        public int MarginRight
        {
            get { return _marginRight; }
            set
            {
                if (value > -1)
                    _marginRight = value;
            }
        }

        /// <summary>
        /// Set all 4 margins to the given value.
        /// </summary>
        /// <param name="value"></param>
        public void SetMargins(int value)
        {
            if (value > -1)
                _marginBottom = _marginLeft = _marginTop = _marginRight = value;
        }

        // The international definitions are:
        //   1 inch == 25.4 mm
        //   1 inch == 72 point

        /// <summary>
        /// Convert the units passed in milimiters to the units used in PdfSharp
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static XSize MilimitersToUnits(double width, double height) {
            return new XSize(width / 25.4 * 72, height / 25.4 * 72);
        }

        /// <summary>
        /// Convert the units passed in inches to the units used in PdfSharp
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static XSize InchesToUnits(double width, double height) {
            return new XSize(width * 72, height * 72);
        }
    }
}