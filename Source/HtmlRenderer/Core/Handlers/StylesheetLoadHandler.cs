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
using System.IO;
using System.Net;
using TheArtOfDev.HtmlRenderer.Core.Entities;
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
        /// If the stylesheet is downloaded from URI we will try to correct local URIs to absolute.<br/>
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the source of the element to load the stylesheet by</param>
        /// <param name="attributes">the attributes of the link element</param>
        /// <param name="stylesheet">return the stylesheet string that has been loaded (null if failed or <paramref name="stylesheetData"/> is given)</param>
        /// <param name="stylesheetData">return stylesheet data object that was provided by overwrite (null if failed or <paramref name="stylesheet"/> is given)</param>
        public static void LoadStylesheet(HtmlContainerInt htmlContainer, string src, Dictionary<string, string> attributes, out string stylesheet, out CssData stylesheetData)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            stylesheet = null;
            stylesheetData = null;
            try
            {
                var args = new HtmlStylesheetLoadEventArgs(src, attributes);
                htmlContainer.RaiseHtmlStylesheetLoadEvent(args);

                if (!string.IsNullOrEmpty(args.SetStyleSheet))
                {
                    stylesheet = args.SetStyleSheet;
                }
                else if (args.SetStyleSheetData != null)
                {
                    stylesheetData = args.SetStyleSheetData;
                }
                else if (args.SetSrc != null)
                {
                    stylesheet = LoadStylesheet(htmlContainer, args.SetSrc);
                }
                else
                {
                    stylesheet = LoadStylesheet(htmlContainer, src);
                }
            }
            catch (Exception ex)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Exception in handling stylesheet source", ex);
            }
        }


        #region Private methods

        /// <summary>
        /// Load stylesheet string from given source (file path or uri).
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the file path or uri to load the stylesheet from</param>
        /// <returns>the stylesheet string</returns>
        private static string LoadStylesheet(HtmlContainerInt htmlContainer, string src)
        {
            var uri = CommonUtils.TryGetUri(src);
            if (uri == null || uri.Scheme == "file")
            {
                return LoadStylesheetFromFile(htmlContainer, uri != null ? uri.AbsolutePath : src);
            }
            else
            {
                return LoadStylesheetFromUri(htmlContainer, uri);
            }
        }

        /// <summary>
        /// Load the stylesheet from local file by given path.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="path">the stylesheet file to load</param>
        /// <returns>the loaded stylesheet string</returns>
        private static string LoadStylesheetFromFile(HtmlContainerInt htmlContainer, string path)
        {
            var fileInfo = CommonUtils.TryGetFileInfo(path);
            if (fileInfo != null)
            {
                if (fileInfo.Exists)
                {
                    using (var sr = new StreamReader(fileInfo.FullName))
                    {
                        return sr.ReadToEnd();
                    }
                }
                else
                {
                    htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "No stylesheet found by path: " + path);
                }
            }
            else
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Failed load image, invalid source: " + path);
            }
            return string.Empty;
        }

        /// <summary>
        /// Load the stylesheet from uri by downloading the string.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="uri">the uri to download from</param>
        /// <returns>the loaded stylesheet string</returns>
        private static string LoadStylesheetFromUri(HtmlContainerInt htmlContainer, Uri uri)
        {
            using (var client = new WebClient())
            {
                var stylesheet = client.DownloadString(uri);
                try
                {
                    stylesheet = CorrectRelativeUrls(stylesheet, uri);
                }
                catch (Exception ex)
                {
                    htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Error in correcting relative URL in loaded stylesheet", ex);
                }
                return stylesheet;
            }
        }

        /// <summary>
        /// Make relative URLs absolute in the stylesheet using the URI of the stylesheet.
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
                if (idx >= 0)
                {
                    int endIdx = stylesheet.IndexOf(')', idx);
                    if (endIdx > idx + 4)
                    {
                        var offset1 = 4 + (stylesheet[idx + 4] == '\'' ? 1 : 0);
                        var offset2 = (stylesheet[endIdx - 1] == '\'' ? 1 : 0);
                        var urlStr = stylesheet.Substring(idx + offset1, endIdx - idx - offset1 - offset2);
                        Uri url;
                        if (Uri.TryCreate(urlStr, UriKind.Relative, out url))
                        {
                            url = new Uri(baseUri, url);
                            stylesheet = stylesheet.Remove(idx + 4, endIdx - idx - 4);
                            stylesheet = stylesheet.Insert(idx + 4, url.AbsoluteUri);
                            idx += url.AbsoluteUri.Length + 4;
                        }
                        else
                        {
                            idx = endIdx + 1;
                        }
                    }
                    else
                    {
                        idx += 4;
                    }
                }
            }

            return stylesheet;
        }

        #endregion
    }
}