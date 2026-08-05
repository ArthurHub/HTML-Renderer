namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IMediaRule : IConditionRule
    {
        MediaList Media { get; }
    }
}