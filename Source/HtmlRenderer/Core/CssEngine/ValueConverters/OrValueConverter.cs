using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OrValueConverter : IValueConverter
    {
        private readonly IValueConverter _next;
        private readonly IValueConverter _previous;

        public OrValueConverter(IValueConverter previous, IValueConverter next)
        {
            _previous = previous;
            _next = next;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            return _previous.Convert(value) ?? _next.Convert(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _previous.Construct(properties) ?? _next.Construct(properties);
        }
    }
}