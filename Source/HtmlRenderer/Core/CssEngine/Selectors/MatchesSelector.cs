using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// :is(), its legacy alias :matches() and :where(), Selectors 4 §4.2 and §4.4.
    /// </summary>
    internal sealed class MatchesSelector : StylesheetNode, ISelector
    {
        public MatchesSelector(ISelector inner, string keyword)
        {
            Inner = inner;
            Keyword = keyword;
        }

        public ISelector Inner { get; }

        public string Keyword { get; }

        // Selectors 4 §15
        public Priority Specificity =>
            string.Equals(Keyword, PseudoClassNames.Where, System.StringComparison.OrdinalIgnoreCase)
                ? Priority.Zero
                : Inner.Specificity;

        public string Text => this.ToCss();

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(':');
            writer.Write(Keyword);
            writer.Write('(');
            writer.Write(Inner.Text);
            writer.Write(')');
        }
    }
}
