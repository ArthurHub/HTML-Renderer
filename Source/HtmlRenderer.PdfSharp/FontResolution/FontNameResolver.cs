using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    public static class FontNameResolver
    {
        public static List<string> ResolveFontNames(string fontName, FontAttributes fontAttributes, FontResolveStrategy resolveStrategy)
        {
            var fontNames = new List<string>();
            
            if (resolveStrategy == FontResolveStrategy.Strict)
            {
                fontNames.Add(StylizeFontNameStrict(fontName, fontAttributes));
            }
            else
            {
                fontNames.AddRange(StylizeFontNameClosest(fontName, fontAttributes));
            }

            return fontNames;
        }

        public static string StylizeFontNameFamily(FontMetadata metadata)
        {
            if (metadata.Family is null || metadata.Attributes is null)
            {
                return string.Empty;
            }
        
            return BuildStylizedFontName(metadata.Family, MatchWeightStrict(metadata.Attributes.Weight), MatchStyleStrict(metadata.Attributes.Style));
        }

        public static string StylizeFontNamePreferredFamily(FontMetadata metadata)
        {
            if (metadata.PreferredFamily is null || metadata.PreferredSubfamily is null)
            {
                return string.Empty;
            }
        
            return string.IsNullOrEmpty(metadata.PreferredSubfamily)
                ? metadata.PreferredFamily
                : $"{metadata.PreferredFamily} {metadata.PreferredSubfamily}";
        }

        public static string StylizeFontNameStrict(string fontName, FontAttributes fontAttributes)
        {
            return BuildStylizedFontName(fontName, MatchWeightStrict(fontAttributes.Weight), MatchStyleStrict(fontAttributes.Style));
        }

        private static string[] StylizeFontNameClosest(string fontName, FontAttributes fontAttributes)
        {
            var fontNames = new List<string>
            {
                StylizeFontNameStrict(fontName, fontAttributes),
                BuildStylizedFontName(fontName, MatchWeightStrict(fontAttributes.Weight), MatchStyleObliqueAndItalic(fontAttributes.Style)),
                BuildStylizedFontName(fontName, MatchWeightMiddle(fontAttributes.Weight), MatchStyleStrict(fontAttributes.Style)),
                BuildStylizedFontName(fontName, MatchWeightMiddle(fontAttributes.Weight), MatchStyleObliqueAndItalic(fontAttributes.Style)),
                BuildStylizedFontName(fontName, MatchWeightOutward(fontAttributes.Weight), MatchStyleStrict(fontAttributes.Style)),
                BuildStylizedFontName(fontName, MatchWeightOutward(fontAttributes.Weight), MatchStyleObliqueAndItalic(fontAttributes.Style)),
                fontName
            };

            return fontNames.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }
    
        private static string MatchStyleStrict(FontStyle fontStyle)
        {
            if (fontStyle == FontStyle.Oblique || fontStyle == FontStyle.Italic)
            {
                return fontStyle.ToString();
            }
            
            return string.Empty;
        }
    
        private static string MatchStyleObliqueAndItalic(FontStyle fontStyle)
        {
            if (fontStyle == FontStyle.Oblique || fontStyle == FontStyle.Italic)
            {
                return nameof(FontStyle.Italic);
            }

            return string.Empty;
        }

        private static string MatchWeightStrict(FontWeight fontWeight)
        {
            if (fontWeight == FontWeight.Normal)
            {
                return string.Empty;
            }
            
            return fontWeight.ToString();
        }

        private static string MatchWeightMiddle(FontWeight fontWeight)
        {
            if (fontWeight == FontWeight.Light)
            {
                return nameof(FontWeight.Light);
            }

            if (fontWeight == FontWeight.Medium)
            {
                return nameof(FontWeight.Medium);
            }

            if (fontWeight < FontWeight.Light)
            {
                return $"{fontWeight + 100}";
            }

            if (fontWeight < FontWeight.Medium)
            {
                return $"{fontWeight - 100}";
            }
            
            return string.Empty;
        }

        private static string MatchWeightOutward(FontWeight fontWeight)
        {
            if (fontWeight == FontWeight.Thin)
            {
                return nameof(FontWeight.Thin);
            }
            
            if (fontWeight == FontWeight.Black)
            {
                return nameof(FontWeight.Black);
            }
            
            if (fontWeight < FontWeight.Normal)
            {
                return $"{fontWeight - 100}";
            }

            if (fontWeight > FontWeight.Normal)
            {
                return $"{fontWeight + 100}";
            }

            return string.Empty;
        }

        private static string BuildStylizedFontName(string fontName, string fontWeight, string fontStyle)
        {
            var stylizedFontName = new StringBuilder(fontName);

            if (!string.IsNullOrEmpty(fontWeight))
            {
                stylizedFontName.Append($" {fontWeight}");
            }

            if (!string.IsNullOrEmpty(fontStyle))
            {
                stylizedFontName.Append($" {fontStyle}");
            }

            return stylizedFontName.ToString();
        }
    }
}