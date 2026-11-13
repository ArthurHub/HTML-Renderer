namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The case-sensitivity flag of an attribute selector value (Selectors 4):
    /// <c>[attr=value i]</c> and <c>[attr=value s]</c>.
    /// </summary>
    internal enum AttrCaseSensitivity
    {
        /// <summary>No flag; the document language decides how the value is compared.</summary>
        Default,

        /// <summary>The <c>i</c> flag: the value is compared ASCII case-insensitively.</summary>
        Insensitive,

        /// <summary>The <c>s</c> flag: the value is compared case-sensitively.</summary>
        Sensitive
    }
}