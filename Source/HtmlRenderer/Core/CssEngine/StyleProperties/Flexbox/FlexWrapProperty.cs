namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FlexWrapProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.FlexWrapConverter;

        internal FlexWrapProperty()
            : base(PropertyNames.FlexWrap)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
