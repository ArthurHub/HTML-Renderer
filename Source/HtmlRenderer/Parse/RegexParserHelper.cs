// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HtmlRenderer.Parse
{
    /// <summary>
    /// Collection of regular expressions used when parsing
    /// </summary>
    internal static class RegexParserHelper
    {
        #region Fields and Consts

        /// <summary>
        /// Extracts CSS style comments; e.g. /* comment */
        /// </summary>
        public const string CssComments = @"/\*[^*/]*\*/";

        /// <summary>
        /// Extracts the media types from a media at-rule; e.g. @media print, 3d, screen {
        /// </summary>
        public const string CssMediaTypes = @"@media[^\{\}]*\{";

        /// <summary>
        /// Extracts defined blocks in CSS. 
        /// WARNING: Blocks will include blocks inside at-rules.
        /// </summary>
        public const string CssBlocks = @"[^\{\}]*\{[^\{\}]*\}";

        /// <summary>
        /// Extracts a number; e.g.  5, 6, 7.5, 0.9
        /// </summary>
        public const string CssNumber = @"{[0-9]+|[0-9]*\.[0-9]+}";

        /// <summary>
        /// Extracts css percentages from the string; e.g. 100% .5% 5.4%
        /// </summary>
        public const string CssPercentage = @"([0-9]+|[0-9]*\.[0-9]+)\%"; //TODO: Check if works fine

        /// <summary>
        /// Extracts CSS lengths; e.g. 9px 3pt .89em
        /// </summary>
        public const string CssLength = @"([0-9]+|[0-9]*\.[0-9]+)(em|ex|px|in|cm|mm|pt|pc)";

        /// <summary>
        /// Extracts CSS colors; e.g. black white #fff #fe98cd rgb(5,5,5) rgb(45%, 0, 0)
        /// </summary>
        public const string CssColors = @"(#\S{6}|#\S{3}|rgb\(\s*[0-9]{1,3}\%?\s*\,\s*[0-9]{1,3}\%?\s*\,\s*[0-9]{1,3}\%?\s*\)|maroon|red|orange|yellow|olive|purple|fuchsia|white|lime|green|navy|blue|aqua|teal|black|silver|gray)";

        /// <summary>
        /// Extracts line-height values (normal, numbers, lengths, percentages)
        /// </summary>
        public const string CssLineHeight = "(normal|" + CssNumber + "|" + CssLength + "|" + CssPercentage + ")";

        /// <summary>
        /// Extracts CSS border styles; e.g. solid none dotted
        /// </summary>
        public const string CssBorderStyle = @"(none|hidden|dotted|dashed|solid|double|groove|ridge|inset|outset)";

        /// <summary>
        /// Extracts CSS border widthe; e.g. 1px thin 3em
        /// </summary>
        public const string CssBorderWidth = "(" + CssLength + "|thin|medium|thick)";

        /// <summary>
        /// Extracts font-family values
        /// </summary>
        public const string CssFontFamily = "(\"[^\"]*\"|'[^']*'|\\S+\\s*)(\\s*\\,\\s*(\"[^\"]*\"|'[^']*'|\\S+))*";

        /// <summary>
        /// Extracts CSS font-styles; e.g. normal italic oblique
        /// </summary>
        public const string CssFontStyle = "(normal|italic|oblique)";

        /// <summary>
        /// Extracts CSS font-variant values; e.g. normal, small-caps
        /// </summary>
        public const string CssFontVariant = "(normal|small-caps)";

        /// <summary>
        /// Extracts font-weight values; e.g. normal, bold, bolder...
        /// </summary>
        public const string CssFontWeight = "(normal|bold|bolder|lighter|100|200|300|400|500|600|700|800|900)";

        /// <summary>
        /// Exracts font sizes: xx-small, larger, small, 34pt, 30%, 2em
        /// </summary>
        public const string CssFontSize = "(" + CssLength + "|" + CssPercentage + "|xx-small|x-small|small|medium|large|x-large|xx-large|larger|smaller)";

        /// <summary>
        /// Gets the font-size[/line-height]? on the font shorthand property.
        /// Check http://www.w3.org/TR/CSS21/fonts.html#font-shorthand
        /// </summary>
        public const string CssFontSizeAndLineHeight = CssFontSize + @"(\/" + CssLineHeight + @")?(\s|$)";

        /// <summary>
        /// Extracts HTML tags
        /// </summary>
        public const string HtmlTag = @"<[^<>]*>";

        /// <summary>
        /// Extracts attributes from a HTML tag; e.g. att=value, att="value"
        /// </summary>
        public const string HmlTagAttributes = "(?<name>\\b\\w+\\b)\\s*=\\s*(?<value>\"[^\"]*\"|'[^']*'|[^\"'<>\\s]+)";

        /// <summary>
        /// the regexes cache that is used by the parser so not to create regex each time
        /// </summary>
        private static readonly Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>();

        #endregion


        /// <summary>
        /// Get CSS at rule from the given stylesheet.
        /// </summary>
        /// <param name="stylesheet">the stylesheet data to retrieve the rule from</param>
        /// <param name="startIdx">the index to start the search for the rule, on return will be the value of the end of the found rule</param>
        /// <returns>the found at rule or null if not exists</returns>
        public static string GetCssAtRules(string stylesheet, ref int startIdx)
        {
            startIdx = stylesheet.IndexOf('@', startIdx);
            if (startIdx > -1)
            {
                int count = 1;
                int endIdx = stylesheet.IndexOf('{', startIdx);
                if (endIdx > -1)
                {
                    while (count > 0 && endIdx < stylesheet.Length)
                    {
                        endIdx++;
                        if (stylesheet[endIdx] == '{')
                        {
                            count++;
                        }
                        else if (stylesheet[endIdx] == '}')
                        {
                            count--;
                        }
                    }
                    if (endIdx < stylesheet.Length)
                    {
                        var atrule = stylesheet.Substring(startIdx, endIdx - startIdx + 1);
                        startIdx = endIdx;
                        return atrule;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Extracts matches from the specified source
        /// </summary>
        /// <param name="regex">Regular expression to extract matches</param>
        /// <param name="source">Source to extract matches</param>
        /// <returns>Collection of matches</returns>
        public static MatchCollection Match(string regex, string source)
        {
            var r = GetRegex(regex);
            return r.Matches(source);
        }

        /// <summary>
        /// Searches the specified regex on the source
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Search(string regex, string source)
        {
            int position;
            return Search(regex, source, out position);
        }

        /// <summary>
        /// Searches the specified regex on the source
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="source"></param>
        /// <param name="position"> </param>
        /// <returns></returns>
        public static string Search(string regex, string source, out int position)
        {
            MatchCollection matches = Match(regex, source);

            if (matches.Count > 0)
            {
                position = matches[0].Index;
                return matches[0].Value;
            }
            else
            {
                position = -1;
            }

            return null;
        }

        /// <summary>
        /// Get regex instance for the given regex string.
        /// </summary>
        /// <param name="regex">the regex string to use</param>
        /// <returns>the regex instance</returns>
        private static Regex GetRegex(string regex)
        {
            Regex r;
            if (!_regexes.TryGetValue(regex, out r))
            {
                r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                _regexes[regex] = r;
            }
            return r;
        }
    }
}
