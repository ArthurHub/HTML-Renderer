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
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WPF;
using Microsoft.Win32;

namespace TheArtOfDev.HtmlRenderer.Demo.WPF
{
    /// <summary>
    /// Interaction logic for GenerateImageWindow.xaml
    /// </summary>
    public partial class GenerateImageWindow
    {
        private readonly string _html;
        private BitmapFrame _generatedImage;

        public GenerateImageWindow(string html)
        {
            _html = html;

            InitializeComponent();

            Loaded += (sender, args) => GenerateImage();
        }

        private void OnSaveToFile_click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Images|*.png;*.bmp;*.jpg;*.tif;*.gif;*.wmp;";
            saveDialog.FileName = "image";
            saveDialog.DefaultExt = ".png";

            var dialogResult = saveDialog.ShowDialog(this);
            if (dialogResult.GetValueOrDefault())
            {
                var encoder = HtmlRenderingHelper.GetBitmapEncoder(Path.GetExtension(saveDialog.FileName));
                encoder.Frames.Add(_generatedImage);
                using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.OpenOrCreate))
                    encoder.Save(stream);
            }
        }

        private void OnGenerateImage_Click(object sender, RoutedEventArgs e)
        {
            GenerateImage();
        }

        private void GenerateImage()
        {
            if (_imageBoxBorder.RenderSize.Width > 0 && _imageBoxBorder.RenderSize.Height > 0)
            {
                _generatedImage = HtmlRender.RenderToImage(_html, _imageBoxBorder.RenderSize, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);
                _imageBox.Source = _generatedImage;
            }
        }
    }
}