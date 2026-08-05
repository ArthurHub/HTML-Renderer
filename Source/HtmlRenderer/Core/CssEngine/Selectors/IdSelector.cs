namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class IdSelector : SelectorBase
    {
        private IdSelector(string name) : base(Priority.OneId, $"#{name}")
        {
            Id = name;
        }

        public string Id { get; }

        public static IdSelector Create(string name)
        {
            return new IdSelector(name);
        }
    }
}