#nullable enable

using System;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Resolves the <c>font-weight</c>, <c>font-style</c> and <c>font-stretch</c> descriptors of an
    /// <c>@font-face</c> rule (CSS Fonts 4 §4.4), the values the face is matched with. The tables of the
    /// font file aren't consulted. Returns null for an absent or unrecognized descriptor.
    /// </summary>
    internal static class FontFaceDescriptorResolver
    {
        /// <summary>
        /// A range resolves to its lower bound, variable fonts aren't supported.
        /// </summary>
        internal static int? ResolveWeight(string? weightDescriptor)
        {
            if (string.IsNullOrWhiteSpace(weightDescriptor))
                return null;

            // See UnicodeRangeParser.Parse for why the ! is needed here on netstandard2.0.
            var tokens = weightDescriptor!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 1 && int.TryParse(tokens[0], out var numeric))
                return numeric;
            if (tokens.Length == 1 && tokens[0] == CssConstants.Bold)
                return 700;
            if (tokens.Length == 1 && tokens[0] == CssConstants.Normal)
                return 400;
            if (tokens.Length == 2 && int.TryParse(tokens[0], out var lowerBound))
                return lowerBound;

            return null;
        }

        /// <summary>
        /// Whether the face is italic, an oblique face counts as italic whatever its angle.
        /// </summary>
        internal static bool? ResolveIsItalic(string? styleDescriptor)
        {
            if (string.IsNullOrWhiteSpace(styleDescriptor))
                return null;

            var firstToken = styleDescriptor!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (firstToken == CssConstants.Italic || firstToken == CssConstants.Oblique)
                return true;
            if (firstToken == CssConstants.Normal)
                return false;

            return null;
        }

        /// <summary>
        /// Resolves a width keyword to the 1-9 scale of <see cref="FontStretchResolver"/>.
        /// </summary>
        internal static int? ResolveStretch(string? stretchDescriptor)
        {
            if (string.IsNullOrWhiteSpace(stretchDescriptor))
                return null;

            var trimmed = stretchDescriptor!.Trim();

            switch (trimmed)
            {
                case CssConstants.UltraCondensed:
                case CssConstants.ExtraCondensed:
                case CssConstants.Condensed:
                case CssConstants.SemiCondensed:
                case CssConstants.Normal:
                case CssConstants.SemiExpanded:
                case CssConstants.Expanded:
                case CssConstants.ExtraExpanded:
                case CssConstants.UltraExpanded:
                    return FontStretchResolver.Resolve(trimmed);
                default:
                    return null;
            }
        }
    }
}
