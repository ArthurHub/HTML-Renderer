namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// clip-path, CSS Masking 1 §5.1. Only <c>none</c> and a &lt;basic-shape&gt; without a &lt;geometry-box&gt;.
    /// </summary>
    internal sealed class ClipPathProperty : Property
    {
        private static readonly IValueConverter StyleConverter = new ClipPathValueConverter().OrDefault();

        internal ClipPathProperty()
            : base(PropertyNames.ClipPath, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
