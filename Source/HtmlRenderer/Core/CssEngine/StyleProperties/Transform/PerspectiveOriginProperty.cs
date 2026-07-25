namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class PerspectiveOriginProperty : Property
    {
        // perspective-origin is a <position> (CSS Transforms 1). As with transform-origin, the two-value
        // <length-percentage> form is ordered (horizontal then vertical) and only the keyword-only form may
        // be reordered - so "2px left" / "top 2px" are invalid (a length forces the ordered form and the
        // other token is a keyword for the wrong axis).
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