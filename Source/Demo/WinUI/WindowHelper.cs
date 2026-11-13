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
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.Storage.Pickers;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    internal static class WindowHelper
    {
        /// <summary>
        /// Set the demo icon, size the window relative to the work area and center it.
        /// </summary>
        public static void Setup(Window window, string title, double widthFactor, double heightFactor)
        {
            window.Title = title;

            // A WinUI window doesn't take the icon from the executable
            window.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "html.ico"));

            var workArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
            var width = (int)(workArea.Width * widthFactor);
            var height = (int)(workArea.Height * heightFactor);
            window.AppWindow.MoveAndResize(new RectInt32(workArea.X + (workArea.Width - width) / 2, workArea.Y + (workArea.Height - height) / 2, width, height));
        }

        /// <summary>
        /// Create a file picker for the window, an unpackaged app has to hand over the window handle.
        /// </summary>
        public static FileSavePicker CreateSavePicker(Window window)
        {
            var picker = new FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, Win32Interop.GetWindowFromWindowId(window.AppWindow.Id));
            return picker;
        }

        /// <summary>
        /// Show a message box like dialog.
        /// </summary>
        public static async Task ShowMessageAsync(XamlRoot xamlRoot, string message, string title)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = title,
                Content = new ScrollViewer { Content = new TextBlock { Text = message, IsTextSelectionEnabled = true, TextWrapping = TextWrapping.Wrap } },
                CloseButtonText = "OK",
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception)
            {
                // Only one dialog can be open at a time, a second error while one is shown is dropped
            }
        }
    }
}
