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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HtmlRenderer.Demo.Common;
using HtmlRenderer.PdfSharp;
using PdfSharp;

namespace HtmlRenderer.Demo.WinForms
{
    public partial class DemoForm : Form
    {
        #region Fields/Consts

        /// <summary>
        /// the html samples used for performance testing
        /// </summary>
        private readonly List<string> _perfTestSamples = new List<string>();

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public DemoForm()
        {
            InitializeComponent();

            Icon = GetIcon();

            StartPosition = FormStartPosition.CenterScreen;
            var size = Screen.GetWorkingArea(Point.Empty);
            Size = new Size((int)(size.Width * 0.7), (int)(size.Height * 0.8));

            foreach (var sample in SamplesLoader.ShowcaseSamples)
            {
                _perfTestSamples.Add(sample.Html);
            }
        }

        /// <summary>
        /// Get icon for the demo.
        /// </summary>
        internal static Icon GetIcon()
        {
            var stream = typeof(DemoForm).Assembly.GetManifestResourceStream("HtmlRenderer.Demo.WinForms.html.ico");
            return stream != null ? new Icon(stream) : null;
        }

        private void OnOpenSampleForm_Click(object sender, EventArgs e)
        {
            using (var f = new SampleForm())
            {
                f.ShowDialog();
            }
        }

        private void OnShowIEView_ButtonClick(object sender, EventArgs e)
        {
            _showIEViewTSSB.Checked = !_showIEViewTSSB.Checked;
            _mainControl.ShowWebBrowserView(_showIEViewTSSB.Checked);
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
            _useGeneratedHtmlTSB.Checked = !_useGeneratedHtmlTSB.Checked;
            _mainControl.UseGeneratedHtml = _useGeneratedHtmlTSB.Checked;
            _mainControl.UpdateWebBrowserHtml();
        }

        /// <summary>
        /// Create PDF using PdfSharp project, save to file and open that file.
        /// </summary>
        private void OnGeneratePdf_Click(object sender, EventArgs e)
        {
            var doc = PdfGenerator.GeneratePdf(_mainControl.GetHtml(), PageSize.A4);
            var tmpFile = Path.GetTempFileName();
            tmpFile = Path.GetFileNameWithoutExtension(tmpFile) + ".pdf";
            doc.Save(tmpFile);
            Process.Start(tmpFile);
        }

        /// <summary>
        /// Execute performance test by setting all sample htmls in a loop.
        /// </summary>
        private void OnRunPerformance_Click(object sender, EventArgs e)
        {
            _mainControl.UpdateLock = true;
            _runPerformanceTSB.Enabled = false;
            Application.DoEvents();

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
                    Application.DoEvents(); // so paint will be called
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
            _runPerformanceTSB.Enabled = true;
        }
    }
}