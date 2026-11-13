using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The negation pseudo-class <c>:not(S)</c> - matches an element that does not match S. Retains the
    /// parsed argument selector rather than collapsing it to a formatted string.
    /// </summary>
    internal sealed class NotSelector : StylesheetNode, ISelector
    {
        public NotSelector(ISelector inner)
        {
            Inner = inner;
        }

        public ISelector Inner { get; }

        // The specificity of :not() is the specificity of its most specific argument (CSS Selectors 4
        // 16.1), which ListSelector.Specificity supplies as the static max when the argument is a list.
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
