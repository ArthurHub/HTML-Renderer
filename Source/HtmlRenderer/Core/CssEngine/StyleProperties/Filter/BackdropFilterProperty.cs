namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BackdropFilterProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new FilterValueConverter().OrDefault();

        internal BackdropFilterProperty()
            : base(PropertyNames.BackdropFilter, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
