using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The ":matches(S)"/":is(S)" pseudo-class - matches an element that matches ANY selector in S.
    /// Both keywords share this same class (":is()" is the modern name, ":matches()" the legacy
    /// alias); <see cref="Keyword"/> just controls how it round-trips back to CSS text.
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

        // Per spec, :is()/:matches() take the specificity of their most specific argument (the
        // static max, via ListSelector's own Specificity, when Inner is a comma-separated list).
        public Priority Specificity => Inner.Specificity;

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
