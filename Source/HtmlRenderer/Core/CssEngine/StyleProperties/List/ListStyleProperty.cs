namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ListStyleProperty : ShorthandProperty
    {
        // list-style's operands are order-independent (CSS Lists 3 - "in any order"), so a token such as
        // "none" that both list-style-type and list-style-image accept must not be claimed positionally.
        // See TylerBrinks/ExCSS#185 ("list-style: none square").
        private static readonly IValueConverter StyleConverter = WithAnyOrderIndependent(
            ListStyleConverter.Option().For(PropertyNames.ListStyleType),
            ListPositionConverter.Option().For(PropertyNames.ListStylePosition),
            OptionalImageSourceConverter.Option().For(PropertyNames.ListStyleImage)).OrDefault();

        internal ListStyleProperty()
            : base(PropertyNames.ListStyle, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}