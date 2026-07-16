namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AnimationTimingFunctionProperty : Property
    {
        private static readonly IValueConverter ListConverter =
            Converters.TransitionConverter.FromList().OrDefault(Map.TimingFunctions[Keywords.Ease]);

        internal AnimationTimingFunctionProperty()
            : base(PropertyNames.AnimationTimingFunction)
        {
        }

        internal override IValueConverter Converter => ListConverter;
    }
}