namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TransformOriginProperty : Property
    {
        // CSS Transforms 1 (https://www.w3.org/TR/css-transforms-1/#transform-origin-property):
        //   [ left | center | right | top | bottom | <length-percentage> ]
        // | [ left | center | right | <length-percentage> ] [ top | center | bottom | <length-percentage> ] <length>?
        // | [ [ center | left | right ] && [ center | top | bottom ] ] <length>?
        //
        // The two-value <length-percentage> form is ORDERED (horizontal then vertical). Only the keyword-only
        // form may be reordered ("center left" == "left center"). So "2px left" and "top 2px" are invalid: a
        // length in one axis forces the ordered form, and the remaining token is a keyword for the wrong axis.
        private static readonly IValueConverter Horizontal =
            LengthOrPercentConverter
                .Or(Keywords.Left, Length.Zero)
                .Or(Keywords.Right, Length.Full)
                .Or(Keywords.Center, Length.Half);

        private static readonly IValueConverter Vertical =
            LengthOrPercentConverter
                .Or(Keywords.Top, Length.Zero)
                .Or(Keywords.Bottom, Length.Full)
                .Or(Keywords.Center, Length.Half);

        private static readonly IValueConverter HorizontalKeyword =
            Assign(Keywords.Left, Length.Zero)
                .Or(Keywords.Right, Length.Full)
                .Or(Keywords.Center, Length.Half);

        private static readonly IValueConverter VerticalKeyword =
            Assign(Keywords.Top, Length.Zero)
                .Or(Keywords.Bottom, Length.Full)
                .Or(Keywords.Center, Length.Half);

        // A one- or two-value <position> with no z-offset (this is exactly the perspective-origin grammar).
        private static readonly IValueConverter Position =
            LengthOrPercentConverter.Or(Keywords.Center, Point.Center)
                // Ordered horizontal-then-vertical (rejects a keyword in the wrong axis position).
                .Or(WithOrder(Horizontal.Option(Length.Half), Vertical.Option(Length.Half)))
                // Keyword-only form, any order ("center left"): a token accepted by both axes must not be
                // claimed positionally, so this needs order-independent matching.
                .Or(WithAnyOrderIndependent(HorizontalKeyword.Option(Length.Half), VerticalKeyword.Option(Length.Half)));

        // A genuine two-value position: BOTH a horizontal and a vertical component must be present. The axis
        // converters are bare (not .Option), so each must actually consume a token. The <length> z-offset only
        // exists in this two-value production, so gating the z-offset behind this (rather than the shared outer
        // WithOrder) is what makes a single-value position + length such as "top 2px" invalid.
        private static readonly IValueConverter TwoValuePosition =
            WithOrder(Horizontal, Vertical)
                .Or(WithAnyOrderIndependent(HorizontalKeyword, VerticalKeyword));

        private static readonly IValueConverter StyleConverter =
            // Two-value position followed by a (required) <length> z-offset - this branch only matches when a
            // third length token is actually present...
            WithOrder(TwoValuePosition, LengthConverter)
                // ...otherwise a plain one- or two-value <position> with no z-offset.
                .Or(Position)
                .OrDefault(Point.Center);

        internal TransformOriginProperty()
            : base(PropertyNames.TransformOrigin, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
