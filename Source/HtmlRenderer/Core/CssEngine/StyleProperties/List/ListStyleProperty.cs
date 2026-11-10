namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ListStyleProperty : ShorthandProperty
    {
        // CSS Lists 3 §3.6, a "none" goes to whichever of type and image isn't set otherwise, so it can't
        // be claimed by position ("none square" parses like "square none").
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