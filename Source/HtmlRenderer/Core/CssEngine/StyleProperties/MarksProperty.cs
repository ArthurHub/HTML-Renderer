namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>marks</c> page descriptor (CSS Paged Media 3 section 12): <c>none | [ crop || cross ]</c>,
    /// the printer's marks drawn outside the page box.
    /// </summary>
    internal sealed class MarksProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new MarksValueConverter().OrDefault();

        internal MarksProperty()
            : base(PropertyNames.Marks)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
