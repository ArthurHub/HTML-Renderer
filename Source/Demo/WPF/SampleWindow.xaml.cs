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

using System.Windows.Input;
using HtmlRenderer.Demo.Common;

namespace HtmlRenderer.Demo.WPF
{
    /// <summary>
    /// Interaction logic for SampleWindow.xaml
    /// </summary>
    public partial class SampleWindow
    {
        public SampleWindow()
        {
            InitializeComponent();

            _htmlLabel.Html = DemoUtils.SampleHtmlLabelText;
            _htmlPanel.Html = DemoUtils.SampleHtmlPanelText;

            _propertyGrid.SelectedObject = _htmlLabel;
        }

        private void OnHtmlControl_mouseDown(object sender, MouseButtonEventArgs e)
        {
            _propertyGrid.SelectedObject = sender;
        }
    }
}
