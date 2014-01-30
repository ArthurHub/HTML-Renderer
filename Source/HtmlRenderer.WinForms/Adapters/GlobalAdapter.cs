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
using HtmlRenderer.Core;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Interfaces;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.WinForms.Utilities;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for general stuff for core.
    /// </summary>
    internal sealed class GlobalAdapter : GlobalBase, IGlobal
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
            FontsUtils.AddFontFamilyMapping("monospace", "Courier New");
            FontsUtils.AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in FontFamily.Families)
            {
                FontsUtils.AddFontFamily(new FontFamilyAdapter(family));
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
        public ColorInt ResolveColorFromName(string colorName)
        {
            var color = Color.FromName(colorName);
            return Utils.Convert(color);
        }

        /// <summary>
        /// Convert image object returned from <see cref="HtmlImageLoadEventArgs"/> to <see cref="IImage"/>.
        /// </summary>
        /// <param name="image">the image returned from load event</param>
        /// <returns>converted image or null</returns>
        public IImage ConvertImage(object image)
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
        /// 
        /// </summary>
        /// <param name="family"></param>
        /// <param name="size"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IFont CreateFont(string family, float size, FontStyleInt style)
        {
            var fontStyle = (FontStyle)( (int)style );
            return new FontAdapter(new Font(family, size, fontStyle));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="family"></param>
        /// <param name="size"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IFont CreateFont(IFontFamily family, float size, FontStyleInt style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(( (FontFamilyAdapter)family ).FontFamily, size, fontStyle));
        }

        /// <summary>
        /// Set the given text to the clipboard
        /// </summary>
        /// <param name="text">the text to set</param>
        public void SetToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        /// <summary>
        /// Copy the given html and plain text data to clipboard.
        /// </summary>
        /// <param name="html">the html data</param>
        /// <param name="plainText">the plain text data</param>
        public void SetToClipboard(string html, string plainText)
        {
            HtmlClipboardUtils.CopyToClipboard(html, plainText);
        }

        /// <summary>
        /// Set the given image to clipboard.
        /// </summary>
        /// <param name="image"></param>
        public void SetToClipboard(IImage image)
        {
            Clipboard.SetImage(((ImageAdapter)image).Image);
        }

        /// <summary>
        /// Create a context menu that can be used on the control
        /// </summary>
        /// <returns>new context menu</returns>
        public IContextMenu CreateContextMenu()
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
        public void SaveToFile(IImage image, string name, string extension, IControl control = null)
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
        /// Create a default CSS data object that will be cached.
        /// </summary>
        /// <returns></returns>
        protected override CssData CreateDefaultCssData()
        {
            return CssData.Parse(this, HtmlRendererUtils.DefaultStyleSheet, false);
        }
    }
}
