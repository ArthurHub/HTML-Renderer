using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    ///     Legacy vendor-prefixed <em>keyword values</em> (<c>display: -webkit-flex</c>, <c>position: -webkit-sticky</c>)
    ///     and legacy keyword vocabularies (<c>-webkit-column-break-before: always</c>) that mean exactly a standard
    ///     keyword. Applied to a declaration's value before it is validated, only when vendor-prefix aliasing is enabled.
    /// </summary>
    internal static class VendorValueAliases
    {
        private static readonly Dictionary<string, string> DisplayValues = new(StringComparer.OrdinalIgnoreCase)
        {
            ["-webkit-flex"] = Keywords.Flex,
            ["-webkit-inline-flex"] = Keywords.InlineFlex,
            ["-ms-flexbox"] = Keywords.Flex,
            ["-ms-inline-flexbox"] = Keywords.InlineFlex,
            // The 2009 flexbox model. The legacy -webkit-box-orient: vertical (block-stacking) case needs layout
            // knowledge; the box itself maps to a flex container.
            ["-webkit-box"] = Keywords.Flex,
            ["-moz-box"] = Keywords.Flex,
            ["-webkit-inline-box"] = Keywords.InlineFlex,
            ["-moz-inline-box"] = Keywords.InlineFlex
        };

        /// <summary>
        ///     Returns <paramref name="value" /> with vendor keyword spellings replaced, or <paramref name="value" />
        ///     itself when nothing needs replacing.
        /// </summary>
        /// <param name="writtenName">The property name as the author wrote it.</param>
        /// <param name="propertyName">The standard name of the property created for it.</param>
        internal static TokenValue Normalize(string writtenName, string propertyName, TokenValue value)
        {
            if (value == null || value.Count == 0) return value;

            Func<Token, string> map = null;

            if (propertyName.Isi(PropertyNames.Display))
                map = t => DisplayValues.TryGetValue(t.Data, out var standard) ? standard : null;
            else if (propertyName.Isi(PropertyNames.Position))
                map = t => t.Data.Isi("-webkit-sticky") ? Keywords.Sticky : null;
            else if (writtenName.Isi("-webkit-column-break-before") || writtenName.Isi("-webkit-column-break-after"))
                map = t => t.Data.Isi(Keywords.Always) ? Keywords.Column
                    : t.Data.Isi(Keywords.Avoid) ? Keywords.AvoidColumn
                    : null;
            else if (writtenName.Isi("-webkit-column-break-inside"))
                map = t => t.Data.Isi(Keywords.Avoid) ? Keywords.AvoidColumn : null;
            else if (propertyName.Isi(PropertyNames.TransitionProperty) || propertyName.Isi(PropertyNames.Transition))
                map = t =>
                {
                    var canonical = VendorPropertyAliases.Resolve(t.Data);
                    return ReferenceEquals(canonical, t.Data) ? null : canonical;
                };

            if (map == null) return value;

            List<Token> tokens = null;
            for (var i = 0; i < value.Count; i++)
            {
                var token = value[i];
                var replacement = token.Type == TokenType.Ident ? map(token) : null;

                if (replacement == null)
                {
                    tokens?.Add(token);
                    continue;
                }

                if (tokens == null)
                {
                    tokens = new List<Token>(value.Count);
                    for (var j = 0; j < i; j++) tokens.Add(value[j]);
                }

                tokens.Add(new Token(TokenType.Ident, replacement, token.Position));
            }

            if (tokens == null) return value;

            return new TokenValue(tokens) {StylesheetText = value.StylesheetText};
        }
    }
}
