namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IContainerRule : IConditionRule
    {
        string Name { get; set; }
        MediaList Media { get; }
    }
}