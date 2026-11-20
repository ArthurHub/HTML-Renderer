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
using Microsoft.UI.Xaml;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.WinUI;

namespace TheArtOfDev.HtmlRenderer.Demo.WinUI
{
    public partial class App : Application
    {
        private Window _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            SamplesLoader.Init("WinUI", typeof(HtmlRender).Assembly.GetName().Version.ToString());

            LoadCustomFonts();

            _window = new MainWindow();
            _window.Activate();
        }

        /// <summary>
        /// Load custom fonts to be used by renderer HTMLs
        /// </summary>
        private static void LoadCustomFonts()
        {
            // Win2D loads fonts only from files
            var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".ttf");
            File.WriteAllBytes(file, Common.Resources.CustomFont);
            HtmlRender.AddFontFamily(new Uri(file));
        }
    }
}
