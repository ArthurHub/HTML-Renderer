using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PseudoClassSelectorFactory
    {
        private static readonly Lazy<PseudoClassSelectorFactory> Lazy =
            new(() =>
                {
                    var factory = new PseudoClassSelectorFactory();
                    Selectors.Add(PseudoElementNames.Before,
                        PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.Before));
                    Selectors.Add(PseudoElementNames.After,
                        PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.After));
                    Selectors.Add(PseudoElementNames.FirstLine,
                        PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.FirstLine));
                    Selectors.Add(PseudoElementNames.FirstLetter,
                        PseudoElementSelectorFactory.Instance.Create(PseudoElementNames.FirstLetter));
                    return factory;
                }
            );

        #region Selectors

        private static readonly Dictionary<string, ISelector> Selectors =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    PseudoClassNames.Root,
                    PseudoClassNames.Scope,
                    PseudoClassNames.FirstOfType,
                    PseudoClassNames.LastOfType,
                    PseudoClassNames.FirstChild,
                    PseudoClassNames.LastChild,
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
                    PseudoClassNames.Host,
                    PseudoClassNames.Defined,
                    PseudoClassNames.Modal,
                    PseudoClassNames.Open,
                    PseudoClassNames.PopoverOpen,
                    PseudoClassNames.Fullscreen,
                    PseudoClassNames.PictureInPicture,
                    PseudoClassNames.UserValid,
                    PseudoClassNames.UserInvalid,
                    PseudoClassNames.Autofill,
                    PseudoClassNames.TargetWithin,
                    PseudoClassNames.LocalLink,
                    PseudoClassNames.Current,
                    PseudoClassNames.Past,
                    PseudoClassNames.Future,
                    PseudoClassNames.Playing,
                    PseudoClassNames.Paused,
                    PseudoClassNames.Seeking,
                    PseudoClassNames.Buffering,
                    PseudoClassNames.Stalled,
                    PseudoClassNames.Muted,
                    PseudoClassNames.VolumeLocked,
                }
                .ToDictionary(x => x, PseudoClassSelector.Create);

        #endregion

        private static readonly ISelector OnlyChild = new OnlyChildSelector();
        private static readonly ISelector OnlyOfType = new OnlyOfTypeSelector();

        internal static PseudoClassSelectorFactory Instance => Lazy.Value;

        public ISelector Create(string name)
        {
            if (name.Equals(PseudoClassNames.OnlyChild, StringComparison.OrdinalIgnoreCase)) return OnlyChild;
            if (name.Equals(PseudoClassNames.OnlyType, StringComparison.OrdinalIgnoreCase)) return OnlyOfType;

            return Selectors.TryGetValue(name, out var selector) ? selector : null;
        }
    }
}