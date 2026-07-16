namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class HyphensProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.Assign(Keywords.None, Keywords.None)
                .Or(Keywords.Manual, Keywords.Manual)
                .Or(Keywords.Auto, Keywords.Auto)
                .OrDefault(Keywords.Manual);

        internal HyphensProperty()
            : base(PropertyNames.Hyphens, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
