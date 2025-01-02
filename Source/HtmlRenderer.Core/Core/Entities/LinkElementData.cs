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

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Holds data on link element in HTML.<br/>
    /// Used to expose data outside of HTML Renderer internal structure.
    /// </summary>
    public sealed class LinkElementData<T>
    {
        /// <summary>
        /// the id of the link element if present
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// the href data of the link
        /// </summary>
        private readonly string _href;

        /// <summary>
        /// the rectangle of element as calculated by html layout
        /// </summary>
        private readonly T _rectangle;

        /// <summary>
        /// Init.
        /// </summary>
        public LinkElementData(string id, string href, T rectangle)
        {
            _id = id;
            _href = href;
            _rectangle = rectangle;
        }

        /// <summary>
        /// the id of the link element if present
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// the href data of the link
        /// </summary>
        public string Href
        {
            get { return _href; }
        }

        /// <summary>
        /// the rectangle of element as calculated by html layout
        /// </summary>
        public T Rectangle
        {
            get { return _rectangle; }
        }

        /// <summary>
        /// Is the link is directed to another element in the html
        /// </summary>
        public bool IsAnchor
        {
            get { return _href.Length > 0 && _href[0] == '#'; }
        }

        /// <summary>
        /// Return the id of the element this anchor link is referencing.
        /// </summary>
        public string AnchorId
        {
            get { return IsAnchor && _href.Length > 1 ? _href.Substring(1) : string.Empty; }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Href: {1}, Rectangle: {2}", _id, _href, _rectangle);
        }
    }
}