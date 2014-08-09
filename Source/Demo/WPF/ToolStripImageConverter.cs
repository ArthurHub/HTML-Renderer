// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Globalization;
using System.Windows.Data;
using TheArtOfDev.HtmlRenderer.Demo.Common;

namespace TheArtOfDev.HtmlRenderer.Demo.WPF
{
    public class ToolStripImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageStream = typeof(Resources).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Demo.Common.Resources." + parameter + ".png");
            return HtmlRenderingHelper.ImageFromStream(imageStream);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}