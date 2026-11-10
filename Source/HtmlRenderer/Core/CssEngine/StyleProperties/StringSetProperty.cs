namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>string-set</c> (CSS Generated Content 3 §2.7.1).
    /// </summary>
    internal sealed class StringSetProperty : Property
    {
        private static readonly IValueConverter ValueConverter = new StringSetValueConverter().OrDefault();

        internal StringSetProperty() : base(PropertyNames.StringSet)
        {
        }

        internal override IValueConverter Converter => ValueConverter;
    }
}
