// Sample class for Copying and Pasting HTML fragments to and from the clipboard.
//
// Mike Stall. http://blogs.msdn.com/jmstall
// 

using System;
using System.Text;
using System.Windows.Forms;

namespace HtmlRenderer.WinForms.Utilities
{
    /// <summary>
    /// Helper class to decode HTML from the clipboard.
    /// See http://blogs.msdn.com/jmstall/archive/2007/01/21/html-clipboard.aspx for details.
    /// </summary>
    internal static class HtmlClipboardUtils
    {
        #region Fields and Consts

        /// <summary>
        /// The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets. <br/>
        /// The <![CDATA[<<<<<<<]]>_ strings are just placeholders. We'll backpatch them actual values afterwards. <br/>
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
EndSelection:<<<<<<<3
";

        #endregion


        /// <summary>
        /// Get data object with given html and plaintext ready to be used for clipboard or drag & drop.
        /// </summary>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static DataObject GetDataObject(string html, string plainText)
        {
            var data = GetHtmlData(html);
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, data);
            dataObject.SetData(DataFormats.Text, plainText);
            return dataObject;
        }

        /// <summary>
        /// Clears clipboard and copy a HTML fragment to the clipboard, providing additional meta-information.
        /// </summary>
        /// <remarks>
        /// Builds the CF_HTML header. See format specification here:
        /// http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp
        /// </remarks>
        /// <example>
        ///    HtmlClipboardUtils.SetToClipboard("<b>Hello!</b>");
        /// </example>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static void CopyToClipboard(string html, string plainText)
        {
            Clipboard.SetDataObject(GetDataObject(html, plainText));
        }


        #region Private methods

        /// <summary>
        /// Generate format html string with header that is required for the clipboard.
        /// </summary>
        /// <param name="html">the html to generate for</param>
        /// <returns>the resulted string</returns>
        private static string GetHtmlData(string html)
        {
            var sb = new StringBuilder();

            sb.Append(Header);
            int startHtml = sb.Length;

            sb.Append(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN""><!--StartFragment-->");
            int fragmentStart = sb.Length;

            sb.Append(html);
            int fragmentEnd = sb.Length;

            sb.Append(@"<!--EndFragment-->");
            int endHtml = sb.Length;

            // Backpatch offsets
            sb.Replace("<<<<<<<1", String.Format("{0,8}", startHtml));
            sb.Replace("<<<<<<<2", String.Format("{0,8}", endHtml));
            sb.Replace("<<<<<<<3", String.Format("{0,8}", fragmentStart));
            sb.Replace("<<<<<<<4", String.Format("{0,8}", fragmentEnd));

            return sb.ToString();
        }

        #endregion
    }
}