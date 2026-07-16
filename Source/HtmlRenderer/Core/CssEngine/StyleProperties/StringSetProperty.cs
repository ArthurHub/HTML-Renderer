namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Represents the CSS string-set property from CSS Generated Content for Paged Media Module.
    /// Syntax: string-set: [ &lt;custom-ident&gt; &lt;content-list&gt; ]# | none
    /// where &lt;content-list&gt; = [ &lt;string&gt; | &lt;counter()&gt; | &lt;counters()&gt; | &lt;content()&gt; | &lt;attr()&gt; ]+
    /// </summary>
    internal sealed class StringSetProperty : Property
    {
        private static readonly IValueConverter ValueConverter = new StringSetValueConverter().OrDefault();

        internal StringSetProperty() : base(PropertyNames.StringSet)
        {
        }

        internal override IValueConverter Converter => ValueConverter;
    }
}
