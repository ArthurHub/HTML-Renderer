using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Shared grammar for the CSS <c>background-size</c> value (<c>auto</c>, <c>cover</c>,
    /// <c>contain</c>, or a 1-2 token length/percentage/auto form). Used by both
    /// <see cref="Converters.BackgroundSizeConverter"/> (CSS-OM grammar validation/serialization)
    /// and the render-time background-layer resolution (size resolution), so there
    /// is one parser for this grammar, not two.
    /// </summary>
    internal static class BackgroundSizeGrammar
    {
        internal readonly struct Component
        {
            public bool IsAuto { get; }

            /// <summary>The length/percentage/calc() token for this component, null when <see cref="IsAuto"/>.</summary>
            public Token Value { get; }

            public Component(bool isAuto, Token value)
            {
                IsAuto = isAuto;
                Value = value;
            }
        }

        internal sealed class ParsedSize
        {
            public bool IsCover { get; }
            public bool IsContain { get; }
            public Component Width { get; }
            public Component Height { get; }

            /// <summary>False for the 1-token auto/length/percent form (height defaults to auto for
            /// resolution purposes, but must NOT be re-serialized - "2em" stays "2em", not "2em auto").</summary>
            public bool HasExplicitHeight { get; }

            private ParsedSize(bool isCover, bool isContain, Component width, Component height, bool hasExplicitHeight)
            {
                IsCover = isCover;
                IsContain = isContain;
                Width = width;
                Height = height;
                HasExplicitHeight = hasExplicitHeight;
            }

            public static ParsedSize Cover() => new ParsedSize(true, false, default, default, false);
            public static ParsedSize Contain() => new ParsedSize(false, true, default, default, false);
            public static ParsedSize SingleValue(Component width) => new ParsedSize(false, false, width, AutoComponent, false);
            public static ParsedSize TwoValue(Component width, Component height) => new ParsedSize(false, false, width, height, true);
        }

        private static readonly Component AutoComponent = new Component(true, null);

        internal static ParsedSize TryParse(IReadOnlyList<Token> tokens)
        {
            if (tokens.Count == 1)
            {
                var t = tokens[0];
                if (IsKeyword(t, Keywords.Cover)) return ParsedSize.Cover();
                if (IsKeyword(t, Keywords.Contain)) return ParsedSize.Contain();

                var w = ToComponent(t);
                return w is null ? null : ParsedSize.SingleValue(w.Value);
            }

            if (tokens.Count == 2)
            {
                var w = ToComponent(tokens[0]);
                var h = ToComponent(tokens[1]);
                return w is null || h is null ? null : ParsedSize.TwoValue(w.Value, h.Value);
            }

            return null;
        }

        private static Component? ToComponent(Token token)
        {
            if (IsKeyword(token, Keywords.Auto)) return AutoComponent;
            if (Converters.LengthOrPercentConverter.Convert(new[] { token }) != null) return new Component(false, token);
            return null;
        }

        private static bool IsKeyword(Token token, string keyword) =>
            token.Type == TokenType.Ident && token.Data.Isi(keyword);
    }
}
