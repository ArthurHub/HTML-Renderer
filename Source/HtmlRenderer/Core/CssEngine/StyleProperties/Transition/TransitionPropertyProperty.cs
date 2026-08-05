namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TransitionPropertyProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.AnimatableConverter.FromList().OrNone().OrDefault(Keywords.All);

        internal TransitionPropertyProperty()
            : base(PropertyNames.TransitionProperty)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}