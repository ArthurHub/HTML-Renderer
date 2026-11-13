namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class InteractivityProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "inert").OrDefault();

        internal InteractivityProperty()
            : base(PropertyNames.Interactivity, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
