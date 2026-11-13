namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The <c>justify-self</c> property (CSS Box Alignment): a grid item's own inline-axis alignment.</summary>
    internal sealed class JustifySelfProperty : Property
    {
        private static readonly IValueConverter StyleConverter = Converters.JustifySelfConverter;

        internal JustifySelfProperty()
            : base(PropertyNames.JustifySelf)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
