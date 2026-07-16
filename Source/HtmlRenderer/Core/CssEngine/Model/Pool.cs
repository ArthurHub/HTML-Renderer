using System.Collections.Generic;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class Pool
    {
        private static readonly Stack<StringBuilder> Builder = new Stack<StringBuilder>();
        private static readonly Stack<SelectorConstructor> Selector = new Stack<SelectorConstructor>();
        private static readonly Stack<ValueBuilder> Value = new Stack<ValueBuilder>();
        private static readonly object Lock = new object();

        public static StringBuilder NewStringBuilder()
        {
            lock (Lock)
            {
                return Builder.Count == 0 ? new StringBuilder(1024) : Builder.Pop().Clear();
            }
        }

        public static SelectorConstructor NewSelectorConstructor(AttributeSelectorFactory attributeSelector,
            PseudoClassSelectorFactory pseudoClassSelector, PseudoElementSelectorFactory pseudoElementSelector)
        {
            lock (Lock)
            {
                return Selector.Count == 0
                    ? new SelectorConstructor(attributeSelector, pseudoClassSelector, pseudoElementSelector)
                    : Selector.Pop().Reset(attributeSelector, pseudoClassSelector, pseudoElementSelector);
            }
        }

        public static ValueBuilder NewValueBuilder()
        {
            lock (Lock)
            {
                return Value.Count == 0
                    ? new ValueBuilder()
                    : Value.Pop().Reset();
            }
        }

        public static string ToPool(this StringBuilder sb)
        {
            var result = sb.ToString();

            lock (Lock)
            {
                Builder.Push(sb);
            }

            return result;
        }

        public static ISelector ToPool(this SelectorConstructor ctor)
        {
            var result = ctor.GetResult();

            lock (Lock)
            {
                Selector.Push(ctor);
            }

            return result;
        }

        public static TokenValue ToPool(this ValueBuilder vb)
        {
            var result = vb.GetResult();

            lock (Lock)
            {
                Value.Push(vb);
            }

            return result;
        }
    }
}