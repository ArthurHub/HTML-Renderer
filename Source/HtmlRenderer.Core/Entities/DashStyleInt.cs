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

using HtmlRenderer.Core.Interfaces;

namespace HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Specifies the style of dashed lines drawn with a <see cref="IPen"/> object.
    /// </summary>
    public enum DashStyleInt
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
        Custom,
    }
}
