namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>The five CSS-wide keywords (CSS Cascade &amp; Inheritance §7.3) a property value can be.</summary>
    internal enum CssGlobalKeyword
    {
        Inherit,
        Initial,
        Unset,
        Revert,
        RevertLayer
    }

    /// <summary>
    /// A typed box-side property value that carries the <b>parsed</b> value through the cascade to the box,
    /// so a layout consumer reads the object rather than re-parsing the authored string. A value is in exactly
    /// one of three states:
    /// <list type="bullet">
    /// <item>a CSS-wide keyword (<see cref="GlobalValue"/> non-null),</item>
    /// <item><i>unresolved</i> — the authored text still contains <c>var()</c> and has not been resolved yet
    /// (<see cref="IsUnresolved"/>; <see cref="Value"/> is default), or</item>
    /// <item>resolved — <see cref="Value"/> holds the parsed <typeparamref name="T"/>.</item>
    /// </list>
    /// The authored/keyword text is retained so the string-based getter/snapshot/revert cascade path keeps
    /// working (<see cref="ToString"/>). Grid templates (<c>T = GridTemplate</c>) are the first adopter; the
    /// mechanism is intended to eventually back every box property.
    /// </summary>
    internal sealed class CssProperty<T>
    {
        private readonly string _cssText;

        private CssProperty(CssGlobalKeyword? global, bool unresolved, T value, string cssText)
        {
            GlobalValue = global;
            IsUnresolved = unresolved;
            Value = value;
            _cssText = cssText;
        }

        /// <summary>The CSS-wide keyword, when this value is one; otherwise null.</summary>
        public CssGlobalKeyword? GlobalValue { get; }

        public bool IsGlobalValue => GlobalValue.HasValue;

        /// <summary>The authored text still contains an unresolved <c>var()</c> — <see cref="Value"/> is not
        /// meaningful until resolution replaces this with a resolved value.</summary>
        public bool IsUnresolved { get; }

        /// <summary>The parsed value — meaningful only when the value is neither global nor unresolved.</summary>
        public T Value { get; }

        public static CssProperty<T> Global(CssGlobalKeyword keyword) =>
            new CssProperty<T>(keyword, unresolved: false, value: default(T), CssGlobalKeywords.ToText(keyword));

        public static CssProperty<T> Unresolved(string rawText) =>
            new CssProperty<T>(global: null, unresolved: true, value: default(T), rawText);

        public static CssProperty<T> FromValue(string cssText, T value) =>
            new CssProperty<T>(global: null, unresolved: false, value, cssText);

        public override string ToString() => _cssText;
    }

    /// <summary>Maps between the CSS-wide keyword string literals and <see cref="CssGlobalKeyword"/>.</summary>
    internal static class CssGlobalKeywords
    {
        public static bool TryParse(string value, out CssGlobalKeyword keyword)
        {
            if (value.Isi(Keywords.Inherit)) { keyword = CssGlobalKeyword.Inherit; return true; }
            if (value.Isi(Keywords.Initial)) { keyword = CssGlobalKeyword.Initial; return true; }
            if (value.Isi(Keywords.Unset)) { keyword = CssGlobalKeyword.Unset; return true; }
            if (value.Isi(Keywords.Revert)) { keyword = CssGlobalKeyword.Revert; return true; }
            if (value.Isi(Keywords.RevertLayer)) { keyword = CssGlobalKeyword.RevertLayer; return true; }
            keyword = default;
            return false;
        }

        public static string ToText(CssGlobalKeyword keyword)
        {
            switch (keyword)
            {
                case CssGlobalKeyword.Inherit: return Keywords.Inherit;
                case CssGlobalKeyword.Initial: return Keywords.Initial;
                case CssGlobalKeyword.Unset: return Keywords.Unset;
                case CssGlobalKeyword.Revert: return Keywords.Revert;
                case CssGlobalKeyword.RevertLayer: return Keywords.RevertLayer;
                default: return Keywords.Initial;
            }
        }
    }
}
