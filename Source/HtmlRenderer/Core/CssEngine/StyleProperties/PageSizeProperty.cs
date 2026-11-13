namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// The <c>size</c> descriptor of an <c>@page</c> rule (CSS Paged Media 3 §6.3): a page-size keyword
    /// (<c>A4</c>, <c>letter</c>, …), an orientation (<c>portrait</c>/<c>landscape</c>), or one or two
    /// explicit lengths.
    /// </summary>
    internal sealed class PageSizeProperty : Property
    {
        internal PageSizeProperty()
            : base(PropertyNames.Size)
        {
        }

        internal override IValueConverter Converter =>
            IdentifierConverter.Or(LengthConverter).Many(1, 2).OrDefault();
    }
}
