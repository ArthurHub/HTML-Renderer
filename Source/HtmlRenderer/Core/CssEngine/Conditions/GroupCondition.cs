using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class GroupCondition : StylesheetNode, IConditionFunction
    {
        private IConditionFunction _content;

        public IConditionFunction Content
        {
            get => _content ?? new EmptyCondition();
            set
            {
                if (_content != null) RemoveChild(_content);

                _content = value;

                if (value != null) AppendChild(_content);
            }
        }

        public bool Check()
        {
            return Content.Check();
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write("(");
            Content.ToCss(writer, formatter);
            writer.Write(")");
        }
    }
}