namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TransformProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.TransformConverter.Many().OrNone().OrDefault();

        internal TransformProperty()
            : base(PropertyNames.Transform, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}