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

using HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.Adapters
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public abstract class RPen
    {
        /// <summary>
        ///  Gets or sets the width of this Pen, in units of the Graphics object used for drawing.
        ///  </summary>
        public abstract double Width { get; set; }

        /// <summary>
        ///  Gets or sets the style used for dashed lines drawn with this Pen.
        ///  </summary>
        public abstract RDashStyle DashStyle { set; }

        /// <summary>
        ///  Gets or sets an array of custom dashes and spaces.
        ///  </summary>
        public abstract double[] DashPattern { set; }
    }
}