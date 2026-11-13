using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    ///     The legacy vendor-prefixed (<c>-webkit-</c>, <c>-moz-</c>) and pre-standard spellings of properties that
    ///     took the standard grammar. Only consulted when <see cref="ParserOptions.EnableVendorPrefixAliases" /> is on.
    ///     An alias is resolved to its standard name <em>before</em> a property is created, so the property object (and
    ///     everything downstream of it) only ever sees the standard name. Aliases of properties that are not registered
    ///     resolve to nothing and are dropped like any other unknown declaration.
    /// </summary>
    internal static class VendorPropertyAliases
    {
        private static readonly Dictionary<string, string> Table = Build();

        // Vendor properties that are accepted and stored verbatim: they have no standard counterpart ExCSS models
        // (or a different legacy grammar), but real-world stylesheets are full of them and dropping them would lose data.
        private static readonly HashSet<string> AcceptedOnly = new(StringComparer.OrdinalIgnoreCase)
        {
            "-webkit-font-smoothing", "-moz-osx-font-smoothing",
            "-webkit-text-size-adjust", "-moz-text-size-adjust", "-ms-text-size-adjust", "text-size-adjust",
            "-webkit-tap-highlight-color", "-webkit-touch-callout", "-webkit-overflow-scrolling",
            "-webkit-user-drag", "-webkit-user-select", "-moz-user-select", "-ms-user-select",
            "-webkit-text-stroke", "-webkit-text-stroke-width", "-webkit-text-stroke-color",
            "-webkit-text-fill-color", "-webkit-text-security", "-webkit-box-reflect"
        };

        /// <summary>Resolves <paramref name="name" /> to its standard name, or returns it unchanged if it is not an alias.</summary>
        internal static string Canonicalize(string name)
        {
            return name != null && name.Length > 1 && Table.TryGetValue(name, out var canonical) ? canonical : name;
        }

        /// <summary>
        ///     Like <see cref="Canonicalize" />, but only when the standard property is actually registered; an alias of
        ///     an unregistered property keeps its own spelling (it may be an accepted-only property).
        /// </summary>
        internal static string Resolve(string name)
        {
            var canonical = Canonicalize(name);

            return ReferenceEquals(canonical, name) || PropertyFactory.Instance.Create(canonical) == null
                ? name
                : canonical;
        }

        /// <summary>
        ///     Creates the property for <paramref name="name" /> allowing vendor spellings: the name as written, then its
        ///     standard name, then the legacy <c>-webkit-box-*</c> container properties, then the accepted-only vendor
        ///     properties. Returns <see langword="null" /> when none applies.
        /// </summary>
        internal static Property Create(string name)
        {
            var property = PropertyFactory.Instance.Create(name);
            if (property != null) return property;

            var canonical = Canonicalize(name);
            if (!ReferenceEquals(canonical, name))
            {
                property = PropertyFactory.Instance.Create(canonical);
                if (property != null) return property;
            }

            var legacyBox = LegacyBoxProperty.BaseNameOf(name);
            if (legacyBox != null) return new LegacyBoxProperty(name, legacyBox);

            return name != null && AcceptedOnly.Contains(name) ? new UnknownProperty(name) : null;
        }

        private static Dictionary<string, string> Build()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["-webkit-backdrop-filter"] = "backdrop-filter",
                ["-webkit-line-clamp"] = "line-clamp",
                ["color-adjust"] = "print-color-adjust",
                ["-webkit-user-select"] = "user-select",
                ["-moz-user-select"] = "user-select",
                ["-ms-user-select"] = "user-select",
                ["-webkit-text-size-adjust"] = "text-size-adjust",
                ["-moz-text-size-adjust"] = "text-size-adjust",
                ["-ms-text-size-adjust"] = "text-size-adjust",
                // Prince's own spellings of properties that became standard.
                ["-prince-hyphenate-character"] = "hyphenate-character",
                ["-prince-hyphenate-limit-lines"] = "hyphenate-limit-lines",
                ["-prince-bookmark-level"] = "bookmark-level",
                ["-prince-bookmark-label"] = "bookmark-label",
                ["-prince-bookmark-state"] = "bookmark-state",
                ["-prince-bookmark-target"] = "bookmark-target",
                // The legacy column-break-* names are aliases of break-*; their values are translated by
                // VendorValueAliases (always -> column, avoid -> avoid-column).
                ["-webkit-column-break-before"] = "break-before",
                ["-webkit-column-break-after"] = "break-after",
                ["-webkit-column-break-inside"] = "break-inside",
                // WebKit's logical box properties predate margin-inline-* / padding-inline-*.
                ["-webkit-margin-start"] = "margin-inline-start",
                ["-moz-margin-start"] = "margin-inline-start",
                ["-webkit-margin-end"] = "margin-inline-end",
                ["-moz-margin-end"] = "margin-inline-end",
                ["-webkit-padding-start"] = "padding-inline-start",
                ["-moz-padding-start"] = "padding-inline-start",
                ["-webkit-padding-end"] = "padding-inline-end",
                ["-moz-padding-end"] = "padding-inline-end"
            };

            // Properties that shipped behind -webkit- and/or -moz- before unprefixing and kept the standard grammar.
            // Standard names that are not registered (yet) are harmless: Create() finds nothing and the declaration
            // is dropped as an unknown one.
            var prefixed = new[]
            {
                "transform", "transform-origin", "transform-style",
                "perspective", "perspective-origin", "backface-visibility",
                "box-shadow", "box-sizing", "box-decoration-break", "mask", "initial-letter",
                "border-radius", "border-top-left-radius", "border-top-right-radius",
                "border-bottom-left-radius", "border-bottom-right-radius",
                "border-image",
                "background-clip", "background-origin", "background-size",
                "print-color-adjust",
                "clip-path", "filter", "opacity",
                "columns", "column-count", "column-width", "column-gap",
                "column-fill", "column-span", "column-rule",
                "column-rule-width", "column-rule-style", "column-rule-color",
                "hyphens", "tab-size",
                "flex", "flex-flow", "flex-direction", "flex-wrap",
                "flex-grow", "flex-shrink", "flex-basis", "order",
                "align-items", "align-self", "align-content", "justify-content",
                "text-decoration", "text-decoration-line", "text-decoration-style",
                "text-decoration-color", "text-decoration-skip-ink",
                "text-emphasis", "text-emphasis-style", "text-emphasis-color", "text-emphasis-position",
                "font-feature-settings", "font-kerning",
                "writing-mode", "text-orientation",
                "mask-image", "mask-mode", "mask-repeat", "mask-position",
                "mask-size", "mask-origin", "mask-clip", "mask-composite",
                "animation", "animation-name", "animation-duration", "animation-timing-function",
                "animation-delay", "animation-iteration-count", "animation-direction",
                "animation-fill-mode", "animation-play-state",
                "transition", "transition-property", "transition-duration",
                "transition-timing-function", "transition-delay"
            };

            foreach (var name in prefixed)
            {
                map["-webkit-" + name] = name;
                map["-moz-" + name] = name;
            }

            // The item side of the 2009 flexbox model: box-flex's <number> and box-ordinal-group's <integer> mean the
            // same thing as flex-grow and order. The container side needs LegacyBoxProperty.
            foreach (var prefix in new[] {"-webkit-", "-moz-"})
            {
                map[prefix + "box-flex"] = "flex-grow";
                map[prefix + "box-ordinal-group"] = "order";
            }

            return map;
        }
    }
}
