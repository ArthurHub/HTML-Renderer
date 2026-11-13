using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// One alternative of the relative selector list taken by <c>:has()</c> (CSS Selectors 4): a complex
    /// selector preceded by an explicit leading combinator (<c>&gt;</c>, <c>+</c> or <c>~</c>), as in
    /// <c>:has(&gt; a)</c>. An alternative with no leading combinator (implicit descendant) is kept as a
    /// plain selector rather than wrapped.
    /// </summary>
    internal sealed class RelativeSelector : StylesheetNode, ISelector
    {
        public RelativeSelector(Combinator combinator, ISelector selector)
        {
            Combinator = combinator;
            Selector = selector;
        }

        public Combinator Combinator { get; }
        public ISelector Selector { get; }

        // The leading combinator never contributes to specificity.
        public Priority Specificity => Selector.Specificity;

        public string Text => this.ToCss();

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(Combinator.Delimiter);
            writer.Write(' ');
            writer.Write(Selector.Text);
        }
    }
}