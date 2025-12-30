using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Discovery
{
    internal static class LinuxFontDiscovery
    {
        public static List<FontInfo> DiscoverFontFiles()
        {
            var fontInfos = new List<FontInfo>();
            
            if (!IsFontConfigAvailable())
            {
                return fontInfos;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "fc-list",
                    Arguments = ":",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return fontInfos;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                    {
                        return fontInfos;
                    }

                    var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var fontFamilies = DiscoverFontFile(line);
                        
                        if (fontFamilies.Count > 0)
                        {
                            fontInfos.AddRange(fontFamilies);
                        }
                    }
                }
            }
            catch
            {
                // Silently continue on errors
            }

            return fontInfos;
        }

        private static bool IsFontConfigAvailable()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "fc-list",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return false;
                    }

                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static List<FontInfo> DiscoverFontFile(string line)
        {
            var fonts = new List<FontInfo>();
            
            // Parse fc-list output: "path: family1,family2:style=style"
            var parts = line.Split(new[] { ':' }, 2, StringSplitOptions.None);
            if (parts.Length < 2)
            {
                return fonts;
            }

            var fontFilePath = parts[0].Trim();
            var families = parts[1].Trim().Split(',');

            if (!File.Exists(fontFilePath) || 
                !FontDiscoveryService.SupportedFontExtensions.Any(e => fontFilePath.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)))
            {
                return fonts;
            }

            foreach (var family in families)
            {
                var familyName = family.Trim();
                if (!string.IsNullOrEmpty(familyName) && File.Exists(fontFilePath))
                {
                    fonts.Add(new FontInfo { Name = familyName, FilePath = fontFilePath });
                }
            }
            return fonts;
        }
    }
}