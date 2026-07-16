namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal class GapProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = Converters.LengthOrPercentConverter
                                                                           .OrGlobalValue()
                                                                           .Periodic(PropertyNames.RowGap, PropertyNames.ColumnGap);

        internal GapProperty()
            : base(PropertyNames.Gap, PropertyFlags.Animatable)
        { }

        internal override IValueConverter Converter => StyleConverter;
    }
}
