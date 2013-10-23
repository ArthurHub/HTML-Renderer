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
using HtmlRenderer.Entities;

namespace HtmlRenderer.Utils
{
    /// <summary>
    /// Helper for loading a stylesheet data.
    /// </summary>
    internal static class StylesheetLoadHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the source of the element to load the stylesheet by</param>
        /// <param name="attributes">the attributes of the link element</param>
        public static string LoadStylesheet(HtmlContainer htmlContainer, string src, Dictionary<string, string> attributes)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            try
            {
                var args = new HtmlStylesheetLoadEventArgs(src, attributes);
                htmlContainer.RaiseHtmlStylesheetLoadEvent(args);

                if(args.SetStyleSheet != null)
                {
                    return args.SetStyleSheet;
                }
                else if(args.SetSrc != null)
                {
                    return LoadStylesheet(htmlContainer, args.SetSrc);
                }
                else
                {
                    return LoadStylesheet(htmlContainer, src);
                }
            }
            catch (Exception ex)
            {
                htmlContainer.ReportError(HtmlRenderErrorType.CssParsing, "Exception in handling stylesheet source", ex);
                return string.Empty;
            }
        }


        #region Private methods

        /// <summary>
        /// Load stylesheet string from given source (file path or uri).
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="src">the file path or uri to load the stylesheet from</param>
        /// <returns>the stylesheet string</returns>
        private static string LoadStylesheet(HtmlContainer htmlContainer, string src)
        {
            var uri = CommonUtils.TryGetUri(src);
            if (uri != null && uri.Scheme != "file")
            {
                return LoadStylesheetFromUri(uri);
            }
            else
            {
                return LoadStylesheetFromFile(htmlContainer, uri != null ? uri.AbsolutePath : src);
            }
        }

        /// <summary>
        /// Load the stylesheet from uri by downloading the string.
        /// </summary>
        /// <param name="uri">the uri to download from</param>
        /// <returns>the loaded stylesheet string</returns>
        private static string LoadStylesheetFromUri(Uri uri)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(uri);
            }
        }

        /// <summary>
        /// Load the stylesheet from local file by given path.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load stylesheet for</param>
        /// <param name="path">the stylesheet file to load</param>
        /// <returns>the loaded stylesheet string</returns>
        private static string LoadStylesheetFromFile(HtmlContainer htmlContainer, string path)
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

        #endregion

    }
}
