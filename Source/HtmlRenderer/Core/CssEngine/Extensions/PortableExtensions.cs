using System;
using System.Linq;
using System.Reflection;

#if !NET40 && !SL50

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class PortableExtensions
    {
        public static string ConvertFromUtf32(this int utf32)
        {
            return char.ConvertFromUtf32(utf32);
        }

        // The [DynamicallyAccessedMembers] trimming annotation from the source this was ported from is
        // deliberately dropped here - it's a metadata-only hint for IL trimmers/AOT (no runtime effect),
        // and HTML-Renderer doesn't use trimming; keeping it would need a netstandard2.0-incompatible
        // reference to System.Diagnostics.CodeAnalysis's copy of the attribute.
        public static PropertyInfo[] GetProperties(this Type type)
        {
            return type.GetRuntimeProperties().ToArray();
        }
    }
}

#endif