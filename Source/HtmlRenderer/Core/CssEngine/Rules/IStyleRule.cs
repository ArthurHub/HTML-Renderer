using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IStyleRule : IRule
    {
        string SelectorText { get; set; }
        StyleDeclaration Style { get; }
        ISelector Selector { get; set; }

        /// <summary>
        /// CSS Nesting: style rules nested inside this rule's block, each already resolved to an
        /// absolute selector against this (parent) rule. Empty for a non-nesting rule.
        /// </summary>
        IReadOnlyList<IStyleRule> NestedRules { get; }

        /// <summary>
        /// CSS Nesting: every rule nested inside this rule's block, in source order - nested style rules
        /// (as in <see cref="NestedRules"/>) interleaved with nested conditional group rules
        /// (<c>@media</c>, <c>@supports</c>, <c>@container</c>, <c>@layer</c>). A nested group's own rules
        /// are style rules whose selector is this rule's, holding the group's declarations.
        /// </summary>
        IReadOnlyList<IRule> NestedRuleList { get; }
    }
}
