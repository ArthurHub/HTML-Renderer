using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The CSS calc() type-checking category a (sub-)expression resolves to. A flags enum so that
    /// <c>Length | Percentage</c> can represent the combined <c>&lt;length-percentage&gt;</c> type once a
    /// length and a percentage have been added or subtracted together.
    /// </summary>
    [Flags]
    internal enum CalcCategory
    {
        Number = 1,
        Length = 2,
        Percentage = 4,
        LengthPercentage = Length | Percentage,
        Angle = 8,
        Time = 16,
        Resolution = 32
    }
}
