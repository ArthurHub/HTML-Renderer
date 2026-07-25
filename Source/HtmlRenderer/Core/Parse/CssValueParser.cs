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
using System.Linq;
using System.Text;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
    /// <summary>
    /// Parse CSS properties values like numbers, Urls, etc.
    /// </summary>
    internal sealed class CssValueParser
    {
        #region Fields and Consts

        /// <summary>
        /// 
        /// </summary>
        private readonly RAdapter _adapter;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public CssValueParser(RAdapter adapter)
        {
            ArgChecker.AssertArgNotNull(adapter, "global");

            _adapter = adapter;
        }

        /// <summary>
        /// Check if the given substring is a valid double number.
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>true - valid double number, false - otherwise</returns>
        public static bool IsFloat(string str, int idx, int length)
        {
            if (length < 1)
                return false;

            bool sawDot = false;
            for (int i = 0; i < length; i++)
            {
                if (str[idx + i] == '.')
                {
                    if (sawDot)
                        return false;
                    sawDot = true;
                }
                else if (!char.IsDigit(str[idx + i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if the given substring is a valid double number.
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>true - valid int number, false - otherwise</returns>
        public static bool IsInt(string str, int idx, int length)
        {
            if (length < 1)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (!char.IsDigit(str[idx + i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the given string is a valid length value.
        /// </summary>
        /// <param name="value">the string value to check</param>
        /// <returns>true - valid, false - invalid</returns>
        public static bool IsValidLength(string value)
        {
            if (IsCalcFunction(value))
                return true;

            if (value.Length > 1)
            {
                string number = string.Empty;
                if (value.EndsWith("%"))
                {
                    number = value.Substring(0, value.Length - 1);
                }
                else if (value.Length > 2)
                {
                    number = value.Substring(0, value.Length - 2);
                }
                double stub;
                return double.TryParse(number, out stub);
            }
            return false;
        }

        /// <summary>
        /// Tokenizes <paramref name="value"/> using the vendored CSS lexer (Core/CssEngine/Parser/Lexer.cs),
        /// skipping whitespace/end-of-file tokens - the same "just enough tokenization to recognize a
        /// single top-level function call" approach used to detect calc()-family expressions.
        /// </summary>
        internal static List<Token> GetCssTokens(string value)
        {
            var lexer = new Lexer(value);
            var tokens = new List<Token>();
            Token token;
            do
            {
                token = lexer.Get();
                if (token.Type != TokenType.EndOfFile && token.Type != TokenType.Whitespace)
                {
                    tokens.Add(token);
                }
            } while (token.Type != TokenType.EndOfFile);

            return tokens;
        }

        /// <summary>
        /// Recognizes a length string that is a single calc-family (calc/min/max/clamp) function call.
        /// Real grammar/type validation happens in the vendored CSS-OM's CalcValueConverter at parse
        /// time (for any value that didn't arrive via var() substitution); this is a syntactic
        /// recognizer only, used to gate the evaluation branch in <see cref="ParseLength(string, double, double, string, bool, bool)"/>.
        /// </summary>
        public static bool IsCalcFunction(string value)
        {
            FunctionToken function;
            return TryGetCalcFunction(value, out function);
        }

        private static bool TryGetCalcFunction(string value, out FunctionToken function)
        {
            var tokens = GetCssTokens(value);
            if (tokens.Count == 1)
            {
                var fn = tokens[0] as FunctionToken;
                if (fn != null && CalcParser.IsCalcFamily(fn.Data))
                {
                    function = fn;
                    return true;
                }
            }

            function = null;
            return false;
        }

        /// <summary>
        /// Evals a number and returns it. If number is a percentage, it will be multiplied by <see cref="hundredPercent"/>
        /// </summary>
        /// <param name="number">Number to be parsed</param>
        /// <param name="hundredPercent">Number that represents the 100% if parsed number is a percentage</param>
        /// <returns>Parsed number. Zero if error while parsing.</returns>
        public static double ParseNumber(string number, double hundredPercent)
        {
            if (string.IsNullOrEmpty(number))
            {
                return 0f;
            }

            string toParse = number;
            bool isPercent = number.EndsWith("%");
            double result;

            if (isPercent)
                toParse = number.Substring(0, number.Length - 1);

            if (!double.TryParse(toParse, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out result))
            {
                return 0f;
            }

            if (isPercent)
            {
                result = (result / 100f) * hundredPercent;
            }

            return result;
        }

        /// <summary>
        /// Parses a length. Lengths are followed by an unit identifier (e.g. 10px, 3.1em)
        /// </summary>
        /// <param name="length">Specified length</param>
        /// <param name="hundredPercent">Equivalent to 100 percent when length is percentage</param>
        /// <param name="fontAdjust">if the length is in pixels and the length is font related it needs to use 72/96 factor</param>
        /// <param name="box"></param>
        /// <returns>the parsed length value with adjustments</returns>
        public static double ParseLength(string length, double hundredPercent, CssBoxProperties box, bool fontAdjust = false)
        {
            return ParseLength(length, hundredPercent, box.GetEmHeight(), null, fontAdjust, false);
        }

        /// <summary>
        /// Parses a length. Lengths are followed by an unit identifier (e.g. 10px, 3.1em)
        /// </summary>
        /// <param name="length">Specified length</param>
        /// <param name="hundredPercent">Equivalent to 100 percent when length is percentage</param>
        /// <param name="box"></param>
        /// <param name="defaultUnit"></param>
        /// <returns>the parsed length value with adjustments</returns>
        public static double ParseLength(string length, double hundredPercent, CssBoxProperties box, string defaultUnit)
        {
            return ParseLength(length, hundredPercent, box.GetEmHeight(), defaultUnit, false, false);
        }

        /// <summary>
        /// Parses a length. Lengths are followed by an unit identifier (e.g. 10px, 3.1em)
        /// </summary>
        /// <param name="length">Specified length</param>
        /// <param name="hundredPercent">Equivalent to 100 percent when length is percentage</param>
        /// <param name="emFactor"></param>
        /// <param name="defaultUnit"></param>
        /// <param name="fontAdjust">if the length is in pixels and the length is font related it needs to use 72/96 factor</param>
        /// <param name="returnPoints">Allows the return double to be in points. If false, result will be pixels</param>
        /// <returns>the parsed length value with adjustments</returns>
        public static double ParseLength(string length, double hundredPercent, double emFactor, string defaultUnit, bool fontAdjust, bool returnPoints)
        {
            //Return zero if no length specified, zero specified
            if (string.IsNullOrEmpty(length) || length == "0")
                return 0f;

            //If percentage, use ParseNumber
            if (length.EndsWith("%"))
                return ParseNumber(length, hundredPercent);

            // calc()/min()/max()/clamp(): evaluate via the vendored Calc engine (Core/CssEngine/Calc/)
            // instead of falling through to the unit-suffix parsing below, which doesn't understand
            // function syntax at all.
            FunctionToken calcFunction;
            if (TryGetCalcFunction(length, out calcFunction))
            {
                var node = CalcParser.Parse(calcFunction);
                var context = new CalcContext(hundredPercent, emFactor, emFactor, fontAdjust, returnPoints);
                var pixels = node != null ? CalcEvaluator.Evaluate(node, context) : null;
                return pixels ?? 0d;
            }

            //Get units of the length
            bool hasUnit;
            string unit = GetUnit(length, defaultUnit, out hasUnit);

            //Factor will depend on the unit
            double factor;

            //Number of the length
            string number = hasUnit ? length.Substring(0, length.Length - 2) : length;

            //TODO: Units behave different in paper and in screen!
            switch (unit)
            {
                case CssConstants.Em:
                    factor = emFactor;
                    break;
                case CssConstants.Ex:
                    factor = emFactor / 2;
                    break;
                case CssConstants.Px:
                    factor = fontAdjust ? 72f / 96f : 1f; //TODO:a check support for hi dpi
                    break;
                case CssConstants.Mm:
                    factor = 3.779527559f; //3 pixels per millimeter
                    break;
                case CssConstants.Cm:
                    factor = 37.795275591f; //37 pixels per centimeter
                    break;
                case CssConstants.In:
                    factor = 96f; //96 pixels per inch
                    break;
                case CssConstants.Pt:
                    factor = 96f / 72f; // 1 point = 1/72 of inch

                    if (returnPoints)
                    {
                        return ParseNumber(number, hundredPercent);
                    }

                    break;
                case CssConstants.Pc:
                    factor = 16f; // 1 pica = 12 points
                    break;
                default:
                    factor = 0f;
                    break;
            }

            return factor * ParseNumber(number, hundredPercent);
        }

        /// <summary>
        /// Get the unit to use for the length, use default if no unit found in length string.
        /// </summary>
        private static string GetUnit(string length, string defaultUnit, out bool hasUnit)
        {
            var unit = length.Length >= 3 ? length.Substring(length.Length - 2, 2) : string.Empty;
            switch (unit)
            {
                case CssConstants.Em:
                case CssConstants.Ex:
                case CssConstants.Px:
                case CssConstants.Mm:
                case CssConstants.Cm:
                case CssConstants.In:
                case CssConstants.Pt:
                case CssConstants.Pc:
                    hasUnit = true;
                    break;
                default:
                    hasUnit = false;
                    unit = defaultUnit ?? String.Empty;
                    break;
            }
            return unit;
        }

        /// <summary>
        /// Check if the given color string value is valid.
        /// </summary>
        /// <param name="colorValue">color string value to parse</param>
        /// <returns>true - valid, false - invalid</returns>
        public bool IsColorValid(string colorValue)
        {
            RColor color;
            return TryGetColor(colorValue, 0, colorValue.Length, out color);
        }

        /// <summary>
        /// Parses a color value in CSS style; e.g. #ff0000, red, rgb(255,0,0), rgb(100%, 0, 0)
        /// </summary>
        /// <param name="colorValue">color string value to parse</param>
        /// <returns>Color value</returns>
        public RColor GetActualColor(string colorValue)
        {
            RColor color;
            TryGetColor(colorValue, 0, colorValue.Length, out color);
            return color;
        }

        /// <summary>
        /// Parses a color value in CSS style; e.g. #ff0000, RED, RGB(255,0,0), RGB(100%, 0, 0)
        /// </summary>
        /// <param name="str">color substring value to parse</param>
        /// <param name="idx">substring start idx </param>
        /// <param name="length">substring length</param>
        /// <param name="color">return the parsed color</param>
        /// <returns>true - valid color, false - otherwise</returns>
        public bool TryGetColor(string str, int idx, int length, out RColor color)
        {
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    if (length > 1 && str[idx] == '#')
                    {
                        return GetColorByHex(str, idx, length, out color);
                    }
                    else if (length > 10 && CommonUtils.SubStringEquals(str, idx, 4, "rgb(") && str[length - 1] == ')')
                    {
                        return GetColorByRgb(str, idx, length, out color);
                    }
                    else if (length > 13 && CommonUtils.SubStringEquals(str, idx, 5, "rgba(") && str[length - 1] == ')')
                    {
                        return GetColorByRgba(str, idx, length, out color);
                    }
                    else if (length > 9 && CommonUtils.SubStringEquals(str, idx, 4, "hsl(") && str[length - 1] == ')')
                    {
                        return GetColorByHsl(str, idx, length, false, out color);
                    }
                    else if (length > 10 && CommonUtils.SubStringEquals(str, idx, 5, "hsla(") && str[length - 1] == ')')
                    {
                        return GetColorByHsl(str, idx, length, true, out color);
                    }
                    else
                    {
                        return GetColorByName(str, idx, length, out color);
                    }
                }
            }
            catch
            { }
            color = RColor.Black;
            return false;
        }

        /// <summary>
        /// Parses a border value in CSS style; e.g. 1px, 1, thin, thick, medium
        /// </summary>
        /// <param name="borderValue"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double GetActualBorderWidth(string borderValue, CssBoxProperties b)
        {
            if (string.IsNullOrEmpty(borderValue))
            {
                return GetActualBorderWidth(CssConstants.Medium, b);
            }

            switch (borderValue)
            {
                case CssConstants.Thin:
                    return 1f;
                case CssConstants.Medium:
                    return 2f;
                case CssConstants.Thick:
                    return 4f;
                default:
                    return Math.Abs(ParseLength(borderValue, 1, b));
            }
        }


        #region Private methods

        /// <summary>
        /// Get color by parsing given hex value color string (#A28B34).
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        private static bool GetColorByHex(string str, int idx, int length, out RColor color)
        {
            int r = -1;
            int g = -1;
            int b = -1;
            if (length == 7)
            {
                r = ParseHexInt(str, idx + 1, 2);
                g = ParseHexInt(str, idx + 3, 2);
                b = ParseHexInt(str, idx + 5, 2);
            }
            else if (length == 4)
            {
                r = ParseHexInt(str, idx + 1, 1);
                r = r * 16 + r;
                g = ParseHexInt(str, idx + 2, 1);
                g = g * 16 + g;
                b = ParseHexInt(str, idx + 3, 1);
                b = b * 16 + b;
            }
            if (r > -1 && g > -1 && b > -1)
            {
                color = RColor.FromArgb(r, g, b);
                return true;
            }
            color = RColor.Empty;
            return false;
        }

        /// <summary>
        /// Get color by parsing given RGB value color string (RGB(255,180,90))
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        private static bool GetColorByRgb(string str, int idx, int length, out RColor color)
        {
            int r = -1;
            int g = -1;
            int b = -1;

            if (length > 10)
            {
                int s = idx + 4;
                r = ParseIntAtIndex(str, ref s);
                if (s < idx + length)
                {
                    g = ParseIntAtIndex(str, ref s);
                }
                if (s < idx + length)
                {
                    b = ParseIntAtIndex(str, ref s);
                }
            }

            if (r > -1 && g > -1 && b > -1)
            {
                color = RColor.FromArgb(r, g, b);
                return true;
            }
            color = RColor.Empty;
            return false;
        }

        /// <summary>
        /// Get color by parsing given RGBA value color string (RGBA(255,180,90,180))
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        private static bool GetColorByRgba(string str, int idx, int length, out RColor color)
        {
            int r = -1;
            int g = -1;
            int b = -1;
            int a = -1;

            if (length > 13)
            {
                int s = idx + 5;
                r = ParseIntAtIndex(str, ref s);

                if (s < idx + length)
                {
                    g = ParseIntAtIndex(str, ref s);
                }
                if (s < idx + length)
                {
                    b = ParseIntAtIndex(str, ref s);
                }
                if (s < idx + length)
                {
                    a = ParseAlphaAtIndex(str, ref s);
                }
            }

            if (r > -1 && g > -1 && b > -1 && a > -1)
            {
                color = RColor.FromArgb(a, r, g, b);
                return true;
            }
            color = RColor.Empty;
            return false;
        }

        /// <summary>
        /// Parses an rgba()/hsla()-style alpha component, per CSS syntax a fractional number in [0,1]
        /// (e.g. "0.5") or a percentage (e.g. "50%") - unlike R/G/B, never a bare 0-255 integer. This is
        /// also the canonical form the vendored CSS engine's Color.ToString() emits for any color with
        /// partial transparency (hex-alpha, hsl()/hsla(), hwb(), modern space-separated rgb() syntax -
        /// all normalized to "rgba(r, g, b, alpha)" at CSS-OM parse time), so this is the path that
        /// makes those all resolve correctly here, not just literal author-written rgba().
        /// </summary>
        /// <returns>alpha as a 0-255 byte value, or -1 if invalid</returns>
        private static int ParseAlphaAtIndex(string str, ref int startIdx)
        {
            while (startIdx < str.Length && char.IsWhiteSpace(str, startIdx))
                startIdx++;

            var start = startIdx;
            var len = 0;
            while (start + len < str.Length && (char.IsDigit(str, start + len) || str[start + len] == '.'))
                len++;

            var isPercent = start + len < str.Length && str[start + len] == '%';

            if (len < 1)
            {
                startIdx = start + len + (isPercent ? 1 : 0) + 1;
                return -1;
            }

            double value;
            if (!double.TryParse(str.Substring(start, len), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out value))
            {
                startIdx = start + len + (isPercent ? 1 : 0) + 1;
                return -1;
            }

            if (isPercent)
            {
                value = value / 100.0;
                len++; // include the '%' when advancing startIdx below
            }

            startIdx = start + len + 1;

            var alpha = (int)Math.Round(value * 255.0);
            return alpha < 0 ? 0 : (alpha > 255 ? 255 : alpha);
        }

        /// <summary>
        /// Get color by parsing given HSL/HSLA value color string (hsl(210, 100%, 50%), hsla(210, 100%, 50%, 0.5)).<br/>
        /// Unlike hex and rgb()/rgba(), the vendored CSS-OM's HslColorConverter/HslaColorConverter only
        /// validate hsl() syntax at parse time (hue is a valid angle, saturation/lightness are
        /// percentages) - they never actually convert it to an RGB Color the way rgb()/rgba()/hex do
        /// (see Core/CssEngine/Model/Converters.cs's HslColorConverter/HslaColorConverter, and compare
        /// to Color.ToString() which only ever emits rgb()/rgba()). So unlike those, the raw string this
        /// engine sees for an hsl()-declared color is still the literal hsl() text (with hue normalized
        /// to a "Ndeg" suffix), not a canonical rgba() - this method does the actual hue/sat/light -> RGB
        /// conversion, reusing the vendored Color.FromHsla for the math.
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        private static bool GetColorByHsl(string str, int idx, int length, bool hasAlpha, out RColor color)
        {
            var openLen = hasAlpha ? 5 : 4; // "hsla(" or "hsl("
            var inner = str.Substring(idx + openLen, length - openLen - 1);
            var parts = inner.Split(',');
            if (parts.Length != (hasAlpha ? 4 : 3))
            {
                color = RColor.Empty;
                return false;
            }

            double hueDegrees;
            if (!TryParseAngleDegrees(parts[0].Trim(), out hueDegrees))
            {
                color = RColor.Empty;
                return false;
            }

            double saturation, lightness;
            if (!double.TryParse(parts[1].Trim().TrimEnd('%'), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out saturation) ||
                !double.TryParse(parts[2].Trim().TrimEnd('%'), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out lightness))
            {
                color = RColor.Empty;
                return false;
            }

            var alpha = 1.0;
            if (hasAlpha)
            {
                var alphaStr = parts[3].Trim();
                if (alphaStr.EndsWith("%"))
                {
                    double a;
                    double.TryParse(alphaStr.TrimEnd('%'), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out a);
                    alpha = a / 100.0;
                }
                else
                {
                    double.TryParse(alphaStr, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out alpha);
                }
            }

            var hueFraction = ((hueDegrees % 360.0) + 360.0) % 360.0 / 360.0;
            var vendored = CssEngine.Color.FromHsla((float)hueFraction, (float)(saturation / 100.0), (float)(lightness / 100.0), (float)alpha);
            color = RColor.FromArgb(vendored.A, vendored.R, vendored.G, vendored.B);
            return true;
        }

        /// <summary>
        /// Parses an hsl() hue component - a bare number (treated as degrees, per CSS) or a number with
        /// an explicit angle unit (deg/rad/grad/turn).
        /// </summary>
        private static bool TryParseAngleDegrees(string value, out double degrees)
        {
            string unit = null;
            foreach (var candidate in new[] { "turn", "grad", "deg", "rad" })
            {
                if (value.EndsWith(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    unit = candidate;
                    break;
                }
            }

            var numberPart = unit != null ? value.Substring(0, value.Length - unit.Length) : value;
            double number;
            if (!double.TryParse(numberPart, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out number))
            {
                degrees = 0;
                return false;
            }

            if (unit == "turn") degrees = number * 360.0;
            else if (unit == "grad") degrees = number * 0.9;
            else if (unit == "rad") degrees = number * 180.0 / Math.PI;
            else degrees = number; // "deg" or bare number

            return true;
        }

        /// <summary>
        /// Get color by given name, including .NET name.
        /// </summary>
        /// <returns>true - valid color, false - otherwise</returns>
        private bool GetColorByName(string str, int idx, int length, out RColor color)
        {
            color = _adapter.GetColor(str.Substring(idx, length));
            return color.A > 0;
        }

        /// <summary>
        /// Parse the given decimal number string to positive int value.<br/>
        /// Start at given <paramref name="startIdx"/>, ignore whitespaces and take
        /// as many digits as possible to parse to int.
        /// </summary>
        /// <param name="str">the string to parse</param>
        /// <param name="startIdx">the index to start parsing at</param>
        /// <returns>parsed int or 0</returns>
        private static int ParseIntAtIndex(string str, ref int startIdx)
        {
            int len = 0;
            while (char.IsWhiteSpace(str, startIdx))
                startIdx++;
            while (char.IsDigit(str, startIdx + len))
                len++;
            var val = ParseInt(str, startIdx, len);
            startIdx = startIdx + len + 1;
            return val;
        }

        /// <summary>
        /// Parse the given decimal number string to positive int value.
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>int value, -1 if not valid</returns>
        private static int ParseInt(string str, int idx, int length)
        {
            if (length < 1)
                return -1;

            int num = 0;
            for (int i = 0; i < length; i++)
            {
                int c = str[idx + i];
                if (!(c >= 48 && c <= 57))
                    return -1;

                num = num * 10 + c - 48;
            }
            return num;
        }

        /// <summary>
        /// Parse the given hex number string to positive int value.
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>int value, -1 if not valid</returns>
        private static int ParseHexInt(string str, int idx, int length)
        {
            if (length < 1)
                return -1;

            int num = 0;
            for (int i = 0; i < length; i++)
            {
                int c = str[idx + i];
                if (!(c >= 48 && c <= 57) && !(c >= 65 && c <= 70) && !(c >= 97 && c <= 102))
                    return -1;

                num = num * 16 + (c <= 57 ? c - 48 : (10 + c - (c <= 70 ? 65 : 97)));
            }
            return num;
        }

        #endregion


        #region Background image / gradient parsing

        /// <summary>
        /// Parses a single <c>background-image</c> value: either a <c>url()</c> reference or a
        /// <c>linear-gradient()</c>/<c>repeating-linear-gradient()</c> function. Returns null for "none",
        /// an empty value, or anything else this engine doesn't recognize (matching this engine's existing
        /// silent-failure behavior for unsupported values).<br/>
        /// Trimmed to the single-value,
        /// linear-gradient-only case this engine supports (no layered background-image, no radial/conic).
        /// </summary>
        public CssImage ParseImage(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, CssConstants.None, StringComparison.OrdinalIgnoreCase))
                return null;

            var tokens = GetCssTokens(value);

            var urlToken = tokens.OfType<UrlToken>().FirstOrDefault();
            if (urlToken != null)
                return new CssImage.Url(urlToken.Data);

            var funcToken = tokens.OfType<FunctionToken>().FirstOrDefault();
            if (funcToken == null)
                return null;

            if (string.Equals(funcToken.Data, FunctionNames.LinearGradient, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(funcToken.Data, FunctionNames.RepeatingLinearGradient, StringComparison.OrdinalIgnoreCase))
            {
                var gradient = ParseLinearGradient(value);
                return gradient != null ? new CssImage.LinearGradient(gradient) : null;
            }

            return null;
        }

        /// <summary>
        /// Parses a <c>linear-gradient()</c>/<c>repeating-linear-gradient()</c> function value: an
        /// optional angle or "to &lt;side&gt;" direction (default 180deg, top to bottom), followed by 2+
        /// comma-separated color stops (each optionally followed by 1-2 length/percent positions), and
        /// bare-position color hints between stops. Returns null if the value isn't a recognized gradient
        /// function or doesn't have at least 2 real color stops.<br/>
        /// Linear-gradient parsing, minus CSS Color 4
        /// interpolation-color-space ("in oklab" etc.) support - see ParsedLinearGradient.
        /// </summary>
        private ParsedLinearGradient ParseLinearGradient(string value)
        {
            var tokens = GetCssTokens(value);

            var funcToken = tokens.OfType<FunctionToken>().FirstOrDefault(t =>
                string.Equals(t.Data, FunctionNames.LinearGradient, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Data, FunctionNames.RepeatingLinearGradient, StringComparison.OrdinalIgnoreCase));

            if (funcToken == null)
                return null;

            bool isRepeating = string.Equals(funcToken.Data, FunctionNames.RepeatingLinearGradient, StringComparison.OrdinalIgnoreCase);

            var args = funcToken.ArgumentTokens.ToList();
            if (args.Count == 0)
                return null;

            double angleRad = Math.PI; // default: 180deg = top to bottom
            int stopOffset = 0;

            var firstGroup = args[0];
            var firstIdents = firstGroup.Where(t => t.Type == TokenType.Ident).Select(t => t.Data.ToLowerInvariant()).ToList();

            if (firstIdents.Count > 0 && firstIdents[0] == "to")
            {
                // keyword direction: "to right", "to bottom left", etc.
                angleRad = SideKeywordsToAngleRad(firstIdents.Skip(1).ToList());
                stopOffset = 1;
            }
            else
            {
                var angle = firstGroup.ToAngle();
                if (angle.HasValue)
                {
                    angleRad = angle.Value.ToRadian();
                    stopOffset = 1;
                }
                // else no angle token, stopOffset stays 0 - first group is already the first color stop
            }

            var stopGroups = args.Skip(stopOffset).ToList();
            if (stopGroups.Count < 2)
                return null;

            var stops = new List<(RColor? Color, Length? Position, bool IsHint)>();

            foreach (var group in stopGroups)
            {
                var items = group.ToItems();
                if (items.Count == 0)
                    continue;

                Length? position1 = null;
                Length? position2 = null;
                int colorItemCount = items.Count;

                // Last item may be a length/percent position.
                var lastItem = items[items.Count - 1];
                var pv = lastItem.ToDistance();
                if (pv.HasValue)
                {
                    position1 = pv.Value;
                    colorItemCount--;

                    // Two-position shorthand (e.g. "red 0 50%").
                    if (colorItemCount > 0)
                    {
                        var pv2 = items[colorItemCount - 1].ToDistance();
                        if (pv2.HasValue)
                        {
                            position2 = position1;
                            position1 = pv2.Value;
                            colorItemCount--;
                        }
                    }
                }

                if (colorItemCount == 0)
                {
                    // Bare position with no color - a color hint.
                    if (position1.HasValue && !position2.HasValue)
                        stops.Add((null, position1, true));
                    continue;
                }

                var colorText = BuildColorText(items.Take(colorItemCount));
                if (string.IsNullOrWhiteSpace(colorText))
                    continue;

                var color = GetActualColor(colorText);
                stops.Add((color, position1, false));
                if (position2.HasValue)
                    stops.Add((color, position2, false));
            }

            if (stops.Count(s => !s.IsHint) < 2)
                return null;

            return new ParsedLinearGradient
            {
                AngleRad = angleRad,
                Stops = stops.ToArray(),
                IsRepeating = isRepeating,
            };
        }

        /// <summary>
        /// Converts "to &lt;side&gt; [&lt;side&gt;]" direction keywords (e.g. "right", "bottom left") to
        /// the equivalent gradient-line angle in radians, per the CSS Images spec's side/corner table.
        /// </summary>
        private static double SideKeywordsToAngleRad(List<string> sides)
        {
            bool hasTop = sides.Contains("top");
            bool hasBottom = sides.Contains("bottom");
            bool hasLeft = sides.Contains("left");
            bool hasRight = sides.Contains("right");

            if (hasTop && hasRight) return Math.PI / 4;         // 45deg
            if (hasBottom && hasRight) return 3 * Math.PI / 4;  // 135deg
            if (hasBottom && hasLeft) return 5 * Math.PI / 4;   // 225deg
            if (hasTop && hasLeft) return 7 * Math.PI / 4;      // 315deg
            if (hasTop) return 0;                                // 0deg
            if (hasRight) return Math.PI / 2;                    // 90deg
            if (hasBottom) return Math.PI;                       // 180deg
            if (hasLeft) return 3 * Math.PI / 2;                // 270deg

            return Math.PI; // default
        }

        /// <summary>
        /// Re-serializes a color stop's token groups (everything before its trailing position token(s))
        /// back to CSS text so it can be handed to <see cref="GetActualColor"/> as a normal color string.
        /// </summary>
        private static string BuildColorText(IEnumerable<IEnumerable<Token>> itemGroups)
        {
            var sb = new StringBuilder();
            foreach (var group in itemGroups)
            {
                sb.Append(group.ToText());
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Splits a CSS value string on top-level whitespace (paren-depth-aware, so a calc()/gradient()
        /// value's internal spaces aren't mistaken for a delimiter). Used to split a "border-radius"
        /// corner value like "10px 5%" into its horizontal/vertical components.
        /// </summary>
        internal static IEnumerable<string> SplitTopLevelWhitespace(string value)
        {
            int depth = 0, start = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (char.IsWhiteSpace(c) && depth == 0)
                {
                    if (i > start) yield return value.Substring(start, i - start);
                    start = i + 1;
                }
            }
            if (start < value.Length) yield return value.Substring(start);
        }

        #endregion
    }
}