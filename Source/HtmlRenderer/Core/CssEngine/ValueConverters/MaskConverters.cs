using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Value converters of the CSS Masking 1 §6 <c>mask-*</c> longhands that have no <c>background-*</c>
    /// counterpart. <c>mask-image</c>, <c>mask-repeat</c>, <c>mask-position</c> and <c>mask-size</c> reuse the
    /// background converters, as the grammars are identical.
    /// </summary>
    internal static class MaskConverters
    {
        private static Dictionary<string, string> Of(params string[] names)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names) map[name] = name;
            return map;
        }

        // <masking-mode> = alpha | luminance | match-source
        public static readonly IValueConverter ModeConverter =
            Of("match-source", "alpha", "luminance").ToConverter().FromList();

        // <coord-box> = <paint-box> | view-box, <paint-box> = content-box | padding-box | border-box | fill-box | stroke-box
        public static readonly IValueConverter OriginConverter =
            Of("border-box", "padding-box", "content-box", "fill-box", "stroke-box", "view-box")
                .ToConverter().FromList();

        // [ <coord-box> | no-clip ]#
        public static readonly IValueConverter ClipConverter =
            Of("border-box", "padding-box", "content-box", "fill-box", "stroke-box", "view-box", "no-clip")
                .ToConverter().FromList();

        // <compositing-operator> = add | subtract | intersect | exclude
        public static readonly IValueConverter CompositeConverter =
            Of("add", "subtract", "intersect", "exclude").ToConverter().FromList();
    }
}
