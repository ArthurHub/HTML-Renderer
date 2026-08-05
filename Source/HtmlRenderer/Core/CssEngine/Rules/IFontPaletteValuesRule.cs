namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A palette override declared with a <c>@font-palette-values</c> at-rule (CSS Fonts Module Level 4).
    /// Exposes the prelude name and the three descriptors.
    /// </summary>
    internal interface IFontPaletteValuesRule : IRule, IProperties
    {
        /// <summary>The palette name, a dashed-ident referenced by <c>font-palette: &lt;dashed-ident&gt;</c>, e.g. <c>--my-palette</c>.</summary>
        string Name { get; set; }

        /// <summary>The raw <c>font-family</c> descriptor value (the family this palette applies to).</summary>
        string Family { get; set; }

        /// <summary>The raw <c>base-palette</c> descriptor value (<c>&lt;integer&gt; | light | dark</c>).</summary>
        string BasePalette { get; set; }

        /// <summary>The raw <c>override-colors</c> descriptor value (a comma list of <c>&lt;index&gt; &lt;color&gt;</c> pairs).</summary>
        string OverrideColors { get; set; }
    }
}
