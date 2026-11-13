using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A <c>@font-feature-values</c> at-rule (CSS Fonts Module Level 4, section 6.4): associates named
    /// OpenType feature-value aliases with one or more font families.
    /// </summary>
    internal interface IFontFeatureValuesRule : IRule
    {
        /// <summary>The <c>&lt;family-name&gt;#</c> prelude, one entry per family. Quoted names keep their quotes.</summary>
        IReadOnlyList<string> FontFamilies { get; }

        /// <summary>The nested <see cref="IFontFeatureValueSetRule"/> blocks, in source order.</summary>
        IRuleList Rules { get; }
    }
}
