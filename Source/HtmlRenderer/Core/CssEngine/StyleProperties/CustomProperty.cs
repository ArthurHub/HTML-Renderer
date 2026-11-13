namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// A custom property - a declaration whose name begins with two dashes, e.g. <c>--main-color</c>
    /// (CSS Custom Properties 1 §2). Its value has no fixed grammar, so it is stored as-is.
    /// </summary>
    internal sealed class CustomProperty : Property
    {
        internal CustomProperty(string name)
            : base(name)
        {
        }

        internal override IValueConverter Converter => Converters.Any;
    }
}
