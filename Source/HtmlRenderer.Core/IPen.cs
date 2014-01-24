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

using HtmlRenderer.Core.Entities;

namespace HtmlRenderer.Core
{
    /// <summary>
    /// atodo: add doc
    /// </summary>
    public interface IPen
    {
        /// <summary>
        /// Gets or sets the width of this Pen, in units of the Graphics object used for drawing.
        /// </summary>
        float Width { get; set; }

        /// <summary>
        /// Gets or sets the style used for dashed lines drawn with this Pen.
        /// </summary>
        DashStyleInt DashStyle { set; }

        /// <summary>
        /// Gets or sets an array of custom dashes and spaces.
        /// </summary>
        float[] DashPattern { set; }
    }
}
