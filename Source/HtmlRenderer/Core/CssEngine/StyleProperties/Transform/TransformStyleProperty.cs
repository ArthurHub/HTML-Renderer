namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TransformStyleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.Toggle(Keywords.Flat, Keywords.Preserve3d).OrDefault(true);

        internal TransformStyleProperty()
            : base(PropertyNames.TransformStyle)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}