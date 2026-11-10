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

using System;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// Evaluates a rule's enclosing <c>@media</c> chain against a <see cref="MediaQueryContext"/>, Media
    /// Queries 4 §3.1.
    /// </summary>
    /// <remarks>
    /// A feature the parser doesn't know turns its query into <c>not all</c>, a known one that isn't
    /// evaluated here is false. A viewport feature matches while the viewport size isn't known yet.
    /// </remarks>
    internal static class MediaQueryMatcher
    {
        /// <summary>
        /// True if every nested <c>@media</c> matches (CSS Conditional 3 §2), each when any query of its
        /// list matches (Media Queries 4 §2.1).
        /// </summary>
        internal static bool Matches(MediaList[] enclosingMedia, MediaQueryContext context)
        {
            if (enclosingMedia == null) return true;

            foreach (var mediaList in enclosingMedia)
            {
                var anyMatches = false;
                foreach (var medium in mediaList)
                {
                    if (MediumMatches(medium, context))
                    {
                        anyMatches = true;
                        break;
                    }
                }

                if (!anyMatches) return false;
            }

            return true;
        }

        private static bool MediumMatches(Medium medium, MediaQueryContext context)
        {
            // Media Queries 4 §2, a query without a type matches any type
            var typeMatches = string.IsNullOrEmpty(medium.Type)
                || string.Equals(medium.Type, context.MediaType, StringComparison.OrdinalIgnoreCase)
                || string.Equals(medium.Type, "all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(context.MediaType, "all", StringComparison.OrdinalIgnoreCase);

            var result = typeMatches && medium.Features.All(feature => FeatureMatches(feature, context));

            // Media Queries 4 §2.2.1, not negates the type and the features
            return medium.IsInverse ? !result : result;
        }

        private static bool FeatureMatches(MediaFeature feature, MediaQueryContext context)
        {
            var name = feature.Name.ToLowerInvariant();
            if (name.StartsWith("min-", StringComparison.Ordinal)) name = name.Substring(4);
            else if (name.StartsWith("max-", StringComparison.Ordinal)) name = name.Substring(4);

            switch (name)
            {
                case "width":
                case "device-width":
                case "inline-size":
                    return CompareLength(feature, context.ViewportWidth);
                case "height":
                case "device-height":
                case "block-size":
                    return CompareLength(feature, context.ViewportHeight);

                case "orientation":
                {
                    if (!context.ViewportWidth.HasValue || !context.ViewportHeight.HasValue) return true;
                    // Media Queries 4 §4.4, a square viewport is portrait
                    var orientation = context.ViewportWidth.Value > context.ViewportHeight.Value ? "landscape" : "portrait";
                    return !feature.HasValue
                        || string.Equals(feature.Value, orientation, StringComparison.OrdinalIgnoreCase);
                }

                case "aspect-ratio":
                case "device-aspect-ratio":
                {
                    if (!context.ViewportWidth.HasValue || !context.ViewportHeight.HasValue) return true;
                    if (context.ViewportHeight.Value <= 0) return true;
                    var ratio = feature.AsRatio();
                    if (!ratio.HasValue) return true;
                    return CompareNumeric(context.ViewportWidth.Value / context.ViewportHeight.Value, ratio.Value, feature.Comparison);
                }

                case "resolution":
                {
                    var resolution = feature.AsResolution();
                    if (!resolution.HasValue) return true;
                    return CompareNumeric(context.ResolutionDpi, resolution.Value.To(Resolution.Unit.Dpi), feature.Comparison);
                }

                case "device-pixel-ratio":
                {
                    // Non-standard, the same as resolution in dppx
                    var dpr = feature.AsRatio();
                    if (!dpr.HasValue) return true;
                    return CompareNumeric(context.ResolutionDpi / 96d, dpr.Value, feature.Comparison);
                }

                // Colour output: 8 bits/channel, not a colour-index or monochrome device, not a grid/tty.
                case "color":
                    return CompareCountOrBoolean(feature, 8);
                case "color-index":
                case "monochrome":
                case "grid":
                    return CompareCountOrBoolean(feature, 0);

                case "prefers-color-scheme":
                {
                    var scheme = context.PreferredColorScheme == RColorScheme.Dark ? "dark" : "light";
                    return !feature.HasValue
                        || string.Equals(feature.Value, scheme, StringComparison.OrdinalIgnoreCase);
                }

                // The output is static, it doesn't animate, update or take pointer input
                case "prefers-reduced-motion":
                    return MatchesKeyword(feature, "reduce", true);
                case "hover":
                case "any-hover":
                case "pointer":
                case "any-pointer":
                case "update":
                case "scripting":
                    return MatchesKeyword(feature, "none", false);
                case "prefers-contrast":
                case "prefers-reduced-transparency":
                    return MatchesKeyword(feature, "no-preference", false);

                default:
                    // e.g. scan, which doesn't apply to the output
                    return false;
            }
        }

        private static bool CompareLength(MediaFeature feature, double? actualPx)
        {
            if (!actualPx.HasValue) return true;    // no viewport geometry -> permissive

            var length = feature.AsLength();
            if (!length.HasValue) return true;

            // Media Queries 4 §1.3, em and rem are relative to the initial font size
            const double initialFontPx = 16d;
            var valuePx = length.Value.ToPixels(initialFontPx, initialFontPx, actualPx.Value);

            return CompareNumeric(actualPx.Value, valuePx, feature.Comparison);
        }

        private static bool CompareCountOrBoolean(MediaFeature feature, int actual)
        {
            if (!feature.HasValue) return actual > 0;   // boolean context, e.g. `(color)`

            int value;
            if (!int.TryParse(feature.Value, out value)) return true;   // unparseable -> permissive
            return CompareNumeric(actual, value, feature.Comparison);
        }

        private static bool MatchesKeyword(MediaFeature feature, string deviceValue, bool booleanContextResult)
        {
            // Media Queries 4 §2.4.2, the boolean form is true unless the device value is none
            if (!feature.HasValue) return booleanContextResult;
            return string.Equals(feature.Value, deviceValue, StringComparison.OrdinalIgnoreCase);
        }

        private static bool CompareNumeric(double actual, double value, MediaFeatureComparison comparison)
        {
            const double epsilon = 1e-6;
            switch (comparison)
            {
                case MediaFeatureComparison.Minimum: return actual >= value - epsilon;
                case MediaFeatureComparison.Maximum: return actual <= value + epsilon;
                case MediaFeatureComparison.GreaterThan: return actual > value + epsilon;
                case MediaFeatureComparison.LessThan: return actual < value - epsilon;
                default: return Math.Abs(actual - value) <= epsilon;
            }
        }
    }
}
