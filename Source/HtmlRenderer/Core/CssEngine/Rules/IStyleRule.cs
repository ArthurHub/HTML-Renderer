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
    }
}