namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ForcedColorAdjustProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "none", "preserve-parent-color").OrDefault();

        internal ForcedColorAdjustProperty()
            : base(PropertyNames.ForcedColorAdjust, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
