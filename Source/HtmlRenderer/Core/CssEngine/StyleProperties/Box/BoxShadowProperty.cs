namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class BoxShadowProperty : Property
    {
        // The box-shadow grammar (none | [ inset? && <length>{2,4} && <color>? ]#) is validated once by
        // the shared BoxShadowGrammar via BoxShadowValueConverter; the authored text is preserved so the
        // paint-time resolver (CssBox.PaintBoxShadows) sees exactly what was written.
        private static readonly IValueConverter StyleConverter = new BoxShadowValueConverter().OrDefault();

        internal BoxShadowProperty()
            : base(PropertyNames.BoxShadow, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }
}
