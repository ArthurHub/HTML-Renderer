using System;
using System.Buffers;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution.Parsing
{
    internal static class BinaryParser
    {
        public static ushort ReadUint16BigEndian(Span<byte> data, int offset)
        {
            if (offset + 1 >= data.Length)
            {
                return 0;
            }
        
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        public static uint ReadUint32BigEndian(Span<byte> data, int offset)
        {
            if (offset + 3 >= data.Length)
            {
                return 0;
            }
        
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
                   ((uint)data[offset + 2] << 8) | data[offset + 3];
        }

        public static string ReadAsciiString(Span<byte> data, int offset, int length)
        {
            return Encoding.ASCII.GetString(data.Slice(offset, length).ToArray());
        }

        public static string ReadAsciiString(Span<byte> data)
        {
            return ReadAsciiString(data, 0, data.Length);
        }

        public static string ReadUtf16StringBigEndian(Span<byte> data)
        {
            var chars = ArrayPool<char>.Shared.Rent(data.Length);

            try
            {
                var index = 0;
            
                for (var i = 0; i < data.Length - 1; i += 2)
                {
                    var c = (char)((data[i] << 8) | data[i + 1]);
                
                    if (c == '\0')
                    {
                        continue;
                    }
                
                    chars[index] = c;
                    ++index;
                }

                return new string(chars, 0, index);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(chars);
            }
        }
    }
}