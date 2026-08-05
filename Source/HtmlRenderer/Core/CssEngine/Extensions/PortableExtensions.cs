#if !NET40 && !SL50

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class PortableExtensions
    {
        public static string ConvertFromUtf32(this int utf32)
        {
            return char.ConvertFromUtf32(utf32);
        }
    }
}

#endif
