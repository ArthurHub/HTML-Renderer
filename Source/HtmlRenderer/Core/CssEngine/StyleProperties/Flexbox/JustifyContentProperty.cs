namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class JustifyContentProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.JustifyContentConverter
                                                                           .OrDefault(Keywords.Normal);

        internal JustifyContentProperty()
            : base(PropertyNames.JustifyContent)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
