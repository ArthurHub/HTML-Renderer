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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.WPF.Utilities;
using Microsoft.Win32;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF platform.
    /// </summary>
    internal sealed class WpfAdapter : RAdapter
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly WpfAdapter _instance = new WpfAdapter();

        /// <summary>
        /// List of valid predefined color names in lower-case
        /// </summary>
        private static readonly List<string> ValidColorNamesLc; 

        #endregion

        static WpfAdapter()
        {
            ValidColorNamesLc = new List<string>();
            var colorList = new List<PropertyInfo>(typeof(Colors).GetProperties());
            foreach (var colorProp in colorList)
            {
                ValidColorNamesLc.Add(colorProp.Name.ToLower());
            }
        }

        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private WpfAdapter()
        {
            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in Fonts.SystemFontFamilies)
            {
	            try
	            {
	                AddFontFamily(new FontFamilyAdapter(family));
	            }
	            catch
	            {
	            }
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static WpfAdapter Instance
        {
            get { return _instance; }
        }

        protected override RColor GetColorInt(string colorName)
        {
            // check if color name is valid to avoid ColorConverter throwing an exception
            if (!ValidColorNamesLc.Contains(colorName.ToLower()))
                return RColor.Empty;

            var convertFromString = ColorConverter.ConvertFromString(colorName) ?? Colors.Black;
            return Utils.Convert((Color)convertFromString);
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(GetSolidColorBrush(color));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            var solidBrush = GetSolidColorBrush(color);
            return new BrushAdapter(solidBrush);
        }

        protected override RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            var startColor = angle <= 180 ? Utils.Convert(color1) : Utils.Convert(color2);
            var endColor = angle <= 180 ? Utils.Convert(color2) : Utils.Convert(color1);
            angle = angle <= 180 ? angle : angle - 180;
            double x = angle < 135 ? Math.Max((angle - 45) / 90, 0) : 1;
            double y = angle <= 45 ? Math.Max(0.5 - angle / 90, 0) : angle > 135 ? Math.Abs(1.5 - angle / 90) : 0;
            return new BrushAdapter(new LinearGradientBrush(startColor, endColor, new Point(x, y), new Point(1 - x, 1 - y)));
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((BitmapImage)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return new ImageAdapter(bitmap);
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString(family) ?? new FontFamily();
            return new FontAdapter(new Typeface(fontFamily, GetFontStyle(style), GetFontWidth(style), FontStretches.Normal), size);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            return new FontAdapter(new Typeface(((FontFamilyAdapter)family).FontFamily, GetFontStyle(style), GetFontWidth(style), FontStretches.Normal), size);
        }

        protected override object GetClipboardDataObjectInt(string html, string plainText)
        {
            return ClipboardHelper.CreateDataObject(html, plainText);
        }

        protected override void SetToClipboardInt(string text)
        {
            ClipboardHelper.CopyToClipboard(text);
        }

        protected override void SetToClipboardInt(string html, string plainText)
        {
            ClipboardHelper.CopyToClipboard(html, plainText);
        }

        protected override void SetToClipboardInt(RImage image)
        {
            Clipboard.SetImage(((ImageAdapter)image).Image);
        }

        protected override RContextMenu CreateContextMenuInt()
        {
            return new ContextMenuAdapter();
        }

        protected override void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Images|*.png;*.bmp;*.jpg;*.tif;*.gif;*.wmp;";
            saveDialog.FileName = name;
            saveDialog.DefaultExt = extension;

            var dialogResult = saveDialog.ShowDialog();
            if (dialogResult.GetValueOrDefault())
            {
                var encoder = Utils.GetBitmapEncoder(Path.GetExtension(saveDialog.FileName));
                encoder.Frames.Add(BitmapFrame.Create(((ImageAdapter)image).Image));
                using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.OpenOrCreate))
                    encoder.Save(stream);
            }
        }


        #region Private/Protected methods

        /// <summary>
        /// Get solid color brush for the given color.
        /// </summary>
        private static Brush GetSolidColorBrush(RColor color)
        {
            Brush solidBrush;
            if (color == RColor.White)
                solidBrush = Brushes.White;
            else if (color == RColor.Black)
                solidBrush = Brushes.Black;
            else if (color.A < 1)
                solidBrush = Brushes.Transparent;
            else
                solidBrush = new SolidColorBrush(Utils.Convert(color));
            return solidBrush;
        }

        /// <summary>
        /// Get WPF font style for the given style.
        /// </summary>
        private static FontStyle GetFontStyle(RFontStyle style)
        {
            switch (style)
            {
                case RFontStyle.Italic:
                    return FontStyles.Italic;
                default:
                    return FontStyles.Normal;
            }
        }

        /// <summary>
        /// Get WPF font style for the given style.
        /// </summary>
        private static FontWeight GetFontWidth(RFontStyle style)
        {
            switch (style)
            {
                case RFontStyle.Bold:
                    return FontWeights.Bold;
                default:
                    return FontWeights.Normal;
            }
        }

        #endregion
    }
}