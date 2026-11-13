namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary><c>line-clamp</c> (CSS Overflow 4): <c>none | &lt;integer [1,inf]&gt;</c>.</summary>
    internal sealed class LineClampProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.PositiveIntegerConverter.OrNone().OrDefault();

        internal LineClampProperty()
            : base(PropertyNames.LineClamp)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>block-ellipsis</c> (CSS Overflow 4): <c>none | auto | &lt;string&gt;</c>.</summary>
    internal sealed class BlockEllipsisProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.StringConverter.OrNone().OrAuto().OrDefault();

        internal BlockEllipsisProperty()
            : base(PropertyNames.BlockEllipsis, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>tab-size</c> (CSS Text 4 3.6): <c>&lt;number [0,inf]&gt; | &lt;length [0,inf]&gt;</c>.</summary>
    internal sealed class TabSizeProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new GrammarValueConverter(NonNegativeLengthGrammar.IsTabSize).OrDefault();

        internal TabSizeProperty()
            : base(PropertyNames.TabSize, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// <c>initial-letter</c> (CSS Inline 3 7.4):
    /// <c>normal | &lt;number [1,inf]&gt; &lt;integer [1,inf]&gt;? | &lt;number [1,inf]&gt; [ drop | raise ]</c>.
    /// </summary>
    internal sealed class InitialLetterProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new GrammarValueConverter(InitialLetterGrammar.IsValid).OrDefault();

        internal InitialLetterProperty()
            : base(PropertyNames.InitialLetter)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>line-height-step</c> (CSS Rhythmic Sizing 1 3.1): <c>none | &lt;length [0,inf]&gt;</c>.</summary>
    internal sealed class LineHeightStepProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new GrammarValueConverter(RhythmGrammar.IsNoneOrLength).OrDefault();

        internal LineHeightStepProperty()
            : base(PropertyNames.LineHeightStep, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
