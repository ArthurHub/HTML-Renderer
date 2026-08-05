using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class StylesheetNode : IStylesheetNode
    {
        private readonly List<IStylesheetNode> _children = new List<IStylesheetNode>();

        protected void ReplaceAll(IStylesheetNode node)
        {
            Clear();
            StylesheetText = node.StylesheetText;
            foreach (var child in node.Children) AppendChild(child);
        }

        public StylesheetText StylesheetText { get; internal set; }

        public IEnumerable<IStylesheetNode> Children => _children.AsEnumerable();

        public abstract void ToCss(TextWriter writer, IStyleFormatter formatter);

        public void AppendChild(IStylesheetNode child)
        {
            Setup(child);
            _children.Add(child);
        }

        public void ReplaceChild(IStylesheetNode oldChild, IStylesheetNode newChild)
        {
            for (var i = 0; i < _children.Count; i++)
            {
                if (ReferenceEquals(oldChild, _children[i]))
                {
                    Teardown(oldChild);
                    Setup(newChild);
                    _children[i] = newChild;
                    return;
                }
            }
        }

        public void InsertBefore(IStylesheetNode referenceChild, IStylesheetNode child)
        {
            if (referenceChild != null)
            {
                var index = _children.IndexOf(referenceChild);
                InsertChild(index, child);
            }
            else
            {
                AppendChild(child);
            }
        }

        public void InsertChild(int index, IStylesheetNode child)
        {
            Setup(child);
            _children.Insert(index, child);
        }

        public void RemoveChild(IStylesheetNode child)
        {
            Teardown(child);
            _children.Remove(child);
        }

        public void Clear()
        {
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                RemoveChild(child);
            }
        }

        private void Setup(IStylesheetNode child)
        {
            if (!(child is Rule rule)) return;
            rule.Owner = this as Stylesheet;
            rule.Parent = this as IRule;
        }

        private static void Teardown(IStylesheetNode child)
        {
            if (!(child is Rule rule)) return;
            rule.Parent = null;
            rule.Owner = null;
        }
    }
}