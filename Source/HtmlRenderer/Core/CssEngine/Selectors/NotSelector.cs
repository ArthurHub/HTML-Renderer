using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// :not(), Selectors 4 §4.3.
    /// </summary>
    internal sealed class NotSelector : StylesheetNode, ISelector
    {
        public NotSelector(ISelector inner)
        {
            Inner = inner;
        }

        public ISelector Inner { get; }

        // Selectors 4 §15
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
