using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Discovery
{
    internal class FontDiscoveryService
    {
        private readonly List<string> _customFontDirectories = new List<string>();
    
        public static string[] SupportedFontExtensions { get; } = new[] { ".ttf", ".otf" };

        public List<FontInfo> DiscoverFontInfos()
        {
            var fontInfos = new List<FontInfo>();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fontInfos = WindowsFontDiscovery.DiscoverFontInfos();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fontInfos = LinuxFontDiscovery.DiscoverFontFiles();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                fontInfos = MacOsFontDiscovery.DiscoverFontFiles();
            }

            // Always use directory scan as fallback
            fontInfos.AddRange(DirectoryFontDiscovery.DiscoverFontFilesFromDirectories(_customFontDirectories));
            
            return fontInfos;
        }

        public void RegisterCustomFontDirectory(string fontDirectory)
        {
            if (_customFontDirectories.Contains(fontDirectory))
            {
                return;
            }
        
            _customFontDirectories.Add(fontDirectory);
        }
    }
}

