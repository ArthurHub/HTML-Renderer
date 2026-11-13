using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The tag-list grammars of <c>font-feature-settings</c> and <c>font-variation-settings</c> (CSS Fonts 4
    /// 6.9 and 6.10).
    /// </summary>
    internal static class FontFeatureTagListGrammar
    {
        public static readonly IValueConverter FeatureSettingsConverter =
            new FontGrammarValueConverter(TryParseFeatureSettings);

        public static readonly IValueConverter VariationSettingsConverter =
            new FontGrammarValueConverter(TryParseVariationSettings);

        /// <summary><c>normal | &lt;feature-tag-value&gt;#</c>, where <c>&lt;feature-tag-value&gt; =
        /// &lt;opentype-tag&gt; [ &lt;integer [0,∞]&gt; | on | off ]?</c>.</summary>
        public static bool TryParseFeatureSettings(IReadOnlyList<Token> tokens, out string text)
        {
            return TryParseList(tokens, TryParseFeatureValue, out text);
        }

        /// <summary><c>normal | [ &lt;opentype-tag&gt; &lt;number&gt; ]#</c></summary>
        public static bool TryParseVariationSettings(IReadOnlyList<Token> tokens, out string text)
        {
            return TryParseList(tokens, TryParseAxisValue, out text);
        }

        private delegate bool ItemParser(IReadOnlyList<Token> item, out string text);

        private static bool TryParseList(IReadOnlyList<Token> tokens, ItemParser itemParser, out string text)
        {
            text = null;

            if (tokens.Count == 1 && FontGrammar.IsIdent(tokens[0], Keywords.Normal))
            {
                text = Keywords.Normal;
                return true;
            }

            var items = new List<string>();

            foreach (var item in ValueExtensions.ToList(tokens))
            {
                var significant = item.Where(t => t.Type != TokenType.Whitespace).ToArray();

                if (!itemParser(significant, out var itemText)) return false;

                items.Add(itemText);
            }

            if (items.Count == 0) return false;

            text = string.Join(", ", items);
            return true;
        }

        private static bool TryParseFeatureValue(IReadOnlyList<Token> item, out string text)
        {
            text = null;

            if (item.Count == 0 || item.Count > 2 || !FontGrammar.IsOpenTypeTag(item[0])) return false;

            var tag = FontGrammar.Quote(item[0].Data);

            if (item.Count == 1)
            {
                text = tag;
                return true;
            }

            var value = item[1];

            if (value.Type == TokenType.Ident && (value.Data.Isi("on") || value.Data.Isi("off")))
            {
                text = tag + " " + value.Data.ToLowerInvariant();
                return true;
            }

            if (value is NumberToken number && number.IsInteger &&
                int.TryParse(number.Data, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integer) &&
                integer >= 0)
            {
                text = tag + " " + integer.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            return false;
        }

        private static bool TryParseAxisValue(IReadOnlyList<Token> item, out string text)
        {
            text = null;

            if (item.Count != 2 || !FontGrammar.IsOpenTypeTag(item[0]) || !(item[1] is NumberToken number))
                return false;

            text = FontGrammar.Quote(item[0].Data) + " " + number.Data;
            return true;
        }
    }
}
