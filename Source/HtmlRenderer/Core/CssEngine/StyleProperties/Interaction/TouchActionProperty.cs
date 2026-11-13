namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TouchActionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new TouchActionValueConverter().OrDefault();

        internal TouchActionProperty()
            : base(PropertyNames.TouchAction)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
