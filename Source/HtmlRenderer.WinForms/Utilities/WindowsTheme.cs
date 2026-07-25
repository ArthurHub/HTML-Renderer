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

using Microsoft.Win32;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinForms.Utilities
{
    /// <summary>
    /// Reads the Windows app theme, which is what <c>prefers-color-scheme</c> reports for on-screen
    /// rendering.
    /// </summary>
    internal static class WindowsTheme
    {
        private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightTheme = "AppsUseLightTheme";

        /// <summary>
        /// The user's app theme, or <see cref="RColorScheme.Light"/> when it cannot be determined -
        /// light is the Windows default and the safer assumption for a document that declares no dark
        /// styling of its own.
        /// </summary>
        public static RColorScheme GetAppsColorScheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(AppsUseLightTheme);
                        if (value is int)
                            return (int)value == 0 ? RColorScheme.Dark : RColorScheme.Light;
                    }
                }
            }
            catch
            {
                // A locked-down or missing key just means "no preference expressed".
            }

            return RColorScheme.Light;
        }
    }
}
