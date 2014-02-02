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
    /// </summary>
    internal sealed class GlobalAdapter : GlobalBase
    {
        #region Fields and Consts

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly GlobalAdapter _instance = new GlobalAdapter();

        #endregion


        /// <summary>
        /// Init color resolve.
        /// </summary>
        private GlobalAdapter()
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
        public static GlobalAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public override RColor GetColor(string colorName)
        {
            var color = Color.FromName(colorName);
            return Utils.Convert(color);
        }

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public override IImage ConvertImage(object image)
        {
            return image != null ? new ImageAdapter((Image)image) : null;
        }

        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public override IImage ImageFromStream(Stream memoryStream)
        {
            return new ImageAdapter(Image.FromStream(memoryStream));
        }

        /// <summary>
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        public override void SetToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        /// <summary>
        /// Copy the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        public override void SetToClipboard(string html, string plainText)
        {
            HtmlClipboardUtils.CopyToClipboard(html, plainText);
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        public override void SetToClipboard(IImage image)
        {
            Clipboard.SetImage(((ImageAdapter)image).Image);
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        public override IContextMenu CreateContextMenu()
        {
            return new ContextMenuAdapter();
        }

        /// <summary>
        /// Save the given image to file by showing save dialog to the client.
        /// </summary>
        /// <param name="image">the image to save</param>
        /// <param name="name">the name of the image for save dialog</param>
        /// <param name="extension">the extension of the image for save dialog</param>
        /// <param name="control">optional: the control to show the dialog on</param>
        public override void SaveToFile(IImage image, string name, string extension, IControl control = null)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";
                saveDialog.FileName = name;
                saveDialog.DefaultExt = extension;

                var dialogResult = control == null ? saveDialog.ShowDialog() : saveDialog.ShowDialog(( (ControlAdapter)control ).Control);
                if (dialogResult == DialogResult.OK)
                {
                    ((ImageAdapter)image).Image.Save(saveDialog.FileName);
                }
            }
        }

        /// <summary>
        /// Get font instance by given font family name, size and style.
        /// </summary>
        /// <param name="family">the font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected internal override IFont CreateFont(string family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(family, (float)size, fontStyle));
        }

        /// <summary>
        /// Get font instance by given font family instance, size and style.<br/>
        /// Used to support custom fonts that require explicit font family instance to be created.
        /// </summary>
        /// <param name="family">the font family instance</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <returns>font instance</returns>
        protected internal override IFont CreateFont(IFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(( (FontFamilyAdapter)family ).FontFamily, (float)size, fontStyle));
        }
    }
}
