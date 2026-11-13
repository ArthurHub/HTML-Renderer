namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>string-set</c> property (GCPM §7.3): sets one or more named strings from a content list, for
    /// later use by the <c>string()</c> function (typically in page margin boxes).
    /// </summary>
    internal sealed class StringSetProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new StringSetValueConverter().OrDefault();

        internal StringSetProperty()
            : base(PropertyNames.StringSet)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
