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
    }
}