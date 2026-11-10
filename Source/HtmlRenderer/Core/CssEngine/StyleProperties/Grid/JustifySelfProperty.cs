namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>CSS Box Alignment 3 §6.1</summary>
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
