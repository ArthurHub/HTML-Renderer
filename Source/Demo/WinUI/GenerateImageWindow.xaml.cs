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
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WinUI;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    public sealed partial class GenerateImageWindow : Window
    {
        private readonly string _html;
        private CanvasRenderTarget _generatedImage;

        public GenerateImageWindow(string html)
        {
            _html = html;

            InitializeComponent();

            WindowHelper.Setup(this, "Generate Image", 0.56, 0.64);

            _imageBoxBorder.SizeChanged += OnFirstSizeChanged;
            Closed += (sender, args) => _generatedImage?.Dispose();
        }

        private async void OnFirstSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _imageBoxBorder.SizeChanged -= OnFirstSizeChanged;
            await GenerateImage();
        }

        private async void OnGenerateImage_Click(object sender, RoutedEventArgs e)
        {
            await GenerateImage();
        }

        private async void OnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (_generatedImage == null)
                return;

            try
            {
                var picker = WindowHelper.CreateSavePicker(this);
                picker.SuggestedFileName = "image";
                picker.DefaultFileExtension = ".png";
                picker.FileTypeChoices.Add("Images", new List<string> { ".png", ".bmp", ".jpg", ".tif", ".gif" });

                var file = await picker.PickSaveFileAsync();
                if (file == null)
                    return;

                using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                stream.Size = 0;
                await _generatedImage.SaveAsync(stream, GetBitmapFileFormat(Path.GetExtension(file.Path)));
            }
            catch (Exception ex)
            {
                await WindowHelper.ShowMessageAsync(Content.XamlRoot, ex.ToString(), "Failed to save the image");
            }
        }

        private async Task GenerateImage()
        {
            var size = new Size(_imageBoxBorder.ActualWidth, _imageBoxBorder.ActualHeight);
            if (size.Width > 0 && size.Height > 0)
            {
                try
                {
                    _generatedImage?.Dispose();
                    _generatedImage = await HtmlRender.RenderToImageAsync(_html, size, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoad);

                    using var stream = new InMemoryRandomAccessStream();
                    await _generatedImage.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                    stream.Seek(0);

                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    _imageBox.Source = bitmap;
                }
                catch (Exception ex)
                {
                    _generatedImage = null;
                    await WindowHelper.ShowMessageAsync(Content.XamlRoot, ex.ToString(), "Failed to generate the image");
                }
            }
        }

        /// <summary>
        /// Get the Win2D format to encode the image with for the given file extension, default is PNG.
        /// </summary>
        private static CanvasBitmapFileFormat GetBitmapFileFormat(string ext)
        {
            switch (ext.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return CanvasBitmapFileFormat.Jpeg;
                case ".bmp":
                    return CanvasBitmapFileFormat.Bmp;
                case ".tif":
                case ".tiff":
                    return CanvasBitmapFileFormat.Tiff;
                case ".gif":
                    return CanvasBitmapFileFormat.Gif;
                default:
                    return CanvasBitmapFileFormat.Png;
            }
        }
    }
}
