using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PdfSharp.Fonts;
using TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Discovery;
using TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Parsing;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    public class FontResolver : IFontResolver
    {   
        private readonly FontDiscoveryService _fontDiscoveryService = new FontDiscoveryService();
        private readonly Dictionary<string, FontMetadata> _fontCache = new Dictionary<string, FontMetadata>();
        
        private List<FontMetadata> _fontMetadataCache = new List<FontMetadata>();
        
        public static string FallbackFont => "Tuffy";
        public FontResolveStrategy ResolveStrategy { get; set; } = FontResolveStrategy.Strict;
        
        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
        {
            if (_fontMetadataCache.Count == 0)
            {
                DiscoverFonts();
            }

            var fontAttributes = new FontAttributes
            {
                Style = italic ? FontStyle.Italic : FontStyle.Normal,
                Weight = bold ? FontWeight.Bold : FontWeight.Normal
            };
            
            var fontNames = FontNameResolver.ResolveFontNames(familyName, fontAttributes, ResolveStrategy);
            
            foreach (var name in fontNames)
            {
                foreach (var metadata in _fontMetadataCache)
                {
                    var stylizedPreferredFamilyName = FontNameResolver.StylizeFontNamePreferredFamily(metadata);
                    var stylizedFamilyName = FontNameResolver.StylizeFontNameFamily(metadata);

                    if (stylizedPreferredFamilyName == name || stylizedFamilyName == name)
                    {
                        if (!_fontCache.ContainsKey(name))
                        {
                            _fontCache[name] = metadata;
                        }

                        return new FontResolverInfo(name);
                    }
                }
            }
            
            // Return fallback if no matching font is found
            var stylizedFallbackFontName = FontNameResolver.StylizeFontNameStrict(FallbackFont, fontAttributes);
            return new FontResolverInfo(stylizedFallbackFontName);
        }

        public byte[] GetFont(string faceName)
        {
            if (_fontCache.TryGetValue(faceName, out var font))
            {
                if (File.Exists(font.FilePath))
                {
                    return File.ReadAllBytes(font.FilePath);
                }
            }

            if (!faceName.StartsWith(FallbackFont))
            {
                return null;
            }

            // Try to load embedded fallback font
            var assembly = Assembly.GetExecutingAssembly();
        
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith($"{faceName}.ttf", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
            {
                return null;
            }

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public void RegisterCustomFontDirectory(string fontDirectory)
        {
            _fontDiscoveryService.RegisterCustomFontDirectory(fontDirectory);
        }

        public List<string> DiscoverFontFamilies()
        {
            if (_fontMetadataCache.Count == 0)
            {
                DiscoverFonts();
            }
            
            var fontFamilies = new List<string>();
            
            foreach (var metadata in _fontMetadataCache)
            {
                var stylizedPreferredFamilyName = FontNameResolver.StylizeFontNamePreferredFamily(metadata);
                var stylizedFamilyName = FontNameResolver.StylizeFontNameFamily(metadata);

                if (!string.IsNullOrEmpty(stylizedPreferredFamilyName))
                {
                    fontFamilies.Add(stylizedFamilyName);
                }
                
                fontFamilies.Add(stylizedFamilyName);
            }
            
            return fontFamilies;
        }
        
        public static FontResolver Register()
        {
            var fontResolver = new FontResolver();
            GlobalFontSettings.FontResolver = fontResolver;
            return fontResolver;
        }

        private void DiscoverFonts()
        {
            var fontInfos = _fontDiscoveryService.DiscoverFontInfos();

            _fontMetadataCache = fontInfos.Select(fontInfo => fontInfo.FilePath)
                .Distinct()
                .Select(FontParser.ExtractFontMetadata)
                .Where(fontMetadata => fontMetadata != null)
                .ToList();
        }
    }
}