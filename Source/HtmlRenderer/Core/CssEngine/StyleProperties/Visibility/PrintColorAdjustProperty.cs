namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PrintColorAdjustProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("economy", "exact").OrDefault();

        internal PrintColorAdjustProperty()
            : base(PropertyNames.PrintColorAdjust, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
