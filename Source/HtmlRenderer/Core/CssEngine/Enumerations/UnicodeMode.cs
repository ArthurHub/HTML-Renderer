namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal enum UnicodeMode : byte
    {
        Normal,
        Embed,
        Isolate,
        BidirectionalOverride,
        IsolateOverride,
        Plaintext
    }
}