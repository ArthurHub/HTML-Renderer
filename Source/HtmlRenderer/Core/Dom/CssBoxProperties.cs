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
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlRenderer.Entities;
using HtmlRenderer.Parse;
using HtmlRenderer.Utils;

namespace HtmlRenderer.Dom
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
        private string _backgroundGradient = "none";
        private string _backgroundGradientAngle = "90";
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
        private string _cornerNWRadius = "0";
        private string _cornerNERadius = "0";
        private string _cornerSERadius = "0";
        private string _cornerSWRadius = "0";
        private string _cornerRadius = "0";
        private string _emptyCells = "show";
        private string _direction = "ltr";
        private string _display = "inline";
        private string _fontFamily = "serif";
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

        #endregion


        #region Fields

        /// <summary>
        /// Gets or sets the location of the box
        /// </summary>
        private PointF _location;
        
        /// <summary>
        /// Gets or sets the size of the box
        /// </summary>
        private SizeF _size;

        private float _actualCornerNW = float.NaN;
        private float _actualCornerNE = float.NaN;
        private float _actualCornerSW = float.NaN;
        private float _actualCornerSE = float.NaN;
        private Color _actualColor = System.Drawing.Color.Empty;
        private float _actualBackgroundGradientAngle = float.NaN;
        private float _actualHeight = float.NaN;
        private float _actualWidth = float.NaN;
        private float _actualPaddingTop = float.NaN;
        private float _actualPaddingBottom = float.NaN;
        private float _actualPaddingRight = float.NaN;
        private float _actualPaddingLeft = float.NaN;
        private float _actualMarginTop = float.NaN;
        private float _collapsedMarginTop = float.NaN;
        private float _actualMarginBottom = float.NaN;
        private float _actualMarginRight = float.NaN;
        private float _actualMarginLeft = float.NaN;
        private float _actualBorderTopWidth = float.NaN;
        private float _actualBorderLeftWidth = float.NaN;
        private float _actualBorderBottomWidth = float.NaN;
        private float _actualBorderRightWidth = float.NaN;
        
        /// <summary>
        /// the width of whitespace between words
        /// </summary>
        private float _actualLineHeight = float.NaN;
        private float _actualWordSpacing = float.NaN;
        private float _actualTextIndent = float.NaN;
        private float _actualBorderSpacingHorizontal = float.NaN;
        private float _actualBorderSpacingVertical = float.NaN;
        private float _fontAscent = float.NaN;
        private float _fontDescent = float.NaN;
        private float _fontLineSpacing = float.NaN;
        private Color _actualBackgroundGradient = System.Drawing.Color.Empty;
        private Color _actualBorderTopColor = System.Drawing.Color.Empty;
        private Color _actualBorderLeftColor = System.Drawing.Color.Empty;
        private Color _actualBorderBottomColor = System.Drawing.Color.Empty;
        private Color _actualBorderRightColor = System.Drawing.Color.Empty;
        private Color _actualBackgroundColor = System.Drawing.Color.Empty;
        private Font _actualFont;

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
                _actualBorderBottomColor = System.Drawing.Color.Empty;
            }
        }

        public string BorderLeftColor
        {
            get { return _borderLeftColor; }
            set
            {
                _borderLeftColor = value;
                _actualBorderLeftColor = System.Drawing.Color.Empty;
            }
        }

        public string BorderRightColor
        {
            get { return _borderRightColor; }
            set
            {
                _borderRightColor = value;
                _actualBorderRightColor = System.Drawing.Color.Empty;
            }
        }

        public string BorderTopColor
        {
            get { return _borderTopColor; }
            set
            {
                _borderTopColor = value;
                _actualBorderTopColor = System.Drawing.Color.Empty;
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

        public string CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                MatchCollection r = RegexParserUtils.Match(RegexParserUtils.CssLength, value);

                switch (r.Count)
                {
                    case 1:
                        CornerNERadius = r[0].Value;
                        CornerNWRadius = r[0].Value;
                        CornerSERadius = r[0].Value;
                        CornerSWRadius = r[0].Value;
                        break;
                    case 2:
                        CornerNERadius = r[0].Value;
                        CornerNWRadius = r[0].Value;
                        CornerSERadius = r[1].Value;
                        CornerSWRadius = r[1].Value;
                        break;
                    case 3:
                        CornerNERadius = r[0].Value;
                        CornerNWRadius = r[1].Value;
                        CornerSERadius = r[2].Value;
                        break;
                    case 4:
                        CornerNERadius = r[0].Value;
                        CornerNWRadius = r[1].Value;
                        CornerSERadius = r[2].Value;
                        CornerSWRadius = r[3].Value;
                        break;
                }

                _cornerRadius = value;
            }
        }

        public string CornerNWRadius
        {
            get { return _cornerNWRadius; }
            set { _cornerNWRadius = value; }
        }

        public string CornerNERadius
        {
            get { return _cornerNERadius; }
            set { _cornerNERadius = value; }
        }

        public string CornerSERadius
        {
            get { return _cornerSERadius; }
            set { _cornerSERadius = value; }
        }

        public string CornerSWRadius
        {
            get { return _cornerSWRadius; }
            set { _cornerSWRadius = value; }
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
            set { _paddingBottom = value; _actualPaddingBottom = float.NaN; }
        }

        public string PaddingLeft
        {
            get { return _paddingLeft; }
            set { _paddingLeft = value; _actualPaddingLeft = float.NaN; }
        }

        public string PaddingRight
        {
            get { return _paddingRight; }
            set { _paddingRight = value; _actualPaddingRight = float.NaN; }
        }

        public string PaddingTop
        {
            get { return _paddingTop; }
            set { _paddingTop = value; _actualPaddingTop = float.NaN; }
        }

        public string Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public string Top
        {
            get { return _top; }
            set { _top = value; }
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
            set { _backgroundImage = value; }
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

        public string BackgroundGradient
        {
            get { return _backgroundGradient; }
            set { _backgroundGradient = value; }
        }

        public string BackgroundGradientAngle
        {
            get { return _backgroundGradientAngle; }
            set { _backgroundGradientAngle = value; }
        }

        public string Color
        {
            get { return _color; }
            set { _color = value; _actualColor = System.Drawing.Color.Empty; }
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
                        computedValue = len.ConvertEmToPoints(GetParent().ActualFont.SizeInPoints).ToString();
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

        #endregion


        /// <summary>
        /// Gets or sets the location of the box
        /// </summary>
        public PointF Location
        {
            get { return _location; }
            set { _location = value; }
        }

        /// <summary>
        /// Gets or sets the size of the box
        /// </summary>
        public SizeF Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Gets the bounds of the box
        /// </summary>
        public RectangleF Bounds
        {
            get { return new RectangleF(Location, Size); }
        }

        /// <summary>
        /// Gets the width available on the box, counting padding and margin.
        /// </summary>
        public float AvailableWidth
        {
            get { return Size.Width - ActualBorderLeftWidth - ActualPaddingLeft - ActualPaddingRight - ActualBorderRightWidth; }
        }

        /// <summary>
        /// Gets the right of the box. When setting, it will affect only the width of the box.
        /// </summary>
        public float ActualRight
        {
            get { return Location.X + Size.Width; }
            set { Size = new SizeF(value - Location.X, Size.Height); }
        }

        /// <summary>
        /// Gets or sets the bottom of the box. 
        /// (When setting, alters only the Size.Height of the box)
        /// </summary>
        public float ActualBottom
        {
            get { return Location.Y + Size.Height; }
            set { Size = new SizeF(Size.Width, value - Location.Y); }
        }

        /// <summary>
        /// Gets the left of the client rectangle (Where content starts rendering)
        /// </summary>
        public float ClientLeft
        {
            get { return Location.X + ActualBorderLeftWidth + ActualPaddingLeft; }
        }

        /// <summary>
        /// Gets the top of the client rectangle (Where content starts rendering)
        /// </summary>
        public float ClientTop
        {
            get { return Location.Y + ActualBorderTopWidth + ActualPaddingTop; }
        }

        /// <summary>
        /// Gets the right of the client rectangle
        /// </summary>
        public float ClientRight
        {
            get { return ActualRight - ActualPaddingRight - ActualBorderRightWidth; }
        }

        /// <summary>
        /// Gets the bottom of the client rectangle
        /// </summary>
        public float ClientBottom
        {
            get { return ActualBottom - ActualPaddingBottom - ActualBorderBottomWidth; }
        }

        /// <summary>
        /// Gets the client rectangle
        /// </summary>
        public RectangleF ClientRectangle
        {
            get { return RectangleF.FromLTRB(ClientLeft, ClientTop, ClientRight, ClientBottom); }
        }

        /// <summary>
        /// Gets the actual height
        /// </summary>
        public float ActualHeight
        {
            get
            {
                if (float.IsNaN(_actualHeight))
                {
                    _actualHeight = CssValueParser.ParseLength(Height, Size.Height, this);
                }
                return _actualHeight;
            }
        }

        /// <summary>
        /// Gets the actual height
        /// </summary>
        public float ActualWidth
        {
            get
            {
                if (float.IsNaN(_actualWidth))
                {
                    _actualWidth = CssValueParser.ParseLength(Width, Size.Width, this);
                }
                return _actualWidth;
            }
        }

        /// <summary>
        /// Gets the actual top's padding
        /// </summary>
        public float ActualPaddingTop
        {
            get
            {
                if (float.IsNaN(_actualPaddingTop))
                {
                    _actualPaddingTop = CssValueParser.ParseLength(PaddingTop, Size.Width, this);
                }
                return _actualPaddingTop;
            }
        }

        /// <summary>
        /// Gets the actual padding on the left
        /// </summary>
        public float ActualPaddingLeft
        {
            get
            {
                if (float.IsNaN(_actualPaddingLeft))
                {
                    _actualPaddingLeft = CssValueParser.ParseLength(PaddingLeft, Size.Width, this);
                }
                return _actualPaddingLeft;
            }
        }

        /// <summary>
        /// Gets the actual Padding of the bottom
        /// </summary>
        public float ActualPaddingBottom
        {
            get
            {
                if (float.IsNaN(_actualPaddingBottom))
                {
                    _actualPaddingBottom = CssValueParser.ParseLength(PaddingBottom, Size.Width, this);
                }
                return _actualPaddingBottom;
            }
        }

        /// <summary>
        /// Gets the actual padding on the right
        /// </summary>
        public float ActualPaddingRight
        {
            get
            {
                if (float.IsNaN(_actualPaddingRight))
                {
                    _actualPaddingRight = CssValueParser.ParseLength(PaddingRight, Size.Width, this);
                }
                return _actualPaddingRight;
            }
        }

        /// <summary>
        /// Gets the actual top's Margin
        /// </summary>
        public float ActualMarginTop
        {
            get
            {
                if (float.IsNaN(_actualMarginTop))
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
        public float CollapsedMarginTop
        {
            get { return float.IsNaN(_collapsedMarginTop) ? 0 : _collapsedMarginTop; }
            set { _collapsedMarginTop = value; }
        }

        /// <summary>
        /// Gets the actual Margin on the left
        /// </summary>
        public float ActualMarginLeft
        {
            get
            {
                if (float.IsNaN(_actualMarginLeft))
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
        public float ActualMarginBottom
        {
            get
            {
                if (float.IsNaN(_actualMarginBottom))
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
        public float ActualMarginRight
        {
            get
            {
                if (float.IsNaN(_actualMarginRight))
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
        public float ActualBorderTopWidth
        {
            get
            {
                if (float.IsNaN(_actualBorderTopWidth))
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
        public float ActualBorderLeftWidth
        {
            get
            {
                if (float.IsNaN(_actualBorderLeftWidth))
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
        public float ActualBorderBottomWidth
        {
            get
            {
                if (float.IsNaN(_actualBorderBottomWidth))
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
        public float ActualBorderRightWidth
        {
            get
            {
                if (float.IsNaN(_actualBorderRightWidth))
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
        public Color ActualBorderTopColor
        {
            get
            {
                if (_actualBorderTopColor.IsEmpty)
                {
                    _actualBorderTopColor = CssValueParser.GetActualColor(BorderTopColor);
                }
                return _actualBorderTopColor;
            }
        }

        /// <summary>
        /// Gets the actual Left border Color
        /// </summary>
        public Color ActualBorderLeftColor
        {
            get
            {
                if ((_actualBorderLeftColor.IsEmpty))
                {
                    _actualBorderLeftColor = CssValueParser.GetActualColor(BorderLeftColor);
                }
                return _actualBorderLeftColor;
            }
        }

        /// <summary>
        /// Gets the actual Bottom border Color
        /// </summary>
        public Color ActualBorderBottomColor
        {
            get
            {
                if ((_actualBorderBottomColor.IsEmpty))
                {
                    _actualBorderBottomColor = CssValueParser.GetActualColor(BorderBottomColor);
                }
                return _actualBorderBottomColor;
            }
        }

        /// <summary>
        /// Gets the actual Right border Color
        /// </summary>
        public Color ActualBorderRightColor
        {
            get
            {
                if ((_actualBorderRightColor.IsEmpty))
                {
                    _actualBorderRightColor = CssValueParser.GetActualColor(BorderRightColor);
                }
                return _actualBorderRightColor;
            }
        }

        /// <summary>
        /// Gets the actual lenght of the north west corner
        /// </summary>
        public float ActualCornerNW
        {
            get
            {
                if (float.IsNaN(_actualCornerNW))
                {
                    _actualCornerNW = CssValueParser.ParseLength(CornerNWRadius, 0, this);
                }
                return _actualCornerNW;
            }
        }

        /// <summary>
        /// Gets the actual lenght of the north east corner
        /// </summary>
        public float ActualCornerNE
        {
            get
            {
                if (float.IsNaN(_actualCornerNE))
                {
                    _actualCornerNE = CssValueParser.ParseLength(CornerNERadius, 0, this);
                }
                return _actualCornerNE;
            }
        }

        /// <summary>
        /// Gets the actual lenght of the south east corner
        /// </summary>
        public float ActualCornerSE
        {
            get
            {
                if (float.IsNaN(_actualCornerSE))
                {
                    _actualCornerSE = CssValueParser.ParseLength(CornerSERadius, 0, this);
                }
                return _actualCornerSE;
            }
        }

        /// <summary>
        /// Gets the actual lenght of the south west corner
        /// </summary>
        public float ActualCornerSW
        {
            get
            {
                if (float.IsNaN(_actualCornerSW))
                {
                    _actualCornerSW = CssValueParser.ParseLength(CornerSWRadius, 0, this);
                }
                return _actualCornerSW;
            }
        }

        /// <summary>
        /// Gets a value indicating if at least one of the corners of the box is rounded
        /// </summary>
        public bool IsRounded
        {
            get { return ActualCornerNE > 0f || ActualCornerNW > 0f || ActualCornerSE > 0f || ActualCornerSW > 0f; }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public float ActualWordSpacing
        {
            get { return _actualWordSpacing; }
        }

        /// <summary>
        /// 
        /// Gets the actual color for the text.
        /// </summary>
        public Color ActualColor
        {
            get
            {

                if (_actualColor.IsEmpty)
                {
                    _actualColor = CssValueParser.GetActualColor(Color);
                }

                return _actualColor;

            }
        }

        /// <summary>
        /// Gets the actual background color of the box
        /// </summary>
        public Color ActualBackgroundColor
        {
            get
            {
                if (_actualBackgroundColor.IsEmpty)
                {
                    _actualBackgroundColor = CssValueParser.GetActualColor(BackgroundColor);
                }

                return _actualBackgroundColor;
            }
        }

        /// <summary>
        /// Gets the second color that creates a gradient for the background
        /// </summary>
        public Color ActualBackgroundGradient
        {
            get
            {
                if (_actualBackgroundGradient.IsEmpty)
                {
                    _actualBackgroundGradient = CssValueParser.GetActualColor(BackgroundGradient);
                }
                return _actualBackgroundGradient;
            }
        }

        /// <summary>
        /// Gets the actual angle specified for the background gradient
        /// </summary>
        public float ActualBackgroundGradientAngle
        {
            get
            {
                if (float.IsNaN(_actualBackgroundGradientAngle))
                {
                    _actualBackgroundGradientAngle = CssValueParser.ParseNumber(BackgroundGradientAngle, 360f);
                }

                return _actualBackgroundGradientAngle;
            }
        }

        /// <summary>
        /// Gets the actual font of the parent
        /// </summary>
        public Font ActualParentFont
        {
            get { return GetParent() == null ? ActualFont : GetParent().ActualFont; }
        }

        /// <summary>
        /// Gets the font that should be actually used to paint the text of the box
        /// </summary>
        public Font ActualFont
        {
            get
            {
                if (_actualFont == null)
                {
                    if (string.IsNullOrEmpty(FontFamily)) { FontFamily = CssConstants.FontSerif; }
                    if (string.IsNullOrEmpty(FontSize)) { FontSize = CssConstants.FontSize.ToString(CultureInfo.InvariantCulture) + "pt"; }

                    FontStyle st = System.Drawing.FontStyle.Regular;

                    if (FontStyle == CssConstants.Italic || FontStyle == CssConstants.Oblique)
                    {
                        st |= System.Drawing.FontStyle.Italic;
                    }

                    if (FontWeight != CssConstants.Normal && FontWeight != CssConstants.Lighter && !string.IsNullOrEmpty(FontWeight) && FontWeight != CssConstants.Inherit)
                    {
                        st |= System.Drawing.FontStyle.Bold;
                    }

                    float fsize;
                    float parentSize = CssConstants.FontSize;

                    if (GetParent() != null)
                        parentSize = GetParent().ActualFont.Size;

                    switch (FontSize)
                    {
                        case CssConstants.Medium:
                            fsize = CssConstants.FontSize; break;
                        case CssConstants.XXSmall:
                            fsize = CssConstants.FontSize - 4; break;
                        case CssConstants.XSmall:
                            fsize = CssConstants.FontSize - 3; break;
                        case CssConstants.Small:
                            fsize = CssConstants.FontSize - 2; break;
                        case CssConstants.Large:
                            fsize = CssConstants.FontSize + 2; break;
                        case CssConstants.XLarge:
                            fsize = CssConstants.FontSize + 3; break;
                        case CssConstants.XXLarge:
                            fsize = CssConstants.FontSize + 4; break;
                        case CssConstants.Smaller:
                            fsize = parentSize - 2; break;
                        case CssConstants.Larger:
                            fsize = parentSize + 2; break;
                        default:
                            fsize = CssValueParser.ParseLength(FontSize, parentSize, parentSize, null, true, true);
                            break;
                    }

                    if (fsize <= 1f)
                    {
                        fsize = CssConstants.FontSize;
                    }

                    _actualFont = FontsUtils.GetCachedFont(FontFamily, fsize, st);
                }
                return _actualFont;
            }
        }

        /// <summary>
        /// Gets the line height
        /// </summary>
        public float ActualLineHeight
        {
            get
            {
                if (float.IsNaN(_actualLineHeight))
                {
                    _actualLineHeight = .9f*CssValueParser.ParseLength(LineHeight, Size.Height, this);
                }
                return _actualLineHeight;
            }
        }

        /// <summary>
        /// Gets the text indentation (on first line only)
        /// </summary>
        public float ActualTextIndent
        {
            get
            {
                if (float.IsNaN(_actualTextIndent))
                {
                    _actualTextIndent = CssValueParser.ParseLength(TextIndent, Size.Width, this);
                }

                return _actualTextIndent;
            }
        }

        /// <summary>
        /// Gets the actual horizontal border spacing for tables
        /// </summary>
        public float ActualBorderSpacingHorizontal
        {
            get
            {
                if (float.IsNaN(_actualBorderSpacingHorizontal))
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
        public float ActualBorderSpacingVertical
        {
            get
            {
                if (float.IsNaN(_actualBorderSpacingVertical))
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
        public float GetEmHeight()
        {
            return FontsUtils.GetFontHeight(ActualFont);
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
        protected void MeasureWordSpacing(IGraphics g)
        {
            if (float.IsNaN(ActualWordSpacing))
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
                _borderSpacing = p._borderSpacing;
                _borderCollapse = p._borderCollapse;
                _color = p._color;
                _emptyCells = p._emptyCells;
                _whiteSpace = p._whiteSpace;
                _visibility = p._visibility;
                _textIndent = p._textIndent;
                _textAlign = p._textAlign;
                _verticalAlign = p._verticalAlign;
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
                    _backgroundGradient = p._backgroundGradient;
                    _backgroundGradientAngle = p._backgroundGradientAngle;
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
                    _cornerNWRadius = p._cornerNWRadius;
                    _cornerNERadius = p._cornerNERadius;
                    _cornerSERadius = p._cornerSERadius;
                    _cornerSWRadius = p._cornerSWRadius;
                    _cornerRadius = p._cornerRadius;
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
                    _width = p._width;
                    _maxWidth = p._maxWidth;
                    _wordSpacing = p._wordSpacing;
                }
            }
        }
    }
}