using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    ///     The container-side properties of the pre-standard <c>display: -webkit-box</c> ("flexbox 2009") model:
    ///     <c>box-orient</c>, <c>box-direction</c>, <c>box-pack</c> and <c>box-align</c>, under the <c>-webkit-</c> and
    ///     <c>-moz-</c> prefixes. Only created when vendor-prefix aliasing is enabled; they keep their own name (no
    ///     standard counterpart has the same grammar). The item-side <c>box-flex</c> and <c>box-ordinal-group</c> are
    ///     plain aliases of <c>flex-grow</c> and <c>order</c>.
    /// </summary>
    internal sealed class LegacyBoxProperty : Property
    {
        private static readonly string[] Prefixes = {"-webkit-", "-moz-"};

        private static readonly Dictionary<string, IValueConverter> Converters =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["box-orient"] = Keywords("horizontal", "vertical", "inline-axis", "block-axis"),
                ["box-direction"] = Keywords("normal", "reverse"),
                ["box-pack"] = Keywords("start", "end", "center", "justify"),
                ["box-align"] = Keywords("start", "end", "center", "baseline", "stretch")
            };

        private readonly IValueConverter _converter;

        internal LegacyBoxProperty(string name, string baseName)
            : base(name)
        {
            _converter = Converters[baseName];
        }

        internal override IValueConverter Converter => _converter;

        private static IValueConverter Keywords(params string[] keywords)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var keyword in keywords) map[keyword] = keyword;

            return map.ToConverter().OrGlobalValue();
        }

        /// <summary>
        ///     The base name (<c>box-orient</c>...) of <paramref name="propertyName" /> if it is a legacy box property,
        ///     else <see langword="null" />.
        /// </summary>
        internal static string BaseNameOf(string propertyName)
        {
            if (propertyName == null) return null;

            foreach (var prefix in Prefixes)
            {
                if (!propertyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                var rest = propertyName.Substring(prefix.Length);
                foreach (var baseName in Converters.Keys)
                {
                    if (rest.Equals(baseName, StringComparison.OrdinalIgnoreCase)) return baseName;
                }
            }

            return null;
        }
    }
}
