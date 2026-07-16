namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IConditionFunction : IStylesheetNode
    {
        bool Check();
    }
}