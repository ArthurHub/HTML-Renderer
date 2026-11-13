using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>overflow-clip-margin</c>, <c>-block</c> and <c>-inline</c> shorthands (CSS Overflow 4 §3.2): one
    /// <c>&lt;visual-box&gt; || &lt;length&gt;</c>, which every longhand the shorthand stands for takes whole.
    /// </summary>
    internal sealed class OverflowClipMarginShorthandValueConverter : IValueConverter
    {
        private readonly string[] _longhands;

        internal OverflowClipMarginShorthandValueConverter(params string[] longhands)
        {
            _longhands = longhands;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = new List<Token>(value);

            return OverflowClipMarginGrammar.TryParse(tokens) ? Create(tokens) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            if (properties.Length != _longhands.Length) return null;

            var first = properties[0].DeclaredValue;

            if (first == null || properties.Any(p => p.DeclaredValue == null || p.DeclaredValue.CssText != first.CssText))
            {
                return null;
            }

            return Create(new List<Token>(first.Original));
        }

        private IPropertyValue Create(List<Token> tokens)
        {
            var map = _longhands.ToDictionary(name => name, _ => (IReadOnlyList<Token>)tokens);

            return new MappedShorthandValue(map, tokens);
        }
    }
}
