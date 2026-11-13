namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>@starting-style</c> at-rule, per
    /// <see href="https://www.w3.org/TR/css-transitions-2/#defining-before-change-style-the-starting-style-rule">CSS Transitions 2</see>.
    /// Groups the style rules that supply starting values for transitions on first style update.
    /// </summary>
    internal interface IStartingStyleRule : IGroupingRule
    {
    }
}
