using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A node of the parsed condition of an <c>@container</c> rule that uses <c>style()</c> queries
    /// (CSS Containment 3), possibly mixed with size features. The tree is data only; ExCSS does not evaluate it.
    /// </summary>
    internal interface IContainerCondition : IStyleFormattable
    {
        ContainerConditionType Type { get; }

        /// <summary>The operands of an And/Or (two or more), Not or Style (exactly one) node; empty for leaves.</summary>
        IReadOnlyList<IContainerCondition> Operands { get; }

        /// <summary>The property or custom property name of a Declaration node; otherwise null.</summary>
        string Property { get; }

        /// <summary>
        /// For a Declaration node the raw value text (null for the boolean form <c>style(--flag)</c>);
        /// for a Size node the text of the feature including its parentheses; otherwise null.
        /// </summary>
        string Value { get; }
    }
}
