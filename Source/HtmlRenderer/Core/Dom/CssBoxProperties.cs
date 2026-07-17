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
using System.Globalization;
using System.Text.RegularExpressions;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Base class for css box to handle the css properties.<br/>
    /// Has field and property for every css property that can be set, the properties add additional parsing like
    /// setting the correct border depending what border value was set (single, two , all four).<br/>
    /// Has additional fields to control the location and size of the box and 'actual' css values for some properties
    /// that require additional calculations and parsing.<br/>
    /// </summary>
    internal abstract class CssBoxProperties
    {
        #region CSS Fields

        private string _backgroundColor = "transparent";
        private string _backgroundImage = "none";
        private string _backgroundPosition = "0% 0%";
        private string _backgroundRepeat = "repeat";
        private string _borderTopWidth = "medium";
        private string _borderRightWidth = "medium";
        private string _borderBottomWidth = "medium";
        private string _borderLeftWidth = "medium";
        private string _borderTopColor = "black";
        private string _borderRightColor = "black";
        private string _borderBottomColor = "black";
        private string _borderLeftColor = "black";
        private string _borderTopStyle = "none";
        private string _borderRightStyle = "none";
        private string _borderBottomStyle = "none";
        private string _borderLeftStyle = "none";
        private string _borderSpacing = "0";
        private string _borderCollapse = "separate";
        private string _bottom;
        private string _color = "black";
        private string _content = "normal";
        private string _borderTopLeftRadius = "0 0";
        private string _borderTopRightRadius = "0 0";
        private string _borderBottomRightRadius = "0 0";
        private string _borderBottomLeftRadius = "0 0";
        private string _emptyCells = "show";
        private string _direction = "ltr";
        private string _display = "inline";
        private string _fontFamily;
        private string _fontSize = "medium";
        private string _fontStyle = "normal";
        private string _fontVariant = "normal";
        private string _fontWeight = "normal";
        private string _float = "none";
        private string _height = "auto";
        private string _marginBottom = "0";
        private string _marginLeft = "0";
        private string _marginRight = "0";
        private string _marginTop = "0";
        private string _left = "auto";
        private string _lineHeight = "normal";
        private string _listStyleType = "disc";
        private string _listStyleImage = string.Empty;
        private string _listStylePosition = "outside";
        private string _listStyle = string.Empty;
        private string _overflow = "visible";
        private string _paddingLeft = "0";
        private string _paddingBottom = "0";
        private string _paddingRight = "0";
        private string _paddingTop = "0";
        private string _pageBreakInside = CssConstants.Auto;
        private string _right;
        private string _textAlign = string.Empty;
        private string _textDecoration = string.Empty;
        private string _textIndent = "0";
        private string _top = "auto";
        private string _position = "static";
        private string _verticalAlign = "baseline";
        private string _width = "auto";
        private string _maxWidth = "none";
        private string _wordSpacing = "normal";
        private string _wordBreak = "normal";
        private string _whiteSpace = "normal";
        private string _visibility = "visible";

        /// <summary>
        /// Specified (not var()-resolved) values of this box's CSS custom properties (--foo), keyed by
        /// their case-sensitive name. Lazily created; null when no custom property has been declared or inherited.
        /// </summary>
        internal Dictionary<string, string> CustomProperties;

        #endregion


        #region Fields

        /// <summary>
        /// Gets or sets the location of the box
        /// </summary>
        private RPoint _location;

        /// <summary>
        /// Gets or sets the size of the box
        /// </summary>
        private RSize _size;

        private double _actualBorderTopLeftRadiusX = double.NaN;
        private double _actualBorderTopLeftRadiusY = double.NaN;
        private double _actualBorderTopRightRadiusX = double.NaN;
        private double _actualBorderTopRightRadiusY = double.NaN;
        private double _actualBorderBottomRightRadiusX = double.NaN;
        private double _actualBorderBottomRightRadiusY = double.NaN;
        private double _actualBorderBottomLeftRadiusX = double.NaN;
        private double _actualBorderBottomLeftRadiusY = double.NaN;
        private RColor _actualColor = RColor.Empty;
        private double _actualHeight = double.NaN;
        private double _actualWidth = double.NaN;
        private double _actualPaddingTop = double.NaN;
        private double _actualPaddingBottom = double.NaN;
        private double _actualPaddingRight = double.NaN;
        private double _actualPaddingLeft = double.NaN;
        private double _actualMarginTop = double.NaN;
        private double _collapsedMarginTop = double.NaN;
        private double _actualMarginBottom = double.NaN;
        private double _actualMarginRight = double.NaN;
        private double _actualMarginLeft = double.NaN;
        private double _actualBorderTopWidth = double.NaN;
        private double _actualBorderLeftWidth = double.NaN;
        private double _actualBorderBottomWidth = double.NaN;
        private double _actualBorderRightWidth = double.NaN;

        /// <summary>
        /// the width of whitespace between words
        /// </summary>
        private double _actualLineHeight = double.NaN;

        private double _actualWordSpacing = double.NaN;
        private double _actualTextIndent = double.NaN;
        private double _actualBorderSpacingHorizontal = double.NaN;
        private double _actualBorderSpacingVertical = double.NaN;
        private bool _actualBackgroundImageComputed;
        private CssImage _actualBackgroundImage;
        private RColor _actualBorderTopColor = RColor.Empty;
        private RColor _actualBorderLeftColor = RColor.Empty;
        private RColor _actualBorderBottomColor = RColor.Empty;
        private RColor _actualBorderRightColor = RColor.Empty;
        private RColor _actualBackgroundColor = RColor.Empty;
        private RFont _actualFont;

        #endregion


        #region CSS Properties

        public string BorderBottomWidth
        {
            get { return _borderBottomWidth; }
            set
            {
                _borderBottomWidth = value;
                _actualBorderBottomWidth = Single.NaN;
            }
        }

        public string BorderLeftWidth
        {
            get { return _borderLeftWidth; }
            set
            {
                _borderLeftWidth = value;
                _actualBorderLeftWidth = Single.NaN;
            }
        }

        public string BorderRightWidth
        {
            get { return _borderRightWidth; }
            set
            {
                _borderRightWidth = value;
                _actualBorderRightWidth = Single.NaN;
            }
        }

        public string BorderTopWidth
        {
            get { return _borderTopWidth; }
            set
            {
                _borderTopWidth = value;
                _actualBorderTopWidth = Single.NaN;
            }
        }

        public string BorderBottomStyle
        {
            get { return _borderBottomStyle; }
            set { _borderBottomStyle = value; }
        }

        public string BorderLeftStyle
        {
            get { return _borderLeftStyle; }
            set { _borderLeftStyle = value; }
        }

        public string BorderRightStyle
        {
            get { return _borderRightStyle; }
            set { _borderRightStyle = value; }
        }

        public string BorderTopStyle
        {
            get { return _borderTopStyle; }
            set { _borderTopStyle = value; }
        }

        public string BorderBottomColor
        {
            get { return _borderBottomColor; }
            set
            {
                _borderBottomColor = value;
                _actualBorderBottomColor = RColor.Empty;
            }
        }

        public string BorderLeftColor
        {
            get { return _borderLeftColor; }
            set
            {
                _borderLeftColor = value;
                _actualBorderLeftColor = RColor.Empty;
            }
        }

        public string BorderRightColor
        {
            get { return _borderRightColor; }
            set
            {
                _borderRightColor = value;
                _actualBorderRightColor = RColor.Empty;
            }
        }

        public string BorderTopColor
        {
            get { return _borderTopColor; }
            set
            {
                _borderTopColor = value;
                _actualBorderTopColor = RColor.Empty;
            }
        }

        public string BorderSpacing
        {
            get { return _borderSpacing; }
            set { _borderSpacing = value; }
        }

        public string BorderCollapse
        {
            get { return _borderCollapse; }
            set { _borderCollapse = value; }
        }

        /// <summary>
        /// The "border-radius" shorthand: sets all four corners at once, with an optional "/" separating
        /// horizontal and vertical radii for elliptical corners (e.g. "10px 5px / 20px 10px"). Each side
        /// of the "/" independently follows the standard 1/2/3/4-value CSS expansion rule
        /// (1=all, 2=TL+BR/TR+BL, 3=TL/TR+BL/BR, 4=TL/TR/BR/BL). Write-only, like the other border-*
        /// shorthands in this class - the four longhand properties below hold the actual state.
        /// </summary>
        public string BorderRadius
        {
            set
            {
                int slash = value.IndexOf('/');
                string hGroup = (slash >= 0 ? value.Substring(0, slash) : value).Trim();
                string vGroup = (slash >= 0 ? value.Substring(slash + 1) : hGroup).Trim();

                string[] h = ExpandRadiusShorthand(hGroup);
                string[] v = ExpandRadiusShorthand(vGroup);

                BorderTopLeftRadius = h[0] + " " + v[0];
                BorderTopRightRadius = h[1] + " " + v[1];
                BorderBottomRightRadius = h[2] + " " + v[2];
                BorderBottomLeftRadius = h[3] + " " + v[3];
            }
        }

        /// <summary>
        /// Expands a 1-4 value length group into [TopLeft, TopRight, BottomRight, BottomLeft] per the
        /// standard CSS shorthand rule.
        /// </summary>
        private static string[] ExpandRadiusShorthand(string group)
        {
            MatchCollection r = RegexParserUtils.Match(RegexParserUtils.CssLength, group);

            switch (r.Count)
            {
                case 1:
                    return new[] { r[0].Value, r[0].Value, r[0].Value, r[0].Value };
                case 2:
                    return new[] { r[0].Value, r[1].Value, r[0].Value, r[1].Value };
                case 3:
                    return new[] { r[0].Value, r[1].Value, r[2].Value, r[1].Value };
                case 4:
                    return new[] { r[0].Value, r[1].Value, r[2].Value, r[3].Value };
                default:
                    return new[] { "0", "0", "0", "0" };
            }
        }

        public string BorderTopLeftRadius
        {
            get { return _borderTopLeftRadius; }
            set
            {
                _borderTopLeftRadius = value;
                _actualBorderTopLeftRadiusX = double.NaN;
                _actualBorderTopLeftRadiusY = double.NaN;
            }
        }

        public string BorderTopRightRadius
        {
            get { return _borderTopRightRadius; }
            set
            {
                _borderTopRightRadius = value;
                _actualBorderTopRightRadiusX = double.NaN;
                _actualBorderTopRightRadiusY = double.NaN;
            }
        }

        public string BorderBottomRightRadius
        {
            get { return _borderBottomRightRadius; }
            set
            {
                _borderBottomRightRadius = value;
                _actualBorderBottomRightRadiusX = double.NaN;
                _actualBorderBottomRightRadiusY = double.NaN;
            }
        }

        public string BorderBottomLeftRadius
        {
            get { return _borderBottomLeftRadius; }
            set
            {
                _borderBottomLeftRadius = value;
                _actualBorderBottomLeftRadiusX = double.NaN;
                _actualBorderBottomLeftRadiusY = double.NaN;
            }
        }

        public string MarginBottom
        {
            get { return _marginBottom; }
            set { _marginBottom = value; }
        }

        public string MarginLeft
        {
            get { return _marginLeft; }
            set { _marginLeft = value; }
        }

        public string MarginRight
        {
            get { return _marginRight; }
            set { _marginRight = value; }
        }

        public string MarginTop
        {
            get { return _marginTop; }
            set { _marginTop = value; }
        }

        public string PaddingBottom
        {
            get { return _paddingBottom; }
            set
            {
                _paddingBottom = value;
                _actualPaddingBottom = double.NaN;
            }
        }

        public string PaddingLeft
        {
            get { return _paddingLeft; }
            set
            {
                _paddingLeft = value;
                _actualPaddingLeft = double.NaN;
            }
        }

        public string PaddingRight
        {
            get { return _paddingRight; }
            set
            {
                _paddingRight = value;
                _actualPaddingRight = double.NaN;
            }
        }

        public string PaddingTop
        {
            get { return _paddingTop; }
            set
            {
                _paddingTop = value;
                _actualPaddingTop = double.NaN;
            }
        }

        public string PageBreakInside
        {
            get { return _pageBreakInside; }
            set
            {
                _pageBreakInside = value;
            }
        }

        public string Left
        {
            get { return _left; }
            set
            {
                _left = value;

                if (Position == CssConstants.Fixed)
                {
                    _location = GetActualLocation(Left, Top);
                }
            }
        }

        public string Top
        {
            get { return _top; }
            set {
                _top = value;

                if (Position == CssConstants.Fixed)
                {
                    _location = GetActualLocation(Left, Top);
                }

            }
        }

        public string Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public string MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; }
        }

        public string Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public string BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        public string BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                _backgroundImage = value;
                _actualBackgroundImageComputed = false;
            }
        }

        public string BackgroundPosition
        {
            get { return _backgroundPosition; }
            set { _backgroundPosition = value; }
        }

        public string BackgroundRepeat
        {
            get { return _backgroundRepeat; }
            set { _backgroundRepeat = value; }
        }

        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _actualColor = RColor.Empty;
            }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string Display
        {
            get { return _display; }
            set { _display = value; }
        }

        public string Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public string EmptyCells
        {
            get { return _emptyCells; }
            set { _emptyCells = value; }
        }

        public string Float
        {
            get { return _float; }
            set { _float = value; }
        }

        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public string LineHeight
        {
            get { return _lineHeight; }
            set { _lineHeight = string.Format(NumberFormatInfo.InvariantInfo, "{0}px", CssValueParser.ParseLength(value, Size.Height, this, CssConstants.Em)); }
        }

        public string VerticalAlign
        {
            get { return _verticalAlign; }
            set { _verticalAlign = value; }
        }

        public string TextIndent
        {
            get { return _textIndent; }
            set { _textIndent = NoEms(value); }
        }

        public string TextAlign
        {
            get { return _textAlign; }
            set { _textAlign = value; }
        }

        public string TextDecoration
        {
            get { return _textDecoration; }
            set { _textDecoration = value; }
        }

        public string WhiteSpace
        {
            get { return _whiteSpace; }
            set { _whiteSpace = value; }
        }

        public string Visibility
        {
            get { return _visibility; }
            set { _visibility = value; }
        }

        public string WordSpacing
        {
            get { return _wordSpacing; }
            set { _wordSpacing = NoEms(value); }
        }

        public string WordBreak
        {
            get { return _wordBreak; }
            set { _wordBreak = value; }
        }

        public string FontFamily
        {
            get { return _fontFamily; }
            set { _fontFamily = value; }
        }

        public string FontSize
        {
            get { return _fontSize; }
            set
            {
                string length = RegexParserUtils.Search(RegexParserUtils.CssLength, value);

                if (length != null)
                {
                    string computedValue;
                    CssLength len = new CssLength(length);

                    if (len.HasError)
                    {
                        computedValue = "medium";
                    }
                    else if (len.Unit == CssUnit.Ems && GetParent() != null)
                    {
                        computedValue = len.ConvertEmToPoints(GetParent().ActualFont.Size).ToString();
                    }
                    else
                    {
                        computedValue = len.ToString();
                    }

                    _fontSize = computedValue;
                }
                else
                {
                    _fontSize = value;
                }
            }
        }

        public string FontStyle
        {
            get { return _fontStyle; }
            set { _fontStyle = value; }
        }

        public string FontVariant
        {
            get { return _fontVariant; }
            set { _fontVariant = value; }
        }

        public string FontWeight
        {
            get { return _fontWeight; }
            set { _fontWeight = value; }
        }

        public string ListStyle
        {
            get { return _listStyle; }
            set { _listStyle = value; }
        }

        public string Overflow
        {
            get { return _overflow; }
            set { _overflow = value; }
        }

        public string ListStylePosition
        {
            get { return _listStylePosition; }
            set { _listStylePosition = value; }
        }

        public string ListStyleImage
        {
            get { return _listStyleImage; }
            set { _listStyleImage = value; }
        }

        public string ListStyleType
        {
            get { return _listStyleType; }
            set { _listStyleType = value; }
        }

        #endregion CSS Propertier

        /// <summary>
        /// Gets or sets the location of the box
        /// </summary>
        public RPoint Location
        {
            get {
                if (_location.IsEmpty && Position == CssConstants.Fixed)
                {
                    var left = Left;
                    var top = Top;

                    _location = GetActualLocation(Left, Top);
                }
                return _location;
            }
            set {
                _location = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the box
        /// </summary>
        public RSize Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Gets the bounds of the box
        /// </summary>
        public RRect Bounds
        {
            get { return new RRect(Location, Size); }
        }

        /// <summary>
        /// Gets the width available on the box, counting padding and margin.
        /// </summary>
        public double AvailableWidth
        {
            get { return Size.Width - ActualBorderLeftWidth - ActualPaddingLeft - ActualPaddingRight - ActualBorderRightWidth; }
        }

        /// <summary>
        /// Gets the right of the box. When setting, it will affect only the width of the box.
        /// </summary>
        public double ActualRight
        {
            get { return Location.X + Size.Width; }
            set { Size = new RSize(value - Location.X, Size.Height); }
        }

        /// <summary>
        /// Gets or sets the bottom of the box. 
        /// (When setting, alters only the Size.Height of the box)
        /// </summary>
        public double ActualBottom
        {
            get { return Location.Y + Size.Height; }
            set { Size = new RSize(Size.Width, value - Location.Y); }
        }

        /// <summary>
        /// Gets the left of the client rectangle (Where content starts rendering)
        /// </summary>
        public double ClientLeft
        {
            get { return Location.X + ActualBorderLeftWidth + ActualPaddingLeft; }
        }

        /// <summary>
        /// Gets the top of the client rectangle (Where content starts rendering)
        /// </summary>
        public double ClientTop
        {
            get { return Location.Y + ActualBorderTopWidth + ActualPaddingTop; }
        }

        /// <summary>
        /// Gets the right of the client rectangle
        /// </summary>
        public double ClientRight
        {
            get { return ActualRight - ActualPaddingRight - ActualBorderRightWidth; }
        }

        /// <summary>
        /// Gets the bottom of the client rectangle
        /// </summary>
        public double ClientBottom
        {
            get { return ActualBottom - ActualPaddingBottom - ActualBorderBottomWidth; }
        }

        /// <summary>
        /// Gets the client rectangle
        /// </summary>
        public RRect ClientRectangle
        {
            get { return RRect.FromLTRB(ClientLeft, ClientTop, ClientRight, ClientBottom); }
        }

        /// <summary>
        /// Gets the actual height
        /// </summary>
        public double ActualHeight
        {
            get
            {
                if (double.IsNaN(_actualHeight))
                {
                    _actualHeight = CssValueParser.ParseLength(Height, Size.Height, this);
                }
                return _actualHeight;
            }
        }

        /// <summary>
        /// Gets the actual height
        /// </summary>
        public double ActualWidth
        {
            get
            {
                if (double.IsNaN(_actualWidth))
                {
                    _actualWidth = CssValueParser.ParseLength(Width, Size.Width, this);
                }
                return _actualWidth;
            }
        }

        /// <summary>
        /// Gets the actual top's padding
        /// </summary>
        public double ActualPaddingTop
        {
            get
            {
                if (double.IsNaN(_actualPaddingTop))
                {
                    _actualPaddingTop = CssValueParser.ParseLength(PaddingTop, Size.Width, this);
                }
                return _actualPaddingTop;
            }
        }

        /// <summary>
        /// Gets the actual padding on the left
        /// </summary>
        public double ActualPaddingLeft
        {
            get
            {
                if (double.IsNaN(_actualPaddingLeft))
                {
                    _actualPaddingLeft = CssValueParser.ParseLength(PaddingLeft, Size.Width, this);
                }
                return _actualPaddingLeft;
            }
        }

        /// <summary>
        /// Gets the actual Padding of the bottom
        /// </summary>
        public double ActualPaddingBottom
        {
            get
            {
                if (double.IsNaN(_actualPaddingBottom))
                {
                    _actualPaddingBottom = CssValueParser.ParseLength(PaddingBottom, Size.Width, this);
                }
                return _actualPaddingBottom;
            }
        }

        /// <summary>
        /// Gets the actual padding on the right
        /// </summary>
        public double ActualPaddingRight
        {
            get
            {
                if (double.IsNaN(_actualPaddingRight))
                {
                    _actualPaddingRight = CssValueParser.ParseLength(PaddingRight, Size.Width, this);
                }
                return _actualPaddingRight;
            }
        }

        /// <summary>
        /// Gets the actual top's Margin
        /// </summary>
        public double ActualMarginTop
        {
            get
            {
                if (double.IsNaN(_actualMarginTop))
                {
                    if (MarginTop == CssConstants.Auto)
                        MarginTop = "0";
                    var actualMarginTop = CssValueParser.ParseLength(MarginTop, Size.Width, this);
                    if (MarginLeft.EndsWith("%"))
                        return actualMarginTop;
                    _actualMarginTop = actualMarginTop;
                }
                return _actualMarginTop;
            }
        }

        /// <summary>
        /// The margin top value if was effected by margin collapse.
        /// </summary>
        public double CollapsedMarginTop
        {
            get { return double.IsNaN(_collapsedMarginTop) ? 0 : _collapsedMarginTop; }
            set { _collapsedMarginTop = value; }
        }

        /// <summary>
        /// Gets the actual Margin on the left
        /// </summary>
        public double ActualMarginLeft
        {
            get
            {
                if (double.IsNaN(_actualMarginLeft))
                {
                    if (MarginLeft == CssConstants.Auto)
                        MarginLeft = "0";
                    var actualMarginLeft = CssValueParser.ParseLength(MarginLeft, Size.Width, this);
                    if (MarginLeft.EndsWith("%"))
                        return actualMarginLeft;
                    _actualMarginLeft = actualMarginLeft;
                }
                return _actualMarginLeft;
            }
        }

        /// <summary>
        /// Gets the actual Margin of the bottom
        /// </summary>
        public double ActualMarginBottom
        {
            get
            {
                if (double.IsNaN(_actualMarginBottom))
                {
                    if (MarginBottom == CssConstants.Auto)
                        MarginBottom = "0";
                    var actualMarginBottom = CssValueParser.ParseLength(MarginBottom, Size.Width, this);
                    if (MarginLeft.EndsWith("%"))
                        return actualMarginBottom;
                    _actualMarginBottom = actualMarginBottom;
                }
                return _actualMarginBottom;
            }
        }

        /// <summary>
        /// Gets the actual Margin on the right
        /// </summary>
        public double ActualMarginRight
        {
            get
            {
                if (double.IsNaN(_actualMarginRight))
                {
                    if (MarginRight == CssConstants.Auto)
                        MarginRight = "0";
                    var actualMarginRight = CssValueParser.ParseLength(MarginRight, Size.Width, this);
                    if (MarginLeft.EndsWith("%"))
                        return actualMarginRight;
                    _actualMarginRight = actualMarginRight;
                }
                return _actualMarginRight;
            }
        }

        /// <summary>
        /// Gets the actual top border width
        /// </summary>
        public double ActualBorderTopWidth
        {
            get
            {
                if (double.IsNaN(_actualBorderTopWidth))
                {
                    _actualBorderTopWidth = CssValueParser.GetActualBorderWidth(BorderTopWidth, this);
                    if (string.IsNullOrEmpty(BorderTopStyle) || BorderTopStyle == CssConstants.None)
                    {
                        _actualBorderTopWidth = 0f;
                    }
                }
                return _actualBorderTopWidth;
            }
        }

        /// <summary>
        /// Gets the actual Left border width
        /// </summary>
        public double ActualBorderLeftWidth
        {
            get
            {
                if (double.IsNaN(_actualBorderLeftWidth))
                {
                    _actualBorderLeftWidth = CssValueParser.GetActualBorderWidth(BorderLeftWidth, this);
                    if (string.IsNullOrEmpty(BorderLeftStyle) || BorderLeftStyle == CssConstants.None)
                    {
                        _actualBorderLeftWidth = 0f;
                    }
                }
                return _actualBorderLeftWidth;
            }
        }

        /// <summary>
        /// Gets the actual Bottom border width
        /// </summary>
        public double ActualBorderBottomWidth
        {
            get
            {
                if (double.IsNaN(_actualBorderBottomWidth))
                {
                    _actualBorderBottomWidth = CssValueParser.GetActualBorderWidth(BorderBottomWidth, this);
                    if (string.IsNullOrEmpty(BorderBottomStyle) || BorderBottomStyle == CssConstants.None)
                    {
                        _actualBorderBottomWidth = 0f;
                    }
                }
                return _actualBorderBottomWidth;
            }
        }

        /// <summary>
        /// Gets the actual Right border width
        /// </summary>
        public double ActualBorderRightWidth
        {
            get
            {
                if (double.IsNaN(_actualBorderRightWidth))
                {
                    _actualBorderRightWidth = CssValueParser.GetActualBorderWidth(BorderRightWidth, this);
                    if (string.IsNullOrEmpty(BorderRightStyle) || BorderRightStyle == CssConstants.None)
                    {
                        _actualBorderRightWidth = 0f;
                    }
                }
                return _actualBorderRightWidth;
            }
        }

        /// <summary>
        /// Gets the actual top border Color
        /// </summary>
        public RColor ActualBorderTopColor
        {
            get
            {
                if (_actualBorderTopColor.IsEmpty)
                {
                    _actualBorderTopColor = GetActualColor(BorderTopColor);
                }
                return _actualBorderTopColor;
            }
        }

        protected abstract RPoint GetActualLocation(string X, string Y);

        protected abstract RColor GetActualColor(string colorStr);

        protected abstract CssImage GetActualBackgroundImageValue(string value);

        /// <summary>
        /// Gets the parsed "background-image" value - either a url() reference or a linear-gradient(),
        /// lazily resolved (needs adapter-dependent named-color lookup for gradient stops, see
        /// <see cref="GetActualBackgroundImageValue"/>) and cached until <see cref="BackgroundImage"/> is set again.
        /// </summary>
        public CssImage ActualBackgroundImage
        {
            get
            {
                if (!_actualBackgroundImageComputed)
                {
                    _actualBackgroundImage = GetActualBackgroundImageValue(BackgroundImage);
                    _actualBackgroundImageComputed = true;
                }
                return _actualBackgroundImage;
            }
        }

        /// <summary>
        /// Gets the actual Left border Color
        /// </summary>
        public RColor ActualBorderLeftColor
        {
            get
            {
                if ((_actualBorderLeftColor.IsEmpty))
                {
                    _actualBorderLeftColor = GetActualColor(BorderLeftColor);
                }
                return _actualBorderLeftColor;
            }
        }

        /// <summary>
        /// Gets the actual Bottom border Color
        /// </summary>
        public RColor ActualBorderBottomColor
        {
            get
            {
                if ((_actualBorderBottomColor.IsEmpty))
                {
                    _actualBorderBottomColor = GetActualColor(BorderBottomColor);
                }
                return _actualBorderBottomColor;
            }
        }

        /// <summary>
        /// Gets the actual Right border Color
        /// </summary>
        public RColor ActualBorderRightColor
        {
            get
            {
                if ((_actualBorderRightColor.IsEmpty))
                {
                    _actualBorderRightColor = GetActualColor(BorderRightColor);
                }
                return _actualBorderRightColor;
            }
        }

        public double ActualBorderTopLeftRadiusX
        {
            get
            {
                if (double.IsNaN(_actualBorderTopLeftRadiusX))
                    _actualBorderTopLeftRadiusX = CssValueParser.ParseLength(FirstCssValue(BorderTopLeftRadius), Size.Width, this);
                return _actualBorderTopLeftRadiusX;
            }
        }

        public double ActualBorderTopLeftRadiusY
        {
            get
            {
                if (double.IsNaN(_actualBorderTopLeftRadiusY))
                    _actualBorderTopLeftRadiusY = CssValueParser.ParseLength(SecondCssValue(BorderTopLeftRadius), Size.Height, this);
                return _actualBorderTopLeftRadiusY;
            }
        }

        public double ActualBorderTopRightRadiusX
        {
            get
            {
                if (double.IsNaN(_actualBorderTopRightRadiusX))
                    _actualBorderTopRightRadiusX = CssValueParser.ParseLength(FirstCssValue(BorderTopRightRadius), Size.Width, this);
                return _actualBorderTopRightRadiusX;
            }
        }

        public double ActualBorderTopRightRadiusY
        {
            get
            {
                if (double.IsNaN(_actualBorderTopRightRadiusY))
                    _actualBorderTopRightRadiusY = CssValueParser.ParseLength(SecondCssValue(BorderTopRightRadius), Size.Height, this);
                return _actualBorderTopRightRadiusY;
            }
        }

        public double ActualBorderBottomRightRadiusX
        {
            get
            {
                if (double.IsNaN(_actualBorderBottomRightRadiusX))
                    _actualBorderBottomRightRadiusX = CssValueParser.ParseLength(FirstCssValue(BorderBottomRightRadius), Size.Width, this);
                return _actualBorderBottomRightRadiusX;
            }
        }

        public double ActualBorderBottomRightRadiusY
        {
            get
            {
                if (double.IsNaN(_actualBorderBottomRightRadiusY))
                    _actualBorderBottomRightRadiusY = CssValueParser.ParseLength(SecondCssValue(BorderBottomRightRadius), Size.Height, this);
                return _actualBorderBottomRightRadiusY;
            }
        }

        public double ActualBorderBottomLeftRadiusX
        {
            get
            {
                if (double.IsNaN(_actualBorderBottomLeftRadiusX))
                    _actualBorderBottomLeftRadiusX = CssValueParser.ParseLength(FirstCssValue(BorderBottomLeftRadius), Size.Width, this);
                return _actualBorderBottomLeftRadiusX;
            }
        }

        public double ActualBorderBottomLeftRadiusY
        {
            get
            {
                if (double.IsNaN(_actualBorderBottomLeftRadiusY))
                    _actualBorderBottomLeftRadiusY = CssValueParser.ParseLength(SecondCssValue(BorderBottomLeftRadius), Size.Height, this);
                return _actualBorderBottomLeftRadiusY;
            }
        }

        /// <summary>
        /// Returns the first top-level-whitespace-delimited token in a "border-*-radius" longhand value
        /// (the horizontal radius).
        /// </summary>
        private static string FirstCssValue(string value)
        {
            foreach (var token in CssValueParser.SplitTopLevelWhitespace(value))
                return token;
            return value;
        }

        /// <summary>
        /// Returns the second top-level-whitespace-delimited token in a "border-*-radius" longhand value
        /// (the vertical radius), or the first if there is no second (spec: omitted v-radius = h-radius).
        /// </summary>
        private static string SecondCssValue(string value)
        {
            var tokens = new List<string>(CssValueParser.SplitTopLevelWhitespace(value));
            return tokens.Count > 1 ? tokens[1] : value;
        }

        /// <summary>
        /// Computes overlap-reduced corner radii for the given rendering rectangle, per CSS Backgrounds
        /// §4. Horizontal and vertical axes are reduced independently so that, e.g., two large radii on
        /// the same edge never sum to more than that edge's length.
        /// </summary>
        public BorderRadii ComputeRadii(RRect rect)
        {
            double tlX = ActualBorderTopLeftRadiusX, tlY = ActualBorderTopLeftRadiusY;
            double trX = ActualBorderTopRightRadiusX, trY = ActualBorderTopRightRadiusY;
            double brX = ActualBorderBottomRightRadiusX, brY = ActualBorderBottomRightRadiusY;
            double blX = ActualBorderBottomLeftRadiusX, blY = ActualBorderBottomLeftRadiusY;

            // Horizontal reduction: check top side and bottom side independently.
            double fTop = tlX + trX > 0 && rect.Width > 0 ? rect.Width / (tlX + trX) : 1.0;
            double fBot = blX + brX > 0 && rect.Width > 0 ? rect.Width / (blX + brX) : 1.0;
            double fX = Math.Min(1.0, Math.Min(fTop, fBot));

            // Vertical reduction: check left side and right side independently.
            double fLeft = tlY + blY > 0 && rect.Height > 0 ? rect.Height / (tlY + blY) : 1.0;
            double fRight = trY + brY > 0 && rect.Height > 0 ? rect.Height / (trY + brY) : 1.0;
            double fY = Math.Min(1.0, Math.Min(fLeft, fRight));

            return new BorderRadii(tlX * fX, tlY * fY, trX * fX, trY * fY,
                brX * fX, brY * fY, blX * fX, blY * fY);
        }

        /// <summary>
        /// Gets a value indicating if at least one of the corners of the box is rounded. A fast,
        /// rect-independent pre-check using the raw (not overlap-reduced) actual radii - use
        /// <see cref="ComputeRadii"/> when the actual per-corner values are needed for painting.
        /// </summary>
        public bool IsRounded
        {
            get
            {
                return ActualBorderTopLeftRadiusX > 0 || ActualBorderTopLeftRadiusY > 0 ||
                    ActualBorderTopRightRadiusX > 0 || ActualBorderTopRightRadiusY > 0 ||
                    ActualBorderBottomRightRadiusX > 0 || ActualBorderBottomRightRadiusY > 0 ||
                    ActualBorderBottomLeftRadiusX > 0 || ActualBorderBottomLeftRadiusY > 0;
            }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public double ActualWordSpacing
        {
            get { return _actualWordSpacing; }
        }

        /// <summary>
        /// 
        /// Gets the actual color for the text.
        /// </summary>
        public RColor ActualColor
        {
            get
            {
                if (_actualColor.IsEmpty)
                {
                    _actualColor = GetActualColor(Color);
                }

                return _actualColor;
            }
        }

        /// <summary>
        /// Gets the actual background color of the box
        /// </summary>
        public RColor ActualBackgroundColor
        {
            get
            {
                if (_actualBackgroundColor.IsEmpty)
                {
                    _actualBackgroundColor = GetActualColor(BackgroundColor);
                }

                return _actualBackgroundColor;
            }
        }

        /// <summary>
        /// Gets the actual font of the parent
        /// </summary>
        public RFont ActualParentFont
        {
            get { return GetParent() == null ? ActualFont : GetParent().ActualFont; }
        }

        /// <summary>
        /// Gets the font that should be actually used to paint the text of the box
        /// </summary>
        public RFont ActualFont
        {
            get
            {
                if (_actualFont == null)
                {
                    if (string.IsNullOrEmpty(FontFamily))
                    {
                        FontFamily = CssConstants.DefaultFont;
                    }
                    if (string.IsNullOrEmpty(FontSize))
                    {
                        FontSize = CssConstants.FontSize.ToString(CultureInfo.InvariantCulture) + "pt";
                    }

                    RFontStyle st = RFontStyle.Regular;

                    if (FontStyle == CssConstants.Italic || FontStyle == CssConstants.Oblique)
                    {
                        st |= RFontStyle.Italic;
                    }

                    if (FontWeight != CssConstants.Normal && FontWeight != CssConstants.Lighter && !string.IsNullOrEmpty(FontWeight) && FontWeight != CssConstants.Inherit)
                    {
                        st |= RFontStyle.Bold;
                    }

                    double fsize;
                    double parentSize = CssConstants.FontSize;

                    if (GetParent() != null)
                        parentSize = GetParent().ActualFont.Size;

                    switch (FontSize)
                    {
                        case CssConstants.Medium:
                            fsize = CssConstants.FontSize;
                            break;
                        case CssConstants.XXSmall:
                            fsize = CssConstants.FontSize - 4;
                            break;
                        case CssConstants.XSmall:
                            fsize = CssConstants.FontSize - 3;
                            break;
                        case CssConstants.Small:
                            fsize = CssConstants.FontSize - 2;
                            break;
                        case CssConstants.Large:
                            fsize = CssConstants.FontSize + 2;
                            break;
                        case CssConstants.XLarge:
                            fsize = CssConstants.FontSize + 3;
                            break;
                        case CssConstants.XXLarge:
                            fsize = CssConstants.FontSize + 4;
                            break;
                        case CssConstants.Smaller:
                            fsize = parentSize - 2;
                            break;
                        case CssConstants.Larger:
                            fsize = parentSize + 2;
                            break;
                        default:
                            fsize = CssValueParser.ParseLength(FontSize, parentSize, parentSize, null, true, true);
                            break;
                    }

                    if (fsize <= 1f)
                    {
                        fsize = CssConstants.FontSize;
                    }

                    _actualFont = GetCachedFont(FontFamily, fsize, st);
                }
                return _actualFont;
            }
        }

        protected abstract RFont GetCachedFont(string fontFamily, double fsize, RFontStyle st);

        /// <summary>
        /// Gets the line height
        /// </summary>
        public double ActualLineHeight
        {
            get
            {
                if (double.IsNaN(_actualLineHeight))
                {
                    _actualLineHeight = .9f * CssValueParser.ParseLength(LineHeight, Size.Height, this);
                }
                return _actualLineHeight;
            }
        }

        /// <summary>
        /// Gets the text indentation (on first line only)
        /// </summary>
        public double ActualTextIndent
        {
            get
            {
                if (double.IsNaN(_actualTextIndent))
                {
                    _actualTextIndent = CssValueParser.ParseLength(TextIndent, Size.Width, this);
                }

                return _actualTextIndent;
            }
        }

        /// <summary>
        /// Gets the actual horizontal border spacing for tables
        /// </summary>
        public double ActualBorderSpacingHorizontal
        {
            get
            {
                if (double.IsNaN(_actualBorderSpacingHorizontal))
                {
                    MatchCollection matches = RegexParserUtils.Match(RegexParserUtils.CssLength, BorderSpacing);

                    if (matches.Count == 0)
                    {
                        _actualBorderSpacingHorizontal = 0;
                    }
                    else if (matches.Count > 0)
                    {
                        _actualBorderSpacingHorizontal = CssValueParser.ParseLength(matches[0].Value, 1, this);
                    }
                }


                return _actualBorderSpacingHorizontal;
            }
        }

        /// <summary>
        /// Gets the actual vertical border spacing for tables
        /// </summary>
        public double ActualBorderSpacingVertical
        {
            get
            {
                if (double.IsNaN(_actualBorderSpacingVertical))
                {
                    MatchCollection matches = RegexParserUtils.Match(RegexParserUtils.CssLength, BorderSpacing);

                    if (matches.Count == 0)
                    {
                        _actualBorderSpacingVertical = 0;
                    }
                    else if (matches.Count == 1)
                    {
                        _actualBorderSpacingVertical = CssValueParser.ParseLength(matches[0].Value, 1, this);
                    }
                    else
                    {
                        _actualBorderSpacingVertical = CssValueParser.ParseLength(matches[1].Value, 1, this);
                    }
                }
                return _actualBorderSpacingVertical;
            }
        }

        /// <summary>
        /// Get the parent of this css properties instance.
        /// </summary>
        /// <returns></returns>
        protected abstract CssBoxProperties GetParent();

        /// <summary>
        /// Gets the height of the font in the specified units
        /// </summary>
        /// <returns></returns>
        public double GetEmHeight()
        {
            return ActualFont.Height;
        }

        /// <summary>
        /// Ensures that the specified length is converted to pixels if necessary
        /// </summary>
        /// <param name="length"></param>
        protected string NoEms(string length)
        {
            var len = new CssLength(length);
            if (len.Unit == CssUnit.Ems)
            {
                length = len.ConvertEmToPixels(GetEmHeight()).ToString();
            }
            return length;
        }

        /// <summary>
        /// Set the style/width/color for all 4 borders on the box.<br/>
        /// if null is given for a value it will not be set.
        /// </summary>
        /// <param name="style">optional: the style to set</param>
        /// <param name="width">optional: the width to set</param>
        /// <param name="color">optional: the color to set</param>
        protected void SetAllBorders(string style = null, string width = null, string color = null)
        {
            if (style != null)
                BorderLeftStyle = BorderTopStyle = BorderRightStyle = BorderBottomStyle = style;
            if (width != null)
                BorderLeftWidth = BorderTopWidth = BorderRightWidth = BorderBottomWidth = width;
            if (color != null)
                BorderLeftColor = BorderTopColor = BorderRightColor = BorderBottomColor = color;
        }

        /// <summary>
        /// Measures the width of whitespace between words (set <see cref="ActualWordSpacing"/>).
        /// </summary>
        protected void MeasureWordSpacing(RGraphics g)
        {
            if (double.IsNaN(ActualWordSpacing))
            {
                _actualWordSpacing = CssUtils.WhiteSpace(g, this);
                if (WordSpacing != CssConstants.Normal)
                {
                    string len = RegexParserUtils.Search(RegexParserUtils.CssLength, WordSpacing);
                    _actualWordSpacing += CssValueParser.ParseLength(len, 1, this);
                }
            }
        }

        /// <summary>
        /// Inherits inheritable values from specified box.
        /// </summary>
        /// <param name="everything">Set to true to inherit all CSS properties instead of only the ineritables</param>
        /// <param name="p">Box to inherit the properties</param>
        protected void InheritStyle(CssBox p, bool everything)
        {
            if (p != null)
            {
                // Custom properties are always inherited, regardless of the "everything" special case.
                // Cloned (not shared) so a child's local override never mutates the parent's or a sibling's dictionary.
                CustomProperties = p.CustomProperties != null && p.CustomProperties.Count > 0
                    ? new Dictionary<string, string>(p.CustomProperties)
                    : null;

                _borderSpacing = p._borderSpacing;
                _borderCollapse = p._borderCollapse;
                _color = p._color;
                _emptyCells = p._emptyCells;
                _whiteSpace = p._whiteSpace;
                _visibility = p._visibility;
                _textIndent = p._textIndent;
                _textAlign = p._textAlign;
                _fontFamily = p._fontFamily;
                _fontSize = p._fontSize;
                _fontStyle = p._fontStyle;
                _fontVariant = p._fontVariant;
                _fontWeight = p._fontWeight;
                _listStyleImage = p._listStyleImage;
                _listStylePosition = p._listStylePosition;
                _listStyleType = p._listStyleType;
                _listStyle = p._listStyle;
                _lineHeight = p._lineHeight;
                _wordBreak = p.WordBreak;
                _direction = p._direction;

                if (everything)
                {
                    _backgroundColor = p._backgroundColor;
                    _backgroundImage = p._backgroundImage;
                    _backgroundPosition = p._backgroundPosition;
                    _backgroundRepeat = p._backgroundRepeat;
                    _borderTopWidth = p._borderTopWidth;
                    _borderRightWidth = p._borderRightWidth;
                    _borderBottomWidth = p._borderBottomWidth;
                    _borderLeftWidth = p._borderLeftWidth;
                    _borderTopColor = p._borderTopColor;
                    _borderRightColor = p._borderRightColor;
                    _borderBottomColor = p._borderBottomColor;
                    _borderLeftColor = p._borderLeftColor;
                    _borderTopStyle = p._borderTopStyle;
                    _borderRightStyle = p._borderRightStyle;
                    _borderBottomStyle = p._borderBottomStyle;
                    _borderLeftStyle = p._borderLeftStyle;
                    _bottom = p._bottom;
                    _borderTopLeftRadius = p._borderTopLeftRadius;
                    _borderTopRightRadius = p._borderTopRightRadius;
                    _borderBottomRightRadius = p._borderBottomRightRadius;
                    _borderBottomLeftRadius = p._borderBottomLeftRadius;
                    _display = p._display;
                    _float = p._float;
                    _height = p._height;
                    _marginBottom = p._marginBottom;
                    _marginLeft = p._marginLeft;
                    _marginRight = p._marginRight;
                    _marginTop = p._marginTop;
                    _left = p._left;
                    _lineHeight = p._lineHeight;
                    _overflow = p._overflow;
                    _paddingLeft = p._paddingLeft;
                    _paddingBottom = p._paddingBottom;
                    _paddingRight = p._paddingRight;
                    _paddingTop = p._paddingTop;
                    _right = p._right;
                    _textDecoration = p._textDecoration;
                    _top = p._top;
                    _position = p._position;
                    _verticalAlign = p._verticalAlign;
                    _width = p._width;
                    _maxWidth = p._maxWidth;
                    _wordSpacing = p._wordSpacing;
                }
            }
        }
    }

    /// <summary>
    /// Holds the eight computed (overlap-reduced) corner radii for a box rectangle - see
    /// <see cref="CssBoxProperties.ComputeRadii"/>.
    /// </summary>
    internal struct BorderRadii
    {
        public readonly double TLX, TLY, TRX, TRY, BRX, BRY, BLX, BLY;

        public BorderRadii(double tlX, double tlY, double trX, double trY,
            double brX, double brY, double blX, double blY)
        {
            TLX = tlX; TLY = tlY;
            TRX = trX; TRY = trY;
            BRX = brX; BRY = brY;
            BLX = blX; BLY = blY;
        }

        public bool IsRounded
        {
            get { return TLX > 0 || TLY > 0 || TRX > 0 || TRY > 0 || BRX > 0 || BRY > 0 || BLX > 0 || BLY > 0; }
        }
    }
}