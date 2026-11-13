using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A feature-value block nested in <c>@font-feature-values</c>: <c>@styleset</c>,
    /// <c>@character-variant</c>, <c>@swash</c>, <c>@ornaments</c>, <c>@annotation</c> or <c>@stylistic</c>.
    /// Each declaration maps an author-chosen name to a list of non-negative integers.
    /// </summary>
    internal interface IFontFeatureValueSetRule : IRule, IProperties
    {
        /// <summary>The block's at-rule name, e.g. <c>styleset</c> (see the <see cref="RuleNames"/> constants).
        /// Named <c>Name</c> because <see cref="IRule.Type"/> is already the <see cref="RuleType"/>.</summary>
        string Name { get; }

        /// <summary>The integers declared for the named entry, or an empty list when it is not declared.</summary>
        IReadOnlyList<int> GetValues(string name);
    }
}
