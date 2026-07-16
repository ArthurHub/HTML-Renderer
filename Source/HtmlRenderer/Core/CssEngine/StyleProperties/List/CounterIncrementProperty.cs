namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class CounterIncrementProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Continuous(
            WithOrder(IdentifierConverter.Required(), IntegerConverter.Option(1))).OrDefault();

        internal CounterIncrementProperty()
            : base(PropertyNames.CounterIncrement)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}