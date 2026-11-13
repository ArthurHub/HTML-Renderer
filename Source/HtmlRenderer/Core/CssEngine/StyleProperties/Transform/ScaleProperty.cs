namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ScaleProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.ScaleIndividualConverter.OrNone();

        internal ScaleProperty()
            : base(PropertyNames.Scale, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
