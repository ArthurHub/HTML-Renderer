namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class CounterResetProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Continuous(
            WithOrder(IdentifierConverter.Required(), IntegerConverter.Option(0))).OrDefault();

        internal CounterResetProperty()
            : base(PropertyNames.CounterReset)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}