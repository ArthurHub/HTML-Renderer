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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Network;
using TheArtOfDev.HtmlRenderer.WinUI.Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for WinUI 3 / Win2D platform.
    /// </summary>
    internal sealed class WinUIAdapter : RAdapter
    {
        #region Fields and Consts

        // Has to be initialized before _instance, the constructor uses it
        private static readonly HttpClient _sharedHttpClient = new HttpClient();

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly WinUIAdapter _instance = new WinUIAdapter();

        /// <summary>
        /// List of valid predefined color names in lower-case
        /// </summary>
        private static readonly List<string> ValidColorNamesLc;

        /// <summary>
        /// The Win2D device all resources are created on and all controls draw with.
        /// </summary>
        private readonly CanvasDevice _device;

        /// <summary>
        /// ColorValuesChanged is only raised as long as the UISettings instance is alive.
        /// </summary>
        private readonly UISettings _uiSettings;

        /// <summary>
        /// Win2D can only load a font set from a file, so the <c>@font-face</c> fonts are written to this directory.
        /// </summary>
        private static readonly string _fontFaceTempDirectory = CreateFontFaceTempDirectory();

        /// <summary>
        /// Cached app theme, null when it needs to be re-read.
        /// </summary>
        private RColorScheme? _colorScheme;

        #endregion

        static WinUIAdapter()
        {
            ValidColorNamesLc = new List<string>();
            foreach (var colorProp in typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                ValidColorNamesLc.Add(colorProp.Name.ToLower());
            }
        }

        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private WinUIAdapter()
        {
            _device = CanvasDevice.GetSharedDevice();

            NetworkLoader = new HttpClientNetworkLoader(_sharedHttpClient, (Uri)null);

            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            try
            {
                var systemFonts = CanvasFontSet.GetSystemFontSet();
                var addedFamilies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var font in systemFonts.Fonts)
                {
                    var name = GetPreferredFamilyName(font.FamilyNames);
                    if (name != null && addedFamilies.Add(name))
                    {
                        try
                        {
                            AddFontFamily(new FontFamilyAdapter(name));
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
                // Without the system fonts, families are still resolved by name when the font is created
            }

            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += (sender, e) =>
            {
                var previous = _colorScheme;
                _colorScheme = null;
                if (previous.HasValue && previous.Value != SystemColorScheme)
                    OnColorSchemeChanged();
            };
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static WinUIAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// The shared Win2D device this adapter and every control it services draws with.
        /// </summary>
        public CanvasDevice Device
        {
            get { return _device; }
        }

        /// <summary>
        /// The user's app theme, cached until the system colors change.
        /// </summary>
        public override RColorScheme SystemColorScheme
        {
            get
            {
                if (SystemColorSchemeOverride.HasValue)
                    return SystemColorSchemeOverride.Value;
                if (!_colorScheme.HasValue)
                    _colorScheme = WindowsTheme.GetAppsColorScheme();
                return _colorScheme.Value;
            }
        }

        protected override RColor GetColorInt(string colorName)
        {
            if (!ValidColorNamesLc.Contains(colorName.ToLower()))
                return RColor.Empty;

            var prop = typeof(Colors).GetProperty(colorName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (prop == null)
                return RColor.Empty;

            var color = (Windows.UI.Color)prop.GetValue(null);
            return Utilities.Utils.Convert(color);
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(GetSolidColorBrush(color));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            return new BrushAdapter(GetSolidColorBrush(color));
        }

        protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            var gradientStops = new CanvasGradientStop[stops.Length];
            for (int i = 0; i < stops.Length; i++)
            {
                gradientStops[i] = new CanvasGradientStop
                {
                    Color = Utilities.Utils.Convert(stops[i].Color),
                    Position = (float)stops[i].Position,
                };
            }

            var brush = new CanvasLinearGradientBrush(_device, gradientStops);
            brush.StartPoint = new System.Numerics.Vector2((float)p1.X, (float)p1.Y);
            brush.EndPoint = new System.Numerics.Vector2((float)p2.X, (float)p2.Y);
            return new BrushAdapter(brush);
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image is CanvasBitmap bitmap ? new ImageAdapter(bitmap) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            // Blocking is fine as long as no continuation needs the calling thread, which may be the UI thread
            using (var memory = new MemoryStream())
            {
                memoryStream.CopyTo(memory);
                memory.Position = 0;
                using (var randomAccessStream = memory.AsRandomAccessStream())
                {
                    var bitmap = CanvasBitmap.LoadAsync(_device, randomAccessStream).AsTask().GetAwaiter().GetResult();
                    return new ImageAdapter(bitmap);
                }
            }
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            return new FontAdapter(_device, family, size, style);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            return new FontAdapter(_device, ((FontFamilyAdapter)family).TextFormatFamily, size, style);
        }

        protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath)
        {
            var faceDirectory = Path.Combine(_fontFaceTempDirectory, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(faceDirectory);
            var tempFilePath = Path.Combine(faceDirectory, "face.ttf");
            File.WriteAllBytes(tempFilePath, fontBytes);

            var fontFile = new Uri(tempFilePath);
            using (var fontSet = new CanvasFontSet(fontFile))
            {
                var name = fontSet.Fonts.Count > 0 ? GetPreferredFamilyName(fontSet.Fonts[0].FamilyNames) : null;
                return name != null ? new FontFamilyAdapter(name, fontFile) : null;
            }
        }

        protected override object GetClipboardDataObjectInt(string html, string plainText)
        {
            return new ClipboardHelper.Content(html, plainText);
        }

        protected override void SetToClipboardInt(string text)
        {
            ClipboardHelper.CopyToClipboard(text);
        }

        protected override void SetToClipboardInt(string html, string plainText)
        {
            ClipboardHelper.CopyToClipboard(html, plainText);
        }

        protected override void SetToClipboardInt(RImage image)
        {
            var bitmap = ((ImageAdapter)image).Bitmap;
            var stream = new InMemoryRandomAccessStream();
            bitmap.SaveAsync(stream, CanvasBitmapFileFormat.Png).AsTask().GetAwaiter().GetResult();

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
            Clipboard.SetContent(dataPackage);
        }

        protected override RContextMenu CreateContextMenuInt()
        {
            return new ContextMenuAdapter();
        }

        protected override void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            _ = SaveToFileAsync(((ImageAdapter)image).Bitmap, name, extension, (control as ControlAdapter)?.Control);
        }

        #region Private/Protected methods

        private static string CreateFontFaceTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "HtmlRenderer.FontFace." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Get solid color brush for the given color.
        /// </summary>
        private CanvasSolidColorBrush GetSolidColorBrush(RColor color)
        {
            return new CanvasSolidColorBrush(_device, Utilities.Utils.Convert(color));
        }

        /// <summary>
        /// Pick a family name from a font's family names, preferring the "en-us" one.
        /// </summary>
        internal static string GetPreferredFamilyName(IReadOnlyDictionary<string, string> familyNames)
        {
            if (familyNames == null || familyNames.Count == 0)
                return null;

            if (familyNames.TryGetValue("en-us", out var name))
                return name;

            foreach (var kvp in familyNames)
                return kvp.Value;

            return null;
        }

        /// <summary>
        /// Get the Win2D bitmap encoding format to use for the given file extension. Default is PNG.
        /// </summary>
        private static CanvasBitmapFileFormat GetBitmapFileFormat(string ext)
        {
            switch (ext.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return CanvasBitmapFileFormat.Jpeg;
                case ".bmp":
                    return CanvasBitmapFileFormat.Bmp;
                case ".tif":
                case ".tiff":
                    return CanvasBitmapFileFormat.Tiff;
                case ".gif":
                    return CanvasBitmapFileFormat.Gif;
                default:
                    return CanvasBitmapFileFormat.Png;
            }
        }

        /// <summary>
        /// Show a save file dialog and encode the image to the chosen file.
        /// </summary>
        /// <remarks>
        /// The picker of an unpackaged app needs the window handle, which is taken from the control. Without a
        /// control nothing is saved.
        /// </remarks>
        private static async Task SaveToFileAsync(CanvasBitmap bitmap, string name, string extension, UIElement element)
        {
            if (element?.XamlRoot == null)
                return;

            var hwnd = Win32Interop.GetWindowFromWindowId(element.XamlRoot.ContentIslandEnvironment.AppWindowId);
            if (hwnd == IntPtr.Zero)
                return;

            try
            {
                var picker = new FileSavePicker
                {
                    SuggestedFileName = name,
                    DefaultFileExtension = extension,
                };
                picker.FileTypeChoices.Add("Image", new List<string> { ".png", ".bmp", ".jpg", ".tif", ".gif" });
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSaveFileAsync();
                if (file == null)
                    return;

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    stream.Size = 0;
                    await bitmap.SaveAsync(stream, GetBitmapFileFormat(Path.GetExtension(file.Path)));
                }
            }
            catch (Exception ex)
            {
                (element as HtmlControl)?.ReportError(HtmlRenderErrorType.Image, "Failed to save the image", ex);
            }
        }

        #endregion
    }
}
