namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>update</c> media feature (Media Queries 4): how frequently the output device can modify the
    /// appearance of content. A printed/exported PDF cannot update once produced, so it is matched against
    /// <c>none</c> in <c>MediaQueryMatcher</c>.
    /// </summary>
    internal sealed class UpdateMediaFeature : MediaFeature
    {
        public UpdateMediaFeature() : base(FeatureNames.Update)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
