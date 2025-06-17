using PdfSharp.Fonts;
using System;
using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Win32;

public class SystemFontResolver : IFontResolver
{
    // Map font face names to font file paths
    private string FindFontFile(string familyName, bool isBold, bool isItalic)
    {
        // Search Windows Fonts folder for the font file
        string fontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        using (InstalledFontCollection fonts = new InstalledFontCollection())
        {
            var fontFamily = fonts.Families.FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));
            if (fontFamily != null)
            {
                // Try to find the font file in the registry
                string fontRegKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(fontRegKey))
                {
                    if (key != null)
                    {
                        foreach (var fontValue in key.GetValueNames())
                        {
                            if (fontValue.StartsWith(familyName, StringComparison.OrdinalIgnoreCase))
                            {
                                string fontFile = key.GetValue(fontValue) as string;
                                if (!string.IsNullOrEmpty(fontFile))
                                {
                                    string fullPath = Path.Combine(fontsPath, fontFile);
                                    if (File.Exists(fullPath))
                                        return fullPath;
                                }
                            }
                        }
                    }
                }
            }
        }
        return null;
    }

    public byte[] GetFont(string faceName)
    {
        // faceName is the unique name you return from ResolveTypeface
        // For simplicity, use the faceName as the font family name
        string fontFile = FindFontFile(faceName, false, false);
        if (fontFile != null)
            return File.ReadAllBytes(fontFile);

        // Fallback: return null if not found
        return null;
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Try to find the font file for the requested style
        string fontFile = FindFontFile(familyName, isBold, isItalic);
        if (fontFile != null)
        {
            // Use the family name as the face name
            return new FontResolverInfo(familyName);
        }

        // Fallback to Arial if not found
        return PlatformFontResolver.ResolveTypeface("Arial", isBold, isItalic);
    }
}
