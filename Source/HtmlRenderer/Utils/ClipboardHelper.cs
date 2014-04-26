// Sample class for Copying and Pasting HTML fragments to and from the clipboard.
//
// Mike Stall. http://blogs.msdn.com/jmstall
// 

using System;
using System.Text;
using System.Windows.Forms;

namespace HtmlRenderer.Utils
{
    /// <summary>
    /// Helper to encode and set HTML fragment to clipboard.<br/>
    /// See http://theartofdev.wordpress.com/2012/11/11/setting-html-and-plain-text-formatting-to-clipboard/.<br/>
    /// <seealso cref="CreateDataObject"/>.
    /// </summary>
    internal static class ClipboardHelper
    {
        #region Fields and Consts

        /// <summary>
        /// The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets. <br/>
        /// The <![CDATA[<<<<<<<]]>_ strings are just placeholders. We'll back-patch them actual values afterwards. <br/>
        /// The string layout (<![CDATA[<<<]]>) also ensures that it can't appear in the body of the html because the <![CDATA[<]]> <br/>
        /// character must be escaped. <br/>
        /// </summary>
        private const string Header = @"Format:HTML Format
  Version:1.0
  StartHTML:<<<<<<<1
  EndHTML:<<<<<<<2
  StartFragment:<<<<<<<3
  EndFragment:<<<<<<<4
  StartSelection:<<<<<<<3
  EndSelection:<<<<<<<3";

        /// <summary>
        /// html comment to point the beginning of html fragment
        /// </summary>
        public const string StartFragment = "<!--StartFragment-->";

        /// <summary>
        /// html comment to point the end of html fragment
        /// </summary>
        public const string EndFragment = @"<!--EndFragment-->";

        #endregion


        /// <summary>
        /// Create <see cref="DataObject"/> with given html and plain-text ready to be used for clipboard or drag and drop.
        /// </summary>
        /// <remarks>
        /// Builds the CF_HTML header correctly for all possible HTMLs<br/>
        /// If given html contains start/end fragments then it will use them in the header:
        /// <code><![CDATA[<html><body><!--StartFragment-->hello <b>world</b><!--EndFragment--></body></html>]]></code>
        /// If given html contains html/body tags then it will inject start/end fragments to exclude html/body tags:
        /// <code><![CDATA[<html><body>hello <b>world</b></body></html>]]></code>
        /// If given html doesn't contain html/body tags then it will inject the tags and start/end fragments properly:
        /// <code><![CDATA[hello <b>world</b>]]></code>
        /// In all cases creating a proper CF_HTML header:<br/>
        /// <code>
        /// <![CDATA[
        /// Format:HTML Format
        /// Version:1.0
        /// StartHTML:     177
        /// EndHTML:     329
        /// StartFragment:     277
        /// EndFragment:     295
        /// StartSelection:     277
        /// EndSelection:     277
        /// <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
        /// <html><body>
        /// <!--StartFragment-->hello <b>world</b><!--EndFragment-->
        /// </body></html>
        /// ]]>
        /// </code>
        /// See format specification here: http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp
        /// </remarks>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static DataObject CreateDataObject(string html, string plainText)
        {
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, !string.IsNullOrEmpty(html) ? GetHtmlDataString(html) : html);
            dataObject.SetData(DataFormats.Text, plainText);
            return dataObject;
        }

        /// <summary>
        /// Clears clipboard and sets the given HTML and plain text fragment to the clipboard, providing additional meta-information for HTML.<br/>
        /// See <see cref="CreateDataObject"/> for HTML fragment details.<br/>
        /// </summary>
        /// <example>
        /// ClipboardHelper.CopyToClipboard("Hello <b>World</b>", "Hello World");
        /// </example>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static void CopyToClipboard(string html, string plainText)
        {
            var dataObject = CreateDataObject(html, plainText);
            Clipboard.SetDataObject(dataObject);
        }

        /// <summary>
        /// Generate HTML fragment data string with header that is required for the clipboard.
        /// </summary>
        /// <param name="html">the html to generate for</param>
        /// <returns>the resulted string</returns>
        private static string GetHtmlDataString(string html)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");

            // if given html already provided the fragments we won't add them
            int fragmentStart, fragmentEnd;
            int fragmentStartIdx = html.IndexOf(StartFragment, StringComparison.OrdinalIgnoreCase);
            int fragmentEndIdx = html.LastIndexOf(EndFragment, StringComparison.OrdinalIgnoreCase);
            if (fragmentStartIdx < 0 && fragmentEndIdx < 0)
            {
                int htmlOpenIdx = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
                int bodyOpenIdx = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf('>', htmlOpenIdx) + 1 : -1;
                int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf('>', bodyOpenIdx) + 1 : -1;

                if (htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0)
                {
                    // the given html doesn't contain html or body tags so we need to add them and place start/end fragments around the given html only
                    sb.Append("<html><body>");
                    sb.AppendLine();
                    sb.Append(StartFragment);
                    fragmentStart = sb.Length;
                    sb.Append(html);
                    fragmentEnd = sb.Length;
                    sb.Append(EndFragment);
                    sb.AppendLine();
                    sb.Append("</body></html>");
                }
                else
                {
                    // if html tag is missing add it surrounding the given html (critical)
                    // insert start/end fragments in the proper place (related to html/body tags if exists) so the paste will work correctly
                    int htmlCloseIdx = html.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);
                    int bodyCloseIdx = html.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);

                    if (htmlOpenEndIdx < 0)
                        sb.Append("<html>");
                    else
                        sb.Append(html, 0, htmlOpenEndIdx);

                    if (bodyOpenEndIdx > -1)
                        sb.Append(html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0, bodyOpenEndIdx - (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0));

                    sb.AppendLine();
                    sb.Append(StartFragment);
                    fragmentStart = sb.Length;

                    var innerHtmlStart = bodyOpenEndIdx > -1 ? bodyOpenEndIdx : (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0);
                    var innerHtmlEnd = bodyCloseIdx > -1 ? bodyCloseIdx : (htmlCloseIdx > -1 ? htmlCloseIdx : html.Length);
                    sb.Append(html, innerHtmlStart, innerHtmlEnd - innerHtmlStart);

                    fragmentEnd = sb.Length;
                    sb.Append(EndFragment);
                    sb.AppendLine();

                    if (innerHtmlEnd < html.Length)
                        sb.Append(html, innerHtmlEnd, html.Length - innerHtmlEnd);

                    if (htmlCloseIdx < 0)
                        sb.Append("</html>");
                }
            }
            else
            {
                fragmentStart = sb.Length + fragmentStartIdx + StartFragment.Length;
                fragmentEnd = sb.Length + fragmentEndIdx;
                sb.Append(html);
            }

            // Back-patch offsets (scan only the header part for performance)
            sb.Replace("<<<<<<<4", String.Format("{0,8}", fragmentEnd), 0, Header.Length);
            sb.Replace("<<<<<<<3", String.Format("{0,8}", fragmentStart), 0, Header.Length);
            sb.Replace("<<<<<<<2", String.Format("{0,8}", sb.Length), 0, Header.Length);
            sb.Replace("<<<<<<<1", String.Format("{0,8}", Header.Length), 0, Header.Length);

            return sb.ToString();
        }
    }
}