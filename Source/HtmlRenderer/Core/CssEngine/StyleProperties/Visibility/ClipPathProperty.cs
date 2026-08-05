namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>clip-path</c> property (CSS Masking Module Level 1). The basic-shape grammar
    /// (<c>polygon()</c>/<c>inset()</c>/<c>circle()</c>/<c>ellipse()</c>) is resolved against the box's
    /// reference box at paint time, so Layer A only needs to accept and preserve the raw value text.
    /// </summary>
    internal sealed class ClipPathProperty : Property
    {
        // The basic-shape grammar (polygon()/inset()/circle()/ellipse()) plus the "none" keyword is
        // validated once by the shared BasicShapeGrammar via ClipPathValueConverter; the authored text
        // is preserved so the paint-time resolver (CssClipPathResolver) sees exactly what was written.
        private static readonly IValueConverter StyleConverter = new ClipPathValueConverter().OrDefault();

        internal ClipPathProperty()
            : base(PropertyNames.ClipPath, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
