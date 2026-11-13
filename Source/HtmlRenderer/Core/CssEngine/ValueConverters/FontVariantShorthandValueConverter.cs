using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value converter of the <c>font-variant</c> shorthand (CSS Fonts 4 6): the components of the
    /// <c>font-variant-*</c> longhands in any order, or <c>normal</c> / <c>none</c>. Parsing is done by
    /// <see cref="FontVariantGrammar.TryParseShorthand"/>; this converter hands each longhand its slice of
    /// the value, and rebuilds the shortest shorthand text from the longhands when serializing.
    /// </summary>
    internal sealed class FontVariantShorthandValueConverter : IValueConverter
    {
        // The slot order of FontVariantGrammar.TryParseShorthand, which is also the serialization order.
        internal static readonly string[] Longhands =
        {
            PropertyNames.FontVariantLigatures,
            PropertyNames.FontVariantAlternates,
            PropertyNames.FontVariantCaps,
            PropertyNames.FontVariantNumeric,
            PropertyNames.FontVariantEastAsian,
            PropertyNames.FontVariantPosition,
            PropertyNames.FontVariantEmoji
        };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var all = value.ToArray();
            var significant = all.Where(t => t.Type != TokenType.Whitespace).ToArray();

            return FontVariantGrammar.TryParseShorthand(significant, out var texts, out var slots)
                ? new ShorthandValue(Compose(texts), slots, all)
                : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var texts = new string[Longhands.Length];

            for (var i = 0; i < Longhands.Length; i++)
            {
                var property = properties.FirstOrDefault(m => m.Name.Is(Longhands[i]));

                if (property == null) return null;

                texts[i] = property.Value;
            }

            // All seven longhands share one CSS-wide keyword (e.g. "inherit"): so does the shorthand.
            if (texts.All(t => t.Is(texts[0])) && IsGlobal(texts[0])) return new ShorthandValue(texts[0], null, null);

            // A CSS-wide keyword mixed with anything else cannot be written as a shorthand. The one
            // exception is "initial": every font-variant longhand's initial value is "normal" (which is
            // what the "font" shorthand resets them to), so it serializes as "normal".
            for (var i = 0; i < texts.Length; i++)
            {
                if (texts[i].Is(Keywords.Initial)) texts[i] = Keywords.Normal;
                else if (IsGlobal(texts[i])) return null;
            }

            // "none" is only expressible as the shorthand's lone value, which resets everything else.
            if (texts[0].Is(Keywords.None))
            {
                return texts.Skip(1).All(t => t.Is(Keywords.Normal)) ? new ShorthandValue(Keywords.None, null, null) : null;
            }

            return new ShorthandValue(Compose(texts), null, null);
        }

        private static bool IsGlobal(string text) =>
            text.Is(Keywords.Inherit) || text.Is(Keywords.Initial) || text.Is(Keywords.Unset) ||
            text.Is(Keywords.Revert) || text.Is(Keywords.RevertLayer);

        // The shortest text: "normal" when nothing is set, otherwise just the longhands that are not
        // "normal", in the longhands' order (a lone "none" is already handled by the callers).
        private static string Compose(string[] texts)
        {
            var parts = texts.Where(t => !string.IsNullOrEmpty(t) && !t.Is(Keywords.Normal)).ToArray();

            if (parts.Length != 0) return string.Join(" ", parts);

            return texts[0] != null && texts[0].Is(Keywords.None) ? Keywords.None : Keywords.Normal;
        }

        private sealed class ShorthandValue : IPropertyValue
        {
            private readonly List<Token>[] _slots;

            public ShorthandValue(string cssText, List<Token>[] slots, IEnumerable<Token> tokens)
            {
                CssText = cssText;
                _slots = slots;
                Original = new TokenValue(tokens ?? Enumerable.Empty<Token>());
            }

            public string CssText { get; }

            public TokenValue Original { get; }

            // A longhand the authored value did not mention yields null, which resets it to "initial".
            public TokenValue ExtractFor(string name)
            {
                if (_slots == null) return Original;

                var index = System.Array.FindIndex(Longhands, m => m.Is(name));

                return index >= 0 && _slots[index] != null ? new TokenValue(_slots[index]) : null;
            }
        }
    }
}
