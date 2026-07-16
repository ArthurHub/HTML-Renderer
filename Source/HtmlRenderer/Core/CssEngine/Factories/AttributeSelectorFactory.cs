using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttributeSelectorFactory
    {
        private static readonly Lazy<AttributeSelectorFactory> Lazy = new Lazy<AttributeSelectorFactory>(() => new AttributeSelectorFactory());

        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>()
        {
            { Combinators.Exactly, typeof(AttrMatchSelector) },
            { Combinators.InList, typeof(AttrListSelector) },
            { Combinators.InToken, typeof(AttrHyphenSelector) },
            { Combinators.Begins, typeof(AttrBeginsSelector) },
            { Combinators.Ends, typeof(AttrEndsSelector) },
            { Combinators.InText, typeof(AttrContainsSelector) },
            { Combinators.Unlike, typeof(AttrNotMatchSelector) },
        };

        private AttributeSelectorFactory()
        {
        }

        internal static AttributeSelectorFactory Instance => Lazy.Value;

        public IAttrSelector Create(string combinator, string match, string value, string prefix)
        {
            var name = match;

            if (!string.IsNullOrEmpty(prefix))
            {
                name = AttributeSelectorFactory.FormFront(prefix, match);
                _ = AttributeSelectorFactory.FormMatch(prefix, match);
            }

            return _types.TryGetValue(combinator, out var type)
                ? (IAttrSelector)Activator.CreateInstance(type, name, value)
                : new AttrAvailableSelector(name, value);
        }

        private static string FormFront(string prefix, string match)
        {
            return string.Concat(prefix, Combinators.Pipe, match);
        }

        private static string FormMatch(string prefix, string match)
        {
            return prefix.Is(Keywords.Asterisk) ? match : string.Concat(prefix, PseudoClassNames.Separator, match);
        }
    }
}