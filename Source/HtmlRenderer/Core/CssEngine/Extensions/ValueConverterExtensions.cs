using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class ValueConverterExtensions
    {
        public static IPropertyValue ConvertDefault(this IValueConverter converter)
        {
            return converter.Convert(Enumerable.Empty<Token>());
        }

        public static IPropertyValue VaryStart(this IValueConverter converter, List<Token> list)
        {
            for (var count = list.Count; count > 0; count--)
            {
                if (list[count - 1].Type == TokenType.Whitespace)
                {
                    continue;
                }

                var value = converter.Convert(list.Take(count));

                if (value == null)
                {
                    continue;
                }

                list.RemoveRange(0, count);
                list.Trim();
                return value;
            }

            return converter.ConvertDefault();
        }

        public static IPropertyValue VaryAll(this IValueConverter converter, List<Token> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Type == TokenType.Whitespace)
                {
                    continue;
                }

                for (var j = list.Count; j > i; j--)
                {
                    var count = j - i;

                    if (list[j - 1].Type == TokenType.Whitespace)
                    {
                        continue;
                    }

                    var value = converter.Convert(list.Skip(i).Take(count));

                    if (value == null) continue;
                    list.RemoveRange(i, count);
                    list.Trim();
                    return value;
                }
            }

            return converter.ConvertDefault();
        }

        public static IValueConverter Many(this IValueConverter converter, int min = 1, int max = ushort.MaxValue)
        {
            return new OneOrMoreValueConverter(converter, min, max);
        }

        public static IValueConverter FromList(this IValueConverter converter)
        {
            return new ListValueConverter(converter);
        }

        public static IValueConverter ToConverter<T>(this IReadOnlyDictionary<string, T> values)
        {
            return new DictionaryValueConverter<T>(values);
        }

        public static IValueConverter Periodic(this IValueConverter converter, params string[] labels)
        {
            return new PeriodicValueConverter(converter, labels);
        }

        public static IValueConverter RequiresEnd(this IValueConverter listConverter, IValueConverter endConverter)
        {
            return new EndListValueConverter(listConverter, endConverter);
        }

        public static IValueConverter Required(this IValueConverter converter)
        {
            return new RequiredValueConverter(converter);
        }

        public static IValueConverter Option(this IValueConverter converter)
        {
            return new OptionValueConverter(converter);
        }

        public static IValueConverter For(this IValueConverter converter, params string[] labels)
        {
            return new ConstraintValueConverter(converter, labels);
        }

        public static IValueConverter Option<T>(this IValueConverter converter, T defaultValue)
        {
            return new OptionValueConverter<T>(converter);
        }

        public static IValueConverter Or(this IValueConverter primary, IValueConverter secondary)
        {
            return new OrValueConverter(primary, secondary);
        }

        public static IValueConverter Or(this IValueConverter primary, string keyword)
        {
            return primary.Or<object>(keyword, null);
        }

        public static IValueConverter Or<T>(this IValueConverter primary, string keyword, T value)
        {
            var identifier = new IdentifierValueConverter<T>(keyword, value);
            return new OrValueConverter(primary, identifier);
        }

        public static IValueConverter OrNone(this IValueConverter primary)
        {
            return primary.Or(Keywords.None);
        }

        public static IValueConverter OrDefault(this IValueConverter primary)
        {
            return primary.OrGlobalValue();
        }

        public static IValueConverter OrDefault<T>(this IValueConverter primary, T value)
        {
            return primary.OrInherit().Or(Keywords.Initial, value)
                          .Or(Keywords.Revert)
                          .Or(Keywords.RevertLayer)
                          .Or(Keywords.Unset);
        }

        public static IValueConverter OrInherit(this IValueConverter primary)
        {
            return primary.Or(Keywords.Inherit);
        }

        public static IValueConverter OrAuto(this IValueConverter primary)
        {
            return primary.Or(Keywords.Auto);
        }

        public static IValueConverter OrGlobalValue(this IValueConverter primary)
        {
            return primary.OrInherit()
                          .Or(Keywords.Initial)
                          .Or(Keywords.Revert)
                          .Or(Keywords.RevertLayer)
                          .Or(Keywords.Unset);
        }

        public static IValueConverter ConditionalStartsWithKeyword(this IValueConverter primary, string when, params string[] keywords)
        {
            return new ConditionalStartsWithValueConverter(when, primary, keywords);
        }

        public static IValueConverter StartsWithKeyword(this IValueConverter converter, string keyword)
        {
            return new StartsWithValueConverter(TokenType.Ident, keyword, converter);
        }

        public static IValueConverter StartsWithDelimiter(this IValueConverter converter)
        {
            return new StartsWithValueConverter(TokenType.Delim, "/", converter);
        }

        public static IValueConverter WithCurrentColor(this IValueConverter converter)
        {
            return converter.Or(Keywords.CurrentColor, Color.Transparent);
        }

        public static IValueConverter WithFallback(this IValueConverter converter, int defaultValue)
        {
            return new FallbackValueConverter(converter, TokenValue.FromNumber(defaultValue));
        }
    }
}