namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ClipProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.ShapeConverter.OrDefault();

        internal ClipProperty()
            : base(PropertyNames.Clip, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}