using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    /// <summary>
    /// One registered face of a <see cref="FontFamilyModel"/>: its CSS Fonts 4 matching axes, its
    /// <c>unicode-range</c> or null when it has none (then it covers everything), and its description.
    /// <see cref="TtfFontDescription.FontNameInvariantCulture"/> is the face's key into
    /// <see cref="FontResolver.GetFont"/>.
    /// </summary>
    internal sealed class FontFaceEntry
    {
        public FontFaceEntry(int weight, bool italic, int stretch, IReadOnlyList<CodepointRange> explicitRanges, TtfFontDescription description)
        {
            Weight = weight;
            Italic = italic;
            Stretch = stretch;
            ExplicitRanges = explicitRanges;
            Description = description;
        }

        public int Weight { get; }
        public bool Italic { get; }
        public int Stretch { get; }
        public IReadOnlyList<CodepointRange> ExplicitRanges { get; }
        public TtfFontDescription Description { get; }
    }

    /// <summary>One CSS font-family: its registered faces.</summary>
    internal sealed class FontFamilyModel
    {
        public string Name { get; set; }

        /// <summary>
        /// Every registered face for this family - a list, not a dictionary keyed by (weight,italic,
        /// stretch), because two faces can legitimately share the same matching axes yet differ by
        /// <c>unicode-range</c> (e.g. a Latin subset and a Cyrillic subset of one webfont family, both
        /// regular weight).
        /// </summary>
        public List<FontFaceEntry> Faces { get; } = new List<FontFaceEntry>();
    }
}
