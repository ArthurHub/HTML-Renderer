namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>bleed</c> page descriptor (CSS Paged Media 3 section 13): <c>auto | &lt;length&gt;</c>, the
    /// extent of the page bleed area outside the page box.
    /// </summary>
    internal sealed class BleedProperty : Property
    {
        internal BleedProperty()
            : base(PropertyNames.Bleed)
        {
        }

        internal override IValueConverter Converter => Converters.AutoLengthConverter.OrDefault();
    }
}
