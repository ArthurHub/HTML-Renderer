namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AlignSelfProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.AlignSelfConverter;

        internal AlignSelfProperty()
            : base(PropertyNames.AlignSelf)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
