namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class PerspectiveOriginProperty : Property
    {
        // CSS Transforms 2 §9, only the one- and two-value <position> forms. A length forces
        // horizontal-then-vertical order, only keyword pairs may be swapped.
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

        private static readonly IValueConverter PerspectiveConverter = LengthOrPercentConverter.Or(
            Keywords.Left, new Point(Length.Zero, Length.Half)).Or(
            Keywords.Center, new Point(Length.Half, Length.Half)).Or(
            Keywords.Right, new Point(Length.Full, Length.Half)).Or(
            Keywords.Top, new Point(Length.Half, Length.Zero)).Or(
            Keywords.Bottom, new Point(Length.Half, Length.Full)).Or(
            WithOrder(Horizontal.Option(Length.Half), Vertical.Option(Length.Half))).Or(
            WithAnyOrderIndependent(HorizontalKeyword.Option(Length.Half), VerticalKeyword.Option(Length.Half)))
            .OrDefault(Point.Center);


        internal PerspectiveOriginProperty()
            : base(PropertyNames.PerspectiveOrigin, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => PerspectiveConverter;
    }
}