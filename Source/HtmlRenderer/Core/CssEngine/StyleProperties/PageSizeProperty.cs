namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class PageSizeProperty : Property
    {
        internal PageSizeProperty() : base(PropertyNames.Size)
        {
        }

        internal override IValueConverter Converter => IdentifierConverter.Or(LengthConverter).Many(1, 2).OrDefault();
    }
}
