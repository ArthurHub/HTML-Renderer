using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Parsing
{
    internal static class FontParser
    {
        public static FontMetadata ExtractFontMetadata(string fontFilePath)
        {
            try
            {
                using (var fs = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length > int.MaxValue)
                    {
                        return null;
                    }

                    var pooledBytes = ArrayPool<byte>.Shared.Rent((int)fs.Length);

                    try
                    {
                        var bytesRead = fs.Read(pooledBytes, 0, (int)fs.Length);
                        var bytes = pooledBytes.AsSpan(0, bytesRead); // Fixed size span view

                        var metadata = ParseFontMetadata(bytes);
                        if (metadata != null)
                        {
                            metadata.FilePath = fontFilePath;
                        }
                        return metadata;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(pooledBytes);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static FontMetadata ParseFontMetadata(Span<byte> bytes)
        {
            // Read the font header (big-endian)
            var scalarType = BinaryParser.ReadUint32BigEndian(bytes, 0);

            // Verify it's a valid TTF/OTF file
            if (scalarType != 0x00010000 && // TrueType version 1.0
                scalarType != 0x74727565 && // "true" - TrueType
                scalarType != 0x4F54544F) // "OTTO" - OpenType with CFF outline
            {
                return null;
            }

            var tableCount = BinaryParser.ReadUint16BigEndian(bytes, 4);

            // Find the "name" and "os/2" tables
            long nameTableOffset = -1;
            uint? os2TableOffset = null;
            var tableRecordOffset = 12;

            for (var i = 0; i < tableCount; i++)
            {
                var tag = BinaryParser.ReadAsciiString(bytes, tableRecordOffset, 4);
                var offset = BinaryParser.ReadUint32BigEndian(bytes, tableRecordOffset + 8);

                if (tag == "name")
                {
                    nameTableOffset = offset;
                }
                else if (tag == "OS/2")
                {
                    os2TableOffset = offset;
                }

                tableRecordOffset += 16;
            }

            if (nameTableOffset == -1)
            {
                return null;
            }

            var metadata = new FontMetadata();

            ParseNameTable(bytes, nameTableOffset, metadata);

            // Extract OS/2 table data if available for more accurate weight/width/style info
            var hasOs2Data = ParseOs2Table(bytes, os2TableOffset, metadata);

            // Parse style from subfamily if not already set from OS/2
            if (!hasOs2Data)
            {
                ParseStyleFromSubfamily(metadata, metadata.Subfamily);
            }

            // Return metadata if we at least have a family name
            return !string.IsNullOrEmpty(metadata.Family) ? metadata : null;
        }

        private static void ParseNameTable(Span<byte> bytes, long nameTableOffset, FontMetadata metadata)
        {
            var nameTablePos = (int)nameTableOffset;
            var count = BinaryParser.ReadUint16BigEndian(bytes, nameTablePos + 2);
            var stringDataOffset = BinaryParser.ReadUint16BigEndian(bytes, nameTablePos + 4);

            // Track which values have been set with English entries
            var hasEnglishValue = new HashSet<int>();

            // First pass: Look for English entries
            var nameRecordOffset = nameTablePos + 6;
            for (var i = 0; i < count; i++, nameRecordOffset += 12)
            {
                var platformId = BinaryParser.ReadUint16BigEndian(bytes, nameRecordOffset);
                var encodingId = BinaryParser.ReadUint16BigEndian(bytes, nameRecordOffset + 2);
                var languageId = BinaryParser.ReadUint16BigEndian(bytes, nameRecordOffset + 4);
                var nameId = BinaryParser.ReadUint16BigEndian(bytes, nameRecordOffset + 6);
                var length = BinaryParser.ReadUint16BigEndian(bytes, nameRecordOffset + 8);
                var offset = BinaryParser.ReadUint16BigEndian(bytes, nameRecordOffset + 10);

                // Filter for English language entries:
                // Platform 3 (Windows): languageID 0x0409 (1033) = US English
                // Platform 1 (Macintosh): languageID 0 = English
                var isEnglish = (platformId == 3 && languageId == 0x0409) ||
                                (platformId == 1 && languageId == 0);

                if ((platformId != 3 && platformId != 1) || !IsRelevantNameId(nameId))
                {
                    continue;
                }
            
                var stringPos = (int)nameTableOffset + stringDataOffset + offset;

                if (stringPos + length > bytes.Length)
                {
                    continue;
                }
            
                var nameBytes = bytes.Slice(stringPos, length);

                var decodedString = DecodeNameTableString(nameBytes, platformId, encodingId);

                if (string.IsNullOrEmpty(decodedString) || decodedString == null)
                {
                    continue;
                }
            
                // If English, set the value and mark as having English
                if (isEnglish)
                {
                    if (SetMetadataValue(metadata, nameId, decodedString))
                    {
                        hasEnglishValue.Add(nameId);
                    }
                }
                // If not English but we haven't found an English entry yet, use it as fallback
                else if (!hasEnglishValue.Contains(nameId))
                {
                    SetMetadataValue(metadata, nameId, decodedString);
                }
            }
        }

        private static bool IsRelevantNameId(ushort nameId)
        {
            return nameId is 1 || nameId is 2 || nameId is 4 || nameId is 6 || nameId is 16 || nameId is 17;
        }

        private static bool SetMetadataValue(FontMetadata metadata, ushort nameId, string value)
        {
            switch (nameId)
            {
                case 1: // Legacy Family name
                    if (metadata.Family == null)
                    {
                        metadata.Family = value;
                        return true;
                    }
                    return false;
                case 2: // Legacy Subfamily
                    if (metadata.Subfamily == null)
                    {
                        metadata.Subfamily = value;
                        ParseStyleFromSubfamily(metadata, value);
                        return true;
                    }
                    return false;
                case 4: // Full font name
                    if (metadata.FullName == null)
                    {
                        metadata.FullName = value;
                        return true;
                    }
                    return false;
                case 6: // PostScript name
                    if (metadata.PostScriptName == null)
                    {
                        metadata.PostScriptName = value;
                        return true;
                    }
                    return false;
                case 16: // Preferred Family (Typographic Family)
                    if (metadata.PreferredFamily == null)
                    {
                        metadata.PreferredFamily = value;
                        return true;
                    }
                    return false;
                case 17: // Preferred Subfamily (Typographic Subfamily)
                    if (metadata.PreferredSubfamily == null)
                    {
                        metadata.PreferredSubfamily = value;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        private static void ParseStyleFromSubfamily(FontMetadata metadata, string subfamily)
        {
            if (subfamily is null)
            {
                return;
            }
        
            var lower = subfamily.ToLowerInvariant();

            // Parse weight, width, and style
            var weight = ParseWeight(lower);
            var width = ParseWidth(lower);
            var style = ParseStyle(lower);

            // Update metadata with parsed attributes (note: FontAttributes order is Weight, Style, Width)
            metadata.Attributes = new FontAttributes
            { 
                Weight = weight,
                Style = style,
                Width = width
            };
        }

        private static FontStyle ParseStyle(string lowerSubfamily)
        {
            if (lowerSubfamily.Contains("oblique"))
            {
                return FontStyle.Oblique;
            }
            
            if (lowerSubfamily.Contains("italic"))
            {
                return FontStyle.Italic;
            }
            
            return FontStyle.Normal;
        }

        private static FontWeight ParseWeight(string lowerSubfamily)
        {
            // Map weight keywords to enum values
            if (lowerSubfamily.Contains("thin") || lowerSubfamily.Contains("hairline"))
            {
                return FontWeight.Thin;
            }
            
            if (lowerSubfamily.Contains("extralight") || lowerSubfamily.Contains("ultra light"))
            {
                return FontWeight.ExtraLight;
            }
            
            if (lowerSubfamily.Contains("light"))
            {
                return FontWeight.Light;
            }
            
            if (lowerSubfamily.Contains("medium"))
            {
                return FontWeight.Medium;
            }
            
            if (lowerSubfamily.Contains("semibold") || lowerSubfamily.Contains("demibold"))
            {
                return FontWeight.SemiBold;
            }
            
            if (lowerSubfamily.Contains("extrabold") || lowerSubfamily.Contains("ultra bold"))
            {
                return FontWeight.ExtraBold;
            }
            
            if (lowerSubfamily.Contains("bold"))
            {
                return FontWeight.Bold;
            }
            
            if (lowerSubfamily.Contains("black") || lowerSubfamily.Contains("heavy"))
            {
                return FontWeight.Black;
            }
            
            return FontWeight.Normal;
        }

        private static FontWidth ParseWidth(string lowerSubfamily)
        {
            if (lowerSubfamily.Contains("ultracondensed") || lowerSubfamily.Contains("ultra condensed"))
            {
                return FontWidth.UltraCondensed;
            }
            
            if (lowerSubfamily.Contains("extracondensed") || lowerSubfamily.Contains("extra condensed"))
            {
                return FontWidth.ExtraCondensed;
            }
            
            if (lowerSubfamily.Contains("condensed"))
            {
                return FontWidth.Condensed;
            }
            
            if (lowerSubfamily.Contains("semicondensed"))
            {
                return FontWidth.SemiCondensed;
            }
            
            if (lowerSubfamily.Contains("semiexpanded"))
            {
                return FontWidth.SemiExpanded;
            }
            
            if (lowerSubfamily.Contains("extraexpanded") || lowerSubfamily.Contains("extra expanded"))
            {
                return FontWidth.ExtraExpanded;
            }
            
            if (lowerSubfamily.Contains("ultraexpanded") || lowerSubfamily.Contains("ultra expanded"))
            {
                return FontWidth.UltraExpanded;
            }
            
            if (lowerSubfamily.Contains("expanded"))
            {
                return FontWidth.Expanded;
            }
            
            return FontWidth.Medium;
        }

        private static bool ParseOs2Table(Span<byte> bytes, uint? os2TableOffset, FontMetadata metadata)
        {
            if (!os2TableOffset.HasValue)
            {
                return false;
            }
        
            var offset = (int)os2TableOffset.Value;
        
            try
            {
                // OS/2 table structure (v0+):
                // Offset 4: usWeightClass (USHORT) - font weight (100-900)
                // Offset 6: usWidthClass (USHORT) - font width (1-9)
                // Offset 8: fsType (USHORT) - embedding permissions
                // Offset 62: fsSelection (USHORT) - contains style bits
            
                // Check if we have enough bytes for OS/2 table
                if (offset + 64 > bytes.Length)
                {
                    return false;
                }

                // Read usWeightClass (offset 4 in OS/2)
                var usWeightClass = BinaryParser.ReadUint16BigEndian(bytes, offset + 4);
                var weight = FontWeight.Normal;
                if (usWeightClass >= 100 && usWeightClass <= 900)
                {
                    weight = (FontWeight)usWeightClass;
                }

                // Read usWidthClass (offset 6 in OS/2)
                var usWidthClass = BinaryParser.ReadUint16BigEndian(bytes, offset + 6);
                var width = FontWidth.Medium;
                if (usWidthClass >= 1 && usWidthClass <= 9)
                {
                    width = WidthClassToEnum(usWidthClass);
                }

                // Read fsSelection for style bits (offset 62 in OS/2)
                var fsSelection = BinaryParser.ReadUint16BigEndian(bytes, offset + 62);
            
                // Bit 9: Oblique, Bit 6: Regular, Bit 0: Italic
                var style = ((fsSelection & 0x0200) != 0) ? FontStyle.Oblique :
                    ((fsSelection & 0x0001) != 0) ? FontStyle.Italic :
                    FontStyle.Normal;

                // Update attributes with extracted OS/2 data (note: FontAttributes order is Weight, Style, Width)
                metadata.Attributes = new FontAttributes
                { 
                    Weight = weight,
                    Style = style,
                    Width = width
                };
                return true;    
            }
            catch
            {
                // Silently continue if OS/2 parsing fails
                return false;
            }
        }

        private static FontWidth WidthClassToEnum(int widthClass)
        {
            switch (widthClass)
            {
                case 1: return FontWidth.UltraCondensed;
                case 2: return FontWidth.ExtraCondensed;
                case 3: return FontWidth.Condensed;
                case 4: return FontWidth.SemiCondensed;
                case 5: return FontWidth.Medium;
                case 6: return FontWidth.SemiExpanded;
                case 7: return FontWidth.Expanded;
                case 8: return FontWidth.ExtraExpanded;
                case 9: return FontWidth.UltraExpanded;
                default: return FontWidth.Medium;
            }
        }

        private static string DecodeNameTableString(Span<byte> data, ushort platformId, ushort encodingId)
        {
            try
            {
                // Windows platform
                if (platformId == 3)
                {
                    if (encodingId == 1)
                    {
                        return BinaryParser.ReadUtf16StringBigEndian(data);
                    }

                    return null;
                }
                
                // Macintosh platform
                if (platformId == 1)
                {
                    if (encodingId == 0)
                    {
                        return BinaryParser.ReadAsciiString(data).TrimEnd('\0');
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
