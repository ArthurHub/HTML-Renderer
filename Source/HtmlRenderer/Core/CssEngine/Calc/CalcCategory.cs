using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The type a calc() expression resolves to (CSS Values 4 §10.9). Flags, so a sum of a length and
    /// a percentage is Length | Percentage.
    /// </summary>
    [Flags]
    internal enum CalcCategory
    {
        Number = 1,
        Length = 2,
        Percentage = 4,
        LengthPercentage = Length | Percentage,
        Angle = 8,

        /// <summary>Only produced by @property syntax validation.</summary>
        Time = 16,

        /// <summary>Only produced by @property syntax validation.</summary>
        Resolution = 32
    }
}
