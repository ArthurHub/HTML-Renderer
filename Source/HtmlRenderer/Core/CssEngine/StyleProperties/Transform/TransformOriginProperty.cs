namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class TransformOriginProperty : Property
    {
        private static readonly IValueConverter StyleConverter = WithOrder(
            LengthOrPercentConverter.Or(Keywords.Center, Point.Center).Or(WithAny(
                LengthOrPercentConverter.Or(Keywords.Left, Length.Zero)
                    .Or(Keywords.Right, Length.Full)
                    .Or(Keywords.Center, Length.Half)
                    .Option(Length.Half),
                LengthOrPercentConverter.Or(Keywords.Top, Length.Zero)
                    .Or(Keywords.Bottom, Length.Full)
                    .Or(Keywords.Center, Length.Half)
                    .Option(Length.Half))).Or(
                WithAny(
                    LengthOrPercentConverter.Or(Keywords.Top, Length.Zero)
                        .Or(Keywords.Bottom, Length.Full)
                        .Or(Keywords.Center, Length.Half)
                        .Option(Length.Half),
                    LengthOrPercentConverter.Or(Keywords.Left, Length.Zero)
                        .Or(Keywords.Right, Length.Full)
                        .Or(Keywords.Center, Length.Half)
                        .Option(Length.Half))).Required(),
            LengthConverter.Option(Length.Zero)).OrDefault(Point.Center);

        internal TransformOriginProperty()
            : base(PropertyNames.TransformOrigin, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}