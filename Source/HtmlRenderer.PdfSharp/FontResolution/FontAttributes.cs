namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    public enum FontWeight
    {
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900,
    }

    public enum FontWidth
    {
        UltraCondensed = 50,
        ExtraCondensed = 62,
        Condensed = 75,
        SemiCondensed = 87,
        Medium = 100,
        SemiExpanded = 112,
        Expanded = 125,
        ExtraExpanded = 150,
        UltraExpanded = 200,
    }

    public enum FontStyle
    {
        Normal,
        Italic,
        Oblique,
    }

    public class FontAttributes
    {   
        public FontWeight Weight { get; set; } = FontWeight.Normal;
        public FontStyle Style { get; set; } = FontStyle.Normal;
        public FontWidth Width { get; set; } = FontWidth.Medium;
    }
}
