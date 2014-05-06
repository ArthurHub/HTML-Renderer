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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HtmlRenderer.Demo.WinForms
{
    public partial class SampleForm : Form
    {
        private readonly Bitmap _background;

        public SampleForm()
        {
            InitializeComponent();

            Icon = DemoForm.GetIcon();

            _background = new Bitmap(10, 10);
            using (var g = Graphics.FromImage(_background))
            {
                g.Clear(Color.White);
                g.FillRectangle(SystemBrushes.Control, new Rectangle(0, 0, 5, 5));
                g.FillRectangle(SystemBrushes.Control, new Rectangle(5, 5, 5, 5));
            }
        }

        private void OnHtmlLabelClick(object sender, EventArgs e)
        {
            _pGrid.SelectedObject = _htmlLabel;
        }

        private void OnHtmlPanelClick(object sender, EventArgs e)
        {
            _pGrid.SelectedObject = _htmlPanel;
        }

        private void OnHtmlLabelHostingPanelPaint(object sender, PaintEventArgs e)
        {
            using (var b = new TextureBrush(_background, WrapMode.Tile))
            {
                e.Graphics.FillRectangle(b, _htmlLabelHostingPanel.ClientRectangle);
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            _htmlToolTip.SetToolTip(_changeTooltipButton, _htmlLabel.Text);
        }
    }
}