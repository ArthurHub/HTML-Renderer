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

                if (property.TrySetValue(value)) property.IsImportant = IsImportant;
            }
        }
    }
}