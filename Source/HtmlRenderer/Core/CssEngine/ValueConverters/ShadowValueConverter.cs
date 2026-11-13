using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    // Validates the shape of a single <shadow> before handing it to the order-independent converter:
    // the offset/blur/spread lengths must form one contiguous run of the allowed size (they may not be
    // interleaved with the color or inset keyword) and the blur radius (third length) may not be negative.
    internal sealed class ShadowValueConverter : IValueConverter
    {
        private readonly IValueConverter _converter;
        private readonly int _maxLengths;
        private readonly bool _nonNegativeSpread;

        public ShadowValueConverter(IValueConverter converter, int maxLengths, bool nonNegativeSpread = false)
        {
            _converter = converter;
            _maxLengths = maxLengths;
            _nonNegativeSpread = nonNegativeSpread;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToItems();
            var first = -1;
            var last = -1;

            for (var i = 0; i < items.Count; i++)
            {
                if (Converters.LengthConverter.Convert(items[i]) == null) continue;

                if (first < 0) first = i;
                last = i;
            }

            if (first < 0) return null;

            var count = last - first + 1;

            if (count < 2 || count > _maxLengths) return null;

            for (var i = first; i <= last; i++)
            {
                if (Converters.LengthConverter.Convert(items[i]) == null) return null;
            }

            // The blur radius is the third length in the run.
            if (count > 2)
            {
                var blur = items[first + 2].ToLength();

                if (blur.HasValue && blur.Value.Value < 0f) return null;
            }

            if (_nonNegativeSpread && count > 3)
            {
                var spread = items[first + 3].ToLength();

                if (spread.HasValue && spread.Value.Value < 0f) return null;
            }

            return _converter.Convert(value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return _converter.Construct(properties);
        }
    }
}
