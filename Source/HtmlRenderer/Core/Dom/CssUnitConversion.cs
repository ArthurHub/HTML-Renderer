using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Core.Dom
{
    public class CssUnitConversion
    {
        /// <summary>
        /// TODO: fully implement, and use in the core as well.
        /// </summary>
        /// <remarks>
        /// Some extra thought needed here as conversion from size to px needs to know DPI.
        /// </remarks>
        public static T Convert<T>(T value, CssUnit from, CssUnit to)
            where T : IFloatingPoint<T>
        {
            var valueInMm = from switch
            {
                CssUnit.Milimeters => value,
                CssUnit.Centimeters => value * (T)T.CreateChecked(10),
                CssUnit.Inches => value * T.CreateChecked(2.54) * T.CreateChecked(10),
                _ => throw new NotImplementedException(),
            };
            var valueInDest = to switch
            {
                CssUnit.Centimeters => valueInMm * T.CreateChecked(10),
                CssUnit.Milimeters => valueInMm,
                CssUnit.Inches => valueInMm / T.CreateChecked(10) / T.CreateChecked(2.54),
                _ => throw new NotImplementedException(),
            };
            return valueInDest;
        }
    }
}
