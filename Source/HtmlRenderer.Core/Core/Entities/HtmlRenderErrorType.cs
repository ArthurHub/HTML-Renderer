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
    /// Enum of possible error types that can be reported.
    /// </summary>
    public enum HtmlRenderErrorType
    {
        General = 0,
        CssParsing = 1,
        HtmlParsing = 2,
        Image = 3,
        Paint = 4,
        Layout = 5,
        KeyboardMouse = 6,
        Iframe = 7,
        ContextMenu = 8,
    }
}