using System;
using System.Globalization;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class StringExtensions
    {
        public static bool Has(this string value, char chr, int index = 0)
        {
            return value != null && value.Length > index && value[index] == chr;
        }

        public static bool Contains(this string[] list, string element,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return list.Any(t => t.Equals(element, comparison));
        }

        public static bool Is(this string current, string other)
        {
            return string.Equals(current, other, StringComparison.Ordinal);
        }

        public static bool Isi(this string current, string other)
        {
            return string.Equals(current, other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOneOf(this string element, string item1, string item2)
        {
            return element.Is(item1) || element.Is(item2);
        }

        public static string StylesheetString(this string value)
        {
            var builder = Pool.NewStringBuilder();
            builder.Append(Symbols.DoubleQuote);

            if (!string.IsNullOrEmpty(value))
                for (var i = 0; i < value.Length; i++)
                {
                    var character = value[i];

                    switch (character)
                    {
                        case Symbols.Null:
                            throw new ParseException("Unable to parse null symbol");
                        case Symbols.DoubleQuote:
                        case Symbols.ReverseSolidus:
                            builder.Append(Symbols.ReverseSolidus).Append(character);
                            break;
                        default:
                            if (character.IsInRange(Symbols.StartOfHeading, Symbols.UnitSeparator)
                                || character == Symbols.CurlyBracketOpen)
                            {
                                builder.Append(Symbols.ReverseSolidus)
                                    .Append(character.ToHex())
                                    .Append(i + 1 != value.Length ? " " : "");
                            }
                            else
                            {
                                builder.Append(character);
                            }

                            break;
                    }
                }

            builder.Append(Symbols.DoubleQuote);
            return builder.ToPool();
        }

        public static string StylesheetFunction(this string value, string argument)
        {
            return string.Concat(value, "(", argument, ")");
        }

        public static string StylesheetUrl(this string value)
        {
            var argument = value.StylesheetString();
            return FunctionNames.Url.StylesheetFunction(argument);
        }

        public static string StylesheetUnit(this string value, out float result)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var firstLetter = value.Length;

                while (!value[firstLetter - 1].IsDigit() && --firstLetter > 0)
                {
                    // Intentional empty.
                }

                var parsed = float.TryParse(value.Substring(0, firstLetter), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out result);

                if (firstLetter > 0 && parsed) return value.Substring(firstLetter);
            }

            result = default;
            return null;
        }
    }
}