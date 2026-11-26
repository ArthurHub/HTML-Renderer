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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Network;
using TheArtOfDev.HtmlRenderer.WPF.Utilities;
using Microsoft.Win32;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF platform.
    /// </summary>
    internal sealed class WpfAdapter : RAdapter
    {
        #region Fields and Consts

        // One HttpClient shared for the adapter's (process) lifetime, not one per request - `new
        // HttpClient()` per call is a well-documented anti-pattern that exhausts sockets under load and
        // never observes DNS changes.
        //
        // Declared BEFORE _instance deliberately: C# runs static field initializers in textual
        // declaration order, and _instance's own initializer (`new WpfAdapter()`) runs the instance
        // constructor immediately, which reads _sharedHttpClient on its very first line. If this field
        // were declared after _instance, that read would observe _sharedHttpClient's still-default value
        // (null - its own initializer hasn't run yet) and permanently capture a null HttpClient into
        // NetworkLoader, since HttpClientNetworkLoader takes it as a constructor parameter, not a live
        // reference to this field. (Confirmed by a real crash with this exact ordering, in the sibling
        // WinFormsAdapter.)
        private static readonly HttpClient _sharedHttpClient = new HttpClient();

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly WpfAdapter _instance = new WpfAdapter();

        /// <summary>
        /// List of valid predefined color names in lower-case
        /// </summary>
        private static readonly List<string> ValidColorNamesLc;

        // Backs LoadFontFaceFontInt's temp-file registration (see its own doc comment for why a real file
        // is necessary - WPF's font-loading APIs are documented as file-URI-only, with no supported
        // memory-only path) - one directory per process, cleaned up by the OS's normal temp-file
        // housekeeping, not by this process.
        private static readonly string _fontFaceTempDirectory = CreateFontFaceTempDirectory();

        private static string CreateFontFaceTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "HtmlRenderer.FontFace." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        #endregion

        static WpfAdapter()
        {
            ValidColorNamesLc = new List<string>();
            var colorList = new List<PropertyInfo>(typeof(Colors).GetProperties());
            foreach (var colorProp in colorList)
            {
                ValidColorNamesLc.Add(colorProp.Name.ToLower());
            }
        }

        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private WpfAdapter()
        {
            // Unlike the PdfSharp backend (which keeps the base RAdapter.NetworkLoader default of
            // DataUriNetworkLoader-only - safer for unattended/server-side PDF generation, matching
            // PeachPDF's own default), WPF is an interactive UI backend where fetching a real http(s):
            // image or stylesheet out of the box is the expected behavior. data:/file: URIs still resolve
            // the same way regardless (RAdapter.GetResourceStreamAsync intercepts both before ever consulting
            // NetworkLoader), so only http(s): actually reaches this loader in practice.
            NetworkLoader = new HttpClientNetworkLoader(_sharedHttpClient, (Uri)null);

            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in Fonts.SystemFontFamilies)
            {
	            try
	            {
	                AddFontFamily(new FontFamilyAdapter(family));
	            }
	            catch
	            {
	            }
            }

            SystemEvents.UserPreferenceChanged += (sender, e) =>
            {
                if (e.Category != UserPreferenceCategory.General) return;

                // The General category covers far more than the theme, so re-read and only report a
                // change if the scheme really moved - otherwise every unrelated preference change
                // would force a re-cascade and repaint.
                var previous = _colorScheme;
                _colorScheme = null;
                if (previous.HasValue && previous.Value != SystemColorScheme)
                    OnColorSchemeChanged();
            };
        }

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        public static WpfAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Rendering onto a Windows surface, so the document should follow the user's app theme.
        /// Cached and invalidated on a system preference change rather than read per query.
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

        /// <summary>
        /// Cached app theme; null when it needs to be re-read.
        /// </summary>
        private RColorScheme? _colorScheme;

        protected override RColor GetColorInt(string colorName)
        {
            // check if color name is valid to avoid ColorConverter throwing an exception
            if (!ValidColorNamesLc.Contains(colorName.ToLower()))
                return RColor.Empty;

            var convertFromString = ColorConverter.ConvertFromString(colorName) ?? Colors.Black;
            return Utils.Convert((Color)convertFromString);
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(GetSolidColorBrush(color));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            var solidBrush = GetSolidColorBrush(color);
            return new BrushAdapter(solidBrush);
        }

        protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            var gradientStops = new GradientStopCollection(stops.Length);
            foreach (var stop in stops)
                gradientStops.Add(new GradientStop(Utils.Convert(stop.Color), stop.Position));

            var brush = new LinearGradientBrush(gradientStops, 0)
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = Utils.Convert(p1),
                EndPoint = Utils.Convert(p2)
            };
            return new BrushAdapter(brush);
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((BitmapImage)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return new ImageAdapter(bitmap);
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString(family) ?? new FontFamily();
            return new FontAdapter(new Typeface(fontFamily, GetFontStyle(style), GetFontWidth(style), FontStretches.Normal), size);
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            return new FontAdapter(new Typeface(((FontFamilyAdapter)family).FontFamily, GetFontStyle(style), GetFontWidth(style), FontStretches.Normal), size);
        }

        /// <summary>
        /// Loads one <c>@font-face</c> face's bytes as a WPF <see cref="System.Windows.Media.FontFamily"/>
        /// via a temp file and <see cref="Fonts.GetFontFamilies(Uri)"/> - WPF's own documented way to load
        /// a font from an arbitrary location.
        /// </summary>
        /// <remarks>
        /// Two earlier approaches were tried and rejected first: the Win32 <c>AddFontMemResourceEx</c> API
        /// registers with GDI, but WPF's text stack (DirectWrite-based) never consults GDI's per-process
        /// font table, so registered faces were silently invisible to WPF. A fully in-memory
        /// <see cref="System.Net.WebRequest"/>-scheme trick (serving the bytes from a
        /// <see cref="MemoryStream"/> for a synthetic URI, the same mechanism that historically let WPF/XBAP
        /// apps reference fonts over plain <c>http://</c>) was also tried and confirmed NOT to work: WPF's
        /// own source documents <c>Fonts.GetFontFamilies</c>'s location parameter as "must be an absolute
        /// file URI or path" - empirically, both the eager folder-scan API and the lazy
        /// <c>new FontFamily(baseUri, "./file#Name")</c> reference came back empty against the synthetic
        /// scheme even though the handler correctly served the bytes. There is no supported WPF API for
        /// loading a font from memory alone, so - like WinForms' own <c>PrivateFontCollection.AddFontFile</c>
        /// path, for its own different reason (GDI+'s <c>AddMemoryFont</c> being unreliable, not a
        /// fundamental API gap) - this writes to a real, never-deleted temp file.
        /// </remarks>
        /// <remarks>
        /// Each face gets its OWN, never-reused temp subdirectory - not one shared directory for every
        /// face. WPF's font-family folder enumeration caches its scan per directory and does not notice
        /// files added to that directory after the first scan (confirmed empirically: with a single shared
        /// directory, every face after the first one silently resolved back to the first face's glyphs,
        /// because <see cref="Fonts.GetFontFamilies(Uri)"/>'s first call had already cached "what's in this
        /// folder" before the later faces' files existed). A fresh, single-file directory per face sidesteps
        /// that cache entirely.
        /// </remarks>
        protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath)
        {
            var faceDirectory = Path.Combine(_fontFaceTempDirectory, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(faceDirectory);
            var tempFilePath = Path.Combine(faceDirectory, "face.ttf");
            File.WriteAllBytes(tempFilePath, fontBytes);

            var family = Fonts.GetFontFamilies(new Uri(tempFilePath)).FirstOrDefault();

            return family != null ? new FontFamilyAdapter(family) : null;
        }

        protected override object GetClipboardDataObjectInt(string html, string plainText)
        {
            return ClipboardHelper.CreateDataObject(html, plainText);
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
            Clipboard.SetImage(((ImageAdapter)image).Image);
        }

        protected override RContextMenu CreateContextMenuInt()
        {
            return new ContextMenuAdapter();
        }

        protected override void SaveToFileInt(RImage image, string name, string extension, RControl control = null)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Images|*.png;*.bmp;*.jpg;*.tif;*.gif;*.wmp;";
            saveDialog.FileName = name;
            saveDialog.DefaultExt = extension;

            var dialogResult = saveDialog.ShowDialog();
            if (dialogResult.GetValueOrDefault())
            {
                var encoder = Utils.GetBitmapEncoder(Path.GetExtension(saveDialog.FileName));
                encoder.Frames.Add(BitmapFrame.Create(((ImageAdapter)image).Image));
                using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.OpenOrCreate))
                    encoder.Save(stream);
            }
        }


        #region Private/Protected methods

        /// <summary>
        /// Get solid color brush for the given color.
        /// </summary>
        private static Brush GetSolidColorBrush(RColor color)
        {
            Brush solidBrush;
            if (color == RColor.White)
                solidBrush = Brushes.White;
            else if (color == RColor.Black)
                solidBrush = Brushes.Black;
            else if (color.A < 1)
                solidBrush = Brushes.Transparent;
            else
                solidBrush = new SolidColorBrush(Utils.Convert(color));
            return solidBrush;
        }

        /// <summary>
        /// Get WPF font style for the given style.
        /// </summary>
        private static FontStyle GetFontStyle(RFontStyle style)
        {
            if ((style & RFontStyle.Italic) == RFontStyle.Italic)
                return FontStyles.Italic;

            return FontStyles.Normal;
        }

        /// <summary>
        /// Get WPF font style for the given style.
        /// </summary>
        private static FontWeight GetFontWidth(RFontStyle style)
        {
            if ((style & RFontStyle.Bold) == RFontStyle.Bold)
                return FontWeights.Bold;

            return FontWeights.Normal;
        }

        #endregion
    }
}