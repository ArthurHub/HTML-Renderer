using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Discovery
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Platform switching in calling class")]
    internal static class WindowsFontDiscovery
    {
        public static List<FontInfo> DiscoverFontInfos()
        {
            var fontInfos = new List<FontInfo>();
            try
            {
                using (var machineRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false))
                using (var userRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false))
                {
                    foreach (var registryKey in new[] { machineRegistryKey, userRegistryKey })
                    {
                        if (registryKey == null)
                        {
                            continue;
                        }

                        foreach (var fontName in registryKey.GetValueNames())
                        {
                            var fontFilePath = registryKey.GetValue(fontName)?.ToString();
                    
                            var fontFile = DiscoverFontFile(fontFilePath);

                            if (fontFile != null)
                            {
                                fontInfos.Add(new FontInfo
                                {
                                    Name = fontName,
                                    FilePath = fontFile
                                });
                            }
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

        private static string DiscoverFontFile(string fontFilePath)
        {
            if (fontFilePath == null ||
                !FontDiscoveryService.SupportedFontExtensions.Any(e => fontFilePath.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)))
            {
                return null;
            }

            // If the path is not absolute, the font is in the Windows Fonts folder
            if (!Path.IsPathRooted(fontFilePath))
            {
                fontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontFilePath);
            }

            if (!File.Exists(fontFilePath))
            {
                return null;
            }

            return fontFilePath;
        }
    }
}