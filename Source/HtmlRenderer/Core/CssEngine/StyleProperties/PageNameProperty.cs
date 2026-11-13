namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// The <c>page</c> property (CSS Paged Media 3 §3.4): assigns a box to a named page defined by an
    /// <c>@page &lt;name&gt;</c> rule.
    /// </summary>
    internal sealed class PageNameProperty : Property
    {
        internal PageNameProperty()
            : base(PropertyNames.PageName)
        {
        }

        internal override IValueConverter Converter => IdentifierConverter.OrDefault();
    }
}
