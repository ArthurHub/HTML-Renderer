using System;
using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Discovery
{
    internal static class MacOsFontDiscovery
    {
        public static List<FontInfo> DiscoverFontFiles()
        {
            var fontDirectories = new List<string>
            {
                "/System/Library/Fonts",
                "/Library/Fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")
            };

            return DirectoryFontDiscovery.DiscoverFontFilesInDirectories(fontDirectories);
        }
    }
}