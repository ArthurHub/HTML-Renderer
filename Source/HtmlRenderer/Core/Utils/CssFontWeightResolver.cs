using System;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Resolves a box's <c>font-weight</c> to a number (CSS 2.1 §15.6).
    /// </summary>
    internal static class CssFontWeightResolver
    {
        internal const int Normal = 400;
        internal const int Bold = 700;

        /// <summary>
        /// Step of <c>bolder</c> and <c>lighter</c>, a fixed step instead of the table of CSS 2.1 §15.6.
        /// </summary>
        private const int RelativeStep = 300;

        internal static int Resolve(string fontWeightValue, int parentWeight)
        {
            if (string.IsNullOrWhiteSpace(fontWeightValue))
                return Normal;

            switch (fontWeightValue)
            {
                case CssConstants.Normal:
                case CssConstants.Inherit:
                    return Normal;
                case CssConstants.Bold:
                    return Bold;
                case CssConstants.Bolder:
                    return Clamp(parentWeight + RelativeStep);
                case CssConstants.Lighter:
                    return Clamp(parentWeight - RelativeStep);
                default:
                    return int.TryParse(fontWeightValue, out var numeric) ? Clamp(numeric) : Normal;
            }
        }

        private static int Clamp(int weight) => Math.Max(1, Math.Min(1000, weight));
    }
}
