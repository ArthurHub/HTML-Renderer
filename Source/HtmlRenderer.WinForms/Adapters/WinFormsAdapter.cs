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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms platforms.
    /// </summary>
    internal sealed class WinFormsAdapter : RAdapter
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly WinFormsAdapter _instance = new WinFormsAdapter();

        #endregion


        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private WinFormsAdapter()
        {
            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in FontFamily.Families)
            {
                AddFontFamily(new FontFamilyAdapter(family));
            }
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static WinFormsAdapter Instance
        {
            get { return _instance; }
        }

        protected override RColor GetColorInt(string colorName)
        {
            var color = Color.FromName(colorName);
            return Utils.Convert(color);
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(new Pen(Utils.Convert(color)));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            Brush solidBrush;
            if (color == RColor.White)
                solidBrush = Brushes.White;
            else if (color == RColor.Black)
                solidBrush = Brushes.Black;
            else if (color.A < 1)
                solidBrush = Brushes.Transparent;
            else
                solidBrush = new SolidBrush(Utils.Convert(color));

            return new BrushAdapter(solidBrush, false);
        }

        protected override RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return new BrushAdapter(new LinearGradientBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), (float)angle), true);
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((Image)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(Image.FromStream(memoryStream));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(family, (float)size, fontStyle));
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(((FontFamilyAdapter)family).FontFamily, (float)size, fontStyle));
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
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";
                saveDialog.FileName = name;
                saveDialog.DefaultExt = extension;

                var dialogResult = control == null ? saveDialog.ShowDialog() : saveDialog.ShowDialog(((ControlAdapter)control).Control);
                if (dialogResult == DialogResult.OK)
                {
                    ((ImageAdapter)image).Image.Save(saveDialog.FileName);
                }
            }
        }
    }
}