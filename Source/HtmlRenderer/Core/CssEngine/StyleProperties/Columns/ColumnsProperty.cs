namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ColumnsProperty : ShorthandProperty
    {
        // columns is order-independent (CSS Multi-column 1 - "<'column-width'> || <'column-count'>"), and
        // both longhands accept "auto", so a positional match lets column-width claim "auto" and strand the
        // length in "columns: auto 12em". Match in any order, like list-style.
        private static readonly IValueConverter StyleConverter = WithAnyOrderIndependent(
            AutoLengthConverter.Option().For(PropertyNames.ColumnWidth),
            OptionalIntegerConverter.Option().For(PropertyNames.ColumnCount)).OrDefault();

        internal ColumnsProperty()
            : base(PropertyNames.Columns, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}