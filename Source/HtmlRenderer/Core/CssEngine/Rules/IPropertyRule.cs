namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A registered custom property declared with an <c>@property</c> at-rule
    /// (CSS Properties &amp; Values API Level 1 §3). Exposes the three descriptors.
    /// </summary>
    internal interface IPropertyRule : IRule, IProperties
    {
        /// <summary>The registered custom property name, e.g. <c>--my-color</c>.</summary>
        string Name { get; set; }

        /// <summary>The raw <c>syntax</c> descriptor value (a string, e.g. <c>&lt;color&gt;</c> or <c>*</c>).</summary>
        string Syntax { get; set; }

        /// <summary>The raw <c>initial-value</c> descriptor value.</summary>
        string InitialValue { get; set; }

        /// <summary>The raw <c>inherits</c> descriptor value (<c>true</c> or <c>false</c>).</summary>
        string Inherits { get; set; }
    }
}
