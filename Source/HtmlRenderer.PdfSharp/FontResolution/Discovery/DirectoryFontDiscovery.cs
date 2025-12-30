using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Parsing;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Discovery
{
    internal static class DirectoryFontDiscovery
    {
        public static List<FontInfo> DiscoverFontFilesFromDirectories(List<string> customFontDirectories)
        {
            var fontDirectories = new List<string>();

            // Add platform-specific directories
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fontDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fontDirectories.AddRange(new[]
                {
                    "/usr/share/fonts",
                    "/usr/local/share/fonts",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts")
                });
            }

            // Add custom directories
            fontDirectories.AddRange(customFontDirectories);

            return DiscoverFontFilesInDirectories(fontDirectories);
        }

        public static List<FontInfo> DiscoverFontFilesInDirectories(List<string> directories)
        {
            var fontInfos = new List<FontInfo>();
            
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    continue;
                }

                try
                {
                    fontInfos.AddRange(FontDiscoveryService.SupportedFontExtensions.SelectMany(e => Directory.GetFiles(directory, $"*{e}", SearchOption.AllDirectories))
                        .Select(f => new FontInfo
                        {
                            Name = Path.GetFileNameWithoutExtension(f),
                            FilePath = f
                        }));
                }
                catch
                {
                    // Silently continue on directory access errors
                }
            }

            return fontInfos;
        }
    }
}
