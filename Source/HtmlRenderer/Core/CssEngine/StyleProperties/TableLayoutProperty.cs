namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TableLayoutProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.TableLayoutConverter.OrDefault(false);

        internal TableLayoutProperty()
            : base(PropertyNames.TableLayout)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}