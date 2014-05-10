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
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using HtmlRenderer.Demo.Common;

namespace HtmlRenderer.Demo.WPF
{
    /// <summary>
    /// Interaction logic for DemoWindow.xaml
    /// </summary>
    public partial class DemoWindow
    {
        #region Fields/Consts

        /// <summary>
        /// the html samples used for performance testing
        /// </summary>
        private readonly List<string> _perfTestSamples = new List<string>();

        /// <summary>
        /// the private font used for the demo
        /// </summary>
        //private readonly PrivateFontCollection _privateFont = new PrivateFontCollection();

        #endregion

        public DemoWindow()
        {
            InitializeComponent();

            // Icon = GetIcon();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = SystemParameters.PrimaryScreenWidth * 0.7;
            Height = SystemParameters.PrimaryScreenHeight * 0.8;

            foreach (var sample in SamplesLoader.ShowcaseSamples)
            {
                _perfTestSamples.Add(sample.Html);
            }

            LoadCustomFonts();
        }

        /// <summary>
        /// Load custom fonts to be used by renderer HTMLs
        /// </summary>
        private void LoadCustomFonts()
        {
            //            // load custom font font into private fonts collection
            //            var file = Path.GetTempFileName();
            //            File.WriteAllBytes(file, Resources.CustomFont);
            //            _privateFont.AddFontFile(file);
            //
            //            // add the fonts to renderer
            //            foreach (var fontFamily in _privateFont.Families)
            //                HtmlRender.AddFontFamily(fontFamily);
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
        /// Execute performance test by setting all sample HTMLs in a loop.
        /// </summary>
        private void OnRunPerformance_Click(object sender, EventArgs e)
        {
            _mainControl.UpdateLock = true;
            _toolBar.IsEnabled = false;
            ApplicationDoEvents();

            GC.Collect();
#if NET_40
            AppDomain.MonitoringIsEnabled = true;
            var startMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
#endif
            var sw = Stopwatch.StartNew();

            const int iterations = 20;
            for (int i = 0; i < iterations; i++)
            {
                foreach (var html in _perfTestSamples)
                {
                    _mainControl.SetHtml(html);
                    ApplicationDoEvents(); // so paint will be called
                }
            }

            sw.Stop();

            long endMemory = 0;
            float totalMem = 0;
#if NET_40
            endMemory = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
            totalMem = (endMemory - startMemory) / 1024f;
#endif
            float htmlSize = 0;
            foreach (var sample in _perfTestSamples)
                htmlSize += sample.Length * 2;
            htmlSize = htmlSize / 1024f;


            var msg = string.Format("{0} HTMLs ({1:N0} KB)\r\n{2} Iterations", _perfTestSamples.Count, htmlSize, iterations);
            msg += "\r\n\r\n";
            msg += string.Format("CPU:\r\nTotal: {0} msec\r\nIterationAvg: {1:N2} msec\r\nSingleAvg: {2:N2} msec",
                sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / (double)iterations, sw.ElapsedMilliseconds / (double)iterations / _perfTestSamples.Count);
            msg += "\r\n\r\n";
            msg += string.Format("Memory:\r\nTotal: {0:N0} KB\r\nIterationAvg: {1:N0} KB\r\nSingleAvg: {2:N0} KB\r\nOverhead: {3:N0}%",
                totalMem, totalMem / iterations, totalMem / iterations / _perfTestSamples.Count, 100 * (totalMem / iterations) / htmlSize);

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