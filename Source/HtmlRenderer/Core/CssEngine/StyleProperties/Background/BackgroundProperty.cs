namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using System.Collections.Generic;
    using static Converters;

    internal sealed class BackgroundProperty : ShorthandProperty
    {
        internal BackgroundProperty()
            : base(PropertyNames.Background, PropertyFlags.Animatable)
        {
        }

        internal override IValueConverter Converter => StyleConverter;

        private static readonly IValueConverter NormalLayerConverter = WithAny(
            OptionalImageSourceConverter.Option().For(PropertyNames.BackgroundImage),
            // <bg-position> [ / <bg-size> ]?: the size cannot stand without the position before its slash.
            WithOrderDependingOnFirst(
                PointConverter.Option().For(PropertyNames.BackgroundPosition),
                BackgroundSizeConverter.StartsWithDelimiter().Option().For(PropertyNames.BackgroundSize)),
            BackgroundRepeatsConverter.Option().For(PropertyNames.BackgroundRepeat),
            BackgroundAttachmentConverter.Option().For(PropertyNames.BackgroundAttachment),
            BoxModelConverter.Option().For(PropertyNames.BackgroundOrigin),
            BackgroundClipConverter.Option().For(PropertyNames.BackgroundClip));

        private static readonly IValueConverter FinalLayerConverter = WithAny(
            OptionalImageSourceConverter.Option().For(PropertyNames.BackgroundImage),
            // <bg-position> [ / <bg-size> ]?: the size cannot stand without the position before its slash.
            WithOrderDependingOnFirst(
                PointConverter.Option().For(PropertyNames.BackgroundPosition),
                BackgroundSizeConverter.StartsWithDelimiter().Option().For(PropertyNames.BackgroundSize)),
            BackgroundRepeatsConverter.Option().For(PropertyNames.BackgroundRepeat),
            BackgroundAttachmentConverter.Option().For(PropertyNames.BackgroundAttachment),
            BoxModelConverter.Option().For(PropertyNames.BackgroundOrigin),
            BackgroundClipConverter.Option().For(PropertyNames.BackgroundClip),
            CurrentColorConverter.Option().For(PropertyNames.BackgroundColor));

        // What a layer takes for a longhand its own text leaves out (the initial values, CSS Backgrounds 3 section 2.10).
        // The colour is not here: it is set by the last layer only.
        private static readonly Dictionary<string, string> LayerDefaults = new()
        {
            [PropertyNames.BackgroundImage] = "none",
            [PropertyNames.BackgroundPosition] = "0% 0%",
            [PropertyNames.BackgroundSize] = "auto",
            [PropertyNames.BackgroundRepeat] = "repeat",
            [PropertyNames.BackgroundAttachment] = "scroll",
            [PropertyNames.BackgroundOrigin] = "padding-box",
            [PropertyNames.BackgroundClip] = "border-box",
        };

        // One <box> value sets both background-origin and background-clip; with two, the first is the origin and the second
        // the clip. The converter hands the one box to the origin, so a layer with no clip of its own takes its origin's.
        private static readonly Dictionary<string, string> LayerFallbacks = new()
        {
            [PropertyNames.BackgroundClip] = PropertyNames.BackgroundOrigin,
        };

        private static readonly IValueConverter StyleConverter =
            NormalLayerConverter.RequiresEnd(FinalLayerConverter, LayerDefaults, LayerFallbacks).OrDefault();
    }
}