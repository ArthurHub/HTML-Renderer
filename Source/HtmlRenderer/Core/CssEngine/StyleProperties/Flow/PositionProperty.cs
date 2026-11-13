namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PositionProperty : Property
    {
        private static readonly IValueConverter StyleConverter =
            Converters.PositionModeConverter.Or(new RunningFunctionConverter()).OrDefault(PositionMode.Static);

        internal PositionProperty()
            : base(PropertyNames.Position)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}