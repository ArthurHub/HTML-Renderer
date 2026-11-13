namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TranslateProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.TranslateIndividualConverter.OrNone();

        internal TranslateProperty()
            : base(PropertyNames.Translate, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
