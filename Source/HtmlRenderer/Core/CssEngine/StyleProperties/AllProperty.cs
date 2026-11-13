namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>all</c> shorthand (CSS Cascade 4 3.2): resets every property except <c>direction</c>,
    /// <c>unicode-bidi</c> and custom properties to a single CSS-wide keyword. Its grammar is exactly the
    /// five keywords, so <see cref="ShorthandProperty.Export"/> hands each longhand the keyword unchanged.
    /// The longhand list is built lazily by <see cref="PropertyFactory"/> from every registered longhand.
    /// </summary>
    internal sealed class AllProperty : ShorthandProperty
    {
        private static readonly IValueConverter KeywordConverter =
            new IdentifierValueConverter<object>(Keywords.Initial, null)
                .Or(Keywords.Inherit)
                .Or(Keywords.Unset)
                .Or(Keywords.Revert)
                .Or(Keywords.RevertLayer);

        internal AllProperty()
            : base(PropertyNames.All)
        {
        }

        internal override IValueConverter Converter => KeywordConverter;

        // Every longhand receives the keyword. A few longhands' own grammars don't list the CSS-wide keywords
        // (fill, for instance); `all` must still reset them, so such a longhand keeps the keyword verbatim.
        public override void Export(Property[] properties)
        {
            foreach (var property in properties)
            {
                var value = DeclaredValue.ExtractFor(property.Name);

                if (!property.TrySetValue(value)) property.DeclaredValue = Converters.Any.Convert(value);

                property.IsImportant = IsImportant;
            }
        }
    }
}
