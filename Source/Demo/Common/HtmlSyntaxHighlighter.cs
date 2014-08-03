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
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Demo.Common
{
    /// <summary>
    /// HTML syntax highlighting using Rich-Text formatting.<br/>
    /// - Handle plain input or already in RTF format.<br/>
    /// - Handle if input already contains RTF color table.<br/>
    /// - Rich coloring adjusted to Visual Studio HTML coloring.<br/>
    /// - Support to provide custom colors.<br/>
    /// - High performance (as much as RTF format allows).<br/>
    /// </summary>
    /// <remarks>
    /// The MIT License (MIT) Copyright (c) 2014 Arthur Teplitzki.<br/>
    /// Based on work by Alun Evans 2006 (http://www.codeproject.com/Articles/15038/C-Formatting-Text-in-a-RichTextBox-by-Parsing-the).
    /// </remarks>
    public static class HtmlSyntaxHighlighter
    {
        #region Fields/Consts

        /// <summary>
        /// RTF header field
        /// </summary>
        private const string Header = "\\rtf";

        /// <summary>
        /// RTF color table
        /// </summary>
        private const string ColorTbl = "\\colortbl";

        /// <summary>
        /// cf0 = default
        /// cf1 = dark red  
        /// cf2 = bright red
        /// cf3 = green
        /// cf4 = blue    
        /// cf5 = blue    
        /// cf6 = purple          
        /// </summary>
        private const string DefaultColorScheme = "\\red128\\green0\\blue0;\\red240\\green0\\blue0;\\red0\\green128\\blue0;\\red0\\green0\\blue255;\\red0\\green0\\blue255;\\red128\\green0\\blue171;";

        /// <summary>
        /// Used to test if a char requires more than 1 byte
        /// </summary>
        private static readonly char[] _unicodeTest = new char[1];

        #endregion

        /// <summary>
        /// Process the given text to create RTF text with HTML syntax highlighting using default Visual Studio colors.<br/>
        /// The given text can be plain HTML or already parsed RTF format.
        /// </summary>
        /// <param name="text">the text to create color RTF text from</param>
        /// <returns>text with RTF formatting for HTML syntax</returns>
        public static string Process(string text)
        {
            return Process(text, DefaultColorScheme);
        }

        /// <summary>
        /// Process the given text to create RTF text with HTML syntax highlighting using custom colors.<br/>
        /// The given text can be plain HTML or already parsed RTF format.
        /// </summary>
        /// <param name="text">the text to create color RTF text from</param>
        /// <param name="element">the color for HTML elements</param>
        /// <param name="attribute">the color for HTML attributes</param>
        /// <param name="comment">the color for HTML comments</param>
        /// <param name="chars">the color for HTML special chars: (<![CDATA[<,>,",',=,:]]>)</param>
        /// <param name="values">the color for HTML attribute or styles values</param>
        /// <param name="style">the color for HTML style attribute</param>
        /// <returns>text with RTF formatting for HTML syntax</returns>
        public static string Process(string text, Color element, Color attribute, Color comment, Color chars, Color values, Color style)
        {
            return Process(text, CreateColorScheme(element, attribute, comment, chars, values, style));
        }


        #region Private/Protected methods

        /// <summary>
        /// Process the given text to create RTF text with HTML syntax highlighting.
        /// </summary>
        /// <param name="text">the text to create color RTF text from</param>
        /// <param name="colorScheme">the color scheme to add to RTF color table</param>
        /// <returns>text with RTF formatting for HTML syntax</returns>
        private static string Process(string text, string colorScheme)
        {
            var sb = new StringBuilder(text.Length * 2);

            // add color table used to set color in RTL formatted text
            bool rtfFormated;
            int i = AddColorTable(sb, text, colorScheme, out rtfFormated);

            // Scan through RTF data adding RTF color tags
            bool inComment = false;
            bool inHtmlTag = false;
            bool inAttributeVal = false;
            for (; i < text.Length; i++)
            {
                var c = text[i];
                var c2 = text.Length > i + 1 ? text[i + 1] : (char)0;

                if (!inComment && c == '<')
                {
                    if (text.Length > i + 3 && c2 == '!' && text[i + 2] == '-' && text[i + 3] == '-')
                    {
                        // Comments tag
                        sb.Append("\\cf3").Append(c);
                        inComment = true;
                    }
                    else
                    {
                        // Html start/end tag
                        sb.Append("\\cf4").Append(c);
                        if (c2 == '/')
                        {
                            sb.Append(c2);
                            i++;
                        }
                        sb.Append("\\cf1 ");
                        inHtmlTag = true;
                    }
                }
                else if (c == '>')
                {
                    //Check for comments tags
                    if (inComment && text[i - 1] == '-' && text[i - 2] == '-')
                    {
                        sb.Append(c).Append("\\cf0 ");
                        inComment = false;
                    }
                    else if (!inComment)
                    {
                        sb.Append("\\cf4").Append(c).Append("\\cf0 ");
                        inHtmlTag = false;
                        inAttributeVal = false;
                    }
                }
                else if (inHtmlTag && !inComment && c == '/' && c2 == '>')
                {
                    sb.Append("\\cf4").Append(c).Append(c2).Append("\\cf0 ");
                    inHtmlTag = false;
                    i++;
                }
                else if (inHtmlTag && !inComment && !inAttributeVal && c == ' ')
                {
                    sb.Append(c).Append("\\cf2 ");
                }
                else if (inHtmlTag && !inComment && c == '=')
                {
                    sb.Append("\\cf4").Append(c).Append("\\cf6 ");
                }
                else if (inHtmlTag && !inComment && inAttributeVal && c == ':')
                {
                    sb.Append("\\cf0").Append(c).Append("\\cf5 ");
                }
                else if (inHtmlTag && !inComment && inAttributeVal && c == ';')
                {
                    sb.Append("\\cf0").Append(c).Append("\\cf6 ");
                }
                else if (inHtmlTag && !inComment && (c == '"' || c == '\''))
                {
                    sb.Append("\\cf4").Append(c).Append("\\cf6 ");
                    inAttributeVal = !inAttributeVal;
                }
                else if (!rtfFormated && c == '\n')
                {
                    sb.Append(c).Append("\\par ");
                }
                else if (!rtfFormated && (c == '{' || c == '}'))
                {
                    sb.Append('\\').Append(c);
                }
                else if (!rtfFormated)
                {
                    _unicodeTest[0] = c;
                    if (Encoding.UTF8.GetByteCount(_unicodeTest, 0, 1) > 1)
                        sb.Append("\\u" + Convert.ToUInt32(c) + "?");
                    else
                        sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            // close the RTF if we added the header ourselves
            if (!rtfFormated)
                sb.Append('}');

            // return the created colored RTF
            return sb.ToString();
        }

        /// <summary>
        /// Add color table used to set color in RTL formatted text.
        /// </summary>
        /// <param name="sb">the builder to add the RTF string to</param>
        /// <param name="text">the original RTF text to build color RTF from</param>
        /// <param name="colorScheme">the color scheme to add to RTF color table</param>
        /// <param name="rtfFormated">return if the given text is already in RTF format</param>
        /// <returns>the index in the given RTF text to start scan from</returns>
        private static int AddColorTable(StringBuilder sb, string text, string colorScheme, out bool rtfFormated)
        {
            // Search for color table, if exists replace it, otherwise add our
            rtfFormated = true;
            int idx = text.IndexOf(ColorTbl, StringComparison.OrdinalIgnoreCase);
            if (idx != -1)
            {
                sb.Append(text, 0, idx);

                // insert our color table at our chosen location                
                sb.Append(ColorTbl).Append(";").Append(colorScheme).Append("}");

                // skip the existing color table
                idx = text.IndexOf('}', idx);
            }
            else
            {
                // find index of start of header if exists
                idx = text.IndexOf(Header, StringComparison.OrdinalIgnoreCase);
                if (idx != -1)
                {
                    // append the existing header
                    idx += Header.Length;
                    sb.Append(text, 0, idx);
                    while (text[idx] != '\\' && text[idx] != '{' && text[idx] != '}')
                        sb.Append(text[idx++]);
                }
                else
                {
                    // not RTF text, add the RTF header as well
                    idx = 0;
                    sb.Append("{").Append(Header);
                    rtfFormated = false;
                }

                // insert the color table at our chosen location                
                sb.Append("{").Append(ColorTbl).Append(";").Append(colorScheme).Append("}");
            }
            return idx;
        }

        /// <summary>
        /// Create RTF colortbl formatted string for the given colors.
        /// </summary>
        private static string CreateColorScheme(Color element, Color attribute, Color comment, Color chars, Color values, Color style)
        {
            var sb = new StringBuilder(DefaultColorScheme.Length);
            AppendColorValue(sb, element);
            AppendColorValue(sb, attribute);
            AppendColorValue(sb, comment);
            AppendColorValue(sb, chars);
            AppendColorValue(sb, values);
            AppendColorValue(sb, style);
            return sb.ToString();
        }

        /// <summary>
        /// Append single color in RTF colortbl format.
        /// </summary>
        private static void AppendColorValue(StringBuilder sb, Color color)
        {
            sb.Append("\\red").Append(color.R)
                .Append("\\green").Append(color.R)
                .Append("\\blue").Append(color.R)
                .Append(';');
        }

        #endregion
    }
}