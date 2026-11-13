using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>contain-intrinsic-size</c> (CSS Sizing 4 §5.1): one or two <c>auto? [ none | &lt;length&gt; ]</c>
    /// groups, the first for the width and the second for the height; one group serves both. A group is one
    /// or two tokens, so the value is split at each <c>auto</c> that follows a complete group.
    /// </summary>
    internal sealed class ContainIntrinsicSizeValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var groups = new List<List<Token>>();

            foreach (var token in value.Where(t => t.Type != TokenType.Whitespace))
            {
                // "auto" opens a group; any other token completes the group it is in, or is a group of its own.
                if (IsAuto(token) || groups.Count == 0 || !IsOpenGroup(groups[groups.Count - 1]))
                {
                    groups.Add(new List<Token> { token });
                }
                else
                {
                    groups[groups.Count - 1].Add(token);
                }
            }

            if (groups.Count is 0 or > 2 || groups.Any(g => !ContainGrammar.TryParseIntrinsic(g))) return null;

            var width = Spaced(groups[0]);
            var height = groups.Count == 2 ? Spaced(groups[1]) : width;

            return new MappedShorthandValue(
                new Dictionary<string, IReadOnlyList<Token>>
                {
                    [PropertyNames.ContainIntrinsicWidth] = width,
                    [PropertyNames.ContainIntrinsicHeight] = height,
                },
                value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var width = properties.FirstOrDefault(p => p.Name.Is(PropertyNames.ContainIntrinsicWidth))?.DeclaredValue;
            var height = properties.FirstOrDefault(p => p.Name.Is(PropertyNames.ContainIntrinsicHeight))?.DeclaredValue;

            if (width == null || height == null) return null;

            var widthTokens = new List<Token>(width.Original);
            var heightTokens = new List<Token>(height.Original);
            var original = width.CssText == height.CssText
                ? widthTokens
                : new List<Token>(widthTokens.Concat(new[] { Token.Whitespace }).Concat(heightTokens));

            return new MappedShorthandValue(
                new Dictionary<string, IReadOnlyList<Token>>
                {
                    [PropertyNames.ContainIntrinsicWidth] = widthTokens,
                    [PropertyNames.ContainIntrinsicHeight] = heightTokens,
                },
                original);
        }

        private static bool IsAuto(Token token) => token.Type == TokenType.Ident && token.Data.Isi(Keywords.Auto);

        private static bool IsOpenGroup(List<Token> group) => group.Count == 1 && IsAuto(group[0]);

        // Whitespace is dropped by the grouping, so the group is rejoined with the single space it was written with.
        private static List<Token> Spaced(List<Token> group)
        {
            var result = new List<Token>();

            foreach (var token in group)
            {
                if (result.Count > 0) result.Add(Token.Whitespace);

                result.Add(token);
            }

            return result;
        }
    }

    /// <summary>
    /// The <c>container</c> shorthand (CSS Containment 3 §2): <c>&lt;'container-name'&gt; [ / &lt;'container-type'&gt; ]?</c>.
    /// An omitted type resets <c>container-type</c> to <c>normal</c>.
    /// </summary>
    internal sealed class ContainerValueConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var front = new List<Token>();
            var back = new List<Token>();
            var current = front;

            foreach (var token in value)
            {
                if (token.Type == TokenType.Delim && token.Data.Is("/"))
                {
                    if (current == back) return null;

                    current = back;
                }
                else
                {
                    current.Add(token);
                }
            }

            if (Converters.LiteralsConverter.Convert(front) == null) return null;

            var hasType = current == back;

            if (hasType && Converters.ContainerTypeConverter.Convert(back) == null) return null;

            return new MappedShorthandValue(
                new Dictionary<string, IReadOnlyList<Token>>
                {
                    [PropertyNames.ContainerName] = Trim(front),
                    [PropertyNames.ContainerType] = hasType
                        ? Trim(back)
                        : new List<Token> { new Token(TokenType.Ident, Keywords.Normal, TextPosition.Empty) },
                },
                value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var name = properties.FirstOrDefault(p => p.Name.Is(PropertyNames.ContainerName))?.DeclaredValue;
            var type = properties.FirstOrDefault(p => p.Name.Is(PropertyNames.ContainerType))?.DeclaredValue;

            if (name == null || type == null) return null;

            var nameTokens = new List<Token>(name.Original);
            var typeTokens = new List<Token>(type.Original);

            // The shortest form: a "normal" type is what the shorthand resets to, so it is left out.
            var original = type.CssText.Is(Keywords.Normal)
                ? nameTokens
                : new List<Token>(nameTokens
                    .Concat(new[] { Token.Whitespace, new Token(TokenType.Delim, "/", TextPosition.Empty), Token.Whitespace })
                    .Concat(typeTokens));

            return new MappedShorthandValue(
                new Dictionary<string, IReadOnlyList<Token>>
                {
                    [PropertyNames.ContainerName] = nameTokens,
                    [PropertyNames.ContainerType] = typeTokens,
                },
                original);
        }

        private static List<Token> Trim(List<Token> tokens)
        {
            var start = 0;
            var end = tokens.Count - 1;

            while (start <= end && tokens[start].Type == TokenType.Whitespace) start++;
            while (end >= start && tokens[end].Type == TokenType.Whitespace) end--;

            return tokens.GetRange(start, end - start + 1);
        }
    }
}
