using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The CSS calc() type-checking category a (sub-)expression resolves to. Flags so that
    /// "Length | Percentage" can represent the combined &lt;length-percentage&gt; type once a
    /// length and a percentage have been added/subtracted together.
    /// </summary>
    [Flags]
    internal enum CalcCategory
    {
        Number = 1,
        Length = 2,
        Percentage = 4,
        LengthPercentage = Length | Percentage,

        /// <summary>
        /// Unlike Length/Percentage, Angle never needs layout context to resolve (deg/grad/rad/turn all
        /// convert via fixed constants), so a valid Angle-category expression always folds fully at
        /// Layer A parse time - see CalcSerializer.
        /// </summary>
        Angle = 8
    }
}
