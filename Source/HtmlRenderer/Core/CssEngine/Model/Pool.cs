using System;
using System.Collections.Generic;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Scratch-object pools. Per-thread (lock-free) rather than one shared lock-guarded pool: the shared
    /// lock was a major contention point under concurrent parsing. An instance is only ever held by the
    /// code that rented it, so a rent and its matching return are safe even if they happen on different
    /// threads (e.g. across an await): the instance simply lands in the returning thread's pool, and is
    /// never reachable from two threads at once. Each per-thread stack is bounded.
    /// </summary>
    internal static class Pool
    {
        private const int MaxPooled = 64;

        [ThreadStatic] private static Stack<StringBuilder> _builder;
        [ThreadStatic] private static Stack<SelectorConstructor> _selector;
        [ThreadStatic] private static Stack<ValueBuilder> _value;

        public static StringBuilder NewStringBuilder()
        {
            var stack = _builder;
            return stack == null || stack.Count == 0 ? new StringBuilder(1024) : stack.Pop().Clear();
        }

        public static SelectorConstructor NewSelectorConstructor(AttributeSelectorFactory attributeSelector,
            PseudoClassSelectorFactory pseudoClassSelector, PseudoElementSelectorFactory pseudoElementSelector)
        {
            var stack = _selector;
            return stack == null || stack.Count == 0
                ? new SelectorConstructor(attributeSelector, pseudoClassSelector, pseudoElementSelector)
                : stack.Pop().Reset(attributeSelector, pseudoClassSelector, pseudoElementSelector);
        }

        public static ValueBuilder NewValueBuilder()
        {
            var stack = _value;
            return stack == null || stack.Count == 0
                ? new ValueBuilder()
                : stack.Pop().Reset();
        }

        public static string ToPool(this StringBuilder sb)
        {
            var result = sb.ToString();
            Push(ref _builder, sb);
            return result;
        }

        public static ISelector ToPool(this SelectorConstructor ctor)
        {
            var result = ctor.GetResult();
            Push(ref _selector, ctor);
            return result;
        }

        public static TokenValue ToPool(this ValueBuilder vb)
        {
            var result = vb.GetResult();
            Push(ref _value, vb);
            return result;
        }

        private static void Push<T>(ref Stack<T> stack, T item)
        {
            stack ??= new Stack<T>();
            if (stack.Count < MaxPooled) stack.Push(item);
        }
    }
}
