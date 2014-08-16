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
using TheArtOfDev.HtmlRenderer.Demo.Common;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    public partial class SampleForm : Form
    {
        private readonly Bitmap _background;

        public SampleForm()
        {
            InitializeComponent();

            Icon = DemoForm.GetIcon();

            _htmlLabel.Text = DemoUtils.SampleHtmlLabelText;
            _htmlPanel.Text = DemoUtils.SampleHtmlPanelText;

            _background = HtmlRenderingHelper.CreateImageForTransparentBackground();
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

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ((Control)_pGrid.SelectedObject).Refresh();
            Refresh();
        }
    }
}