namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    internal sealed class ColumnsProperty : ShorthandProperty
    {
        // CSS Multi-column 1 §3.3. Both longhands take "auto", a positional match would let column-width
        // claim the "auto" of "columns: auto 12em" and strand the length.
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