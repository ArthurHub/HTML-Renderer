namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>system</c> descriptor of an <c>@counter-style</c> rule.</summary>
    internal sealed class CounterSystemDescriptorProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new CounterSystemValueConverter();

        internal CounterSystemDescriptorProperty()
            : base("system")
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
