namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using System.Collections.Generic;
    using static Converters;

    internal sealed class FontProperty : ShorthandProperty
    {
        // The only font-variant values the shorthand embeds are those of <font-variant-css2>:
        // normal | small-caps. They set font-variant-caps; the other font-variant-* longhands - like
        // font-kerning, font-feature-settings, ... - are reset implicitly (CSS Fonts 4 7.7).
        private static readonly IValueConverter ShorthandConverter = WithOrder(
            WithAny(
                FontStyleConverter.Option().For(PropertyNames.FontStyle),
                FontVariantConverter.Option().For(PropertyNames.FontVariantCaps),
                FontWeightConverter.Or(WeightNumberConverter).Option().For(PropertyNames.FontWeight),
                FontStretchConverter.Option().For(PropertyNames.FontStretch)),
            WithOrder(
                FontSizeConverter.Required().For(PropertyNames.FontSize),
                LineHeightConverter.StartsWithDelimiter().Option().For(PropertyNames.LineHeight),
                FontFamiliesConverter.Required().For(PropertyNames.FontFamily)))
            .Or(SystemFontConverter)
            .OrGlobalValue();

        private static readonly IValueConverter StyleConverter = new ImplicitResetValueConverter(
            ShorthandConverter,
            new Dictionary<string, string[]>
            {
                // font-variant-caps is written by the shorthand, but only normal | small-caps can be.
                {PropertyNames.FontVariantCaps, new[] {Keywords.Normal, "small-caps"}},
                {PropertyNames.FontVariantLigatures, new[] {Keywords.Normal}},
                {PropertyNames.FontVariantAlternates, new[] {Keywords.Normal}},
                {PropertyNames.FontVariantNumeric, new[] {Keywords.Normal}},
                {PropertyNames.FontVariantEastAsian, new[] {Keywords.Normal}},
                {PropertyNames.FontVariantPosition, new[] {Keywords.Normal}},
                {PropertyNames.FontVariantEmoji, new[] {Keywords.Normal}},
                {PropertyNames.FontKerning, new[] {Keywords.Auto}},
                {PropertyNames.FontOpticalSizing, new[] {Keywords.Auto}},
                {PropertyNames.FontSizeAdjust, new[] {Keywords.None}},
                {PropertyNames.FontVariationSettings, new[] {Keywords.Normal}},
                {PropertyNames.FontFeatureSettings, new[] {Keywords.Normal}},
                {PropertyNames.FontLanguageOverride, new[] {Keywords.Normal}}
            });

        internal FontProperty()
            : base(PropertyNames.Font, PropertyFlags.Inherited | PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}