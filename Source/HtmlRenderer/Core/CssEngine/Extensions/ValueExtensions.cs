using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class ValueExtensions
    {
        private static bool IsWeight(int value)
        {
            return value == 100 || value == 200 || value == 300 || value == 400 ||
                   value == 500 || value == 600 || value == 700 || value == 800 ||
                   value == 900;
        }

        public static Token OnlyOrDefault(this IEnumerable<Token> value)
        {
            var result = default(Token);

            foreach (var item in value)
            {
                if (result == null)
                {
                    result = item;
                    continue;
                }

                result = default;
                break;
            }

            return result;
        }

        public static bool Is(this IEnumerable<Token> value, string expected)
        {
            var identifier = value.ToIdentifier();
            return identifier != null && identifier.Isi(expected);
        }

        public static string ToUri(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Url) return element.Data;

            return null;
        }

        public static Length? ToDistance(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var percent = enumerable.ToPercent();

            return percent.HasValue
                ? new Length(percent.Value.Value, Length.Unit.Percent)
                : enumerable.ToLength();
        }

        public static Length ToLength(this FontSize fontSize)
        {
            switch (fontSize)
            {
                case FontSize.Big: //1.5em
                    return new Length(1.5f, Length.Unit.Em);
                case FontSize.Huge: //2em
                    return new Length(2f, Length.Unit.Em);
                case FontSize.Large: //1.2em
                    return new Length(1.2f, Length.Unit.Em);
                case FontSize.Larger: //*120%
                    return new Length(120f, Length.Unit.Percent);
                case FontSize.Little: //0.75em
                    return new Length(0.75f, Length.Unit.Em);
                case FontSize.Small: //8/9em
                    return new Length(8f / 9f, Length.Unit.Em);
                case FontSize.Smaller: //*80%
                    return new Length(80f, Length.Unit.Percent);
                case FontSize.Tiny: //0.6em
                    return new Length(0.6f, Length.Unit.Em);
                default: //1em
                    return new Length(1f, Length.Unit.Em);
            }
        }

        public static Percent? ToPercent(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Percentage)
                return new Percent(((UnitToken)element).Value);

            return null;
        }

        public static Percent? ToPercentOrFraction(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var percent = ToPercent(enumerable);

            if (percent != null)
            {
                return percent;
            }

            var element = value.OnlyOrDefault();
            if (!(element is NumberToken token))
            {
                return null;
            }

            try
            {
                var number = token.Value;
                var percentage = number * 100;
                return new Percent(percentage);
            }
            catch
            {
                return null;
            }
        }

        public static Number? ToPercentOrNumber(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var percent = ToPercent(enumerable);

            if (percent != null)
            {
                return new Number(percent.Value.Value, Number.Unit.Percent);
            }

            var element = value.OnlyOrDefault();
            if (!(element is NumberToken token))
            {
                return null;
            }

            try
            {
                return new Number(token.Value, Number.Unit.Float);
            }
            catch
            {
                return null;
            }
        }

        public static string ToCssString(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.String) return element.Data;

            return null;
        }

        public static string ToLiterals(this IEnumerable<Token> value)
        {
            var elements = new List<string>();
            var it = value.GetEnumerator();

            if (it.MoveNext())
            {
                do
                {
                    if (it.Current?.Type != TokenType.Ident) return null;

                    elements.Add(it.Current.Data);

                    if (it.MoveNext() && it.Current?.Type != TokenType.Whitespace) return null;
                } while (it.MoveNext());

                it.Dispose();
                return string.Join(" ", elements);
            }

            it.Dispose();
            return null;
        }

        public static string ToIdentifier(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Ident) return element.Data.ToLowerInvariant();

            return null;
        }

        public static string ToIdentifierCaseInsensitive(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Ident) return element.Data;

            return null;
        }

        public static string ToAnimatableIdentifier(this IEnumerable<Token> value)
        {
            var identifier = value.ToIdentifier();

            if (identifier != null &&
                (identifier.Isi(Keywords.All) || PropertyFactory.Instance.IsAnimatable(identifier)))
            {
                return identifier;
            }

            return null;
        }

        public static float? ToSingle(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Number) return ((NumberToken)element).Value;

            return null;
        }

        public static float? ToNaturalSingle(this IEnumerable<Token> value)
        {
            var element = value.ToSingle();
            return element >= 0f ? element : null;
        }

        public static float? ToGreaterOrEqualOneSingle(this IEnumerable<Token> value)
        {
            var element = value.ToSingle();
            return element >= 1f ? element : null;
        }

        public static int? ToInteger(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Number && ((NumberToken)element).IsInteger)
            {
                return ((NumberToken)element).IntegerValue;
            }

            return null;
        }

        public static int? ToNaturalInteger(this IEnumerable<Token> value)
        {
            var element = value.ToInteger();
            return element >= 0 ? element : null;
        }

        public static int? ToPositiveInteger(this IEnumerable<Token> value)
        {
            var element = value.ToInteger();
            return element > 0 ? element : null;
        }

        public static int? ToWeightInteger(this IEnumerable<Token> value)
        {
            var element = value.ToPositiveInteger();
            return element.HasValue && IsWeight(element.Value) ? element : null;
        }

        public static int? ToBinary(this IEnumerable<Token> value)
        {
            var element = value.ToInteger();
            return element.HasValue && (element.Value == 0 || element.Value == 1) ? element : null;
        }

        public static float? ToAlphaValue(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var element = enumerable.ToNaturalSingle();

            if (element.HasValue) return Math.Min(element.Value, 1f);

            var percent = enumerable.ToPercent();

            return percent?.NormalizedValue;
        }

        public static byte? ToRgbComponent(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var element = enumerable.ToNaturalInteger();

            if (element.HasValue) return (byte)Math.Min(element.Value, 255);

            var percent = enumerable.ToPercent();

            if (!percent.HasValue) return null;

            return (byte)(255f * percent.Value.NormalizedValue);
        }

        public static Angle? ToAngle(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element == null || element.Type != TokenType.Dimension) return null;

            var token = (UnitToken)element;
            var unit = Angle.GetUnit(token.Unit);

            if (unit != Angle.Unit.None) return new Angle(token.Value, unit);

            return null;
        }

        public static Angle? ToAngleNumber(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var angle = enumerable.ToAngle();

            if (angle.HasValue) return angle.Value;

            var number = enumerable.ToSingle();

            if (!number.HasValue) return null;

            return new Angle(number.Value, Angle.Unit.Deg);
        }

        public static Length? ToLength(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null)
            {
                switch (element.Type)
                {
                    case TokenType.Dimension:
                        {
                            var token = (UnitToken)element;
                            var unit = Length.GetUnit(token.Unit);

                            if (unit != Length.Unit.None) return new Length(token.Value, unit);
                            break;
                        }
                    case TokenType.Number when ((NumberToken)element).Value == 0f:
                        return Length.Zero;
                }
            }

            return null;
        }

        public static Resolution? ToResolution(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element == null || element.Type != TokenType.Dimension) return null;

            var token = (UnitToken)element;
            var unit = Resolution.GetUnit(token.Unit);

            if (unit != Resolution.Unit.None) return new Resolution(token.Value, unit);

            return null;
        }

        public static Time? ToTime(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element == null || element.Type != TokenType.Dimension) return null;

            var token = (UnitToken)element;
            var unit = Time.GetUnit(token.Unit);

            if (unit != Time.Unit.None) return new Time(token.Value, unit);

            return null;
        }

        public static Length? ToBorderWidth(this IEnumerable<Token> value)
        {
            var enumerable = value as Token[] ?? value.ToArray();
            var length = enumerable.ToLength();

            if (length != null) return length;

            if (enumerable.Is(Keywords.Thin)) return Length.Thin;

            if (enumerable.Is(Keywords.Medium)) return Length.Medium;

            return enumerable.Is(Keywords.Thick) ? Length.Thick : length;
        }

        public static List<List<Token>> ToItems(this IEnumerable<Token> value)
        {
            var list = new List<List<Token>>();
            var current = new List<Token>();
            var nested = 0;
            list.Add(current);

            foreach (var token in value)
            {
                var whitespace = token.Type == TokenType.Whitespace;
                var newItem = token.Type == TokenType.String || token.Type == TokenType.Url ||
                              token.Type == TokenType.Function;

                if (nested == 0 && (whitespace || newItem))
                {
                    if (current.Count != 0)
                    {
                        current = new List<Token>();
                        list.Add(current);
                    }

                    if (whitespace) continue;
                }
                else if (token.Type == TokenType.RoundBracketOpen)
                {
                    nested++;
                }
                else if (token.Type == TokenType.RoundBracketClose)
                {
                    nested--;
                }

                current.Add(token);
            }

            return list;
        }

        public static void Trim(this List<Token> value)
        {
            var begin = 0;
            var end = value.Count - 1;

            while (begin < end)
                if (value[begin].Type == TokenType.Whitespace)
                    begin++;
                else if (value[end].Type == TokenType.Whitespace)
                    end--;
                else
                    break;

            value.RemoveRange(++end, value.Count - end);
            value.RemoveRange(0, begin);
        }

        public static List<List<Token>> ToList(this IEnumerable<Token> value)
        {
            var list = new List<List<Token>>();
            var current = new List<Token>();
            var nested = 0;
            list.Add(current);

            foreach (var token in value)
            {
                if (nested == 0 && token.Type == TokenType.Comma)
                {
                    current = new List<Token>();
                    list.Add(current);
                    continue;
                }

                switch (token.Type)
                {
                    case TokenType.RoundBracketOpen:
                        nested++;
                        break;
                    case TokenType.RoundBracketClose:
                        nested--;
                        break;
                    case TokenType.Whitespace when current.Count == 0:
                        continue;
                }

                current.Add(token);
            }

            foreach (var token in list) token.Trim();

            return list;
        }

        public static string ToText(this IEnumerable<Token> value)
        {
            return string.Join(string.Empty, value.Select(m => m.ToValue()));
        }

        public static bool ContainsFunction(this IEnumerable<Token> tokens, string functionName)
        {
            foreach (var token in tokens)
            {
                if (token is FunctionToken function &&
                    (function.Data.Isi(functionName) || function.ArgumentTokens.ContainsFunction(functionName)))
                {
                    return true;
                }
            }

            return false;
        }

        public static Color? ToColor(this IEnumerable<Token> value)
        {
            var element = value.OnlyOrDefault();

            if (element != null && element.Type == TokenType.Ident) return Color.FromName(element.Data);

            if (element != null && element.Type == TokenType.Color && !((ColorToken)element).IsValid)
            {
                return Color.FromHex(element.Data);
            }

            return null;
        }
    }
}