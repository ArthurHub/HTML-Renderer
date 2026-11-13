namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct ParserOptions
    {
        public bool IncludeUnknownRules { get; set; }
        public bool IncludeUnknownDeclarations { get; set; }
        public bool AllowInvalidSelectors { get; set; }
        public bool AllowInvalidValues { get; set; }
        public bool AllowInvalidConstraints { get; set; }
        public bool PreserveComments { get; set; }
        public bool PreserveDuplicateProperties { get; set; }
        /// <summary>
        ///     Accept vendor-prefixed aliases of standard rules and properties (for example
        ///     <c>@-webkit-keyframes</c> as <c>@keyframes</c>). Off by default.
        /// </summary>
        public bool EnableVendorPrefixAliases { get; set; }
    }
}