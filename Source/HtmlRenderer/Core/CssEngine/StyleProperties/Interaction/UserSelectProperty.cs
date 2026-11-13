namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class UserSelectProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            new KeywordSetValueConverter("auto", "text", "none", "all", "contain").OrDefault();

        internal UserSelectProperty()
            : base(PropertyNames.UserSelect)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
