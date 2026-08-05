using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>One operand of a <c>palette-mix()</c>: a palette reference and an optional mixing percentage.</summary>
    internal sealed class PaletteMixOperand
    {
        public PaletteMixOperand(string palette, double? percentage)
        {
            Palette = palette;
            Percentage = percentage;
        }

        public string Palette { get; }
        public double? Percentage { get; }
    }

    /// <summary>
    /// A parsed <c>palette-mix()</c> value (CSS Fonts Module Level 4). The two operands are blended per CPAL
    /// entry in <see cref="ColorSpace"/> (with an optional polar <see cref="HueMethod"/>), weighted by the
    /// operand percentages.
    /// </summary>
    internal sealed class ParsedPaletteMix
    {
        public ParsedPaletteMix(string colorSpace, string hueMethod, PaletteMixOperand first, PaletteMixOperand second)
        {
            ColorSpace = colorSpace;
            HueMethod = hueMethod;
            First = first;
            Second = second;
        }

        public string ColorSpace { get; }
        public string HueMethod { get; }
        public PaletteMixOperand First { get; }
        public PaletteMixOperand Second { get; }
    }

    /// <summary>
    /// The single, shared grammar for the CSS <c>palette-mix()</c> function
    /// (<a href="https://www.w3.org/TR/css-fonts-4/#font-palette-values">CSS Fonts 4</a>):
    /// <code>palette-mix( &lt;color-interpolation-method&gt; , [ [ normal | light | dark | &lt;palette-identifier&gt; ] &lt;percentage&gt;? ]#{2} )</code>.
    /// Used by both Layer A (the <c>font-palette</c> value converter, which drops a malformed value at parse
    /// time) and Layer B (<c>ActualFontPalette</c> resolution), so the two layers cannot disagree. Color-space
    /// membership is owned by <see cref="ColorInterpolationMethodGrammar"/> (its predicates classify the space
    /// and hue direction here), keeping a single source of truth for which spaces are valid.
    /// </summary>
    internal static class PaletteMixGrammar
    {
        private const string FunctionName = "palette-mix";
        private const string In = "in";
        private const string Hue = "hue";

        public static ParsedPaletteMix TryParse(IReadOnlyList<Token> value)
        {
            var fn = SinglePaletteMixFunction(value);
            if (fn == null)
                return null;

            var groups = SplitTopLevelComma(new List<Token>(fn.ArgumentTokens));
            if (groups.Count != 3)
                return null;

            if (!TryParseMethod(groups[0], out var space, out var hue) ||
                !TryParseOperand(groups[1], out var first) ||
                !TryParseOperand(groups[2], out var second))
                return null;

            // Both percentages explicitly zero has no valid interpretation (mirrors color-mix()).
            if (first.Percentage == 0 && second.Percentage == 0)
                return null;

            return new ParsedPaletteMix(space, hue, first, second);
        }

        private static FunctionToken SinglePaletteMixFunction(IReadOnlyList<Token> value)
        {
            FunctionToken found = null;
            foreach (var token in value)
            {
                if (token.Type == TokenType.Whitespace)
                    continue;
                var fn = token as FunctionToken;
                if (found != null || fn == null || !fn.Data.Isi(FunctionName))
                    return null;
                found = fn;
            }

            return found;
        }

        // in <color-space> [ <hue-interpolation-method> hue ]?  (the hue method only after a polar space)
        private static bool TryParseMethod(List<Token> tokens, out string space, out string hue)
        {
            space = null;
            hue = null;

            int i = 0;

            if (i >= tokens.Count || !IsIdent(tokens[i], out var inWord) || !inWord.Isi(In))
                return false;
            i++;

            if (i >= tokens.Count || !IsIdent(tokens[i], out var spaceName) ||
                !ColorInterpolationMethodGrammar.IsColorSpace(spaceName))
                return false;
            space = spaceName;
            i++;

            if (i < tokens.Count)
            {
                // A trailing "<dir> hue" is valid only for a polar color space.
                if (!ColorInterpolationMethodGrammar.IsPolarColorSpace(spaceName))
                    return false;
                if (!IsIdent(tokens[i], out var dir) || !ColorInterpolationMethodGrammar.IsHueDirection(dir))
                    return false;
                i++;
                if (i >= tokens.Count || !IsIdent(tokens[i], out var hueWord) || !hueWord.Isi(Hue))
                    return false;
                hue = dir;
                i++;
            }

            return i == tokens.Count;
        }

        // [ normal | light | dark | <dashed-ident> ] <percentage>?
        private static bool TryParseOperand(List<Token> tokens, out PaletteMixOperand operand)
        {
            operand = null;

            if (tokens.Count == 0 || tokens.Count > 2)
                return false;

            if (!IsIdent(tokens[0], out var palette) || !IsPaletteIdentifier(palette))
                return false;

            double? percentage = null;
            if (tokens.Count == 2)
            {
                var pct = tokens[1] as UnitToken;
                if (pct == null || pct.Type != TokenType.Percentage)
                    return false;
                percentage = pct.Value;
            }

            operand = new PaletteMixOperand(palette, percentage);
            return true;
        }

        private static bool IsPaletteIdentifier(string ident) =>
            ident.Isi(Keywords.Normal) || ident.Isi("light") || ident.Isi("dark") ||
            ident.StartsWith("--", System.StringComparison.Ordinal);

        // Splits the argument tokens on top-level commas, dropping whitespace (insignificant in this grammar).
        private static List<List<Token>> SplitTopLevelComma(List<Token> tokens)
        {
            var groups = new List<List<Token>> { new List<Token>() };
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Whitespace)
                    continue;
                if (token.Type == TokenType.Comma)
                    groups.Add(new List<Token>());
                else
                    groups[groups.Count - 1].Add(token);
            }

            return groups;
        }

        private static bool IsIdent(Token token, out string data)
        {
            if (token.Type == TokenType.Ident)
            {
                data = token.Data;
                return true;
            }

            data = string.Empty;
            return false;
        }
    }
}
