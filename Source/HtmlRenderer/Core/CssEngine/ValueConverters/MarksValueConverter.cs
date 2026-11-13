using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>marks</c> page descriptor (CSS Paged Media 3 section 12): <c>none | [ crop || cross ]</c>.
    /// The keywords may appear in either order and are serialized as <c>crop cross</c>.
    /// </summary>
    internal sealed class MarksValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var names = new List<string>();

            foreach (var token in value)
            {
                if (token.Type == TokenType.Whitespace) continue;
                if (token.Type != TokenType.Ident) return null;
                names.Add(token.Data.ToLowerInvariant());
            }

            if (names.Count == 0 || names.Count > 2) return null;

            if (names.Count == 1 && names[0] == Keywords.None) return new GcpmFunctionValue(Keywords.None, value);

            var crop = names.Count(n => n == "crop");
            var cross = names.Count(n => n == "cross");
            if (crop > 1 || cross > 1 || crop + cross != names.Count) return null;

            var text = string.Join(" ", new[] { crop == 1 ? "crop" : null, cross == 1 ? "cross" : null }
                .Where(n => n != null));

            return new GcpmFunctionValue(text, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GcpmFunctionValue>();
        }
    }
}
