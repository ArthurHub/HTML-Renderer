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

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.WPF.Utilities;

namespace HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for general stuff for core.
    /// TODO:a add doc.
    /// </summary>
    internal sealed class WpfAdapter : Adapter
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly WpfAdapter _instance = new WpfAdapter();

        #endregion


        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private WpfAdapter()
        {
            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in Fonts.SystemFontFamilies)
            {
                AddFontFamily(new FontFamilyAdapter(family));
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

        protected internal override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString(family) ?? new FontFamily();
            return new FontAdapter(new Typeface(fontFamily, GetFontStyle(style), GetFontWidth(style), FontStretches.Normal), size);
        }

        protected internal override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            return new FontAdapter(new Typeface(((FontFamilyAdapter)family).FontFamily, GetFontStyle(style), GetFontWidth(style), FontStretches.Normal), size);
        }

        protected override void SetToClipboardInt(string text)
        {
            Clipboard.SetText(text);
        }

        protected override void SetToClipboardInt(string html, string plainText)
        {
            // TODO:a handle WPF clipboard
            //            ClipboardHelper.CopyToClipboard(html, plainText);
        }

        protected override void SetToClipboardInt(RImage image)
        {
            // TODO:a handle WPF clipboard
            //            Clipboard.SetImage(((ImageAdapter)image).Image);
        }

        protected override RContextMenu CreateContextMenuInt()
        {
            return new ContextMenuAdapter();
        }

        protected override void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            // TODO:a handle save to file
            //            using (var saveDialog = new SaveFileDialog())
            //            {
            //                saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";
            //                saveDialog.FileName = name;
            //                saveDialog.DefaultExt = extension;
            //
            //                var dialogResult = control == null ? saveDialog.ShowDialog() : saveDialog.ShowDialog(((ControlAdapter)control).Control);
            //                if (dialogResult == DialogResult.OK)
            //                {
            //                    ((ImageAdapter)image).Image.Save(saveDialog.FileName);
            //                }
            //            }
        }

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
    }
}