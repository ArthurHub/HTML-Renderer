using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ConstraintValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly string[] _labels;

        public ConstraintValueConverter(IValueConverter converter, string[] labels)
        {
            _converter = converter;
            _labels = labels;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var result = _converter.Convert(value);
            return result != null ? new TransformationValueConverter(result, _labels) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var filtered = properties.Where(m => _labels.Contains(m.Name));
            var existing = default(string);

            foreach (var filter in filtered)
            {
                var value = filter.Value;

                if (existing != null && value != existing) return null;

                existing = value;
            }

            var result = _converter.Construct(filtered.Take(1).ToArray());
            return result != null ? new TransformationValueConverter(result, _labels) : null;
        }

        private sealed class TransformationValueConverter : IPropertyValue
        {
            private readonly string[] _labels;
            private readonly IPropertyValue _value;

            public TransformationValueConverter(IPropertyValue value, string[] labels)
            {
                _value = value;
                _labels = labels;
            }

            public string CssText => _value.CssText;

            public TokenValue Original => _value.Original;

            public TokenValue ExtractFor(string name)
            {
                return _labels.Contains(name) ? _value.ExtractFor(name) : null;
            }
        }
    }
}