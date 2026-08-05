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
    /// Evaluates a rule's enclosing <c>@media</c> chain against a <see cref="MediaQueryContext"/>, so
    /// feature conditions (<c>min-width</c>, range syntax like <c>width &gt;= 48rem</c>,
    /// <c>orientation</c>, <c>resolution</c>, <c>prefers-color-scheme</c>, ...) actually gate which rules
    /// apply. Media Queries 4.
    /// </summary>
    /// <remarks>
    /// Any feature this matcher cannot actually evaluate causes its <c>@media</c> block to be ignored: a
    /// feature name the CSS engine does not register fails to parse and its query becomes <c>not all</c>
    /// upstream, and a registered feature this matcher does not model hits the <c>default</c> arm and
    /// returns false. Both are Media Queries 4's "unknown feature is false" - rather than apply rules
    /// guarded by a condition that cannot be tested, the block is dropped. A feature that IS modelled but
    /// whose runtime context is missing (a <c>width</c> query before the viewport is known) stays
    /// permissive; that is an unknown *context*, not an unsupported feature.
    /// </remarks>
    internal static class MediaQueryMatcher
    {
        /// <summary>
        /// True if every level of the enclosing <c>@media</c> chain matches (nesting is conjunctive), or
        /// the rule isn't nested in any <c>@media</c> at all. Within one level's comma-separated media
        /// query list, at least one query must match.
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
            // A feature-only query (e.g. `@media (min-width: 768px)`) has no type - treat as `all`.
            var typeMatches = string.IsNullOrEmpty(medium.Type)
                || string.Equals(medium.Type, context.MediaType, StringComparison.OrdinalIgnoreCase)
                || string.Equals(medium.Type, "all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(context.MediaType, "all", StringComparison.OrdinalIgnoreCase);

            var result = typeMatches && medium.Features.All(feature => FeatureMatches(feature, context));

            // `not` negates the whole query (type + features), per Media Queries 4 §3.1.
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
                    // MQ4 §4.1: portrait is height >= width, so a square viewport is portrait.
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
                    // Non-standard feature; the value is a plain number of device pixels per CSS px,
                    // i.e. the same as `resolution` in dppx.
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

                // The rendered output is static: it does not animate, cannot be hovered or pointed at,
                // and once painted does not update. The stance for reduced motion is `reduce`, which is
                // NOT the `no-preference` resting value, so the boolean form `(prefers-reduced-motion)`
                // is true (MQ5 §5.3).
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
                    // A registered feature this matcher does not evaluate (e.g. `scan`): treat as not
                    // matching so the whole @media block is ignored, rather than applying rules guarded
                    // by a condition that cannot be tested (see the class remarks).
                    return false;
            }
        }

        private static bool CompareLength(MediaFeature feature, double? actualPx)
        {
            if (!actualPx.HasValue) return true;    // no viewport geometry -> permissive

            var length = feature.AsLength();
            if (!length.HasValue) return true;

            // In a media query, `em`/`rem` resolve against the initial font size (16px), NOT the
            // document root's font size (Media Queries 4 §1.3 / §6).
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
            // The value form `(feature: value)` matches iff the queried value equals the device's value.
            // The boolean form `(feature)` is true iff the device value is not the feature's "none"/
            // "no-preference" resting state.
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
