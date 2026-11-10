namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TransformOriginProperty : Property
    {
        // CSS Transforms 1 §5. A length forces horizontal-then-vertical order, only keyword pairs may be swapped.
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

        private static readonly IValueConverter Position =
            LengthOrPercentConverter.Or(Keywords.Center, Point.Center)
                .Or(WithOrder(Horizontal.Option(Length.Half), Vertical.Option(Length.Half)))
                // "center" fits both axes, so it must not be claimed positionally
                .Or(WithAnyOrderIndependent(HorizontalKeyword.Option(Length.Half), VerticalKeyword.Option(Length.Half)));

        // Both axes required, the z-offset only follows a two-value position ("top 2px" is invalid)
        private static readonly IValueConverter TwoValuePosition =
            WithOrder(Horizontal, Vertical)
                .Or(WithAnyOrderIndependent(HorizontalKeyword, VerticalKeyword));

        private static readonly IValueConverter StyleConverter =
            WithOrder(TwoValuePosition, LengthConverter)
                .Or(Position)
                .OrDefault(Point.Center);

        internal TransformOriginProperty()
            : base(PropertyNames.TransformOrigin, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
