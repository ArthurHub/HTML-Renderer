namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OrderProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.IntegerConverter
                                                                           .OrGlobalValue()
                                                                           .OrDefault(0);

        internal OrderProperty()
            : base(PropertyNames.Order)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
