// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// The resolved "device" a CSS <c>@media</c> query is evaluated against: the media type plus the
    /// viewport geometry and the display characteristics the adapter reports. Built once per render and
    /// threaded through <see cref="CssData"/>'s rule matching so <c>@media</c> conditions
    /// (<c>min-width</c>, <c>orientation</c>, <c>prefers-color-scheme</c>, ...) actually gate which rules
    /// apply. See <see cref="MediaQueryMatcher"/> for the evaluation itself.
    /// </summary>
    internal struct MediaQueryContext
    {
        /// <summary>
        /// A context carrying only the media type, with no viewport geometry - for callers that have no
        /// container (the public <see cref="CssData.Parse(RAdapter,string,bool)"/> entry point, tests).
        /// Conditions that need geometry match permissively.
        /// </summary>
        internal static MediaQueryContext TypeOnly(string mediaType)
        {
            return new MediaQueryContext(
                string.IsNullOrEmpty(mediaType) ? "all" : mediaType,
                null,
                null,
                96d,
                RColorScheme.Light);
        }

        /// <summary>
        /// Builds the context from the adapter's characteristics and the container's viewport.
        /// </summary>
        /// <param name="adapter">supplies the media type and colour scheme</param>
        /// <param name="viewportWidth">viewport width in CSS pixels, or 0 when not yet known</param>
        /// <param name="viewportHeight">viewport height in CSS pixels, or 0 when not yet known</param>
        internal static MediaQueryContext FromAdapter(RAdapter adapter, double viewportWidth, double viewportHeight)
        {
            if (adapter == null) return TypeOnly("all");

            return new MediaQueryContext(
                adapter.DefaultMediaType,
                viewportWidth > 0 ? viewportWidth : (double?)null,
                viewportHeight > 0 ? viewportHeight : (double?)null,
                96d,
                adapter.SystemColorScheme);
        }

        private MediaQueryContext(string mediaType, double? viewportWidth, double? viewportHeight, double resolutionDpi, RColorScheme colorScheme)
        {
            MediaType = mediaType;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            ResolutionDpi = resolutionDpi;
            PreferredColorScheme = colorScheme;
        }

        /// <summary>The active media type (<c>"screen"</c>, <c>"print"</c> or <c>"all"</c>).</summary>
        public string MediaType { get; }

        /// <summary>Viewport width in CSS pixels, or null when unknown (then width/orientation/
        /// aspect-ratio conditions match permissively).</summary>
        public double? ViewportWidth { get; }

        /// <summary>Viewport height in CSS pixels, or null when unknown.</summary>
        public double? ViewportHeight { get; }

        /// <summary>Output resolution in dots per inch, for <c>resolution</c> queries.</summary>
        public double ResolutionDpi { get; }

        /// <summary>What <c>prefers-color-scheme</c> reports.</summary>
        public RColorScheme PreferredColorScheme { get; }
    }
}
