using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class FormatExtensions
    {
        public static string ToCss(this IStyleFormattable style)
        {
            return style.ToCss(CompressedStyleFormatter.Instance);
        }

        public static string ToCss(this IStyleFormattable style, IStyleFormatter formatter)
        {
            var sb = Pool.NewStringBuilder();
            using (var writer = new StringWriter(sb))
            {
                style.ToCss(writer, formatter);
            }

            return sb.ToPool();
        }

        public static void ToCss(this IStyleFormattable style, TextWriter writer)
        {
            style.ToCss(writer, CompressedStyleFormatter.Instance);
        }
    }
}