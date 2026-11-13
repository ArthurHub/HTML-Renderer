namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>contain</c> (CSS Containment 2 §2):
    /// <c>none | strict | content | [ [ size | inline-size ] || layout || style || paint ]</c>.
    /// </summary>
    internal sealed class ContainProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new GrammarValueConverter(ContainGrammar.TryParseContain).OrDefault();

        internal ContainProperty()
            : base(PropertyNames.Contain)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>content-visibility</c> (CSS Containment 2 §4): <c>visible | auto | hidden</c>.</summary>
    internal sealed class ContentVisibilityProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new IdentifierValueConverter<bool>(Keywords.Visible, true)
                .Or(Keywords.Auto)
                .Or(Keywords.Hidden)
                .OrDefault();

        internal ContentVisibilityProperty()
            : base(PropertyNames.ContentVisibility)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>The <c>contain-intrinsic-*</c> longhands (CSS Sizing 4 §5.1): <c>auto? [ none | &lt;length [0,∞]&gt; ]</c>.</summary>
    internal abstract class ContainIntrinsicLonghandProperty : Property
    {
        protected static readonly IValueConverter IntrinsicConverter =
            new GrammarValueConverter(ContainGrammar.TryParseIntrinsic).OrDefault();

        protected ContainIntrinsicLonghandProperty(string name)
            : base(name, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => IntrinsicConverter;
    }

    internal sealed class ContainIntrinsicWidthProperty : ContainIntrinsicLonghandProperty
    {
        internal ContainIntrinsicWidthProperty() : base(PropertyNames.ContainIntrinsicWidth)
        {
        }
    }

    internal sealed class ContainIntrinsicHeightProperty : ContainIntrinsicLonghandProperty
    {
        internal ContainIntrinsicHeightProperty() : base(PropertyNames.ContainIntrinsicHeight)
        {
        }
    }

    internal sealed class ContainIntrinsicInlineSizeProperty : ContainIntrinsicLonghandProperty
    {
        internal ContainIntrinsicInlineSizeProperty() : base(PropertyNames.ContainIntrinsicInlineSize)
        {
        }
    }

    internal sealed class ContainIntrinsicBlockSizeProperty : ContainIntrinsicLonghandProperty
    {
        internal ContainIntrinsicBlockSizeProperty() : base(PropertyNames.ContainIntrinsicBlockSize)
        {
        }
    }

    /// <summary><c>contain-intrinsic-size</c> (CSS Sizing 4 §5.1): <c>[ auto? [ none | &lt;length&gt; ] ]{1,2}</c>.</summary>
    internal sealed class ContainIntrinsicSizeProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new ContainIntrinsicSizeValueConverter().OrDefault();

        internal ContainIntrinsicSizeProperty()
            : base(PropertyNames.ContainIntrinsicSize, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>The <c>container</c> shorthand (CSS Containment 3 §2): <c>&lt;'container-name'&gt; [ / &lt;'container-type'&gt; ]?</c>.</summary>
    internal sealed class ContainerProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new ContainerValueConverter().OrDefault();

        internal ContainerProperty()
            : base(PropertyNames.Container)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
