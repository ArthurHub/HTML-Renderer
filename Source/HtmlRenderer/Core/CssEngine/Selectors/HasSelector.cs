using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// :has(), Selectors 4 §4.5. Only the descendant form, SelectorConstructor drops a leading
    /// combinator.
    /// </summary>
    internal sealed class HasSelector : StylesheetNode, ISelector
    {
        public HasSelector(ISelector inner)
        {
            Inner = inner;
        }

        public ISelector Inner { get; }

        // Selectors 4 §15
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
