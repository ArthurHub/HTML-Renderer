// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
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
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using HtmlRenderer.Demo.Properties;

namespace HtmlRenderer.Demo
{
    public partial class PerfForm : Form
    {
        #region Fields and Consts

        /// <summary>
        /// the html samples to show in the demo
        /// </summary>
        private readonly Dictionary<string, string> _samples = new Dictionary<string, string>();

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public PerfForm()
        {
            InitializeComponent();

            Icon = Resources.html;

            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 800);

            LoadSamples();
        }


        #region Private methods

        /// <summary>
        /// Loads the tree of document samples
        /// </summary>
        private void LoadSamples()
        {
            var root = new TreeNode("HTML Renderer");
            _samplesTreeView.Nodes.Add(root);
            
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Array.Sort(names);
            foreach (string name in names)
            {
                int extPos = name.LastIndexOf('.');
                int namePos = extPos > 0 && name.Length > 1 ? name.LastIndexOf('.', extPos - 1) : 0;
                string ext = name.Substring(extPos >= 0 ? extPos : 0);
                string shortName = namePos > 0 && name.Length > 2 ? name.Substring(namePos + 1, name.Length - namePos - ext.Length - 1) : name;

                if (".htm".IndexOf(ext) >= 0)
                {
                    if (name.IndexOf("PerfSamples", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                        if (resourceStream != null)
                        {
                            using (StreamReader sreader = new StreamReader(resourceStream, Encoding.Default))
                            {
                                _samples[name] = sreader.ReadToEnd();
                            }

                            var node = new TreeNode(shortName);
                            root.Nodes.Add(node);
                            node.Tag = name;
                        }
                    }
                }
            }
           
            root.Expand();
        }

        /// <summary>
        /// On tree view node click load the html to the html panel and html editor.
        /// </summary>
        private void OnSamplesTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            var name = e.Node.Tag as string;
            if (!string.IsNullOrEmpty(name))
            {
                _htmlPanel.Text = _samples[name];
            }
        }

        /// <summary>
        /// Clear the html in the renderer
        /// </summary>
        private void OnClearLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _samplesTreeView.SelectedNode = null;
            _htmlPanel.Text = null;
            GC.Collect();
        }
        
        /// <summary>
        /// Execute performance test by setting all sample htmls in a loop.
        /// </summary>
        private void OnRunTestButtonClick(object sender, EventArgs e)
        {
            if (_samplesTreeView.SelectedNode != null && _samplesTreeView.SelectedNode.Tag != null)
            {
                GC.Collect();

                _runTestButton.Text = "Running..";
                _runTestButton.Enabled = false;
                Application.DoEvents();

                var html = _samples[(string)_samplesTreeView.SelectedNode.Tag];

                var sw = Stopwatch.StartNew();

                for (int i = 0; i < _iterations.Value; i++)
                {
                    _htmlPanel.Text = html;
                    Application.DoEvents(); // so paint will be called
                }

                sw.Stop();

                var msg = string.Format("Total: {0} mSec\r\nIterationAvg: {1:N2}", sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / (double)_iterations.Value);
                Clipboard.SetDataObject(msg);
                MessageBox.Show(msg, "Test run results");

                _runTestButton.Text = "Run Tests";
                _runTestButton.Enabled = true;
            }
        }

        #endregion
    }
}