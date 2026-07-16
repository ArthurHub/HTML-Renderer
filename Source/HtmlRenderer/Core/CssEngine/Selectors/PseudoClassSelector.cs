namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PseudoClassSelector : SelectorBase
    {
        private PseudoClassSelector(string name) : base(Priority.OneClass, $"{PseudoClassNames.Separator}{name}")
        {
            Class = name;
        }

        public string Class { get; }

        public static ISelector Create(string name)
        {
            return new PseudoClassSelector(name);
        }
    }
}