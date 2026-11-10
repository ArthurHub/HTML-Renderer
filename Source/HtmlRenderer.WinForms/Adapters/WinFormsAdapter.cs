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
using System.Drawing.Text;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.Network;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinForms.Utilities;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms platforms.
    /// </summary>
    internal sealed class WinFormsAdapter : RAdapter
    {
        #region Fields and Consts

        // One HttpClient shared for the adapter's (process) lifetime, not one per request - `new
        // HttpClient()` per call is a well-documented anti-pattern that exhausts sockets under load and
        // never observes DNS changes.
        //
        // Declared BEFORE _instance deliberately: C# runs static field initializers in textual
        // declaration order, and _instance's own initializer (`new WinFormsAdapter()`) runs the instance
        // constructor immediately, which reads _sharedHttpClient on its very first line. If this field
        // were declared after _instance, that read would observe _sharedHttpClient's still-default value
        // (null - its own initializer hasn't run yet) and permanently capture a null HttpClient into
        // NetworkLoader, since HttpClientNetworkLoader takes it as a constructor parameter, not a live
        // reference to this field. (Confirmed by a real crash with this exact ordering.)
        private static readonly HttpClient _sharedHttpClient = new HttpClient();

        /// <summary>
        /// Singleton instance of global adapter.
        /// </summary>
        private static readonly WinFormsAdapter _instance = new WinFormsAdapter();

        // The @font-face faces, kept for the lifetime of the process.
        private readonly PrivateFontCollection _fontFaceCollection = new PrivateFontCollection();

        // The font files of the @font-face faces, see LoadFontFaceFontInt. Left to the OS to clean up.
        private static readonly string _fontFaceTempDirectory = CreateFontFaceTempDirectory();

        private static string CreateFontFaceTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "HtmlRenderer.FontFace." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        #endregion


        /// <summary>
        /// Init installed font families and set default font families mapping.
        /// </summary>
        private WinFormsAdapter()
        {
            // Unlike the PdfSharp backend (which keeps the base RAdapter.NetworkLoader default of
            // DataUriNetworkLoader-only - safer for unattended/server-side PDF generation, matching
            // PeachPDF's own default), WinForms is an interactive UI backend where fetching a real
            // http(s): image or stylesheet out of the box is the expected behavior. data:/file: URIs
            // still resolve the same way regardless (RAdapter.GetResourceStream intercepts both before
            // ever consulting NetworkLoader), so only http(s): actually reaches this loader in practice.
            NetworkLoader = new HttpClientNetworkLoader(_sharedHttpClient, (Uri)null);

            AddFontFamilyMapping("monospace", "Courier New");
            AddFontFamilyMapping("Helvetica", "Arial");

            foreach (var family in FontFamily.Families)
            {
                AddFontFamily(new FontFamilyAdapter(family));
            }

            Microsoft.Win32.SystemEvents.UserPreferenceChanged += (sender, e) =>
            {
                if (e.Category != Microsoft.Win32.UserPreferenceCategory.General) return;

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
        public static WinFormsAdapter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Rendering onto a Windows control, so the document should follow the user's app theme.
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
            var color = Color.FromName(colorName);
            return Utils.Convert(color);
        }

        protected override RPen CreatePen(RColor color)
        {
            return new PenAdapter(new Pen(Utils.Convert(color)));
        }

        protected override RBrush CreateSolidBrush(RColor color)
        {
            Brush solidBrush;
            if (color == RColor.White)
                solidBrush = Brushes.White;
            else if (color == RColor.Black)
                solidBrush = Brushes.Black;
            else if (color.A < 1)
                solidBrush = Brushes.Transparent;
            else
                solidBrush = new SolidBrush(Utils.Convert(color));

            return new BrushAdapter(solidBrush, false);
        }

        protected override RBrush CreateLinearGradientBrush(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            var brush = new LinearGradientBrush(Utils.Convert(p1), Utils.Convert(p2), Color.Black, Color.Black);

            var colors = new Color[stops.Length];
            var positions = new float[stops.Length];
            for (int i = 0; i < stops.Length; i++)
            {
                colors[i] = Utils.Convert(stops[i].Color);
                var pos = (float)Math.Min(Math.Max(stops[i].Position, 0.0), 1.0);
                // GDI+ requires strictly increasing positions - nudge any duplicate up by an epsilon.
                positions[i] = i > 0 && pos <= positions[i - 1] ? positions[i - 1] + 0.0001f : pos;
            }
            // GDI+ requires the first/last position to be exactly 0/1 - forcing them here (rather than
            // requiring the caller to pre-normalize) also matches spec behavior for a gradient whose
            // outermost stops aren't at the very ends: the outermost color simply extends flat to the edge.
            positions[0] = 0f;
            positions[positions.Length - 1] = 1f;

            brush.InterpolationColors = new ColorBlend
            {
                Colors = colors,
                Positions = positions
            };

            return new BrushAdapter(brush, true);
        }

        protected override RImage ConvertImageInt(object image)
        {
            return image != null ? new ImageAdapter((Image)image) : null;
        }

        protected override RImage ImageFromStreamInt(Stream memoryStream)
        {
            return new ImageAdapter(Image.FromStream(memoryStream));
        }

        protected override RFont CreateFontInt(string family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(family, (float)size, fontStyle));
        }

        protected override RFont CreateFontInt(RFontFamily family, double size, RFontStyle style)
        {
            var fontStyle = (FontStyle)((int)style);
            return new FontAdapter(new Font(((FontFamilyAdapter)family).FontFamily, (float)size, fontStyle));
        }

        /// <summary>
        /// Adds the font to the <see cref="PrivateFontCollection"/> and returns its family, found by the
        /// family name in the font file. <see cref="PrivateFontCollection.Families"/> groups the faces of
        /// a family and isn't in insertion order.
        /// </summary>
        /// <remarks>
        /// Goes through a temp file, <see cref="PrivateFontCollection.AddMemoryFont"/> doesn't reliably
        /// list the added families. The file can't be deleted while the collection uses it.
        /// </remarks>
        protected override RFontFamily LoadFontFaceFontInt(byte[] fontBytes, string filePath)
        {
            string familyName;
            using (var stream = new MemoryStream(fontBytes))
            {
                familyName = TtfFontDescription.LoadDescription(stream).FontFamilyInvariantCulture;
            }

            if (string.IsNullOrEmpty(familyName))
            {
                return null;
            }

            var tempFilePath = Path.Combine(_fontFaceTempDirectory, Guid.NewGuid().ToString("N") + ".ttf");
            File.WriteAllBytes(tempFilePath, fontBytes);
            _fontFaceCollection.AddFontFile(tempFilePath);

            foreach (var family in _fontFaceCollection.Families)
            {
                if (string.Equals(family.Name, familyName, StringComparison.OrdinalIgnoreCase))
                {
                    return new FontFamilyAdapter(family);
                }
            }

            return null;
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
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";
                saveDialog.FileName = name;
                saveDialog.DefaultExt = extension;

                var dialogResult = control == null ? saveDialog.ShowDialog() : saveDialog.ShowDialog(((ControlAdapter)control).Control);
                if (dialogResult == DialogResult.OK)
                {
                    ((ImageAdapter)image).Image.Save(saveDialog.FileName);
                }
            }
        }
    }
}