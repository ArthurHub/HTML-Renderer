namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A named counter style declared with an <c>@counter-style</c> at-rule (CSS Counter Styles Level 3).
    /// Exposes the prelude name and the descriptors as raw values.
    /// </summary>
    internal interface ICounterStyleRule : IRule, IProperties
    {
        /// <summary>The counter style name, a case-sensitive custom ident.</summary>
        string Name { get; set; }

        /// <summary>The raw <c>system</c> descriptor value.</summary>
        string System { get; set; }

        /// <summary>The raw <c>negative</c> descriptor value.</summary>
        string Negative { get; set; }

        /// <summary>The raw <c>prefix</c> descriptor value.</summary>
        string Prefix { get; set; }

        /// <summary>The raw <c>suffix</c> descriptor value.</summary>
        string Suffix { get; set; }

        /// <summary>The raw <c>range</c> descriptor value.</summary>
        string Range { get; set; }

        /// <summary>The raw <c>pad</c> descriptor value.</summary>
        string Pad { get; set; }

        /// <summary>The raw <c>fallback</c> descriptor value.</summary>
        string Fallback { get; set; }

        /// <summary>The raw <c>symbols</c> descriptor value.</summary>
        string Symbols { get; set; }

        /// <summary>The raw <c>additive-symbols</c> descriptor value.</summary>
        string AdditiveSymbols { get; set; }

        /// <summary>The raw <c>speak-as</c> descriptor value.</summary>
        string SpeakAs { get; set; }
    }
}
