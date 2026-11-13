using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Grammar of the <c>filter</c> / <c>backdrop-filter</c> value (Filter Effects 1 §3):
    /// <c>none | &lt;filter-value-list&gt;</c>, where each entry is a <c>url()</c> reference or one of
    /// <c>blur() brightness() contrast() drop-shadow() grayscale() hue-rotate() invert() opacity() saturate() sepia()</c>.
    /// The entries are separated by whitespace only. Arguments are kept as authored text.
    /// </summary>
    internal static class FilterGrammar
    {
        private const string Blur = "blur";
        private const string Brightness = "brightness";
        private const string Contrast = "contrast";
        private const string DropShadow = "drop-shadow";
        private const string Grayscale = "grayscale";
        private const string HueRotate = "hue-rotate";
        private const string Invert = "invert";
        private const string Opacity = "opacity";
        private const string Saturate = "saturate";
        private const string Sepia = "sepia";

        private static readonly HashSet<string> KnownFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Blur, Brightness, Contrast, DropShadow, Grayscale, HueRotate, Invert, Opacity, Saturate, Sepia
        };

        private static readonly IValueConverter AngleCalc = new CalcValueConverter(CalcCategory.Angle);
        private static readonly IValueConverter LengthCalc = new CalcValueConverter(CalcCategory.Length);

        private static readonly IValueConverter NumberOrPercentCalc =
            new CalcValueConverter(CalcCategory.Number | CalcCategory.Percentage);

        /// <summary>
        /// One parsed filter entry. <see cref="Name"/> is the lower-cased function name, or <c>url</c> for a
        /// <c>url()</c> reference. <see cref="Arguments"/> holds the raw authored text of each argument.
        /// </summary>
        internal sealed class FilterFunction
        {
            public FilterFunction(string name, IReadOnlyList<string> arguments)
            {
                Name = name;
                Arguments = arguments;
            }

            public string Name { get; }
            public IReadOnlyList<string> Arguments { get; }
        }

        /// <summary>
        /// Parses the tokens of a filter value. Returns <c>null</c> when invalid, an empty list for the keyword
        /// <c>none</c>, otherwise the entries in source order.
        /// </summary>
        internal static List<FilterFunction> TryParse(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length == 0) return null;

            if (tokens.Length == 1 && tokens[0].Type == TokenType.Ident && tokens[0].Data.Isi(Keywords.None))
                return new List<FilterFunction>();

            var result = new List<FilterFunction>();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Url)
                {
                    result.Add(new FilterFunction(FunctionNames.Url, new[] { token.Data }));
                    continue;
                }

                if (!(token is FunctionToken function) || !KnownFunctions.Contains(function.Data)) return null;

                var name = function.Data.ToLowerInvariant();
                var args = function.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

                if (!TryParseArguments(name, args, out var arguments)) return null;

                result.Add(new FilterFunction(name, arguments));
            }

            return result;
        }

        private static bool TryParseArguments(string name, Token[] args, out List<string> arguments)
        {
            if (name == DropShadow) return TryParseDropShadow(args, out arguments);

            arguments = new List<string>();

            // Every other function takes one optional argument.
            if (args.Length == 0) return true;
            if (args.Length != 1) return false;

            var token = args[0];
            var valid = name == HueRotate ? IsAngle(token)
                : name == Blur ? IsNonNegativeLength(token)
                : IsNonNegativeNumberOrPercentage(token);

            if (valid) arguments.Add(token.ToValue());

            return valid;
        }

        // drop-shadow( [ <color>? && <length>{2,3} ] ): the colour leads or trails the lengths, the optional
        // third length (blur radius) is non-negative.
        private static bool TryParseDropShadow(Token[] args, out List<string> arguments)
        {
            arguments = null;

            var lengths = new List<Token>();
            var colors = new List<Token>();

            foreach (var token in args)
            {
                if (IsLength(token)) lengths.Add(token);
                else colors.Add(token);
            }

            if (lengths.Count < 2 || lengths.Count > 3 || colors.Count > 1) return false;
            if (lengths.Count == 3 && IsNegative(lengths[2])) return false;

            if (colors.Count == 1)
            {
                // The lengths must be contiguous, so the colour is first or last.
                var index = Array.IndexOf(args, colors[0]);
                if (index != 0 && index != args.Length - 1) return false;
                if (Converters.CurrentColorConverter.Convert(colors) == null) return false;
            }

            arguments = new List<string>(args.Select(t => t.ToValue()));
            return true;
        }

        private static bool IsLength(Token token)
        {
            if (token is NumberToken number) return number.Value == 0f;
            if (token.Type == TokenType.Dimension)
                return Length.GetUnit(((UnitToken)token).Unit.ToLowerInvariant()) != Length.Unit.None;

            return token is FunctionToken && LengthCalc.Convert(new[] { token }) != null;
        }

        private static bool IsNegative(Token token) =>
            (token is NumberToken number && number.Value < 0f) || (token is UnitToken unit && unit.Value < 0f);

        private static bool IsAngle(Token token)
        {
            if (token is NumberToken number) return number.Value == 0f; // unitless zero, CSS Values 4 §6.1
            if (token.Type == TokenType.Dimension)
                return Angle.GetUnit(((UnitToken)token).Unit.ToLowerInvariant()) != Angle.Unit.None;

            return token is FunctionToken && AngleCalc.Convert(new[] { token }) != null;
        }

        private static bool IsNonNegativeLength(Token token) => IsLength(token) && !IsNegative(token);

        private static bool IsNonNegativeNumberOrPercentage(Token token)
        {
            if (token is NumberToken number) return number.Value >= 0f;
            if (token.Type == TokenType.Percentage) return ((UnitToken)token).Value >= 0f;

            return token is FunctionToken && NumberOrPercentCalc.Convert(new[] { token }) != null;
        }
    }
}
