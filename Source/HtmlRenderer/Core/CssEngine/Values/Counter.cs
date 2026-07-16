namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class Counter
    {
        public Counter(string identifier, string listStyle, string separator)
        {
            CounterIdentifier = identifier;
            ListStyle = listStyle;
            DefinedSeparator = separator;
        }

        public string CounterIdentifier { get; }
        public string ListStyle { get; }
        public string DefinedSeparator { get; }
    }
}