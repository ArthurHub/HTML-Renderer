using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    /// <summary>
    /// The SubstitutingFontResolver tries to find some standard Windows fonts.
    /// When used under Windows, it searches "C:\Windows\Fonts".
    /// When used under WSL, it searches "/mnt/c/Windows/Fonts".
    /// It also searches substitute fonts in folders that are used by common Linux distributions.
    /// When used under MacOS, Android, Xamarin, ... it will probably find no fonts.
    /// When you ask for Arial, it may return FreeSans under Linux if Arial cannot be found.
    /// When you ask for Times New Roman, it may return FreeSerif under Linux if Times New Roman cannot be found.
    /// Because of font substitutions, the look of PDF files will vary depending on the configuration of the
    /// computer that runs the app.
    /// Usage tips:
    /// * Set a fallback font resolver like FailsafeFontResolver to avoid exceptions if no font can be found.
    /// * If you need a specific font, create a font resolver that includes that font as a resource.
    /// * Use SubstitutingFontResolver to play around and evaluate PDFsharp, but better do not use it when distributing apps to the public.
    /// </summary>
    class SubstitutingFontResolver : IFontResolver
    {
        class TypefaceInfo
        {
            public string FontFaceName { get; }

            public FontSimulation Simulation { get; }

            public string WindowsFileName { get; }

            public string LinuxFileName { get; }

            public string[] LinuxSubstituteFaceNames { get; }

            internal TypefaceInfo(
                string fontFaceName,
                FontSimulation fontSimulation,
                string windowsFileName,
                string linuxFileName = null!,
                params string[] linuxSubstituteFaceNames)
            {
                FontFaceName = fontFaceName;
                Simulation = fontSimulation;
                WindowsFileName = windowsFileName;
                LinuxFileName = linuxFileName;
                LinuxSubstituteFaceNames = linuxSubstituteFaceNames;
            }

            [Flags]
            public enum FontSimulation
            {
                None = 0,
                Bold = 1,
                Italic = 2,
                Both = Bold | Italic
            }
        }

        static readonly List<TypefaceInfo> TypefaceInfos =
        [
            // ReSharper disable StringLiteralTypo

            new("Arial", TypefaceInfo.FontSimulation.None, "arial", "Arial", "FreeSans"),
            new("Arial Black", TypefaceInfo.FontSimulation.None, "ariblk", "Arial-Black"),
            new("Arial Bold", TypefaceInfo.FontSimulation.None, "arialbd", "Arial-Bold", "FreeSansBold"),
            new("Arial Italic", TypefaceInfo.FontSimulation.None, "ariali", "Arial-Italic", "FreeSansOblique"),
            new("Arial Bold Italic", TypefaceInfo.FontSimulation.None, "arialbi", "Arial-BoldItalic", "FreeSansBoldOblique"),

            new("Times New Roman", TypefaceInfo.FontSimulation.None, "times", "TimesNewRoman", "FreeSerif"),
            new("Times New Roman Bold", TypefaceInfo.FontSimulation.None, "timesbd", "TimesNewRoman-Bold", "FreeSerifBold"),
            new("Times New Roman Italic", TypefaceInfo.FontSimulation.None, "timesi", "TimesNewRoman-Italic", "FreeSerifItalic"),
            new("Times New Roman Bold Italic", TypefaceInfo.FontSimulation.None, "timesbi", "TimesNewRoman-BoldItalic", "FreeSerifBoldItalic"),

            new("Courier New", TypefaceInfo.FontSimulation.None, "cour", "Courier-Bold", "DejaVu Sans Mono", "Bitstream Vera Sans Mono", "FreeMono"),
            new("Courier New Bold", TypefaceInfo.FontSimulation.None, "courbd", "CourierNew-Bold", "DejaVu Sans Mono Bold", "Bitstream Vera Sans Mono Bold", "FreeMonoBold"),
            new("Courier New Italic", TypefaceInfo.FontSimulation.None, "couri", "CourierNew-Italic", "DejaVu Sans Mono Oblique", "Bitstream Vera Sans Mono Italic", "FreeMonoOblique"),
            new("Courier New Bold Italic", TypefaceInfo.FontSimulation.None, "courbi", "CourierNew-BoldItalic", "DejaVu Sans Mono Bold Oblique", "Bitstream Vera Sans Mono Bold Italic", "FreeMonoBoldOblique"),

            new("Verdana", TypefaceInfo.FontSimulation.None, "verdana", "Verdana", "DejaVu Sans", "Bitstream Vera Sans", "FreeSans"),
            new("Verdana Bold", TypefaceInfo.FontSimulation.None, "verdanab", "Verdana-Bold", "DejaVu Sans Bold", "Bitstream Vera Sans Bold", "FreeSansBold"),
            new("Verdana Italic", TypefaceInfo.FontSimulation.None, "verdanai", "Verdana-Italic", "DejaVu Sans Oblique", "Bitstream Vera Sans Italic", "FreeSansOblique"),
            new("Verdana Bold Italic", TypefaceInfo.FontSimulation.None, "verdanaz", "Verdana-BoldItalic", "DejaVu Sans Bold Oblique", "Bitstream Vera Sans Bold Italic", "FreeSansBoldOblique"),

            new("Lucida Console", TypefaceInfo.FontSimulation.None, "lucon", "LucidaConsole", "DejaVu Sans Mono"),
            new("Lucida Console Bold", TypefaceInfo.FontSimulation.Bold, "lucon", "LucidaConsole", "DejaVu Sans Mono"),
            new("Lucida Console Italic", TypefaceInfo.FontSimulation.Italic, "lucon", "LucidaConsole", "DejaVu Sans Mono"),
            new("Lucida Console Bold Italic", TypefaceInfo.FontSimulation.Both, "lucon", "LucidaConsole", "DejaVu Sans Mono"),

            new("Symbol", TypefaceInfo.FontSimulation.None, "symbol", "", "Noto Sans Symbols Regular"), // Noto Symbols may not replace exactly
            new("Symbol Bold", TypefaceInfo.FontSimulation.Bold, "symbol", "", "Noto Sans Symbols Regular"), // Noto Symbols may not replace exactly
            new("Symbol Italic", TypefaceInfo.FontSimulation.Italic, "symbol", "", "Noto Sans Symbols Regular"), // Noto Symbols may not replace exactly
            new("Symbol Bold Italic", TypefaceInfo.FontSimulation.Both, "symbol", "", "Noto Sans Symbols Regular"), // Noto Symbols may not replace exactly
            new("Segoe UI", TypefaceInfo.FontSimulation.None, "segoeui", "SegoeUI", "DejaVu Sans"),
            new("Tahoma", TypefaceInfo.FontSimulation.None, "tahoma", "Tahoma", "DejaVu Sans"),
#if true
            //new("Wingdings", "wingding"), // No Linux substitute

            // Linux Substitute Fonts
            // TODO_OLD Nimbus and Liberation are only readily available as OTF.

            // Ubuntu packages: fonts-dejavu fonts-dejavu-core fonts-dejavu-extra
            new("DejaVu Sans", TypefaceInfo.FontSimulation.None, "DejaVuSans"),
            new("DejaVu Sans Bold", TypefaceInfo.FontSimulation.None, "DejaVuSans-Bold"),
            new("DejaVu Sans Oblique", TypefaceInfo.FontSimulation.None, "DejaVuSans-Oblique"),
            new("DejaVu Sans Bold Oblique", TypefaceInfo.FontSimulation.None, "DejaVuSans-BoldOblique"),
            new("DejaVu Sans Mono", TypefaceInfo.FontSimulation.None, "DejaVuSansMono"),
            new("DejaVu Sans Mono Bold", TypefaceInfo.FontSimulation.None, "DejaVuSansMono-Bold"),
            new("DejaVu Sans Mono Oblique", TypefaceInfo.FontSimulation.None, "DejaVuSansMono-Oblique"),
            new("DejaVu Sans Mono Bold Oblique", TypefaceInfo.FontSimulation.None, "DejaVuSansMono-BoldOblique"),

            // Ubuntu packages: fonts-freefont-ttf
            new("FreeSans", TypefaceInfo.FontSimulation.None, "FreeSans"),
            new("FreeSansBold", TypefaceInfo.FontSimulation.None, "FreeSansBold"),
            new("FreeSansOblique", TypefaceInfo.FontSimulation.None, "FreeSansOblique"),
            new("FreeSansBoldOblique", TypefaceInfo.FontSimulation.None, "FreeSansBoldOblique"),
            new("FreeMono", TypefaceInfo.FontSimulation.None, "FreeMono"),
            new("FreeMonoBold", TypefaceInfo.FontSimulation.None, "FreeMonoBold"),
            new("FreeMonoOblique", TypefaceInfo.FontSimulation.None, "FreeMonoOblique"),
            new("FreeMonoBoldOblique", TypefaceInfo.FontSimulation.None, "FreeMonoBoldOblique"),
            new("FreeSerif", TypefaceInfo.FontSimulation.None, "FreeSerif"),
            new("FreeSerifBold", TypefaceInfo.FontSimulation.None, "FreeSerifBold"),
            new("FreeSerifItalic", TypefaceInfo.FontSimulation.None, "FreeSerifItalic"),
            new("FreeSerifBoldItalic", TypefaceInfo.FontSimulation.None, "FreeSerifBoldItalic"),

            // Ubuntu packages: ttf-bitstream-vera
            new("Bitstream Vera Sans", TypefaceInfo.FontSimulation.None, "Vera"),
            new("Bitstream Vera Sans Bold", TypefaceInfo.FontSimulation.None, "VeraBd"),
            new("Bitstream Vera Sans Italic", TypefaceInfo.FontSimulation.None, "VeraIt"),
            new("Bitstream Vera Sans Bold Italic", TypefaceInfo.FontSimulation.None, "VeraBI"),
            new("Bitstream Vera Sans Mono", TypefaceInfo.FontSimulation.None, "VeraMono"),
            new("Bitstream Vera Sans Mono Bold", TypefaceInfo.FontSimulation.None, "VeraMoBd"),
            new("Bitstream Vera Sans Mono Italic", TypefaceInfo.FontSimulation.None, "VeraMoIt"),
            new("Bitstream Vera Sans Mono Bold Italic", TypefaceInfo.FontSimulation.None, "VeraMoBI"),

            // Ubuntu packages: fonts-noto-core
            new("Noto Sans Symbols Regular", TypefaceInfo.FontSimulation.None, "NotoSansSymbols-Regular"),
            new("Noto Sans Symbols Bold", TypefaceInfo.FontSimulation.None, "NotoSansSymbols-Bold")
#endif
            // ReSharper restore StringLiteralTypo
        ];

        static SubstitutingFontResolver()
        {
            var fcp = Environment.GetEnvironmentVariable("FONTCONFIG_PATH");
            if (fcp is not null && !LinuxFontLocations.Contains(fcp))
                LinuxFontLocations.Add(fcp);
        }

        // Returns a FontResolverInfo...
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var typefaces = TypefaceInfos.Where(f => f.FontFaceName.StartsWith(familyName, StringComparison.OrdinalIgnoreCase));
            var baseFamily = TypefaceInfos.FirstOrDefault();

            if (isBold)
                typefaces = typefaces.Where(f => f.FontFaceName.Contains("bold", StringComparison.OrdinalIgnoreCase) || f.FontFaceName.Contains("heavy", StringComparison.OrdinalIgnoreCase));

            if (isItalic)
                typefaces = typefaces.Where(f => f.FontFaceName.Contains("italic", StringComparison.OrdinalIgnoreCase) || f.FontFaceName.Contains("oblique", StringComparison.OrdinalIgnoreCase));

            var family = typefaces.FirstOrDefault();
#if true
            if (family is not null)
                return new FontResolverInfo(family.WindowsFileName,
                    (family.Simulation & TypefaceInfo.FontSimulation.Bold) != 0,
                    (family.Simulation & TypefaceInfo.FontSimulation.Italic) != 0);
#else
            if (family is not null)
                return new FontResolverInfo(family.WindowsFileName, false, false);
#endif

            // Return null if there is no exact match.
            //if (baseFamily is not null)
            //    return new FontResolverInfo(baseFamily.WindowsFileName, isBold, isItalic);

            return null;
        }

        public byte[]? GetFont(string faceName)
        {
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //    return GetFontWindows(faceName);

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //    return GetFontLinux(faceName);

            if (_isWindows == null)
            {
#if NET462
                _isWindows = true;
#else
                // May be too simple.
                _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
            }

            if (_isLinux == null)
            {
#if NET462
                _isLinux = false;
#else
                // May be too simple.
                _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif
            }

            if (_isWindows.Value)
            {
                return GetWindowsFontFace(faceName);
            }
            else if (_isLinux.Value)
            {
                return GetLinuxFontFace(faceName);
            }
            else
            {
                return GetWindowsFontFace(faceName);
            }
        }
        bool? _isWindows;
        bool? _isLinux;

        byte[]? GetWindowsFontFace(string faceName)
        {
            foreach (var fontLocation in WindowsFontLocations)
            {
                var filepath = Path.Combine(fontLocation, faceName + ".ttf");
                if (File.Exists(filepath))
                    return File.ReadAllBytes(filepath);
            }
            return null;
        }

        static readonly List<string> WindowsFontLocations =
        [
            @"C:\Windows\Fonts",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Fonts")
        ];

        byte[]? GetLinuxFontFace(string faceName)
        {
            // TODO_OLD Query fontconfig.
            // Fontconfig is the de facto standard for indexing and managing fonts on linux.
            // Example command that should return a full file path to FreeSansBoldOblique.ttf:
            //     fc-match -f '%{file}\n' 'FreeSans:Bold:Oblique:fontformat=TrueType' : file
            //
            // Caveat: fc-match *always* returns a "next best" match or default font, even if it’s bad.
            // Caveat: some preprocessing/refactoring needed to produce a pattern fc-match can understand.
            // Caveat: fontconfig needs additional configuration to know about WSL having Windows Fonts available at /mnt/c/Windows/Fonts.

            foreach (var fontLocation in LinuxFontLocations)
            {
                if (!Directory.Exists(fontLocation))
                    continue;

                var fontPath = FindFileRecursive(fontLocation, faceName);
                if (fontPath is not null && File.Exists(fontPath))
                    return File.ReadAllBytes(fontPath);
            }

            return null;
        }

        static readonly List<string> LinuxFontLocations =
        [
            "/mnt/c/Windows/Fonts", // WSL first or substitutes will be found.
            "/usr/share/fonts",
            "/usr/share/X11/fonts",
            "/usr/X11R6/lib/X11/fonts",
            // TODO_OLD Avoid calling Environment.GetFolderPath twice.
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts")
        ];

        /// <summary>
        /// Finds filename candidates recursively on Linux, as organizing fonts into arbitrary subdirectories is allowed.
        /// </summary>
        string? FindFileRecursive(string basePath, string faceName)
        {
            var filenameCandidates = FaceNameToFilenameCandidates(faceName);

            foreach (var file in Directory.GetFiles(basePath).Select(Path.GetFileName))
                foreach (var filenameCandidate in filenameCandidates)
                {
                    // Most programs treat fonts case-sensitive on Linux. We ignore case because we also target WSL.
                    if (!String.IsNullOrEmpty(file) && file.Equals(filenameCandidate, StringComparison.OrdinalIgnoreCase))
                        return Path.Combine(basePath, filenameCandidate);
                }

            // Linux allows arbitrary subdirectories for organizing fonts.
            foreach (var directory in Directory.GetDirectories(basePath).Select(Path.GetFileName))
            {
                if (String.IsNullOrEmpty(directory))
                    continue;

                var file = FindFileRecursive(Path.Combine(basePath, directory), faceName);
                if (file is not null)
                    return file;
            }

            return null;
        }

        /// <summary>
        /// Generates filename candidates for Linux systems.
        /// </summary>
        string[] FaceNameToFilenameCandidates(string faceName)
        {
            const string fileExtension = ".ttf";
            // TODO_OLD OTF Fonts are popular on Linux too.

            var candidates = new List<string>
            {
                faceName + fileExtension // We need to look for Windows face name too in case of WSL or copied files.
            };

            var family = TypefaceInfos.FirstOrDefault(f => f.WindowsFileName == faceName);
            if (family is null)
                return candidates.ToArray();

            if (!String.IsNullOrEmpty(family.LinuxFileName))
                candidates.Add(family.LinuxFileName + fileExtension);
            candidates.Add(family.FontFaceName + fileExtension);

            // Add substitute fonts as last candidates.
            foreach (var replacement in family.LinuxSubstituteFaceNames)
            {
                var replacementFamily = TypefaceInfos.FirstOrDefault(f => f.FontFaceName == replacement);
                if (replacementFamily is null)
                    continue;

                candidates.Add(replacementFamily.FontFaceName + fileExtension);
                if (!String.IsNullOrEmpty(replacementFamily.WindowsFileName))
                    candidates.Add(replacementFamily.WindowsFileName + fileExtension);
                if (!String.IsNullOrEmpty(replacementFamily.LinuxFileName))
                    candidates.Add(replacementFamily.LinuxFileName + fileExtension);
            }

            return candidates.ToArray();
        }
    }

#if !NET6_0_OR_GREATER
    static class StringExtensions
    {
        /// <summary>
        /// String.Contains implementation for .NET Framework and .NET Standard as an extension method.
        /// </summary>
        internal static bool Contains(this string s, string value, StringComparison comparisonType) => s.IndexOf(value, comparisonType) >= 0;
    }
#endif
}
