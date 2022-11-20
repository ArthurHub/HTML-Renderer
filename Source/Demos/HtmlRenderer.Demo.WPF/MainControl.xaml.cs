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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WPF;

namespace TheArtOfDev.HtmlRenderer.Demo.WPF
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl
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
            _htmlPanel.StylesheetLoad += HtmlRenderingHelper.OnStylesheetLoad;
            _htmlPanel.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            _htmlPanel.LoadComplete += (sender, args) => _htmlPanel.ScrollToElement("C4");

            _htmlTooltipLabel.AvoidImagesLateLoading = true;
            _htmlTooltipLabel.StylesheetLoad += HtmlRenderingHelper.OnStylesheetLoad;
            _htmlTooltipLabel.ImageLoad += HtmlRenderingHelper.OnImageLoad;
            _htmlTooltipLabel.Text = "<div class='htmltooltip'>" + Common.Resources.Tooltip + "</div>";


            LoadSamples();

            _updateHtmlTimer = new Timer(OnUpdateHtmlTimerTick);
        }


        /// <summary>
        /// used ignore html editor updates when updating separately
        /// </summary>
        public bool UpdateLock
        {
            get { return _updateLock; }
            set { _updateLock = value; }
        }

        /// <summary>
        /// In IE view if to show original html or the html generated from the html control
        /// </summary>
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
            _webBrowser.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            _splitter.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            _grid2.ColumnDefinitions[2].Width = show ? new GridLength(_grid2.ActualWidth / 2, GridUnitType.Pixel) : GridLength.Auto;

            if (show)
                UpdateWebBrowserHtml();
        }

        /// <summary>
        /// Update the html shown in the web browser
        /// </summary>
        public void UpdateWebBrowserHtml()
        {
            if (_webBrowser.IsVisible)
            {
                _webBrowser.NavigateToString(_useGeneratedHtml ? _htmlPanel.GetHtml() : GetFixedHtml());
            }
        }

        public string GetHtml()
        {
            return _useGeneratedHtml ? _htmlPanel.GetHtml() : GetHtmlEditorText();
        }

        public void SetHtml(string html)
        {
            _htmlPanel.Text = html;
            if (string.IsNullOrWhiteSpace(html))
            {
                _htmlPanel.InvalidateMeasure();
                _htmlPanel.InvalidateVisual();
            }
        }


        #region Private methods

        /// <summary>
        /// Loads the tree of document samples
        /// </summary>
        private void LoadSamples()
        {
            var showcaseRoot = new TreeViewItem();
            showcaseRoot.Header = "HTML Renderer";
            _samplesTreeView.Items.Add(showcaseRoot);

            foreach (var sample in SamplesLoader.ShowcaseSamples)
            {
                AddTreeItem(showcaseRoot, sample);
            }

            var testSamplesRoot = new TreeViewItem();
            testSamplesRoot.Header = "Test Samples";
            _samplesTreeView.Items.Add(testSamplesRoot);

            foreach (var sample in SamplesLoader.TestSamples)
            {
                AddTreeItem(testSamplesRoot, sample);
            }

            if (SamplesLoader.PerformanceSamples.Count > 0)
            {
                var perfTestSamplesRoot = new TreeViewItem();
                perfTestSamplesRoot.Header = PerformanceSamplesTreeNodeName;
                _samplesTreeView.Items.Add(perfTestSamplesRoot);

                foreach (var sample in SamplesLoader.PerformanceSamples)
                {
                    AddTreeItem(perfTestSamplesRoot, sample);
                }
            }

            showcaseRoot.IsExpanded = true;

            if (showcaseRoot.Items.Count > 0)
                ((TreeViewItem)showcaseRoot.Items[0]).IsSelected = true;
        }

        /// <summary>
        /// Add an html sample to the tree and to all samples collection
        /// </summary>
        private void AddTreeItem(TreeViewItem root, HtmlSample sample)
        {
            var html = sample.Html.Replace("$$Release$$", _htmlPanel.GetType().Assembly.GetName().Version.ToString());

            var node = new TreeViewItem();
            node.Header = sample.Name;
            node.Tag = new HtmlSample(sample.Name, sample.FullName, html);
            root.Items.Add(node);
        }

        /// <summary>
        /// On tree view node click load the html to the html panel and html editor.
        /// </summary>
        private void OnTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ((TreeViewItem)e.NewValue);
            var sample = item.Tag as HtmlSample;
            if (sample != null)
            {
                _updateLock = true;

                if (!Equals(((TreeViewItem)item.Parent).Header, PerformanceSamplesTreeNodeName))
                {
                    SetColoredText(sample.Html, _coloredCheckBox.IsChecked.GetValueOrDefault(false));
                }
                else
                {
                    _htmlEditor.Document.Blocks.Clear();
                    _htmlEditor.Document.Blocks.Add(new Paragraph(new Run(sample.Html)));
                }

                Cursor = Cursors.Wait;

                try
                {
                    _htmlPanel.AvoidImagesLateLoading = !sample.FullName.Contains("Many images");
                    _htmlPanel.Text = sample.Html;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to render HTML");
                }

                Cursor = Cursors.Arrow;
                _updateLock = false;

                UpdateWebBrowserHtml();
            }
        }

        /// <summary>
        /// On text change in the html editor update 
        /// </summary>
        private void OnHtmlEditor_TextChanged(object sender, TextChangedEventArgs e)
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
            Dispatcher.BeginInvoke(new Action<Object>(o =>
            {
                _updateLock = true;

                try
                {
                    _htmlPanel.Text = GetHtmlEditorText();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to render HTML");
                }

                //SyntaxHilight.AddColoredText(_htmlEditor.Text, _htmlEditor);

                UpdateWebBrowserHtml();

                _updateLock = false;
            }), state);
        }

        /// <summary>
        /// Fix the raw html by replacing bridge object properties calls with path to file with the data returned from the property.
        /// </summary>
        /// <returns>fixed html</returns>
        private string GetFixedHtml()
        {
            var html = GetHtmlEditorText();

            html = Regex.Replace(html, @"src=\""(\w.*?)\""", match =>
            {
                var img = HtmlRenderingHelper.TryLoadResourceImage(match.Groups[1].Value);
                if (img != null)
                {
                    var tmpFile = Path.GetTempFileName();
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    using (FileStream stream = new FileStream(tmpFile, FileMode.Create))
                        encoder.Save(stream);
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
        private void OnRefreshLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetColoredText(GetHtmlEditorText(), true);
        }

        /// <summary>
        /// Toggle if the html syntax is colored.
        /// </summary>
        private void OnColoredCheckbox_click(object sender, RoutedEventArgs e)
        {
            SetColoredText(GetHtmlEditorText(), _coloredCheckBox.IsChecked.GetValueOrDefault(false));
        }

        /// <summary>
        /// Show error raised from html renderer.
        /// </summary>
        private void OnRenderError(object sender, RoutedEventArgs<HtmlRenderErrorEventArgs> args)
        {
            Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(args.Data.Message + (args.Data.Exception != null ? "\r\n" + args.Data.Exception : null), "Error in Html Renderer", MessageBoxButton.OK)));
        }

        /// <summary>
        /// On specific link click handle it here.
        /// </summary>
        private void OnLinkClicked(object sender, RoutedEventArgs<HtmlLinkClickedEventArgs> args)
        {
            if (args.Data.Link == "SayHello")
            {
                MessageBox.Show("Hello you!");
                args.Data.Handled = true;
            }
            else if (args.Data.Link == "ShowSampleForm")
            {
                var w = new SampleWindow();
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    w.Owner = window;
                    w.Width = window.Width * 0.8;
                    w.Height = window.Height * 0.8;
                    w.ShowDialog();
                }
                args.Data.Handled = true;
            }
        }

        /// <summary>
        /// Set html syntax color text on the RTF html editor.
        /// </summary>
        private void SetColoredText(string text, bool color)
        {
            var selectionStart = _htmlEditor.CaretPosition;
            _htmlEditor.Text = color ? HtmlSyntaxHighlighter.Process(text) : text.Replace("\n", "\\par ");
            _htmlEditor.CaretPosition = selectionStart;
        }

        /// <summary>
        /// Get the html text from the html editor control.
        /// </summary>
        private string GetHtmlEditorText()
        {
            return new TextRange(_htmlEditor.Document.ContentStart, _htmlEditor.Document.ContentEnd).Text;
        }

        #endregion
    }
}