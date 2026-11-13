namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The kind of a node in a parsed <c>@container</c> condition containing a <c>style()</c> query.
    /// </summary>
    internal enum ContainerConditionType : byte
    {
        /// <summary>A size feature such as <c>(min-width: 400px)</c>; <see cref="IContainerCondition.Value"/> holds its text.</summary>
        Size,
        /// <summary>A <c>style( ... )</c> query; the single operand is the query inside the function.</summary>
        Style,
        /// <summary>A style feature <c>--name: value</c> (or the boolean form <c>--name</c>, whose value is null).</summary>
        Declaration,
        /// <summary>Operands joined by <c>and</c>.</summary>
        And,
        /// <summary>Operands joined by <c>or</c>.</summary>
        Or,
        /// <summary>A <c>not</c> negation of the single operand.</summary>
        Not
    }
}
