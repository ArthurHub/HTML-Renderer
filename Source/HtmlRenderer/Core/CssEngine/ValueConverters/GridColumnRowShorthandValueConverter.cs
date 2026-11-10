using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>grid-row</c>/<c>grid-column</c>, CSS Grid 2 §8.4. Not built from <c>WithOrder(...).Option()</c>,
    /// that resets an omitted end to auto instead of copying a bare <c>&lt;custom-ident&gt;</c>.
    /// </summary>
    internal sealed class GridColumnRowShorthandValueConverter : IValueConverter
    {
        private readonly string _startName;
        private readonly string _endName;

        public GridColumnRowShorthandValueConverter(string startName, string endName)
        {
            _startName = startName;
            _endName = endName;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var components = GridPlacementShorthand.Split(value, maxComponents: 2);
            if (components is null) return null;

            var start = components[0];
            var end = components.Count > 1 ? components[1]
                : start.IsBareCustomIdent ? start : (GridPlacementShorthand.Component?)null;

            var map = new Dictionary<string, IReadOnlyList<Token>>
            {
                [_startName] = start.Tokens,
                [_endName] = GridPlacementShorthand.Or(end),
            };
            return new MappedShorthandValue(map, value);
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<MappedShorthandValue>();
    }
}
