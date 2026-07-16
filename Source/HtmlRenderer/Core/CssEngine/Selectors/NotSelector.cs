using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The negation pseudo-class ":not(S)" - matches an element that does NOT match S.
    /// </summary>
    internal sealed class NotSelector : StylesheetNode, ISelector
    {
        public NotSelector(ISelector inner)
        {
            Inner = inner;
        }

        public ISelector Inner { get; }

        // Per spec, :not() takes the specificity of its argument (the most specific selector in a
        // comma-separated argument, via ListSelector's own static-max Specificity).
        public Priority Specificity => Inner.Specificity;

        public string Text => this.ToCss();

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(":not(");
            writer.Write(Inner.Text);
            writer.Write(')');
        }
    }
}
