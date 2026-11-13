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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WinUI;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    public sealed partial class SampleWindow : Window
    {
        private HtmlControl _selectedControl;

        public SampleWindow()
        {
            InitializeComponent();

            WindowHelper.Setup(this, "Sample Window", 0.56, 0.64);

            _htmlLabel.Text = DemoUtils.SampleHtmlLabelText;
            _htmlPanel.Text = DemoUtils.SampleHtmlPanelText;

            // The controls handle the pointer events themselves, so listen to the handled ones as well
            _htmlLabel.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnHtmlControl_PointerReleased), true);
            _htmlPanel.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnHtmlControl_PointerReleased), true);

            SelectControl(_htmlLabel);
        }

        private void OnHtmlControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SelectControl((HtmlControl)sender);
        }

        private void SelectControl(HtmlControl control)
        {
            _selectedControl = control;

            var label = control as HtmlLabel;
            _selectedControlText.Text = label != null ? "HtmlLabel" : "HtmlPanel";
            _isSelectionEnabled.IsChecked = control.IsSelectionEnabled;
            _isContextMenuEnabled.IsChecked = control.IsContextMenuEnabled;
            _avoidImagesLateLoading.IsChecked = control.AvoidImagesLateLoading;
            _autoSize.IsChecked = label?.AutoSize ?? false;
            _autoSizeHeightOnly.IsChecked = label?.AutoSizeHeightOnly ?? false;
            _autoSize.IsEnabled = _autoSizeHeightOnly.IsEnabled = label != null;
            _text.Text = control.Text ?? string.Empty;
            _baseStylesheet.Text = control.BaseStylesheet ?? string.Empty;
        }

        private void OnPropertyChanged(object sender, RoutedEventArgs e)
        {
            _selectedControl.IsSelectionEnabled = _isSelectionEnabled.IsChecked == true;
            _selectedControl.IsContextMenuEnabled = _isContextMenuEnabled.IsChecked == true;
            _selectedControl.AvoidImagesLateLoading = _avoidImagesLateLoading.IsChecked == true;

            if (_selectedControl is HtmlLabel label)
            {
                if (ReferenceEquals(sender, _autoSize))
                    label.AutoSize = _autoSize.IsChecked == true;
                else if (ReferenceEquals(sender, _autoSizeHeightOnly))
                    label.AutoSizeHeightOnly = _autoSizeHeightOnly.IsChecked == true;

                // AutoSize and AutoSizeHeightOnly exclude each other
                _autoSize.IsChecked = label.AutoSize;
                _autoSizeHeightOnly.IsChecked = label.AutoSizeHeightOnly;
            }

            if (_selectedControl.BaseStylesheet != _baseStylesheet.Text)
                _selectedControl.BaseStylesheet = _baseStylesheet.Text;
            if (_selectedControl.Text != _text.Text)
                _selectedControl.Text = _text.Text;

            _selectedControl.InvalidateMeasure();
        }
    }
}
