namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// The <c>counter-set</c> property (CSS Lists 3): <c>[ &lt;counter-name&gt; &lt;integer&gt;? ]+ | none</c>.
    /// </summary>
    internal sealed class CounterSetProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Continuous(
            WithOrder(IdentifierConverter.Required(), IntegerConverter.Option(0))).OrDefault();

        internal CounterSetProperty()
            : base(PropertyNames.CounterSet)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
