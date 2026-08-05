using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PseudoClassSelectorFactory
    {
        private static readonly Lazy<PseudoClassSelectorFactory> Lazy =
            new Lazy<PseudoClassSelectorFactory>(() => new PseudoClassSelectorFactory());

        #region Selectors

        private static Dictionary<string, ISelector> BuildSelectors()
        {
            var selectors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    PseudoClassNames.Root,
                    PseudoClassNames.Scope,
                    PseudoClassNames.Empty,
                    PseudoClassNames.AnyLink,
                    PseudoClassNames.Link,
                    PseudoClassNames.Visited,
                    PseudoClassNames.Active,
                    PseudoClassNames.Hover,
                    PseudoClassNames.Focus,
                    PseudoClassNames.FocusVisible,
                    PseudoClassNames.FocusWithin,
                    PseudoClassNames.Target,
                    PseudoClassNames.Enabled,
                    PseudoClassNames.Disabled,
                    PseudoClassNames.Default,
                    PseudoClassNames.Checked,
                    PseudoClassNames.Indeterminate,
                    PseudoClassNames.PlaceholderShown,
                    PseudoClassNames.Unchecked,
                    PseudoClassNames.Valid,
                    PseudoClassNames.Invalid,
                    PseudoClassNames.Required,
                    PseudoClassNames.ReadOnly,
                    PseudoClassNames.ReadWrite,
                    PseudoClassNames.InRange,
                    PseudoClassNames.OutOfRange,
                    PseudoClassNames.Optional,
                    PseudoClassNames.Shadow,
                }
                .ToDictionary(x => x, PseudoClassSelector.Create);

            selectors.Add(PseudoElementNames.Before,
                PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.Before));
            selectors.Add(PseudoElementNames.After,
                PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.After));
            selectors.Add(PseudoElementNames.FirstLine,
                PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.FirstLine));
            selectors.Add(PseudoElementNames.FirstLetter,
                PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.FirstLetter));

            // Structural pseudo-classes: bare idents are equivalent to their nth-*(1) function form
            // (see CSS Selectors spec), so wire them to the same ChildSelector subtypes that
            // "nth-child(...)" etc. produce, rather than a generic PseudoClassSelector with no
            // positional semantics. Kind uses AllSelector.Create() (never null) to match the
            // invariant ChildFunctionState<T>.Produce() upholds for the function-form parse path.
            selectors.Add(PseudoClassNames.FirstChild, new FirstChildSelector().With(0, 1, AllSelector.Create()));
            selectors.Add(PseudoClassNames.LastChild, new LastChildSelector().With(0, 1, AllSelector.Create()));
            selectors.Add(PseudoClassNames.FirstOfType, new FirstTypeSelector().With(0, 1, AllSelector.Create()));
            selectors.Add(PseudoClassNames.LastOfType, new LastTypeSelector().With(0, 1, AllSelector.Create()));
            selectors.Add(PseudoClassNames.OnlyChild, new OnlyChildSelector());
            selectors.Add(PseudoClassNames.OnlyType, new OnlyOfTypeSelector());

            return selectors;
        }

        private static readonly Dictionary<string, ISelector> Selectors = BuildSelectors();

        #endregion

        internal static PseudoClassSelectorFactory Instance => Lazy.Value;

        public ISelector Create(string name)
        {
            return Selectors.TryGetValue(name, out var selector) ? selector : null;
        }
    }
}