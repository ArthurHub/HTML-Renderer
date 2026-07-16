namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ColumnsProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            AutoLengthConverter.Option().For(PropertyNames.ColumnWidth),
            OptionalIntegerConverter.Option().For(PropertyNames.ColumnCount)).OrDefault();

        internal ColumnsProperty()
            : base(PropertyNames.Columns, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}