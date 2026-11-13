namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class Combinators
    {
        public static readonly string Exactly = "=";
        public static readonly string Unlike = "!=";
        public static readonly string InList = "~=";
        public static readonly string InToken = "|=";
        public static readonly string Begins = "^=";
        public static readonly string Ends = "$=";
        public static readonly string InText = "*=";
        public static readonly string Column = "||";
        public static readonly string Pipe = "|";
        public static readonly string Adjacent = "+";
        public static readonly string Descendent = " ";
        public static readonly string Deep = ">>>";
        public static readonly string Child = ">";
        public static readonly string Sibling = "~";
    }
}