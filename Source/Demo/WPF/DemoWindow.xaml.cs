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
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WPF;

namespace TheArtOfDev.HtmlRenderer.Demo.WPF
{
    /// <summary>
    /// Interaction logic for DemoWindow.xaml
    /// </summary>
    public partial class DemoWindow
    {
        #region Fields/Consts

        /// <summary>
        /// the private font used for the demo
        /// </summary>
        //private readonly PrivateFontCollection _privateFont = new PrivateFontCollection();

        #endregion
        public DemoWindow()
        {
            SamplesLoader.Init("WPF", typeof(HtmlRender).Assembly.GetName().Version.ToString());

            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = SystemParameters.PrimaryScreenWidth * 0.7;
            Height = SystemParameters.PrimaryScreenHeight * 0.8;

            LoadCustomFonts();
        }

        /// <summary>
        /// Load custom fonts to be used by renderer HTMLs
        /// </summary>
        private static void LoadCustomFonts()
        {
            // load custom font font into private fonts collection
            foreach (FontFamily fontFamily in Fonts.GetFontFamilies(new Uri("pack://application:,,,/"), "./fonts/"))
            {
                // add the fonts to renderer
                HtmlRender.AddFontFamily(fontFamily);
            }
        }

        /// <summary>
        /// Open sample window.
        /// </summary>
        private void OnOpenSampleWindow_click(object sender, RoutedEventArgs e)
        {
            var w = new SampleWindow();
            w.Owner = this;
            w.Width = Width * 0.8;
            w.Height = Height * 0.8;
            w.ShowDialog();
        }

        /// <summary>
        /// Toggle if to show split view of HtmlPanel and WinForms WebBrowser control.
        /// </summary>
        private void OnShowIEView_ButtonClick(object sender, EventArgs e)
        {
            _mainControl.ShowWebBrowserView(_showIEView.IsChecked.GetValueOrDefault(false));
        }

        /// <summary>
        /// Open the current html is external process - the default user browser.
        /// </summary>
        private void OnOpenInExternalView_Click(object sender, EventArgs e)
        {
            var tmpFile = Path.ChangeExtension(Path.GetTempFileName(), ".htm");
            File.WriteAllText(tmpFile, _mainControl.GetHtml());
            Process.Start(tmpFile);
        }

        /// <summary>
        /// Toggle the use generated html button state.
        /// </summary>
        private void OnUseGeneratedHtml_Click(object sender, EventArgs e)
        {
            _mainControl.UseGeneratedHtml = _useGeneratedHtml.IsChecked.GetValueOrDefault(false);
            _mainControl.UpdateWebBrowserHtml();
        }

        /// <summary>
        /// Open generate image window for the current html.
        /// </summary>
        private void OnGenerateImage_Click(object sender, RoutedEventArgs e)
        {
            var w = new GenerateImageWindow(_mainControl.GetHtml());
            w.Owner = this;
            w.Width = Width * 0.8;
            w.Height = Height * 0.8;
            w.ShowDialog();
        }

        /// <summary>
        /// Execute performance test by setting all sample HTMLs in a loop.
        /// </summary>
        private void OnRunPerformance_Click(object sender, EventArgs e)
        {
            _mainControl.UpdateLock = true;
            _toolBar.IsEnabled = false;
            ApplicationDoEvents();

            var msg = DemoUtils.RunSamplesPerformanceTest(html =>
            {
                _mainControl.SetHtml(html);
                ApplicationDoEvents(); // so paint will be called
            });

            Clipboard.SetDataObject(msg);
            MessageBox.Show(msg, "Test run results");

            _mainControl.UpdateLock = false;
            _toolBar.IsEnabled = true;
        }

        private static void ApplicationDoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action<bool>(delegate { }), false);
        }
    }
}