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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using Timer = System.Threading.Timer;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    public partial class MainControl : UserControl
    {
        #region Fields and Consts

        /// <summary>
        /// the name of the tree node root for all performance samples
        /// </summary>
        private const string PerformanceSamplesTreeNodeName = "Performance Samples";

        /// <summary>
        /// timer to update the rendered html when html in editor changes with delay
        /// </summary>
        private readonly Timer _updateHtmlTimer;

        /// <summary>
        /// used ignore html editor updates when updating separately
        /// </summary>
        private bool _updateLock;

        /// <summary>
        /// In IE view if to show original html or the html generated from the html control
        /// </summary>
        private bool _useGeneratedHtml;

        #endregion


        public MainControl()
        {
            InitializeComponent();

            _htmlPanel.RenderError += OnRenderError;
            _htmlPanel.LinkClicked += OnLinkClicked;
            _htmlPanel.StylesheetLoad += DemoUtils.OnStylesheetLoad;
            _htmlPanel.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            _htmlToolTip.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            _htmlPanel.LoadComplete += (sender, args) => _htmlPanel.ScrollToElement("C4");

            _htmlToolTip.SetToolTip(_htmlPanel, Resources.Tooltip);

            _htmlEditor.Font = new Font(FontFamily.GenericMonospace, 10);

            LoadSamples();

            _updateHtmlTimer = new Timer(OnUpdateHtmlTimerTick);
        }

        /// <summary>
        /// used ignore html editor updates when updating separately
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UpdateLock
        {
            get { return _updateLock; }
            set { _updateLock = value; }
        }

        /// <summary>
        /// In IE view if to show original html or the html generated from the html control
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseGeneratedHtml
        {
            get { return _useGeneratedHtml; }
            set { _useGeneratedHtml = value; }
        }

        /// <summary>
        /// Show\Hide the web browser viewer.
        /// </summary>
        public void ShowWebBrowserView(bool show)
        {
            _webBrowser.Visible = show;
            _splitter.Visible = show;

            if (_webBrowser.Visible)
            {
                _webBrowser.Width = _splitContainer2.Panel2.Width / 2;
                UpdateWebBrowserHtml();
            }
        }

        /// <summary>
        /// Update the html shown in the web browser
        /// </summary>
        public void UpdateWebBrowserHtml()
        {
            if (_webBrowser.Visible)
            {
                _webBrowser.DocumentText = _useGeneratedHtml ? _htmlPanel.GetHtml() : GetFixedHtml();
            }
        }

        public string GetHtml()
        {
            return _useGeneratedHtml ? _htmlPanel.GetHtml() : _htmlEditor.Text;
        }

        public void SetHtml(string html)
        {
            _htmlPanel.Text = html;
        }


        #region Private methods

        /// <summary>
        /// Loads the tree of document samples
        /// </summary>
        private void LoadSamples()
        {
            var showcaseRoot = new TreeNode("HTML Renderer");
            _samplesTreeView.Nodes.Add(showcaseRoot);

            foreach (var sample in SamplesLoader.ShowcaseSamples)
            {
                AddTreeNode(showcaseRoot, sample);
            }

            var testSamplesRoot = new TreeNode("Test Samples");
            _samplesTreeView.Nodes.Add(testSamplesRoot);

            foreach (var sample in SamplesLoader.TestSamples)
            {
                AddTreeNode(testSamplesRoot, sample);
            }

            if (SamplesLoader.PerformanceSamples.Count > 0)
            {
                var perfTestSamplesRoot = new TreeNode(PerformanceSamplesTreeNodeName);
                _samplesTreeView.Nodes.Add(perfTestSamplesRoot);

                foreach (var sample in SamplesLoader.PerformanceSamples)
                {
                    AddTreeNode(perfTestSamplesRoot, sample);
                }
            }

            showcaseRoot.Expand();

            if (showcaseRoot.Nodes.Count > 0)
            {
                _samplesTreeView.SelectedNode = showcaseRoot.Nodes[0];
            }
        }

        /// <summary>
        /// Add an html sample to the tree and to all samples collection
        /// </summary>
        private void AddTreeNode(TreeNode root, HtmlSample sample)
        {
            var node = new TreeNode(sample.Name);
            node.Tag = new HtmlSample(sample.Name, sample.FullName, sample.Html);
            root.Nodes.Add(node);
        }

        /// <summary>
        /// On tree view node click load the html to the html panel and html editor.
        /// </summary>
        private void OnSamplesTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            var sample = e.Node.Tag as HtmlSample;
            if (sample != null)
            {
                _updateLock = true;

                if (e.Node.Parent.Text != PerformanceSamplesTreeNodeName)
                    SetColoredText(sample.Html);
                else
                    _htmlEditor.Text = sample.Html;

                Application.UseWaitCursor = true;

                try
                {
                    _htmlPanel.AvoidImagesLateLoading = !sample.FullName.Contains("Many images");

                    _htmlPanel.Text = sample.Html;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to render HTML");
                }

                Application.UseWaitCursor = false;
                _updateLock = false;

                UpdateWebBrowserHtml();
            }
        }

        /// <summary>
        /// On text change in the html editor update 
        /// </summary>
        private void OnHtmlEditorTextChanged(object sender, EventArgs e)
        {
            if (!_updateLock)
            {
                _updateHtmlTimer.Change(1000, int.MaxValue);
            }
        }

        /// <summary>
        /// Update the html renderer with text from html editor.
        /// </summary>
        private void OnUpdateHtmlTimerTick(object state)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                _updateLock = true;

                try
                {
                    _htmlPanel.Text = _htmlEditor.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to render HTML");
                }

                //SyntaxHilight.AddColoredText(_htmlEditor.Text, _htmlEditor);

                UpdateWebBrowserHtml();

                _updateLock = false;
            }));
        }

        /// <summary>
        /// Fix the raw html by replacing bridge object properties calls with path to file with the data returned from the property.
        /// </summary>
        /// <returns>fixed html</returns>
        private string GetFixedHtml()
        {
            var html = _htmlEditor.Text;

            html = Regex.Replace(html, @"src=\""(\w.*?)\""", match =>
            {
                var img = HtmlRenderingHelper.TryLoadResourceImage(match.Groups[1].Value);
                if (img != null)
                {
                    var tmpFile = Path.GetTempFileName();
                    img.Save(tmpFile, ImageFormat.Jpeg);
                    return string.Format("src=\"{0}\"", tmpFile);
                }
                return match.Value;
            }, RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"href=\""(\w.*?)\""", match =>
            {
                var stylesheet = DemoUtils.GetStylesheet(match.Groups[1].Value);
                if (stylesheet != null)
                {
                    var tmpFile = Path.GetTempFileName();
                    File.WriteAllText(tmpFile, stylesheet);
                    return string.Format("href=\"{0}\"", tmpFile);
                }
                return match.Value;
            }, RegexOptions.IgnoreCase);

            return html;
        }

        /// <summary>
        /// Reload the html shown in the html editor by running coloring again.
        /// </summary>
        private void OnReloadColorsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetColoredText(_htmlEditor.Text);
        }

        /// <summary>
        /// Show error raised from html renderer.
        /// </summary>
        private static void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            MessageBox.Show(e.Message + (e.Exception != null ? "\r\n" + e.Exception : null), "Error in Html Renderer", MessageBoxButtons.OK);
        }

        /// <summary>
        /// On specific link click handle it here.
        /// </summary>
        private static void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            if (e.Link == "SayHello")
            {
                MessageBox.Show("Hello you!");
                e.Handled = true;
            }
            else if (e.Link == "ShowSampleForm")
            {
                using (var f = new SampleForm())
                {
                    f.ShowDialog();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Set html syntax color text on the RTF html editor.
        /// </summary>
        private void SetColoredText(string text)
        {
            var selectionStart = _htmlEditor.SelectionStart;
            _htmlEditor.Clear();
            _htmlEditor.Rtf = HtmlSyntaxHighlighter.Process(text);
            _htmlEditor.SelectionStart = selectionStart;
        }

        #endregion
    }
}