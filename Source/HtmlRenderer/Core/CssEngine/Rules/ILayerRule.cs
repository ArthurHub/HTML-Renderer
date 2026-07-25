namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The block form of the <c>@layer</c> cascade-layer at-rule (<c>@layer name { … }</c>, or an
    /// anonymous <c>@layer { … }</c>). Groups the style rules that belong to a named cascade layer,
    /// per <see href="https://www.w3.org/TR/css-cascade-5/#layering">CSS Cascade 5</see>.
    /// </summary>
    internal interface ILayerRule : IGroupingRule
    {
        /// <summary>The layer's (optionally dotted) name; empty for an anonymous layer.</summary>
        string Name { get; }
    }
}
