using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The grammars of the scroll-driven animation properties (Scroll-driven Animations 1 2-4, CSS Animations 2 3.7),
    /// each validating one comma-separated list entry given as its whitespace-free tokens.
    /// </summary>
    internal static class TimelineGrammar
    {
        private static readonly string[] Axes = {"block", "inline", "x", "y"};
        private static readonly string[] Scrollers = {"root", "nearest", "self"};

        private static readonly string[] RangeNames =
            {"cover", "contain", "entry", "exit", "entry-crossing", "exit-crossing"};

        /// <summary>One entry of a list value, without its whitespace.</summary>
        internal static Token[] Compact(IEnumerable<Token> tokens)
        {
            return tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();
        }

        internal static bool IsKeyword(Token token, params string[] keywords)
        {
            return token.Type == TokenType.Ident && keywords.Any(k => token.Data.Isi(k));
        }

        internal static bool IsAxis(Token token) => IsKeyword(token, Axes);

        internal static bool IsDashedIdent(Token token)
        {
            return token.Type == TokenType.Ident && token.Data.Length > 2 &&
                   token.Data.StartsWith("--", StringComparison.Ordinal);
        }

        internal static bool IsRangeName(Token token) => IsKeyword(token, RangeNames);

        internal static bool IsLengthPercentage(Token token)
        {
            return Converters.LengthOrPercentConverter.Convert(new[] {token}) != null;
        }

        /// <summary><c>normal | allow-discrete</c>.</summary>
        internal static bool IsTransitionBehavior(Token[] entry) =>
            entry.Length == 1 && IsKeyword(entry[0], Keywords.Normal, "allow-discrete");

        /// <summary><c>replace | add | accumulate</c>.</summary>
        internal static bool IsAnimationComposition(Token[] entry) =>
            entry.Length == 1 && IsKeyword(entry[0], Keywords.Replace, "add", "accumulate");

        /// <summary><c>none | &lt;dashed-ident&gt;</c>: one entry of <c>scroll-timeline-name</c> / <c>view-timeline-name</c>.</summary>
        internal static bool IsTimelineName(Token[] entry) =>
            entry.Length == 1 && (IsKeyword(entry[0], Keywords.None) || IsDashedIdent(entry[0]));

        /// <summary><c>none | all | &lt;dashed-ident&gt;</c>: one entry of <c>timeline-scope</c>.</summary>
        internal static bool IsTimelineScopeEntry(Token[] entry) =>
            IsTimelineName(entry) || entry.Length == 1 && IsKeyword(entry[0], Keywords.All);

        /// <summary>A single <c>&lt;dashed-ident&gt;</c>.</summary>
        internal static bool IsDashedIdentEntry(Token[] entry) => entry.Length == 1 && IsDashedIdent(entry[0]);

        /// <summary><c>block | inline | x | y</c>.</summary>
        internal static bool IsAxisEntry(Token[] entry) => entry.Length == 1 && IsAxis(entry[0]);

        /// <summary><c>[ auto | &lt;length-percentage&gt; ]{1,2}</c>.</summary>
        internal static bool IsInset(Token[] entry) => entry.Length is 1 or 2 && entry.All(IsInsetPart);

        /// <summary>
        /// <c>auto | none | &lt;dashed-ident&gt; | scroll( [ &lt;scroller&gt; || &lt;axis&gt; ]? ) | view( [ &lt;axis&gt; || &lt;'view-timeline-inset'&gt; ]? )</c>.
        /// </summary>
        internal static bool IsAnimationTimeline(Token[] entry)
        {
            if (entry.Length != 1) return false;

            var token = entry[0];

            if (IsKeyword(token, Keywords.Auto, Keywords.None) || IsDashedIdent(token)) return true;

            return IsTimelineFunction(token);
        }

        /// <summary>The <c>&lt;single-animation-timeline&gt;</c> forms other than <c>none</c>, which the <c>animation</c> shorthand reads as the animation name.</summary>
        internal static bool IsShorthandTimeline(Token[] entry)
        {
            return entry.Length == 1 && !IsKeyword(entry[0], Keywords.None) && IsAnimationTimeline(entry);
        }

        /// <summary>One end of an animation range: <c>normal | &lt;length-percentage&gt; | &lt;timeline-range-name&gt; &lt;length-percentage&gt;?</c>.</summary>
        internal static bool IsRangeEdge(Token[] entry)
        {
            var index = 0;
            return TryConsumeRangeEdge(entry, ref index) && index == entry.Length;
        }

        /// <summary>
        /// One <c>animation-range</c> entry: <c>&lt;'animation-range-start'&gt; &lt;'animation-range-end'&gt;?</c>. The tokens of each
        /// part are returned; the end is empty when omitted.
        /// </summary>
        internal static bool TryParseAnimationRange(Token[] entry, out Token[] start, out Token[] end)
        {
            start = end = null;
            var index = 0;

            if (!TryConsumeRangeEdge(entry, ref index)) return false;

            var split = index;
            if (split < entry.Length && !TryConsumeRangeEdge(entry, ref index)) return false;

            if (index != entry.Length) return false;

            start = entry.Take(split).ToArray();
            end = entry.Skip(split).ToArray();
            return true;
        }

        private static bool TryConsumeRangeEdge(Token[] entry, ref int index)
        {
            if (index >= entry.Length) return false;

            var token = entry[index];

            if (IsKeyword(token, Keywords.Normal) || IsLengthPercentage(token))
            {
                index++;
                return true;
            }

            if (!IsRangeName(token)) return false;

            index++;
            if (index < entry.Length && IsLengthPercentage(entry[index])) index++;
            return true;
        }

        private static bool IsInsetPart(Token token) => IsKeyword(token, Keywords.Auto) || IsLengthPercentage(token);

        private static bool IsTimelineFunction(Token token)
        {
            if (!(token is FunctionToken function)) return false;

            var arguments = Compact(function.ArgumentTokens);

            if (function.Data.Isi("scroll")) return IsScrollArguments(arguments);
            return function.Data.Isi("view") && IsViewArguments(arguments);
        }

        // [ <scroller> || <axis> ]?
        private static bool IsScrollArguments(Token[] arguments)
        {
            if (arguments.Length > 2) return false;

            var scrollers = arguments.Count(a => IsKeyword(a, Scrollers));
            var axes = arguments.Count(IsAxis);

            return scrollers <= 1 && axes <= 1 && scrollers + axes == arguments.Length;
        }

        // [ <axis> || <'view-timeline-inset'> ]?
        private static bool IsViewArguments(Token[] arguments)
        {
            if (arguments.Length == 0) return true;

            var axes = arguments.Count(IsAxis);
            if (axes > 1) return false;

            if (axes == 0) return IsInset(arguments);

            // The axis is a whole component of the "||", so it cannot sit between two inset values.
            var rest = arguments.Where(a => !IsAxis(a)).ToArray();
            var axisIsAtEdge = IsAxis(arguments[0]) || IsAxis(arguments[arguments.Length - 1]);

            return axisIsAtEdge && (rest.Length == 0 || IsInset(rest));
        }
    }
}
