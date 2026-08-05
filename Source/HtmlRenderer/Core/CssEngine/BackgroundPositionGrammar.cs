using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Shared grammar for the CSS <c>background-position</c> value (the 1/2/3/4-token forms,
    /// including the edge-relative offset syntax, e.g. "right 20px bottom 10px"). Used by both
    /// <see cref="Converters.PointConverter"/> (CSS-OM grammar validation/serialization) and
    /// the render-time background-layer resolution (pixel-offset arithmetic) so
    /// there is one parser for this grammar, not two independent re-implementations - mirroring how
    /// CalcParser is shared between CalcValueConverter and CssValueParser rather than re-parsed twice.
    /// </summary>
    internal static class BackgroundPositionGrammar
    {
        internal enum AxisKeyword { None, Left, Right, Top, Bottom, Center }

        internal readonly struct Component
        {
            public AxisKeyword Keyword { get; }

            /// <summary>The length/percentage/calc() token for this component, or null when there is
            /// none (a keyword-only component, or the implicit "center" filled in for a 1-token value).</summary>
            public Token Offset { get; }

            public Component(AxisKeyword keyword, Token offset)
            {
                Keyword = keyword;
                Offset = offset;
            }
        }

        internal sealed class ParsedPosition
        {
            public Component X { get; }
            public Component Y { get; }

            /// <summary>The 1-2 authored components in original left-to-right order, needed to
            /// re-serialize in the exact order/form the author wrote (CSS round-trips verbatim).</summary>
            public IReadOnlyList<Component> AuthoredOrder { get; }

            public ParsedPosition(Component x, Component y, IReadOnlyList<Component> authoredOrder)
            {
                X = x;
                Y = y;
                AuthoredOrder = authoredOrder;
            }
        }

        internal static ParsedPosition TryParse(IReadOnlyList<Token> tokens)
        {
            switch (tokens.Count)
            {
                case 1:
                    return ParseSingle(tokens[0]);
                case 2:
                    return ParseDouble(tokens[0], tokens[1]);
                case 3:
                    return ParseTriple(tokens[0], tokens[1], tokens[2]);
                case 4:
                    return ParseQuadruple(tokens[0], tokens[1], tokens[2], tokens[3]);
                default:
                    return null;
            }
        }

        private static ParsedPosition ParseSingle(Token token)
        {
            var keyword = KeywordOf(token);
            if (keyword == AxisKeyword.None)
            {
                if (!IsLengthOrPercent(token)) return null;

                var bare = new Component(AxisKeyword.None, token);
                var center = new Component(AxisKeyword.Center, null);
                return new ParsedPosition(bare, center, new[] { bare });
            }

            if (IsVerticalOnly(keyword))
            {
                var y = new Component(keyword, null);
                var x = new Component(AxisKeyword.Center, null);
                return new ParsedPosition(x, y, new[] { y });
            }

            var xComp = new Component(keyword, null);
            var yComp = new Component(AxisKeyword.Center, null);
            return new ParsedPosition(xComp, yComp, new[] { xComp });
        }

        private static ParsedPosition ParseDouble(Token t0, Token t1)
        {
            // "h then v" (horizontal-ish first): left/right/center/bare-length, then top/bottom/center/bare-length.
            if (IsHSlotValid(t0) && IsVSlotValid(t1))
            {
                var x = ToComponent(t0);
                var y = ToComponent(t1);
                return new ParsedPosition(x, y, new[] { x, y });
            }

            // "v then h" (vertical-ish first, e.g. "bottom center", "top left").
            if (IsVSlotValid(t0) && IsHSlotValid(t1))
            {
                var y = ToComponent(t0);
                var x = ToComponent(t1);
                return new ParsedPosition(x, y, new[] { y, x });
            }

            return null;
        }

        private static ParsedPosition ParseTriple(Token t0, Token t1, Token t2)
        {
            // hi, vi, offset (offset applies to the vertical group): e.g. "right bottom 20px".
            if (IsEdgeKeywordSlot(t0, horizontal: true) && IsEdgeKeywordSlot(t1, horizontal: false) && IsLengthOrPercent(t2))
            {
                var x = new Component(KeywordOf(t0), null);
                var y = new Component(KeywordOf(t1), t2);
                return new ParsedPosition(x, y, new[] { x, y });
            }

            // hi, offset, vi (offset applies to the horizontal group): e.g. "right 20px bottom".
            if (IsEdgeKeywordSlot(t0, horizontal: true) && IsLengthOrPercent(t1) && IsEdgeKeywordSlot(t2, horizontal: false))
            {
                var x = new Component(KeywordOf(t0), t1);
                var y = new Component(KeywordOf(t2), null);
                return new ParsedPosition(x, y, new[] { x, y });
            }

            return null;
        }

        private static ParsedPosition ParseQuadruple(Token t0, Token t1, Token t2, Token t3)
        {
            // hi, offset, vi, offset: e.g. "right 20px bottom 10px".
            if (IsEdgeKeywordSlot(t0, horizontal: true) && IsLengthOrPercent(t1) &&
                IsEdgeKeywordSlot(t2, horizontal: false) && IsLengthOrPercent(t3))
            {
                var x = new Component(KeywordOf(t0), t1);
                var y = new Component(KeywordOf(t2), t3);
                return new ParsedPosition(x, y, new[] { x, y });
            }

            return null;
        }

        private static Component ToComponent(Token token) =>
            new Component(KeywordOf(token), KeywordOf(token) == AxisKeyword.None ? token : null);

        private static AxisKeyword KeywordOf(Token token)
        {
            if (token.Type != TokenType.Ident) return AxisKeyword.None;
            if (token.Data.Isi(Keywords.Left)) return AxisKeyword.Left;
            if (token.Data.Isi(Keywords.Right)) return AxisKeyword.Right;
            if (token.Data.Isi(Keywords.Top)) return AxisKeyword.Top;
            if (token.Data.Isi(Keywords.Bottom)) return AxisKeyword.Bottom;
            if (token.Data.Isi(Keywords.Center)) return AxisKeyword.Center;
            return AxisKeyword.None;
        }

        private static bool IsHorizontalOnly(AxisKeyword k) => k == AxisKeyword.Left || k == AxisKeyword.Right;
        private static bool IsVerticalOnly(AxisKeyword k) => k == AxisKeyword.Top || k == AxisKeyword.Bottom;

        private static bool IsLengthOrPercent(Token token) =>
            Converters.LengthOrPercentConverter.Convert(new[] { token }) != null;

        /// <summary>Valid for the 2-value "h" slot: left/right/center keywords, or a bare length/percent - not top/bottom.</summary>
        private static bool IsHSlotValid(Token token)
        {
            var k = KeywordOf(token);
            return k != AxisKeyword.None ? !IsVerticalOnly(k) : IsLengthOrPercent(token);
        }

        /// <summary>Valid for the 2-value "v" slot: top/bottom/center keywords, or a bare length/percent - not left/right.</summary>
        private static bool IsVSlotValid(Token token)
        {
            var k = KeywordOf(token);
            return k != AxisKeyword.None ? !IsHorizontalOnly(k) : IsLengthOrPercent(token);
        }

        /// <summary>Valid for the 3/4-value "hi"/"vi" edge-keyword slots: keyword only, no bare-length fallback.</summary>
        private static bool IsEdgeKeywordSlot(Token token, bool horizontal)
        {
            var k = KeywordOf(token);
            if (k == AxisKeyword.Center) return true;
            if (k == AxisKeyword.None) return false;
            return horizontal ? IsHorizontalOnly(k) : IsVerticalOnly(k);
        }
    }
}
