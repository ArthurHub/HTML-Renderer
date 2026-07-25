namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class ShorthandProperty : Property
    {
        protected ShorthandProperty(string name, PropertyFlags flags = PropertyFlags.None)
            : base(name, flags | PropertyFlags.Shorthand)
        {
        }

        public string Stringify(Property[] properties)
        {
            return Converter.Construct(properties)?.CssText;
        }

        public void Export(Property[] properties)
        {
            foreach (var property in properties)
            {
                var value = DeclaredValue.ExtractFor(property.Name);

                // A longhand this shorthand's grammar didn't actually match any tokens for (e.g.
                // "background-color" when "background: none" only matched the image slot) must reset
                // to its initial value, per CSS Cascading - a shorthand declaration always sets every
                // longhand it covers, explicitly or implicitly. An empty (but non-null) extracted
                // TokenValue means exactly that "omitted", so it's treated the same as null here to
                // reach TrySetValue's existing null -> TokenValue.Initial fallback; passing the empty
                // value through as-is would instead fail conversion silently, leaving the longhand's
                // previous declaration (from an earlier, lower-priority rule) as the winning cascade
                // value instead of being properly overridden.
                var effectiveValue = value != null && value.Count == 0 ? null : value;

                if (property.TrySetValue(effectiveValue)) property.IsImportant = IsImportant;
            }
        }
    }
}