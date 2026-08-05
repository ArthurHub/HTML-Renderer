using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The relational pseudo-class ":has(S)" - matches an element that has a descendant matching S.
    /// v1 only supports the default (descendant) relative-selector form; leading-combinator forms
    /// like ":has(&gt; S)"/":has(+ S)"/":has(~ S)" are not supported (the parser silently discards a
    /// leading combinator today - see SelectorConstructor.Insert(ISelector) - and fixing that touches
    /// shared parsing code well beyond :has() itself, so it's an explicit, documented follow-up).
    /// </summary>
    internal sealed class HasSelector : StylesheetNode, ISelector
    {
        public HasSelector(ISelector inner)
        {
            Inner = inner;
        }

        public ISelector Inner { get; }

        // Per spec, :has() takes the specificity of its most specific argument, same as :is()/:not().
        public Priority Specificity => Inner.Specificity;

        public string Text => this.ToCss();

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(":has(");
            writer.Write(Inner.Text);
            writer.Write(')');
        }
    }
}
