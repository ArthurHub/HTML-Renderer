using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class ChildSelector : StylesheetNode, ISelector
    {
        private readonly string _name;
        public int Step { get; private set; }
        public int Offset { get; private set; }
        internal ISelector Kind { get; private set; }

        protected ChildSelector(string name)
        {
            _name = name;
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var a = Step.ToString();

            var b = Offset switch
            {
                > 0 => "+" + Offset,
                < 0 => Offset.ToString(),
                _ => string.Empty
            };

            writer.Write(":{0}({1}n{2})", _name, a, b);
        }

        public Priority Specificity => Priority.OneClass;
        public string Text => this.ToCss();

        internal ChildSelector With(int step, int offset, ISelector kind)
        {
            Step = step;
            Offset = offset;
            Kind = kind;
            return this;
        }
    }
}