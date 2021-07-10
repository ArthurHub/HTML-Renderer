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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// Holds parsed stylesheet css blocks arranged by media and classes.<br/>
    /// <seealso cref="CssBlock"/>
    /// </summary>
    /// <remarks>
    /// To learn more about CSS blocks visit CSS spec: http://www.w3.org/TR/CSS21/syndata.html#block
    /// </remarks>
    public sealed class CssData
    {
        #region Fields and Consts

        /// <summary>
        /// used to return empty array
        /// </summary>
        private static readonly List<CssBlock> _emptyArray = new List<CssBlock>();

        /// <summary>
        /// dictionary of media type to dictionary of css class name to the cssBlocks collection with all the data.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, List<CssBlock>>> _mediaBlocks = new Dictionary<string, Dictionary<string, List<CssBlock>>>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        internal CssData()
        {
            _mediaBlocks.Add("all", new Dictionary<string, List<CssBlock>>(StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Parse the given stylesheet to <see cref="CssData"/> object.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed css blocks are added to the 
        /// default css data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="adapter">Platform adapter</param>
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the parsed css data</returns>
        public static CssData Parse(RAdapter adapter, string stylesheet, bool combineWithDefault = true)
        {
            CssParser parser = new CssParser(adapter);
            return parser.ParseStyleSheet(stylesheet, combineWithDefault);
        }

        /// <summary>
        /// dictionary of media type to dictionary of css class name to the cssBlocks collection with all the data
        /// </summary>
        internal IDictionary<string, Dictionary<string, List<CssBlock>>> MediaBlocks
        {
            get { return _mediaBlocks; }
        }

        /// <summary>
        /// Check if there are css blocks for the given class selector.
        /// </summary>
        /// <param name="className">the class selector to check for css blocks by</param>
        /// <param name="media">optional: the css media type (default - all)</param>
        /// <returns>true - has css blocks for the class, false - otherwise</returns>
        public bool ContainsCssBlock(string className, string media = "all")
        {
            Dictionary<string, List<CssBlock>> mid;
            return _mediaBlocks.TryGetValue(media, out mid) && mid.ContainsKey(className);
        }

        /// <summary>
        /// Get collection of css blocks for the requested class selector.<br/>
        /// the <paramref name="className"/> can be: class name, html element name, html element and 
        /// class name (elm.class), hash tag with element id (#id).<br/>
        /// returned all the blocks that word on the requested class selector, it can contain simple
        /// selector or hierarchy selector.
        /// </summary>
        /// <param name="className">the class selector to get css blocks by</param>
        /// <param name="media">optional: the css media type (default - all)</param>
        /// <returns>collection of css blocks, empty collection if no blocks exists (never null)</returns>
        public IEnumerable<CssBlock> GetCssBlock(string className, string media = "all")
        {
            List<CssBlock> block = null;
            Dictionary<string, List<CssBlock>> mid;
            if (_mediaBlocks.TryGetValue(media, out mid))
            {
                mid.TryGetValue(className, out block);
            }
            return block ?? _emptyArray;
        }

        /// <summary>
        /// Add the given css block to the css data, merging to existing block if required.
        /// </summary>
        /// <remarks>
        /// If there is no css blocks for the same class it will be added to data collection.<br/>
        /// If there is already css blocks for the same class it will check for each existing block
        /// if the hierarchical selectors match (or not exists). if do the two css blocks will be merged into
        /// one where the new block properties overwrite existing if needed. if the new block doesn't mach any
        /// existing it will be added either to the beginning of the list if it has no  hierarchical selectors or at the end.<br/>
        /// Css block without hierarchical selectors must be added to the beginning of the list so more specific block
        /// can overwrite it when the style is applied.
        /// </remarks>
        /// <param name="media">the media type to add the CSS to</param>
        /// <param name="cssBlock">the css block to add</param>
        public void AddCssBlock(string media, CssBlock cssBlock)
        {
            Dictionary<string, List<CssBlock>> mid;
            if (!_mediaBlocks.TryGetValue(media, out mid))
            {
                mid = new Dictionary<string, List<CssBlock>>(StringComparer.InvariantCultureIgnoreCase);
                _mediaBlocks.Add(media, mid);
            }

            if (!mid.ContainsKey(cssBlock.Class))
            {
                var list = new List<CssBlock>();
                list.Add(cssBlock);
                mid[cssBlock.Class] = list;
            }
            else
            {
                bool merged = false;
                var list = mid[cssBlock.Class];
                foreach (var block in list)
                {
                    if (block.EqualsSelector(cssBlock))
                    {
                        merged = true;
                        block.Merge(cssBlock);
                        break;
                    }
                }

                if (!merged)
                {
                    // general block must be first
                    if (cssBlock.Selectors == null)
                        list.Insert(0, cssBlock);
                    else
                        list.Add(cssBlock);
                }
            }
        }

        /// <summary>
        /// Combine this CSS data blocks with <paramref name="other"/> CSS blocks for each media.<br/>
        /// Merge blocks if exists in both.
        /// </summary>
        /// <param name="other">the CSS data to combine with</param>
        public void Combine(CssData other)
        {
            ArgChecker.AssertArgNotNull(other, "other");

            // for each media block
            foreach (var mediaBlock in other.MediaBlocks)
            {
                // for each css class in the media block
                foreach (var bla in mediaBlock.Value)
                {
                    // for each css block of the css class
                    foreach (var cssBlock in bla.Value)
                    {
                        // combine with this
                        AddCssBlock(mediaBlock.Key, cssBlock);
                    }
                }
            }
        }

        /// <summary>
        /// Create deep copy of the css data with cloned css blocks.
        /// </summary>
        /// <returns>cloned object</returns>
        public CssData Clone()
        {
            var clone = new CssData();
            foreach (var mid in _mediaBlocks)
            {
                var cloneMid = new Dictionary<string, List<CssBlock>>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var blocks in mid.Value)
                {
                    var cloneList = new List<CssBlock>();
                    foreach (var cssBlock in blocks.Value)
                    {
                        cloneList.Add(cssBlock.Clone());
                    }
                    cloneMid[blocks.Key] = cloneList;
                }
                clone._mediaBlocks[mid.Key] = cloneMid;
            }
            return clone;
        }
    }
}