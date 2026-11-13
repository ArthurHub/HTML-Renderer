using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value converter of the <c>font-synthesis</c> shorthand (CSS Fonts 4 3.5):
    /// <c>none | [ weight || style || small-caps || position ]</c>. A longhand the value names is
    /// <c>auto</c>; every one it leaves out is <c>none</c> (so <c>font-synthesis: weight</c> still forbids
    /// synthetic italic). That differs from the usual "an omitted longhand resets to its initial value",
    /// which would make an omitted one <c>auto</c>.
    /// </summary>
    internal sealed class FontSynthesisShorthandValueConverter : IValueConverter
    {
        private static readonly string[] Longhands =
        {
            PropertyNames.FontSynthesisWeight,
            PropertyNames.FontSynthesisStyle,
            PropertyNames.FontSynthesisSmallCaps,
            PropertyNames.FontSynthesisPosition
        };

        private static readonly string[] Keys = {"weight", "style", "small-caps", "position"};

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length == 0 || tokens.Length > Keys.Length) return null;

            var enabled = new bool[Keys.Length];

            if (tokens.Length == 1 && FontGrammar.IsIdent(tokens[0], Keywords.None))
            {
                return new SynthesisValue(enabled, value);
            }

            foreach (var token in tokens)
            {
                var index = token.Type == TokenType.Ident ? System.Array.FindIndex(Keys, k => k.Isi(token.Data)) : -1;

                if (index < 0 || enabled[index]) return null;

                enabled[index] = true;
            }

            return new SynthesisValue(enabled, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var enabled = new bool[Keys.Length];

            for (var i = 0; i < Longhands.Length; i++)
            {
                var property = properties.FirstOrDefault(m => m.Name.Is(Longhands[i]));

                if (property == null) return null;

                var text = property.Value;

                // Only auto/none (and the initial value, auto) can be written as the shorthand.
                if (text.Is(Keywords.None)) continue;

                if (!text.Is(Keywords.Auto) && !text.Is(Keywords.Initial)) return null;

                enabled[i] = true;
            }

            return new SynthesisValue(enabled, null);
        }

        private sealed class SynthesisValue : IPropertyValue
        {
            private readonly bool[] _enabled;

            public SynthesisValue(bool[] enabled, IEnumerable<Token> tokens)
            {
                _enabled = enabled;
                Original = new TokenValue(tokens ?? Enumerable.Empty<Token>());
            }

            public string CssText => _enabled.Any(e => e)
                ? string.Join(" ", Keys.Where((_, i) => _enabled[i]))
                : Keywords.None;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var index = System.Array.FindIndex(Longhands, m => m.Is(name));

                return index < 0 ? null : TokenValue.FromString(_enabled[index] ? Keywords.Auto : Keywords.None);
            }
        }
    }
}
