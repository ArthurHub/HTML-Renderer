namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IContainerRule : IConditionRule
    {
        string Name { get; set; }
        MediaList Media { get; }

        /// <summary>
        /// The parsed condition tree when the condition uses <c>style()</c> queries (alone or mixed with size
        /// features); null for a plain size query, which is exposed through <see cref="Media"/>.
        /// </summary>
        IContainerCondition Condition { get; }
  }
}