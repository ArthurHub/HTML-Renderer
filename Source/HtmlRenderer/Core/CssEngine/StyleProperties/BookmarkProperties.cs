namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// The <c>bookmark-level</c> property (CSS Generated Content 3): <c>none | &lt;integer&gt;</c> (a positive
    /// integer), the level of the PDF outline entry an element generates.
    /// </summary>
    internal sealed class BookmarkLevelProperty : Property
    {
        private static readonly IValueConverter StyleConverter = PositiveIntegerConverter.OrNone().OrDefault();

        internal BookmarkLevelProperty()
            : base(PropertyNames.BookmarkLevel)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// The <c>bookmark-label</c> property (CSS Generated Content 3): a <c>&lt;content-list&gt;</c>, the text of
    /// the outline entry. The initial value is <c>content(text)</c>.
    /// </summary>
    internal sealed class BookmarkLabelProperty : Property
    {
        private static readonly IValueConverter StyleConverter = ContentListItemConverter.Many().OrDefault();

        internal BookmarkLabelProperty()
            : base(PropertyNames.BookmarkLabel)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// The <c>bookmark-state</c> property (CSS Generated Content 3): <c>open | closed</c>.
    /// </summary>
    internal sealed class BookmarkStateProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Assign("open", "open").Or("closed", "closed").OrDefault();

        internal BookmarkStateProperty()
            : base(PropertyNames.BookmarkState)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// The <c>bookmark-target</c> property: <c>self | none | &lt;url&gt; | attr()</c>, the document location the
    /// outline entry points at. The initial value is <c>self</c>.
    /// </summary>
    internal sealed class BookmarkTargetProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Assign("self", "self").Or(UrlConverter).Or(AttrConverter).OrNone().OrDefault();

        internal BookmarkTargetProperty()
            : base(PropertyNames.BookmarkTarget)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
