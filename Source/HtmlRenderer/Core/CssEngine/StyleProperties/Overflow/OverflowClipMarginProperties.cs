namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>overflow-clip-margin-*</c> longhands (CSS Overflow 4 §3.2): <c>&lt;visual-box&gt; || &lt;length [0,∞]&gt;</c>.</summary>
    internal abstract class OverflowClipMarginLonghandProperty : Property
    {
        private static readonly IValueConverter LonghandConverter =
            new GrammarValueConverter(OverflowClipMarginGrammar.TryParse).OrDefault();

        protected OverflowClipMarginLonghandProperty(string name)
            : base(name, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => LonghandConverter;
    }

    internal sealed class OverflowClipMarginTopProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginTopProperty() : base(PropertyNames.OverflowClipMarginTop)
        {
        }
    }

    internal sealed class OverflowClipMarginRightProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginRightProperty() : base(PropertyNames.OverflowClipMarginRight)
        {
        }
    }

    internal sealed class OverflowClipMarginBottomProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginBottomProperty() : base(PropertyNames.OverflowClipMarginBottom)
        {
        }
    }

    internal sealed class OverflowClipMarginLeftProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginLeftProperty() : base(PropertyNames.OverflowClipMarginLeft)
        {
        }
    }

    internal sealed class OverflowClipMarginBlockStartProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginBlockStartProperty() : base(PropertyNames.OverflowClipMarginBlockStart)
        {
        }
    }

    internal sealed class OverflowClipMarginBlockEndProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginBlockEndProperty() : base(PropertyNames.OverflowClipMarginBlockEnd)
        {
        }
    }

    internal sealed class OverflowClipMarginInlineStartProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginInlineStartProperty() : base(PropertyNames.OverflowClipMarginInlineStart)
        {
        }
    }

    internal sealed class OverflowClipMarginInlineEndProperty : OverflowClipMarginLonghandProperty
    {
        internal OverflowClipMarginInlineEndProperty() : base(PropertyNames.OverflowClipMarginInlineEnd)
        {
        }
    }

    /// <summary>The <c>overflow-clip-margin</c> shorthand: one value for all four physical sides.</summary>
    internal sealed class OverflowClipMarginProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new OverflowClipMarginShorthandValueConverter(
            PropertyNames.OverflowClipMarginTop, PropertyNames.OverflowClipMarginRight,
            PropertyNames.OverflowClipMarginBottom, PropertyNames.OverflowClipMarginLeft).OrDefault();

        internal OverflowClipMarginProperty()
            : base(PropertyNames.OverflowClipMargin, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>The <c>overflow-clip-margin-block</c> shorthand: one value for both block sides.</summary>
    internal sealed class OverflowClipMarginBlockProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new OverflowClipMarginShorthandValueConverter(
            PropertyNames.OverflowClipMarginBlockStart, PropertyNames.OverflowClipMarginBlockEnd).OrDefault();

        internal OverflowClipMarginBlockProperty()
            : base(PropertyNames.OverflowClipMarginBlock, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>The <c>overflow-clip-margin-inline</c> shorthand: one value for both inline sides.</summary>
    internal sealed class OverflowClipMarginInlineProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new OverflowClipMarginShorthandValueConverter(
            PropertyNames.OverflowClipMarginInlineStart, PropertyNames.OverflowClipMarginInlineEnd).OrDefault();

        internal OverflowClipMarginInlineProperty()
            : base(PropertyNames.OverflowClipMarginInline, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
