namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal enum RuleType : byte
    {
        Unknown,
        Style,
        Charset,
        Import,
        Media,
        FontFace,
        Page,
        Keyframes,
        Keyframe,
        MarginBox,
        Namespace,
        CounterStyle,
        Supports,
        Document,
        FontFeatureValues,
        Viewport,
        RegionStyle,
        Container
    }
}