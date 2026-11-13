namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class TimelineScopeProperty : Property
    {
        internal TimelineScopeProperty()
            : base(PropertyNames.TimelineScope)
        {
        }

        internal override IValueConverter Converter => TimelineConverters.TimelineScope;
    }
}