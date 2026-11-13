using System;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Value converters for the CSS Scroll Snap, Overscroll Behavior, Scroll Behavior and Scrollbars
    /// properties. Keyword-only values are validated here and keep their authored text.
    /// </summary>
    internal static class ScrollConverters
    {
        private static IValueConverter KeywordsOf(params string[] keywords)
        {
            return keywords.ToDictionary(k => k, k => k, StringComparer.OrdinalIgnoreCase).ToConverter();
        }

        private static bool IsAny(Token token, params string[] keywords)
        {
            return token.Type == TokenType.Ident && keywords.Any(k => token.Data.Isi(k));
        }

        // scroll-margin-*: <length>
        public static readonly IValueConverter MarginConverter = Converters.LengthConverter;

        // scroll-padding-*: auto | <length-percentage>
        public static readonly IValueConverter PaddingConverter = Converters.LengthOrPercentConverter.OrAuto();

        // overscroll-behavior-x/y: contain | none | auto
        public static readonly IValueConverter OverscrollBehaviorConverter = KeywordsOf("auto", "contain", "none");

        // scroll-behavior: auto | smooth
        public static readonly IValueConverter ScrollBehaviorConverter = KeywordsOf("auto", "smooth");

        // scroll-snap-stop: normal | always
        public static readonly IValueConverter ScrollSnapStopConverter = KeywordsOf("normal", "always");

        // scrollbar-width: auto | thin | none
        public static readonly IValueConverter ScrollbarWidthConverter = KeywordsOf("auto", "thin", "none");

        // scroll-snap-type: none | [ x | y | block | inline | both ] [ mandatory | proximity ]?
        public static readonly IValueConverter ScrollSnapTypeConverter = new KeywordGrammarValueConverter(tokens =>
        {
            if (tokens.Length == 1 && IsAny(tokens[0], Keywords.None)) return true;

            if (tokens.Length < 1 || tokens.Length > 2) return false;
            if (!IsAny(tokens[0], "x", "y", Keywords.Block, Keywords.Inline, Keywords.Both)) return false;

            return tokens.Length == 1 || IsAny(tokens[1], "mandatory", "proximity");
        });

        // scroll-snap-align: none | [ start | end | center ]{1,2}
        public static readonly IValueConverter ScrollSnapAlignConverter = new KeywordGrammarValueConverter(tokens =>
        {
            if (tokens.Length == 1 && IsAny(tokens[0], Keywords.None)) return true;

            return tokens.Length >= 1 && tokens.Length <= 2
                   && tokens.All(t => IsAny(t, Keywords.Start, Keywords.End, Keywords.Center));
        });

        // scrollbar-gutter: auto | stable && both-edges?
        public static readonly IValueConverter ScrollbarGutterConverter = new KeywordGrammarValueConverter(tokens =>
        {
            if (tokens.Length == 1) return IsAny(tokens[0], Keywords.Auto, "stable");

            return tokens.Length == 2
                   && ((IsAny(tokens[0], "stable") && IsAny(tokens[1], "both-edges"))
                       || (IsAny(tokens[0], "both-edges") && IsAny(tokens[1], "stable")));
        });

        // scrollbar-color: auto | <color>{2}
        public static readonly IValueConverter ScrollbarColorConverter = Converters.WithOrder(
            Converters.ColorConverter.Required(),
            Converters.ColorConverter.Required()).Or(Keywords.Auto);
    }
}
