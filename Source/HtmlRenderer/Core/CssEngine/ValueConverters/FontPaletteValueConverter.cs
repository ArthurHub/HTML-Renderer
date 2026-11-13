using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Validates a <c>font-palette</c> value (CSS Fonts Module Level 4): the keywords <c>normal</c>/
    /// <c>light</c>/<c>dark</c>, a <c>&lt;dashed-ident&gt;</c> naming an <c>@font-palette-values</c> rule, or a
    /// <c>palette-mix()</c> of those. Anything else is dropped at parse time. The authored value text is
    /// preserved verbatim, mirroring <see cref="ClipPathValueConverter"/>.
    /// </summary>
    internal sealed class FontPaletteValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            var valid = tokens.Length == 1 && IsPalette(tokens[0]);

            return valid ? new FontPaletteValue(value) : null;
        }

        private static bool IsPalette(Token token) =>
            token.Type == TokenType.Ident
                ? IsKeywordOrDashedIdent(token.Data)
                : token is FunctionToken fn && fn.Data.Isi("palette-mix") && IsPaletteMix(fn);

        // CSS Fonts 4 §9.1, palette-mix( <color-interpolation-method> , [ <'font-palette'> && <percentage [0,100]>? ]# ),
        // the interpolation method is required though the grammar marks it optional, see the changes section
        private static bool IsPaletteMix(FunctionToken fn)
        {
            var groups = new List<List<Token>> { new List<Token>() };

            foreach (var token in fn.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace))
            {
                if (token.Type == TokenType.Comma) groups.Add(new List<Token>());
                else groups[groups.Count - 1].Add(token);
            }

            if (groups.Count < 2 || !IsInterpolationMethod(groups[0])) return false;

            var percentages = new List<float>();

            for (var i = 1; i < groups.Count; i++)
            {
                if (!TryGetOperand(groups[i], out var percentage)) return false;
                if (percentage.HasValue) percentages.Add(percentage.Value);
            }

            // Like color-mix(), explicit percentages that are all 0% leave nothing to mix
            return percentages.Count < groups.Count - 1 || percentages.Any(p => p != 0f);
        }

        // in <color-space> [ <hue-interpolation-method> hue ]?, the hue method only after a polar space
        private static bool IsInterpolationMethod(List<Token> tokens)
        {
            if (tokens.Any(t => t.Type != TokenType.Ident)) return false;
            if (tokens.Count < 2 || !tokens[0].Data.Isi("in") ||
                !ColorInterpolationMethodGrammar.IsColorSpace(tokens[1].Data)) return false;

            return tokens.Count == 2 ||
                   tokens.Count == 4 && ColorInterpolationMethodGrammar.IsPolarColorSpace(tokens[1].Data) &&
                   ColorInterpolationMethodGrammar.IsHueDirection(tokens[2].Data) && tokens[3].Data.Isi("hue");
        }

        private static bool TryGetOperand(List<Token> tokens, out float? percentage)
        {
            percentage = null;

            if (tokens.Count == 1) return IsPalette(tokens[0]);
            if (tokens.Count != 2) return false;

            var pct = tokens.FirstOrDefault(t => t.Type == TokenType.Percentage) as UnitToken;
            var palette = tokens.FirstOrDefault(t => !ReferenceEquals(t, pct));

            if (pct == null || pct.Value is < 0f or > 100f || palette == null || !IsPalette(palette)) return false;

            percentage = pct.Value;
            return true;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<FontPaletteValue>();
        }

        private static bool IsKeywordOrDashedIdent(string ident) =>
            ident.Isi(Keywords.Normal) || ident.Isi("light") || ident.Isi("dark") ||
            ident.StartsWith("--", StringComparison.Ordinal);

        private sealed class FontPaletteValue : IPropertyValue
        {
            public FontPaletteValue(IEnumerable<Token> tokens)
            {
                Original = new TokenValue(tokens);
            }

            public string CssText => Original.Text;

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}
