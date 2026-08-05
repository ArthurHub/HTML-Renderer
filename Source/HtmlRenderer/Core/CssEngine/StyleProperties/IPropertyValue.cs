namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IPropertyValue
    {
        string CssText { get; }
        TokenValue Original { get; }
        TokenValue ExtractFor(string name);
    }

    /// <summary>
    /// Implemented by an <see cref="IPropertyValue"/> that carries a <b>parsed</b>, strongly-typed model
    /// (a <see cref="CssProperty{T}"/>) alongside its text, so the cascade can hand the parsed value straight
    /// to the box without re-parsing the authored string. The cascade knows the concrete <typeparamref name="T"/>
    /// from the property name it is applying (its typed setter casts to the matching
    /// <c>ITypedPropertyValue&lt;T&gt;</c>). Grid templates (<c>T = GridTemplate</c>) are the first adopter.
    /// </summary>
    internal interface ITypedPropertyValue<T>
    {
        CssProperty<T> GetTypedValue();
    }

    internal static class PropertyValueExtensions
    {
        /// <summary>
        /// Returns the parsed <see cref="CssProperty{T}"/> this value carries when it is an
        /// <see cref="ITypedPropertyValue{T}"/> for the requested <typeparamref name="T"/>; otherwise false
        /// (e.g. a global-keyword value or a value without a typed carrier).
        /// </summary>
        public static bool TryGetValue<T>(this IPropertyValue value, out CssProperty<T> typed)
        {
            if (value is ITypedPropertyValue<T> carrier)
            {
                typed = carrier.GetTypedValue();
                return true;
            }

            typed = null;
            return false;
        }
    }
}