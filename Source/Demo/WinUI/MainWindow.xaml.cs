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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Windows.ApplicationModel.DataTransfer;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WindowHelper.Setup(this, "HTML Renderer WinUI Demo", 0.7, 0.8);
        }

        /// <summary>
        /// Open sample window.
        /// </summary>
        private void OnOpenSampleWindow_Click(object sender, RoutedEventArgs e)
        {
            new SampleWindow().Activate();
        }

        /// <summary>
        /// Toggle if to show split view of HtmlPanel and WebView2 control.
        /// </summary>
        private void OnShowBrowserView_Click(object sender, RoutedEventArgs e)
        {
            _mainControl.ShowWebBrowserView(_showBrowserView.IsChecked == true);
        }

        /// <summary>
        /// Open the current html in the default browser.
        /// </summary>
        private void OnOpenExternal_Click(object sender, RoutedEventArgs e)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".htm");
            File.WriteAllText(tmpFile, _mainControl.GetHtml());
            Process.Start(new ProcessStartInfo(tmpFile) { UseShellExecute = true });
        }

        /// <summary>
        /// Toggle the use generated html button state.
        /// </summary>
        private void OnUseGeneratedHtml_Click(object sender, RoutedEventArgs e)
        {
            _mainControl.UseGeneratedHtml = _useGeneratedHtml.IsChecked == true;
            _mainControl.UpdateWebBrowserHtml();
        }

        /// <summary>
        /// Open generate image window for the current html.
        /// </summary>
        private void OnGenerateImage_Click(object sender, RoutedEventArgs e)
        {
            new GenerateImageWindow(_mainControl.GetHtml()).Activate();
        }

        /// <summary>
        /// Create PDF using PdfSharp project, save to file and open that file.
        /// </summary>
        private async void OnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = new PdfGenerateConfig();
                config.PageSize = PageSize.A4;
                config.SetMargins(20);

                var html = _mainControl.GetHtml();

                // Off the UI thread, as the image conversion blocks on Win2D
                var pdfFile = await Task.Run(async () =>
                {
                    var doc = await PdfGenerator.GeneratePdf(html, config, null, DemoUtils.OnStylesheetLoad, HtmlRenderingHelper.OnImageLoadPdfSharp);
                    var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
                    doc.Save(file);
                    return file;
                });

                Process.Start(new ProcessStartInfo(pdfFile) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                await WindowHelper.ShowMessageAsync(Content.XamlRoot, ex.ToString(), "Failed to generate PDF");
            }
        }

        /// <summary>
        /// Execute performance test by setting all sample HTMLs in a loop.
        /// </summary>
        private async void OnRunPerformance_Click(object sender, RoutedEventArgs e)
        {
            _mainControl.UpdateLock = true;
            _toolBar.IsEnabled = false;

            // There's no paint in between, unlike WinForms and WPF, WinUI has no way to pump the messages
            var msg = DemoUtils.RunSamplesPerformanceTest(_mainControl.SetHtml);

            var dataPackage = new DataPackage();
            dataPackage.SetText(msg);
            Clipboard.SetContent(dataPackage);

            await WindowHelper.ShowMessageAsync(Content.XamlRoot, msg, "Test run results");

            _mainControl.UpdateLock = false;
            _toolBar.IsEnabled = true;
        }
    }
}
