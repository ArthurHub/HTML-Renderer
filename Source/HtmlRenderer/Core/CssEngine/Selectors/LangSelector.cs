using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>:lang()</c> pseudo-class. Each argument is an identifier or a string, and several
    /// can be given as a comma-separated list.
    /// </summary>
    internal sealed class LangSelector : SelectorBase
    {
        internal LangSelector(IReadOnlyList<KeyValuePair<string, bool>> ranges)
            : base(Priority.OneClass, CreateText(ranges))
        {
            LanguageRanges = ranges.Select(r => r.Key).ToList();
        }

        /// <summary>The language ranges in source order, without quotes.</summary>
        public IReadOnlyList<string> LanguageRanges { get; }

        /// <summary>The first language range.</summary>
        public string LanguageRange => LanguageRanges[0];

        private static string CreateText(IReadOnlyList<KeyValuePair<string, bool>> ranges)
        {
            var arguments = ranges.Select(r => r.Value ? r.Key.StylesheetString() : r.Key);
            return $"{PseudoClassNames.Separator}{PseudoClassNames.Lang.StylesheetFunction(string.Join(",", arguments))}";
        }
    }
}
