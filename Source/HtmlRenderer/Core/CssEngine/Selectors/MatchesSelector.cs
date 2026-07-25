using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The ":matches(S)"/":is(S)"/":where(S)" pseudo-class - matches an element that matches ANY
    /// selector in S. All three keywords share this same class (":is()" is the modern name,
    /// ":matches()" the legacy alias, ":where()" the zero-specificity variant); <see cref="Keyword"/>
    /// controls how it round-trips back to CSS text and, for ":where()", drops its specificity to zero.
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

        // Per CSS Selectors 4 §16: :is()/:matches() take the specificity of their most specific
        // argument (the static max, via ListSelector's own Specificity, when Inner is a
        // comma-separated list), while :where() always contributes zero specificity - this is what
        // lets a utility-framework reset authored in :where(...) be overridden by any real selector.
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
