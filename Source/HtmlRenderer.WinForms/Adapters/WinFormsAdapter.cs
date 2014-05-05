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
using System.IO;
using System.Windows.Forms;
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;
using HtmlRenderer.WinForms.Utilities;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for general stuff for core.
    /// TODO:a add doc.
    /// </summary>
    internal sealed class WinFormsAdapter : AdapterBase
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

        protected override IPen CreatePen(RColor color)
        {
            return new PenAdapter(new Pen(Utils.Convert(color)));
        }

        protected override IBrush CreateSolidBrush(RColor color)
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

        protected override IImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((Image)image) : null;
        }

        protected override IImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(Image.FromStream(memoryStream));
        }

        protected internal override IFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(family, (float)size, fontStyle));
        }

        protected internal override IFont CreateFontInt(IFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(((FontFamilyAdapter)family).FontFamily, (float)size, fontStyle));
        }

        protected override void SetToClipboardInt(string text)
        {
            Clipboard.SetText(text);
        }

        protected override void SetToClipboardInt(string html, string plainText)
        {
            ClipboardHelper.CopyToClipboard(html, plainText);
        }

        protected override void SetToClipboardInt(IImage image)
        {
            Clipboard.SetImage(((ImageAdapter)image).Image);
        }

        protected override IContextMenu CreateContextMenuInt()
        {
            return new ContextMenuAdapter();
        }

        protected override void SaveToFileInt(IImage image, string name, string extension, IControl control = null)
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