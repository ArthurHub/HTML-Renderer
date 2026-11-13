using System;
using System.Diagnostics.CodeAnalysis;
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

        public static PropertyInfo[] GetProperties(
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
#endif
            this Type type)
        {
            return type.GetRuntimeProperties().ToArray();
        }
    }
}

#endif