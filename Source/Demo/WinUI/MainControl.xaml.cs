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
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    public sealed partial class MainControl : UserControl
    {
        #region Fields and Consts

        /// <summary>
        /// the name of the tree node root for all performance samples
        /// </summary>
        private const string PerformanceSamplesTreeNodeName = "Performance Samples";

        /// <summary>
        /// The sample of each tree node, the node itself only shows the name
        /// </summary>
        private readonly Dictionary<TreeViewNode, HtmlSample> _nodeToSample = new Dictionary<TreeViewNode, HtmlSample>();

        /// <summary>
        /// timer to update the rendered html when html in editor changes with delay
        /// </summary>
        private readonly DispatcherQueueTimer _updateHtmlTimer;

        /// <summary>
        /// The html last set to the editor by the code, to tell it apart from the user's changes
        /// </summary>
        private string _setEditorText;

        #endregion

        public MainControl()
        {
            InitializeComponent();

            _htmlPanel.RenderError += OnRenderError;
            _htmlPanel.LinkClicked += OnLinkClicked;
            _htmlPanel.StylesheetLoad += DemoUtils.OnStylesheetLoad;
            _htmlPanel.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            _htmlPanel.LoadComplete += (sender, args) => _htmlPanel.ScrollToElement("C4");

            _htmlTooltipLabel.AvoidImagesLateLoading = true;
            _htmlTooltipLabel.StylesheetLoad += DemoUtils.OnStylesheetLoad;
            _htmlTooltipLabel.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            _htmlTooltipLabel.Text = "<div class='htmltooltip'>" + Common.Resources.Tooltip + "</div>";

            _updateHtmlTimer = DispatcherQueue.CreateTimer();
            _updateHtmlTimer.Interval = TimeSpan.FromSeconds(1);
            _updateHtmlTimer.IsRepeating = false;
            _updateHtmlTimer.Tick += OnUpdateHtmlTimerTick;

            LoadSamples();
        }

        /// <summary>
        /// used ignore html editor updates when updating separately
        /// </summary>
        public bool UpdateLock { get; set; }

        /// <summary>
        /// In browser view if to show original html or the html generated from the html control
        /// </summary>
        public bool UseGeneratedHtml { get; set; }

        /// <summary>
        /// Show\Hide the web browser viewer.
        /// </summary>
        public void ShowWebBrowserView(bool show)
        {
            _webView.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            _viewsGrid.ColumnDefinitions[1].Width = show ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

            if (show)
                UpdateWebBrowserHtml();
        }

        /// <summary>
        /// Update the html shown in the web browser
        /// </summary>
        public async void UpdateWebBrowserHtml()
        {
            if (_webView.Visibility == Visibility.Visible)
            {
                try
                {
                    await _webView.EnsureCoreWebView2Async();
                    _webView.NavigateToString(UseGeneratedHtml ? _htmlPanel.GetHtml() : GetFixedHtml());
                }
                catch (Exception ex)
                {
                    await WindowHelper.ShowMessageAsync(XamlRoot, ex.ToString(), "Failed to show the HTML in WebView2");
                }
            }
        }

        public string GetHtml()
        {
            return UseGeneratedHtml ? _htmlPanel.GetHtml() : GetHtmlEditorText();
        }

        /// <summary>
        /// Set the html and lay it out right away, used to measure the performance.
        /// </summary>
        public void SetHtml(string html)
        {
            _htmlPanel.Text = html;
            _htmlPanel.UpdateLayout();
        }

        #region Private methods

        /// <summary>
        /// Loads the tree of document samples
        /// </summary>
        private void LoadSamples()
        {
            var showcaseRoot = AddRootNode("HTML Renderer", SamplesLoader.ShowcaseSamples);
            AddRootNode("Test Samples", SamplesLoader.TestSamples);
            if (SamplesLoader.PerformanceSamples.Count > 0)
                AddRootNode(PerformanceSamplesTreeNodeName, SamplesLoader.PerformanceSamples);

            showcaseRoot.IsExpanded = true;
            if (showcaseRoot.Children.Count > 0)
            {
                _samplesTreeView.SelectedNode = showcaseRoot.Children[0];
                ShowSample(showcaseRoot.Children[0]);
            }
        }

        private TreeViewNode AddRootNode(string name, List<HtmlSample> samples)
        {
            var root = new TreeViewNode { Content = name };
            _samplesTreeView.RootNodes.Add(root);
            foreach (var sample in samples)
            {
                var node = new TreeViewNode { Content = sample.Name };
                _nodeToSample[node] = sample;
                root.Children.Add(node);
            }
            return root;
        }

        /// <summary>
        /// On tree view node click load the html to the html panel and html editor.
        /// </summary>
        private void OnSamplesTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is TreeViewNode node)
                ShowSample(node);
        }

        private void ShowSample(TreeViewNode node)
        {
            if (!_nodeToSample.TryGetValue(node, out var sample))
                return;

            UpdateLock = true;

            // Coloring the large performance samples takes too long
            SetEditorText(sample.Html, !Equals(node.Parent?.Content, PerformanceSamplesTreeNodeName) && _coloredCheckBox.IsChecked == true);

            try
            {
                _htmlPanel.AvoidImagesLateLoading = !sample.FullName.Contains("Many images");
                _htmlPanel.Text = sample.Html;
                UpdateFixedHtmlView();
            }
            catch (Exception ex)
            {
                SetFixedHtmlText(string.Empty, false);
                _ = WindowHelper.ShowMessageAsync(XamlRoot, ex.ToString(), "Failed to render HTML");
            }

            UpdateLock = false;

            UpdateWebBrowserHtml();
        }

        /// <summary>
        /// On text change in the html editor update
        /// </summary>
        private void OnHtmlEditor_TextChanged(object sender, RoutedEventArgs e)
        {
            // TextChanged is raised later, also for the text set by the code
            if (!UpdateLock && GetHtmlEditorText() != _setEditorText)
            {
                _updateHtmlTimer.Stop();
                _updateHtmlTimer.Start();
            }
        }

        /// <summary>
        /// Update the html renderer with text from html editor.
        /// </summary>
        private void OnUpdateHtmlTimerTick(DispatcherQueueTimer sender, object args)
        {
            UpdateLock = true;

            try
            {
                _htmlPanel.Text = GetHtmlEditorText();
                UpdateFixedHtmlView();
            }
            catch (Exception ex)
            {
                SetFixedHtmlText(string.Empty, false);
                _ = WindowHelper.ShowMessageAsync(XamlRoot, ex.ToString(), "Failed to render HTML");
            }

            UpdateWebBrowserHtml();

            UpdateLock = false;
        }

        /// <summary>
        /// Fix the raw html by replacing the resource keys of images and stylesheets with their data,
        /// a page shown from a string can't load local files.
        /// </summary>
        /// <returns>fixed html</returns>
        private string GetFixedHtml()
        {
            var html = GetHtmlEditorText();

            html = Regex.Replace(html, @"src=\""(\w.*?)\""", match =>
            {
                var dataUri = HtmlRenderingHelper.TryGetResourceImageDataUri(match.Groups[1].Value);
                return dataUri != null ? string.Format("src=\"{0}\"", dataUri) : match.Value;
            }, RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"href=\""(\w.*?)\""", match =>
            {
                var stylesheet = DemoUtils.GetStylesheet(match.Groups[1].Value);
                return stylesheet != null
                    ? string.Format("href=\"data:text/css;base64,{0}\"", Convert.ToBase64String(Encoding.UTF8.GetBytes(stylesheet)))
                    : match.Value;
            }, RegexOptions.IgnoreCase);

            return html;
        }

        /// <summary>
        /// Update the fixed html shown in the read-only editor.
        /// </summary>
        private void UpdateFixedHtmlView()
        {
            SetFixedHtmlText(HtmlPrettyPrinter.Format(_htmlPanel.GetHtml() ?? string.Empty), _coloredCheckBox.IsChecked == true);
        }

        /// <summary>
        /// Set formatted HTML text in the fixed HTML viewer.
        /// </summary>
        private void SetFixedHtmlText(string text, bool color)
        {
            // The document of a read-only box can't be changed
            _fixedHtmlEditor.IsReadOnly = false;
            SetRtf(_fixedHtmlEditor, text ?? string.Empty, color);
            _fixedHtmlEditor.IsReadOnly = true;
        }

        /// <summary>
        /// Reload the html shown in the html editor by running coloring again.
        /// </summary>
        private void OnRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetEditorText(GetHtmlEditorText(), true);
        }

        /// <summary>
        /// Toggle if the html syntax is colored.
        /// </summary>
        private void OnColored_Click(object sender, RoutedEventArgs e)
        {
            SetEditorText(GetHtmlEditorText(), _coloredCheckBox.IsChecked == true);
            UpdateFixedHtmlView();
        }

        /// <summary>
        /// Show error raised from html renderer.
        /// </summary>
        private void OnRenderError(object sender, HtmlRenderErrorEventArgs args)
        {
            _ = WindowHelper.ShowMessageAsync(XamlRoot, args.Message + (args.Exception != null ? "\r\n" + args.Exception : null), "Error in Html Renderer");
        }

        /// <summary>
        /// On specific link click handle it here.
        /// </summary>
        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs args)
        {
            if (args.Link == "SayHello")
            {
                _ = WindowHelper.ShowMessageAsync(XamlRoot, "Hello you!", "Hello");
                args.Handled = true;
            }
            else if (args.Link == "ShowSampleForm")
            {
                new SampleWindow().Activate();
                args.Handled = true;
            }
        }

        /// <summary>
        /// Set the html in the editor without triggering an update of the rendered html.
        /// </summary>
        private void SetEditorText(string text, bool color)
        {
            SetRtf(_htmlEditor, text, color);
            _setEditorText = GetHtmlEditorText();
        }

        /// <summary>
        /// Set html syntax colored text, uncolored text goes through the highlighter as well to escape it for RTF.
        /// </summary>
        private static void SetRtf(RichEditBox editor, string text, bool color)
        {
            var black = System.Drawing.Color.Black;
            var rtf = color ? HtmlSyntaxHighlighter.Process(text) : HtmlSyntaxHighlighter.Process(text, black, black, black, black, black, black);

            // RichEditBox shows the RTF as text without a version and charset in the header
            if (rtf.StartsWith(@"{\rtf{", StringComparison.Ordinal))
                rtf = @"{\rtf1\ansi" + rtf.Substring(5);

            editor.Document.SetText(TextSetOptions.FormatRtf, rtf);
        }

        /// <summary>
        /// Get the html text from the html editor control.
        /// </summary>
        private string GetHtmlEditorText()
        {
            _htmlEditor.Document.GetText(TextGetOptions.UseCrlf, out var text);
            return text;
        }

        #endregion
    }
}
