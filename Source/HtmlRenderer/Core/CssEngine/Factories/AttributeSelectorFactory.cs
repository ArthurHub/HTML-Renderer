using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class AttributeSelectorFactory
    {
        private static readonly Lazy<AttributeSelectorFactory> Lazy = new Lazy<AttributeSelectorFactory>(() => new AttributeSelectorFactory());

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

            // A reflection-free dispatch (no Activator.CreateInstance) so the two-arg Attr*Selector
            // constructors are statically reachable and survive trimming/AOT (IsTrimmable=true) - see
            // upstream ExCSS commit c497ca7. Unknown combinators fall back to a presence selector.
            switch (combinator)
            {
                case Combinators.Exactly: return new AttrMatchSelector(name, value);
                case Combinators.InList: return new AttrListSelector(name, value);
                case Combinators.InToken: return new AttrHyphenSelector(name, value);
                case Combinators.Begins: return new AttrBeginsSelector(name, value);
                case Combinators.Ends: return new AttrEndsSelector(name, value);
                case Combinators.InText: return new AttrContainsSelector(name, value);
                case Combinators.Unlike: return new AttrNotMatchSelector(name, value);
                default: return new AttrAvailableSelector(name, value);
            }
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