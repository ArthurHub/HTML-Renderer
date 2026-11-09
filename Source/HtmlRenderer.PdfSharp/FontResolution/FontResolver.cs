using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using PdfSharp.Fonts;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    /// <summary>
    /// PDFsharp <see cref="IFontResolver"/> of the PdfSharp backend: system font discovery and
    /// CSS Fonts 4 §5 face matching (slant, stretch, weight). PDFsharp needs the font bytes for embedding,
    /// so this does its own matching instead of going through <c>Core.Handlers.FontsHandler</c>.
    /// </summary>
    /// <remarks>
    /// A face without <c>unicode-range</c> is assumed to cover everything, its glyphs aren't checked,
    /// there's one font per box anyway. The resolver is a process-wide singleton (<see cref="Register"/>).
    /// On Linux, fonts are found with the <c>fc-list</c> CLI, see <see cref="LinuxSystemFontResolver"/>.
    /// </remarks>
    public sealed class FontResolver : IFontResolver
    {
        public const string FallbackFont = "Tuffy";

        private static readonly string[] FontExtensions = { "*.ttf", "*.otf" };

        private static readonly Dictionary<string, string> _systemFontPaths;
        private static readonly Dictionary<string, FontFamilyModel> _systemFamilies;

        private readonly Dictionary<string, byte[]> _customFonts = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        private readonly Dictionary<string, FontFamilyModel> _installedFonts;
        private readonly List<string> _customFontDirectories = new List<string>();

        /// <summary>
        /// When true, a family/codepoint that can't be resolved returns null instead of falling back to
        /// the embedded Tuffy font - lets a caller detect "nothing matched" instead of silently
        /// substituting a visually different fallback face.
        /// </summary>
        public bool NullIfFontNotFound { get; set; }

        static FontResolver()
        {
            var supportedFonts = DiscoverSupportedFonts();
            var parsed = ParseSystemFonts(supportedFonts);
            _systemFontPaths = parsed.Paths;
            _systemFamilies = parsed.Families;
        }

        public FontResolver()
        {
            _installedFonts = new Dictionary<string, FontFamilyModel>(_systemFamilies, StringComparer.Ordinal);
        }

        /// <summary>Registers a new <see cref="FontResolver"/> instance as PDFsharp's process-wide global resolver.</summary>
        public static FontResolver Register()
        {
            var fontResolver = new FontResolver();
            GlobalFontSettings.FontResolver = fontResolver;
            return fontResolver;
        }

        #region OS font discovery

        private static string[] GetFontFiles(string dir)
        {
            if (!Directory.Exists(dir))
                return Array.Empty<string>();

            try
            {
                return FontExtensions
                    .SelectMany(pattern => Directory.GetFiles(dir, pattern, SearchOption.AllDirectories))
                    .ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                // Some directories may exist but be unreadable depending on OS/permission configuration -
                // treat that the same as "no fonts here" rather than failing discovery entirely.
                return Array.Empty<string>();
            }
        }

        internal static string[] DiscoverSupportedFonts()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var homeDir = Environment.GetEnvironmentVariable("HOME");
                var candidateDirs = new List<string> { "/System/Library/Fonts", "/Library/Fonts" };
                if (!string.IsNullOrEmpty(homeDir))
                    candidateDirs.Add(Path.Combine(homeDir, "Library", "Fonts"));

                return candidateDirs.SelectMany(GetFontFiles).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxSystemFontResolver.Resolve();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var fontDir = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts");
                var fontPaths = new List<string>(GetFontFiles(fontDir));

                // Covers per-user-installed fonts (Windows 10+ lets a non-admin install fonts for just
                // their own account), which the machine-wide Fonts folder above doesn't.
                var appdataFontDir = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\Windows\Fonts");
                fontPaths.AddRange(GetFontFiles(appdataFontDir));

                return fontPaths.ToArray();
            }

            // No system font discovery on this platform - start with nothing and rely on fonts registered
            // via AddFont/RegisterCustomFontDirectory.
            return Array.Empty<string>();
        }

        private static (Dictionary<string, string> Paths, Dictionary<string, FontFamilyModel> Families) ParseSystemFonts(string[] supportedFonts)
        {
            var fontPaths = new Dictionary<string, string>(StringComparer.Ordinal);
            var descriptions = new List<TtfFontDescription>();

            foreach (var fontPathFile in supportedFonts)
            {
                try
                {
                    var description = TtfFontDescription.LoadDescription(fontPathFile);
                    descriptions.Add(description);

                    if (!fontPaths.ContainsKey(description.FontNameInvariantCulture))
                        fontPaths.Add(description.FontNameInvariantCulture, fontPathFile);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            var families = new Dictionary<string, FontFamilyModel>(StringComparer.Ordinal);

            foreach (var familyGroup in descriptions.GroupBy(d => d.FontFamilyInvariantCulture))
            {
                try
                {
                    var familyName = familyGroup.Key;
                    var family = DeserializeFontFamily(familyName, familyGroup);
                    families[familyName.ToLowerInvariant()] = family;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            return (fontPaths, families);
        }

        private static FontFamilyModel DeserializeFontFamily(string fontFamilyName, IEnumerable<TtfFontDescription> descriptions)
        {
            var family = new FontFamilyModel { Name = fontFamilyName };

            foreach (var description in descriptions)
            {
                var isItalic = (description.Style & RFontStyle.Italic) != 0;
                // System fonts declare no explicit unicode-range - their effective coverage is
                // "everything" (see this type's own doc comment). Keep the first face seen per
                // (weight, italic, stretch).
                if (!family.Faces.Any(f => f.Weight == description.Weight && f.Italic == isItalic && f.Stretch == description.Stretch))
                    family.Faces.Add(new FontFaceEntry(description.Weight, isItalic, description.Stretch, null, description));
            }

            return family;
        }

        #endregion

        #region Registration (@font-face src: url()/local(), custom directories)

        /// <summary>Registers a font under <paramref name="fontFamilyName"/>, using the values sniffed from the file itself.</summary>
        public void AddFont(Stream stream, string fontFamilyName)
        {
            AddFont(stream, fontFamilyName, null, null, null, null);
        }

        /// <summary>
        /// Registers a font under <paramref name="fontFamilyName"/>, optionally overriding the face's own
        /// sniffed weight/style/stretch with the values an <c>@font-face</c> rule declared for it - those
        /// descriptors are authoritative for how that specific resource participates in matching,
        /// independent of what the file's own internal tables say. Null means "use the value sniffed from
        /// the file itself". <paramref name="unicodeRanges"/> restricts which codepoints this face is used
        /// for; null means "covers whatever is asked of it" (see this type's own doc comment).
        /// </summary>
        public void AddFont(Stream stream, string fontFamilyName, int? weightOverride, bool? isItalicOverride, int? stretchOverride, IReadOnlyList<CodepointRange> unicodeRanges)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var fontBytes = memoryStream.ToArray();
            memoryStream.Seek(0, SeekOrigin.Begin);

            var description = TtfFontDescription.LoadDescription(memoryStream);

            var weight = weightOverride ?? description.Weight;
            var isItalic = isItalicOverride ?? (description.Style & RFontStyle.Italic) != 0;
            var stretch = stretchOverride ?? description.Stretch;

            // The face name is the identity under which the bytes are stored and later fetched (GetFont)
            // for embedding - normally the font's own internal name. But two DIFFERENT fonts can share one
            // internal name (a common webfont-subset pattern, e.g. every "Roboto" subset file reports
            // "Roboto"); those must not collide in _customFonts (the second would overwrite the first's
            // bytes), so disambiguate with a content checksum when that happens.
            var internalName = description.FontNameInvariantCulture;
            var faceName = internalName;
            if (_customFonts.TryGetValue(internalName, out var existingBytes) && !ByteArraysEqual(existingBytes, fontBytes))
            {
                faceName = internalName + "#" + ComputeChecksum(fontBytes).ToString("x", CultureInfo.InvariantCulture);
            }

            var faceDescription = new TtfFontDescription(description.FontFamilyInvariantCulture, faceName, description.Style, weight, stretch);

            RegisterFace(fontFamilyName, new FontFaceEntry(weight, isItalic, stretch, unicodeRanges, faceDescription));
            _customFonts[faceName] = fontBytes;
        }

        /// <summary>
        /// Satisfies an <c>@font-face</c> <c>src: local(...)</c> candidate: finds the nearest face already
        /// registered under <paramref name="localFamilyName"/> (a system font or an earlier registration)
        /// for the given axes, and registers that same face's bytes as a face of
        /// <paramref name="familyName"/> too - reusing the OS font's embedded bytes, not copying them.
        /// </summary>
        /// <returns>true if a local family by that name was found and registered, false otherwise (the caller tries the next <c>src</c> candidate)</returns>
        public bool AddLocalFontFamily(string familyName, string localFamilyName, int? weightOverride, bool? isItalicOverride, int? stretchOverride, IReadOnlyList<CodepointRange> ranges)
        {
            if (!_installedFonts.TryGetValue(localFamilyName.ToLowerInvariant(), out var localFamily) || localFamily.Faces.Count == 0)
                return false;

            var weight = weightOverride ?? TtfFontDescription.DefaultWeight;
            var isItalic = isItalicOverride ?? false;
            var stretch = stretchOverride ?? TtfFontDescription.DefaultStretch;

            if (!TryFindNearestFace(localFamily, weight, isItalic, stretch, null, out var sourceFace))
                return false;

            var entry = new FontFaceEntry(
                weightOverride ?? sourceFace.Weight,
                isItalicOverride ?? sourceFace.Italic,
                stretchOverride ?? sourceFace.Stretch,
                ranges,
                sourceFace.Description);

            RegisterFace(familyName, entry);
            return true;
        }

        /// <summary>
        /// Registers <paramref name="fontDirectory"/> to be scanned for TTF/OTF files, each registered
        /// under its own sniffed family name (unlike <see cref="AddFont(Stream,string)"/>, which requires
        /// the caller to already know the family name up front).
        /// </summary>
        public void RegisterCustomFontDirectory(string fontDirectory)
        {
            if (_customFontDirectories.Contains(fontDirectory, StringComparer.OrdinalIgnoreCase))
                return;

            _customFontDirectories.Add(fontDirectory);

            foreach (var path in GetFontFiles(fontDirectory))
            {
                try
                {
                    var description = TtfFontDescription.LoadDescription(path);
                    using (var stream = File.OpenRead(path))
                    {
                        AddFont(stream, description.FontFamilyInvariantCulture);
                    }
                }
                catch
                {
                    // Not every *.ttf/*.otf found in a directory scan is necessarily a valid/readable font
                    // file - skip it and keep discovering the rest.
                }
            }
        }

        /// <summary>Every family name currently registered (system-discovered, plus any added via <see cref="AddFont(Stream,string)"/>/<see cref="RegisterCustomFontDirectory"/>).</summary>
        public List<string> DiscoverFontFamilies()
        {
            return _installedFonts.Values.Select(f => f.Name).ToList();
        }

        /// <summary>
        /// Replaces any existing same-slot face (same weight/italic/stretch/unicode-range - a
        /// re-registration) while letting a same-axes face with a different range set coexist (the
        /// unicode-range subset case), then adds <paramref name="entry"/>. Clones the family before
        /// mutating: <paramref name="familyName"/> may currently resolve to the shared static
        /// <c>_systemFamilies</c> snapshot (or an already-private clone from a prior call), and this must
        /// never write into state shared across instances.
        /// </summary>
        private void RegisterFace(string familyName, FontFaceEntry entry)
        {
            var key = familyName.ToLowerInvariant();
            _installedFonts.TryGetValue(key, out var existingFamily);

            var clonedFamily = new FontFamilyModel { Name = existingFamily?.Name ?? familyName };
            if (existingFamily != null)
            {
                foreach (var face in existingFamily.Faces)
                {
                    if (!IsSameFaceSlot(face, entry.Weight, entry.Italic, entry.Stretch, entry.ExplicitRanges))
                        clonedFamily.Faces.Add(face);
                }
            }

            clonedFamily.Faces.Add(entry);
            _installedFonts[key] = clonedFamily;
        }

        private static bool IsSameFaceSlot(FontFaceEntry entry, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            return entry.Weight == weight && entry.Italic == isItalic && entry.Stretch == stretch && RangesEqual(entry.ExplicitRanges, ranges);
        }

        private static bool RangesEqual(IReadOnlyList<CodepointRange> a, IReadOnlyList<CodepointRange> b)
        {
            if (a == null || b == null)
                return a == null && b == null;
            if (a.Count != b.Count)
                return false;
            for (var i = 0; i < a.Count; i++)
            {
                if (a[i].Start != b[i].Start || a[i].End != b[i].End)
                    return false;
            }
            return true;
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        // A cheap, non-cryptographic content hash - only used to disambiguate two different font files
        // that happen to report the same internal name (see AddFont), not for any security purpose.
        private static uint ComputeChecksum(byte[] bytes)
        {
            unchecked
            {
                const uint fnvPrime = 16777619;
                var hash = 2166136261;
                foreach (var b in bytes)
                {
                    hash ^= b;
                    hash *= fnvPrime;
                }
                return hash;
            }
        }

        #endregion

        #region IFontResolver

        public byte[] GetFont(string faceName)
        {
            if (_customFonts.TryGetValue(faceName, out var fontBytes))
                return fontBytes;

            if (_systemFontPaths.TryGetValue(faceName, out var fontPath) && File.Exists(fontPath))
                return File.ReadAllBytes(fontPath);

            if (faceName != null && faceName.StartsWith(FallbackFont, StringComparison.Ordinal))
            {
                var embedded = LoadEmbeddedFallbackFont(faceName);
                if (embedded != null)
                    return embedded;
            }

            throw new ArgumentOutOfRangeException(nameof(faceName), faceName, "Unknown font face name.");
        }

        public bool HasFont(string faceName)
        {
            return _customFonts.ContainsKey(faceName) || _systemFontPaths.ContainsKey(faceName);
        }

        /// <summary>
        /// Whether <paramref name="familyName"/> is registered - a system-discovered family, or one
        /// registered via <see cref="AddFont(Stream,string,int?,bool?,int?,IReadOnlyList{CodepointRange})"/>/
        /// <see cref="AddLocalFontFamily"/>. Used by <see cref="Adapters.PdfSharpAdapter.IsFontExists"/> so
        /// <c>@font-face</c>-only families (registered here, not in the shared <c>FontsHandler</c> - see
        /// this type's own doc comment) are still recognized by <see cref="Core.Parse.CssParser.ParseFontFamily"/>.
        /// </summary>
        public bool HasFamily(string familyName)
        {
            return !string.IsNullOrEmpty(familyName) && _installedFonts.ContainsKey(familyName.ToLowerInvariant());
        }

        /// <summary>Whether any face of <paramref name="familyName"/> declares an explicit <c>unicode-range</c>.</summary>
        public bool HasExplicitRanges(string familyName)
        {
            return _installedFonts.TryGetValue(familyName.ToLowerInvariant(), out var family)
                   && family.Faces.Any(f => f.ExplicitRanges != null);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
            ResolveTypeface(familyName, isBold ? 700 : 400, isItalic);

        public FontResolverInfo ResolveTypeface(string familyName, int weight, bool isItalic) =>
            ResolveTypeface(familyName, weight, isItalic, TtfFontDescription.DefaultStretch);

        public FontResolverInfo ResolveTypeface(string familyName, int weight, bool isItalic, int stretch) =>
            ResolveTypeface(familyName, weight, isItalic, stretch, null);

        /// <summary>
        /// Resolves a face for <paramref name="familyName"/> at the requested axes, optionally restricted
        /// to faces whose <c>unicode-range</c> covers <paramref name="codepoint"/>. A codepoint-scoped
        /// request that finds no covering face returns null, so per-codepoint matching can move on to the
        /// next family instead of substituting a face that cannot render the character. A codepoint-less
        /// request keeps the previous behavior (no coverage filter, plus a fallback rather than null/throw).
        /// </summary>
        public FontResolverInfo ResolveTypeface(string familyName, int weight, bool isItalic, int stretch, int? codepoint)
        {
            if (!string.IsNullOrEmpty(familyName) && _installedFonts.TryGetValue(familyName.ToLowerInvariant(), out var family))
            {
                if (TryFindNearestFace(family, weight, isItalic, stretch, codepoint, out var face))
                {
                    // The chosen face may be a compromise (nearest-weight/slant match, not exact) - decide
                    // whether the gap is large enough that faux-bold/italic synthesis should kick in.
                    // Threshold mirrors the common UA convention that weights >=600 read as "bold".
                    var mustSimulateBold = weight >= 600 && face.Weight < 600;
                    var mustSimulateItalic = isItalic && !face.Italic;
                    return new FontResolverInfo(face.Description.FontNameInvariantCulture, mustSimulateBold, mustSimulateItalic);
                }
            }

            // A codepoint-scoped miss must not substitute an arbitrary non-covering face - report it so
            // the caller tries the next family (and ultimately the box default).
            if (codepoint.HasValue)
                return null;

            if (NullIfFontNotFound)
                return null;

            return new FontResolverInfo(StylizeTuffyFaceName(weight, isItalic));
        }

        /// <summary>
        /// CSS Fonts Level 4 §5 face matching. When <paramref name="codepoint"/> is supplied, only faces
        /// whose <c>unicode-range</c> includes it (or which declare none at all - see this type's own doc
        /// comment) are candidates; among equally-good matches the last-declared wins (CSS cascade order
        /// for overlapping ranges). Otherwise every face is a candidate. Within the candidates it narrows
        /// italic/slant first, then stretch, then weight; an exact axis match short-circuits.
        /// </summary>
        private static bool TryFindNearestFace(FontFamilyModel family, int weight, bool isItalic, int stretch, int? codepoint, out FontFaceEntry face)
        {
            face = null;

            var covering = codepoint.HasValue
                ? family.Faces.Where(f => FaceCovers(f, codepoint.Value)).ToList()
                : family.Faces;

            if (covering.Count == 0)
                return false;

            var exact = covering.Where(f => f.Weight == weight && f.Italic == isItalic && f.Stretch == stretch).ToList();
            if (exact.Count > 0)
            {
                face = exact[exact.Count - 1];
                return true;
            }

            var sameSlant = covering.Where(f => f.Italic == isItalic).ToList();
            var candidates = sameSlant.Count > 0 ? sameSlant : covering;

            var availableStretches = candidates.Select(f => f.Stretch).Distinct().ToList();
            var chosenStretch = PickNearestStretch(availableStretches, stretch);
            var stretchCandidates = candidates.Where(f => f.Stretch == chosenStretch).ToList();

            var availableWeights = stretchCandidates.Select(f => f.Weight).Distinct().ToList();
            var chosenWeight = PickNearestWeight(availableWeights, weight);

            face = stretchCandidates.Last(f => f.Weight == chosenWeight);
            return true;
        }

        private static bool FaceCovers(FontFaceEntry entry, int codepoint) =>
            entry.ExplicitRanges == null || UnicodeRangeParser.Covers(entry.ExplicitRanges, codepoint);

        /// <summary>
        /// CSS Fonts Level 4 §5.2's nearest-stretch search order: a target at or narrower than normal (5)
        /// searches narrower first (down to 1), then wider; a target wider than normal searches wider
        /// first (up to 9), then narrower. <paramref name="availableStretches"/> must be non-empty.
        /// </summary>
        private static int PickNearestStretch(List<int> availableStretches, int target)
        {
            if (availableStretches.Contains(target))
                return target;

            var candidates = target <= TtfFontDescription.DefaultStretch
                ? availableStretches.Where(s => s < target).OrderByDescending(s => s)
                    .Concat(availableStretches.Where(s => s > target).OrderBy(s => s))
                : availableStretches.Where(s => s > target).OrderBy(s => s)
                    .Concat(availableStretches.Where(s => s < target).OrderByDescending(s => s));

            return candidates.First();
        }

        /// <summary>
        /// CSS Fonts Level 4 §5.2's nearest-weight search order (the standard browser algorithm): a target
        /// in [400,500] searches upward to 500 first, then below the target, then above 500; a target
        /// below 400 searches downward first, then upward; a target above 500 searches upward first, then
        /// downward. <paramref name="availableWeights"/> must be non-empty and is assumed to NOT already
        /// contain an exact match for <paramref name="target"/> (the caller checks that separately, since
        /// an exact match also has to match the requested italic-ness, which this purely-numeric helper
        /// doesn't know about).
        /// </summary>
        private static int PickNearestWeight(List<int> availableWeights, int target)
        {
            IEnumerable<int> candidates;
            if (target >= 400 && target <= 500)
            {
                candidates = availableWeights.Where(w => w >= target && w <= 500).OrderBy(w => w)
                    .Concat(availableWeights.Where(w => w < target).OrderByDescending(w => w))
                    .Concat(availableWeights.Where(w => w > 500).OrderBy(w => w));
            }
            else if (target < 400)
            {
                candidates = availableWeights.Where(w => w < target).OrderByDescending(w => w)
                    .Concat(availableWeights.Where(w => w > target).OrderBy(w => w));
            }
            else
            {
                candidates = availableWeights.Where(w => w > target).OrderBy(w => w)
                    .Concat(availableWeights.Where(w => w < target).OrderByDescending(w => w));
            }

            return candidates.First();
        }

        private static string StylizeTuffyFaceName(int weight, bool isItalic)
        {
            var isBold = weight >= 600;
            if (isBold && isItalic) return "Tuffy Bold Italic";
            if (isBold) return "Tuffy Bold";
            if (isItalic) return "Tuffy Italic";
            return FallbackFont;
        }

        private static byte[] LoadEmbeddedFallbackFont(string faceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(faceName + ".ttf", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                return null;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        #endregion
    }
}
