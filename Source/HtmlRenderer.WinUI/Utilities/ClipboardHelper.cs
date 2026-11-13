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

using Windows.ApplicationModel.DataTransfer;

namespace TheArtOfDev.HtmlRenderer.WinUI.Utilities
{
    /// <summary>
    /// Helper to set HTML fragment and plain text data to the clipboard or a drag operation.
    /// </summary>
    internal static class ClipboardHelper
    {
        /// <summary>
        /// HTML fragment and plain text to put on the clipboard or drag.
        /// </summary>
        /// <remarks>
        /// A drag operation brings its own <see cref="DataPackage"/>, so the content is kept separate from it.
        /// </remarks>
        public sealed class Content
        {
            public Content(string html, string plainText)
            {
                Html = html ?? string.Empty;
                PlainText = plainText ?? string.Empty;
            }

            public string Html { get; }

            public string PlainText { get; }
        }

        /// <summary>
        /// Set the given content as HTML and plain text on the data package.
        /// </summary>
        public static void Fill(DataPackage dataPackage, Content content)
        {
            // CreateHtmlFormat builds the CF_HTML header with its byte offsets
            dataPackage.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(content.Html));
            dataPackage.SetText(content.PlainText);
        }

        /// <summary>
        /// Clears clipboard and sets the given HTML and plain text fragment to the clipboard.
        /// </summary>
        /// <param name="html">a html fragment</param>
        /// <param name="plainText">the plain text</param>
        public static void CopyToClipboard(string html, string plainText)
        {
            var dataPackage = new DataPackage();
            Fill(dataPackage, new Content(html, plainText));
            Clipboard.SetContent(dataPackage);
        }

        /// <summary>
        /// Clears clipboard and sets the given plain text fragment to the clipboard.
        /// </summary>
        /// <param name="plainText">the plain text</param>
        public static void CopyToClipboard(string plainText)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(plainText ?? string.Empty);
            Clipboard.SetContent(dataPackage);
        }
    }
}
