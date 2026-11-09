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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Network;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Handler for loading a stylesheet data.
    /// </summary>
    internal static class StylesheetLoadHandler
    {
        /// <summary>
        /// Load stylesheet data from the given source.<br/>
        /// The source can be local file or web URI.<br/>
        /// First raise <see cref="HtmlStylesheetLoadEventArgs"/> event to allow the client to overwrite the stylesheet loading.<br/>
        /// If the event doesn't handle it, the reference is resolved against the document base and fetched
        /// through <see cref="Adapters.RAdapter.GetResourceStream"/>, uniformly for local files, <c>data:</c>
        /// URIs, and remote HTTP(S) sources.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the source of the element to load the stylesheet by</param>
        /// <param name="attributes">the attributes of the link element</param>
        /// <returns>
        /// the stylesheet string that has been loaded (empty if not found/failed, or if
        /// <c>stylesheetData</c> is given instead), and the stylesheet data object provided by an event
        /// overwrite (null unless the event set it).
        /// </returns>
        public static async Task<(string stylesheet, CssData stylesheetData)> LoadStylesheet(HtmlContainerInt htmlContainer, string src, Dictionary<string, string> attributes)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            try
            {
                var args = new HtmlStylesheetLoadEventArgs(src, attributes);
                htmlContainer.RaiseHtmlStylesheetLoadEvent(args);

                if (!string.IsNullOrEmpty(args.SetStyleSheet))
                {
                    return (args.SetStyleSheet, null);
                }

                if (args.SetStyleSheetData != null)
                {
                    return (null, args.SetStyleSheetData);
                }

                var stylesheet = await LoadStylesheet(htmlContainer, args.SetSrc ?? src).ConfigureAwait(false);
                return (stylesheet, null);
            }
            catch (Exception ex)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Exception in handling stylesheet source", ex);
                return (null, null);
            }
        }


        #region Private methods

        /// <summary>
        /// Load stylesheet string from given source (file path or uri), resolved against the document base
        /// and fetched through <see cref="Adapters.RAdapter.GetResourceStream"/>.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the file path or uri to load the stylesheet from</param>
        /// <returns>the stylesheet string, or empty if it could not be resolved/loaded</returns>
        private static async Task<string> LoadStylesheet(HtmlContainerInt htmlContainer, string src)
        {
            var uri = CommonUtils.ResolveAgainstDocumentBase(htmlContainer, src);
            if (uri == null)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Failed load stylesheet, invalid source: " + src);
                return string.Empty;
            }

            var networkResponse = await htmlContainer.Adapter.GetResourceStream(uri).ConfigureAwait(false);

            if (networkResponse == null || networkResponse.ResourceStream == null)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "No stylesheet found by path: " + src);
                return string.Empty;
            }

            // CSS 2.1 §3.4, HTML §4.2.4.2. A response of another type is skipped like a missing one. A response without
            // a Content-Type, e.g. a local file, is trusted.
            if (networkResponse.ResponseHeaders != null &&
                networkResponse.ResponseHeaders.TryGetValue("Content-Type", out var contentTypeValues) &&
                contentTypeValues.Length > 0 &&
                !contentTypeValues.Any(ct => ct.IndexOf("text/css", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                using (networkResponse.ResourceStream) { }
                return string.Empty;
            }

            string stylesheet;
            using (var sr = new StreamReader(networkResponse.ResourceStream))
            {
                stylesheet = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            try
            {
                stylesheet = CorrectRelativeUrls(stylesheet, uri.Uri);
            }
            catch (Exception ex)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Error in correcting relative URL in loaded stylesheet", ex);
            }

            return stylesheet;
        }

        /// <summary>
        /// Make relative URLs absolute in the stylesheet using the URI of the stylesheet (CSS 2.1 §4.3.4).
        /// </summary>
        /// <param name="stylesheet">the stylesheet to correct</param>
        /// <param name="baseUri">the stylesheet uri to use to create absolute URLs</param>
        /// <returns>Corrected stylesheet</returns>
        private static string CorrectRelativeUrls(string stylesheet, Uri baseUri)
        {
            int idx = 0;
            while (idx >= 0 && idx < stylesheet.Length)
            {
                idx = stylesheet.IndexOf("url(", idx, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    break;

                if (TryReadUrl(stylesheet, idx + 4, out var urlStr, out var endIdx) &&
                    Uri.TryCreate(urlStr, UriKind.Relative, out var url))
                {
                    // Written back quoted, unquoted it couldn't hold the ' ( ) an absolute URI may contain
                    var replacement = "\"" + new Uri(baseUri, url).AbsoluteUri + "\"";
                    stylesheet = stylesheet.Remove(idx + 4, endIdx - idx - 4).Insert(idx + 4, replacement);
                    idx += 4 + replacement.Length + 1;
                }
                else
                {
                    idx += 4;
                }
            }

            return stylesheet;
        }

        /// <summary>
        /// Read the URL of a url() without its quotes and with its escapes resolved.
        /// </summary>
        /// <param name="stylesheet">the stylesheet to read from</param>
        /// <param name="start">the index right after "url("</param>
        /// <param name="url">the URL</param>
        /// <param name="endIdx">the index of the closing ')'</param>
        /// <returns>false if it isn't a well-formed url()</returns>
        private static bool TryReadUrl(string stylesheet, int start, out string url, out int endIdx)
        {
            // CSS 2.1 4.3.4: optional whitespace and a pair of matching ' or " quotes around the URL
            url = null;
            endIdx = -1;

            int i = SkipWhitespace(stylesheet, start);
            char quote = i < stylesheet.Length && (stylesheet[i] == '"' || stylesheet[i] == '\'') ? stylesheet[i++] : '\0';

            var sb = new StringBuilder();
            while (true)
            {
                if (i >= stylesheet.Length)
                    return false;

                char c = stylesheet[i];
                if (quote != '\0' ? c == quote : c == ')' || IsWhitespace(c))
                    break;
                if (quote != '\0' ? c == '\n' || c == '\r' || c == '\f' : c == '"' || c == '\'' || c == '(')
                    return false;

                if (c == '\\')
                {
                    i = Unescape(stylesheet, i + 1, sb);
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }

            if (quote != '\0')
                i++;

            i = SkipWhitespace(stylesheet, i);
            if (i >= stylesheet.Length || stylesheet[i] != ')' || sb.Length == 0)
                return false;

            url = sb.ToString();
            endIdx = i;
            return true;
        }

        /// <summary>
        /// Append what the escape after a backslash stands for (CSS 2.1 4.1.3).
        /// </summary>
        /// <param name="stylesheet">the stylesheet to read from</param>
        /// <param name="i">the index right after the backslash</param>
        /// <param name="sb">the builder to append to</param>
        /// <returns>the index after the escape</returns>
        private static int Unescape(string stylesheet, int i, StringBuilder sb)
        {
            if (i >= stylesheet.Length)
                return i;

            int hexEnd = i;
            while (hexEnd < stylesheet.Length && hexEnd - i < 6 && Uri.IsHexDigit(stylesheet[hexEnd]))
                hexEnd++;

            if (hexEnd == i)
            {
                // A backslash before a newline in a string continues the line
                if (stylesheet[i] == '\r' && i + 1 < stylesheet.Length && stylesheet[i + 1] == '\n')
                    return i + 2;
                if (stylesheet[i] != '\n' && stylesheet[i] != '\r' && stylesheet[i] != '\f')
                    sb.Append(stylesheet[i]);
                return i + 1;
            }

            int codePoint = int.Parse(stylesheet.Substring(i, hexEnd - i), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            sb.Append(codePoint == 0 || codePoint > 0x10FFFF || (codePoint >= 0xD800 && codePoint <= 0xDFFF) ? "�" : char.ConvertFromUtf32(codePoint));

            if (hexEnd + 1 < stylesheet.Length && stylesheet[hexEnd] == '\r' && stylesheet[hexEnd + 1] == '\n')
                return hexEnd + 2;
            return hexEnd < stylesheet.Length && IsWhitespace(stylesheet[hexEnd]) ? hexEnd + 1 : hexEnd;
        }

        private static int SkipWhitespace(string stylesheet, int i)
        {
            while (i < stylesheet.Length && IsWhitespace(stylesheet[i]))
                i++;
            return i;
        }

        private static bool IsWhitespace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';

        #endregion
    }
}
