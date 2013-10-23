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

namespace HtmlRenderer.Entities
{
    /// <summary>
    /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
    /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
    /// If no alternative data is provided the original source will be used.<br/>
    /// </summary>
    public sealed class HtmlStylesheetLoadEventArgs : EventArgs
    {
        #region Fields and Consts

        /// <summary>
        /// the source of the stylesheet (file path or uri)
        /// </summary>
        private readonly string _src;

        /// <summary>
        /// collection of all the attributes that are defined on the link element
        /// </summary>
        private readonly Dictionary<string, string> _attributes;

        /// <summary>
        /// provide the new source (file path or uri) to load stylesheet from
        /// </summary>
        private string _setSrc;

        /// <summary>
        /// provide the stylesheet to load
        /// </summary>
        private string _setStyleSheet;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="src">the source of the image (file path or uri)</param>
        /// <param name="attributes">collection of all the attributes that are defined on the image element</param>
        public HtmlStylesheetLoadEventArgs(string src, Dictionary<string, string> attributes)
        {
            _src = src;
            _attributes = attributes;
        }

        /// <summary>
        /// the source of the image (file path or uri)
        /// </summary>
        public string Src
        {
            get { return _src; }
        }

        /// <summary>
        /// collection of all the attributes that are defined on the image element
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// provide the new source (file path or uri) to load stylesheet from
        /// </summary>
        public string SetSrc
        {
            get { return _setSrc; }
            set { _setSrc = value; }
        }

        /// <summary>
        /// provide the stylesheet to load
        /// </summary>
        public string SetStyleSheet
        {
            get { return _setStyleSheet; }
            set { _setStyleSheet = value; }
        }
    }
}
