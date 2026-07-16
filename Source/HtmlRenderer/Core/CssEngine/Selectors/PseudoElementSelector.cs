namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PseudoElementSelector : SelectorBase
    {
        private PseudoElementSelector(string name) : base(Priority.OneTag, $"{PseudoElementNames.Separator}{name}")
        {
            Name = name;
        }

        public string Name { get; }

        public static ISelector Create(string name)
        {
            return new PseudoElementSelector(name);
        }
    }
}