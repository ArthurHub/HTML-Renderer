namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>hyphenate-character</c> (CSS Text 4 6.3.1): <c>auto | &lt;string&gt;</c>.</summary>
    internal sealed class HyphenateCharacterProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.StringConverter.Or(Keywords.Auto).OrDefault();

        internal HyphenateCharacterProperty()
            : base(PropertyNames.HyphenateCharacter, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>hyphenate-limit-chars</c> (CSS Text 4 6.3.4): <c>[ auto | &lt;integer&gt; ]{1,3}</c>.</summary>
    internal sealed class HyphenateLimitCharsProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new GrammarValueConverter(HyphenateLimitCharsGrammar.IsValid).OrDefault();

        internal HyphenateLimitCharsProperty()
            : base(PropertyNames.HyphenateLimitChars, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>hyphenate-limit-lines</c> (CSS Text 4 6.3.5): <c>no-limit | &lt;integer [0,inf]&gt;</c>.</summary>
    internal sealed class HyphenateLimitLinesProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.NaturalIntegerConverter.Or("no-limit").OrDefault();

        internal HyphenateLimitLinesProperty()
            : base(PropertyNames.HyphenateLimitLines, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>hyphenate-limit-last</c> (CSS Text 4 6.3.5): <c>none | always | column | page | spread</c>.</summary>
    internal sealed class HyphenateLimitLastProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            GrammarValueConverter.OneOf(Keywords.None, Keywords.Always, Keywords.Column, Keywords.Page, "spread")
                .OrDefault();

        internal HyphenateLimitLastProperty()
            : base(PropertyNames.HyphenateLimitLast, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>hyphenate-limit-zone</c> (CSS Text 4 6.3.3): <c>&lt;length-percentage&gt;</c>.</summary>
    internal sealed class HyphenateLimitZoneProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.LengthOrPercentConverter.OrDefault(Length.Zero);

        internal HyphenateLimitZoneProperty()
            : base(PropertyNames.HyphenateLimitZone, PropertyFlags.Inherited | PropertyFlags.Unitless)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// <c>hanging-punctuation</c> (CSS Text 3 8): <c>none | [ first || [ force-end | allow-end ] || last ]</c>.
    /// </summary>
    internal sealed class HangingPunctuationProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new GrammarValueConverter(HangingPunctuationGrammar.IsValid).OrDefault();

        internal HangingPunctuationProperty()
            : base(PropertyNames.HangingPunctuation, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
