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

namespace TheArtOfDev.HtmlRenderer.Demo.Common
{
    /// <summary>
    /// Used to hold a single html sample with its name.
    /// </summary>
    public sealed class HtmlSample
    {
        private readonly string _name;
        private readonly string _fullName;
        private readonly string _html;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public HtmlSample(string name, string fullName, string html)
        {
            _name = name;
            _fullName = fullName;
            _html = html;
        }

        public string Name
        {
            get { return _name; }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public string Html
        {
            get { return _html; }
        }
    }
}