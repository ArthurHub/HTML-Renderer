namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class Combinators
    {
        // const (not static readonly) so these can be used as constant patterns in switch statements
        // (e.g. AttributeSelectorFactory's reflection-free dispatch). String literals are interned, so
        // reference/value equality behaviour is unchanged.
        public const string Exactly = "=";
        public const string Unlike = "!=";
        public const string InList = "~=";
        public const string InToken = "|=";
        public const string Begins = "^=";
        public const string Ends = "$=";
        public const string InText = "*=";
        public const string Column = "||";
        public const string Pipe = "|";
        public const string Adjacent = "+";
        public const string Descendent = " ";
        public const string Deep = ">>>";
        public const string Child = ">";
        public const string Sibling = "~";
    }
}