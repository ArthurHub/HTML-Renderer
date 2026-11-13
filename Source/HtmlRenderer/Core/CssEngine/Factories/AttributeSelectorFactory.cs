using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttributeSelectorFactory
    {
        private static readonly Lazy<AttributeSelectorFactory> Lazy = new(() => new AttributeSelectorFactory());

        private AttributeSelectorFactory()
        {
        }

        internal static AttributeSelectorFactory Instance => Lazy.Value;

        public IAttrSelector Create(string combinator, string match, string value, string prefix)
        {
            return Create(combinator, match, value, prefix, AttrCaseSensitivity.Default);
        }

        public IAttrSelector Create(string combinator, string match, string value, string prefix,
            AttrCaseSensitivity caseSensitivity)
        {
            var name = match;

            if (!string.IsNullOrEmpty(prefix))
            {
                name = AttributeSelectorFactory.FormFront(prefix, match);
                _ = AttributeSelectorFactory.FormMatch(prefix, match);
            }

            if (combinator == Combinators.Exactly)
                return new AttrMatchSelector(name, value, caseSensitivity);
            if (combinator == Combinators.InList)
                return new AttrListSelector(name, value, caseSensitivity);
            if (combinator == Combinators.InToken)
                return new AttrHyphenSelector(name, value, caseSensitivity);
            if (combinator == Combinators.Begins)
                return new AttrBeginsSelector(name, value, caseSensitivity);
            if (combinator == Combinators.Ends)
                return new AttrEndsSelector(name, value, caseSensitivity);
            if (combinator == Combinators.InText)
                return new AttrContainsSelector(name, value, caseSensitivity);
            if (combinator == Combinators.Unlike)
                return new AttrNotMatchSelector(name, value, caseSensitivity);
            return new AttrAvailableSelector(name, value);
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