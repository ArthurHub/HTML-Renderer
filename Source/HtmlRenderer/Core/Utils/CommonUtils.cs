// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    internal delegate void ActionInt<in T>(T obj);

    internal delegate void ActionInt<in T1, in T2>(T1 arg1, T2 arg2);

    internal delegate void ActionInt<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    /// <summary>
    /// Utility methods for general stuff.
    /// </summary>
    internal static class CommonUtils
    {
        #region Fields and Consts

        /// <summary>
        /// Table to convert numbers into roman digits
        /// </summary>
        private static readonly string[,] _romanDigitsTable =
        {
            { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" },
            { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" },
            { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" },
            {
                "", "M", "MM", "MMM", "M(V)", "(V)", "(V)M",
                "(V)MM", "(V)MMM", "M(X)"
            }
        };

        private static readonly string[,] _hebrewDigitsTable =
        {
            { "א", "ב", "ג", "ד", "ה", "ו", "ז", "ח", "ט" },
            { "י", "כ", "ל", "מ", "נ", "ס", "ע", "פ", "צ" },
            { "ק", "ר", "ש", "ת", "תק", "תר", "תש", "תת", "תתק", }
        };

        private static readonly string[,] _georgianDigitsTable =
        {
            { "ა", "ბ", "გ", "დ", "ე", "ვ", "ზ", "ჱ", "თ" },
            { "ი", "პ", "ლ", "მ", "ნ", "ჲ", "ო", "პ", "ჟ" },
            { "რ", "ს", "ტ", "ჳ", "ფ", "ქ", "ღ", "ყ", "შ" }
        };

        private static readonly string[,] _armenianDigitsTable =
        {
            { "Ա", "Բ", "Գ", "Դ", "Ե", "Զ", "Է", "Ը", "Թ" },
            { "Ժ", "Ի", "Լ", "Խ", "Ծ", "Կ", "Հ", "Ձ", "Ղ" },
            { "Ճ", "Մ", "Յ", "Ն", "Շ", "Ո", "Չ", "Պ", "Ջ" }
        };

        private static readonly string[] _hiraganaDigitsTable = new[]
        {
            "あ", "ぃ", "ぅ", "ぇ", "ぉ", "か", "き", "く", "け", "こ", "さ", "し", "す", "せ", "そ", "た", "ち", "つ", "て", "と", "な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ", "ま", "み", "む", "め", "も", "ゃ", "ゅ", "ょ", "ら", "り", "る", "れ", "ろ", "ゎ", "ゐ", "ゑ", "を", "ん"
        };

        private static readonly string[] _satakanaDigitsTable = new[]
        {
            "ア", "イ", "ウ", "エ", "オ", "カ", "キ", "ク", "ケ", "コ", "サ", "シ", "ス", "セ", "ソ", "タ", "チ", "ツ", "テ", "ト", "ナ", "ニ", "ヌ", "ネ", "ノ", "ハ", "ヒ", "フ", "ヘ", "ホ", "マ", "ミ", "ム", "メ", "モ", "ヤ", "ユ", "ヨ", "ラ", "リ", "ル", "レ", "ロ", "ワ", "ヰ", "ヱ", "ヲ", "ン"
        };

        /// <summary>
        /// the temp path to use for local files
        /// </summary>
        public static String _tempPath;

        #endregion


        /// <summary>
        /// Check if the given char is of Asian range.
        /// </summary>
        /// <param name="ch">the character to check</param>
        /// <returns>true - Asian char, false - otherwise</returns>
        public static bool IsAsianCharecter(char ch)
        {
            return ch >= 0x4e00 && ch <= 0xFA2D;
        }

        /// <summary>
        /// Check if the given char is a digit character (0-9) and (0-9, a-f for HEX)
        /// </summary>
        /// <param name="ch">the character to check</param>
        /// <param name="hex">optional: is hex digit check</param>
        /// <returns>true - is digit, false - not a digit</returns>
        public static bool IsDigit(char ch, bool hex = false)
        {
            return (ch >= '0' && ch <= '9') || (hex && ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')));
        }

        /// <summary>
        /// Convert the given char to digit.
        /// </summary>
        /// <param name="ch">the character to check</param>
        /// <param name="hex">optional: is hex digit check</param>
        /// <returns>true - is digit, false - not a digit</returns>
        public static int ToDigit(char ch, bool hex = false)
        {
            if (ch >= '0' && ch <= '9')
                return ch - '0';
            else if (hex)
            {
                if (ch >= 'a' && ch <= 'f')
                    return ch - 'a' + 10;
                else if (ch >= 'A' && ch <= 'F')
                    return ch - 'A' + 10;
            }

            return 0;
        }

        /// <summary>
        /// Get size that is max of <paramref name="size"/> and <paramref name="other"/> for width and height separately.
        /// </summary>
        public static RSize Max(RSize size, RSize other)
        {
            return new RSize(Math.Max(size.Width, other.Width), Math.Max(size.Height, other.Height));
        }

        /// <summary>
        /// Get Uri object for the given path if it is valid uri path.
        /// </summary>
        /// <param name="path">the path to get uri for</param>
        /// <returns>uri or null if not valid</returns>
        public static Uri TryGetUri(string path)
        {
            try
            {
                if (Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
                {
                    return new Uri(path);
                }
            }
            catch
            { }

            return null;
        }

        /// <summary>
        /// Get the first value in the given dictionary.
        /// </summary>
        /// <typeparam name="TKey">the type of dictionary key</typeparam>
        /// <typeparam name="TValue">the type of dictionary value</typeparam>
        /// <param name="dic">the dictionary</param>
        /// <param name="defaultValue">optional: the default value to return of no elements found in dictionary </param>
        /// <returns>first element or default value</returns>
        public static TValue GetFirstValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dic, TValue defaultValue = default(TValue))
        {
            if (dic != null)
            {
                foreach (var value in dic)
                    return value.Value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get file info object for the given path if it is valid file path.
        /// </summary>
        /// <param name="path">the path to get file info for</param>
        /// <returns>file info or null if not valid</returns>
        public static FileInfo TryGetFileInfo(string path)
        {
            try
            {
                return new FileInfo(path);
            }
            catch
            { }

            return null;
        }

        /// <summary>
        /// Get web client response content type.
        /// </summary>
        /// <param name="client">the web client to get the response content type from</param>
        /// <returns>response content type or null</returns>
        public static string GetResponseContentType(WebClient client)
        {
            foreach (string header in client.ResponseHeaders)
            {
                if (header.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase))
                    return client.ResponseHeaders[header];
            }
            return null;
        }

        /// <summary>
        /// Gets the representation of the online uri on the local disk.
        /// </summary>
        /// <param name="imageUri">The online image uri.</param>
        /// <returns>The path of the file on the disk.</returns>
        public static FileInfo GetLocalfileName(Uri imageUri)
        {
            StringBuilder fileNameBuilder = new StringBuilder();
            string absoluteUri = imageUri.AbsoluteUri;
            int lastSlash = absoluteUri.LastIndexOf('/');
            if (lastSlash == -1)
            {
                return null;
            }

            string uriUntilSlash = absoluteUri.Substring(0, lastSlash);
            fileNameBuilder.Append(uriUntilSlash.GetHashCode().ToString());
            fileNameBuilder.Append('_');

            string restOfUri = absoluteUri.Substring(lastSlash + 1);
            int indexOfParams = restOfUri.IndexOf('?');
            if (indexOfParams == -1)
            {
                string ext = ".png";
                int indexOfDot = restOfUri.IndexOf('.');
                if (indexOfDot > -1)
                {
                    ext = restOfUri.Substring(indexOfDot);
                    restOfUri = restOfUri.Substring(0, indexOfDot);
                }

                fileNameBuilder.Append(restOfUri);
                fileNameBuilder.Append(ext);
            }
            else
            {
                int indexOfDot = restOfUri.IndexOf('.');
                if (indexOfDot == -1 || indexOfDot > indexOfParams)
                {
                    //The uri is not for a filename
                    fileNameBuilder.Append(restOfUri);
                    fileNameBuilder.Append(".png");
                }
                else if (indexOfParams > indexOfDot)
                {
                    //Adds the filename without extension.
                    fileNameBuilder.Append(restOfUri, 0, indexOfDot);
                    //Adds the parameters
                    fileNameBuilder.Append(restOfUri, indexOfParams, restOfUri.Length - indexOfParams);
                    //Adds the filename extension.
                    fileNameBuilder.Append(restOfUri, indexOfDot, indexOfParams - indexOfDot);
                }
            }

            var validFileName = GetValidFileName(fileNameBuilder.ToString());
            if (validFileName.Length > 25)
            {
                validFileName = validFileName.Substring(0, 24) + validFileName.Substring(24).GetHashCode() + Path.GetExtension(validFileName);
            }

            if (_tempPath == null)
            {
                _tempPath = Path.Combine(Path.GetTempPath(), "HtmlRenderer");
                if (!Directory.Exists(_tempPath))
                    Directory.CreateDirectory(_tempPath);
            }

            return new FileInfo(Path.Combine(_tempPath, validFileName));
        }

        /// <summary>
        /// Get substring separated by whitespace starting from the given idex.
        /// </summary>
        /// <param name="str">the string to get substring in</param>
        /// <param name="idx">the index to start substring search from</param>
        /// <param name="length">return the length of the found string</param>
        /// <returns>the index of the substring, -1 if no valid sub-string found</returns>
        public static int GetNextSubString(string str, int idx, out int length)
        {
            while (idx < str.Length && Char.IsWhiteSpace(str[idx]))
                idx++;
            if (idx < str.Length)
            {
                var endIdx = idx + 1;
                while (endIdx < str.Length && !Char.IsWhiteSpace(str[endIdx]))
                    endIdx++;
                length = endIdx - idx;
                return idx;
            }
            length = 0;
            return -1;
        }

        /// <summary>
        /// Compare that the substring of <paramref name="str"/> is equal to <paramref name="str2"/>
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>true - equals, false - not equals</returns>
        public static bool SubStringEquals(string str, int idx, int length, string str2)
        {
            if (length == str2.Length && idx + length <= str.Length)
            {
                for (int i = 0; i < length; i++)
                {
                    if (Char.ToLowerInvariant(str[idx + i]) != Char.ToLowerInvariant(str2[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Replaces invalid filename chars to '_'
        /// </summary>
        /// <param name="source">The possibly-not-valid filename</param>
        /// <returns>A valid filename.</returns>
        private static string GetValidFileName(string source)
        {
            string retVal = source;
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (var invalidFileNameChar in invalidFileNameChars)
            {
                retVal = retVal.Replace(invalidFileNameChar, '_');
            }
            return retVal;
        }

        /// <summary>
        /// Convert number to alpha numeric system by the requested style (UpperAlpha, LowerRoman, Hebrew, etc.).
        /// </summary>
        /// <param name="number">the number to convert</param>
        /// <param name="style">the css style to convert by</param>
        /// <returns>converted string</returns>
        public static string ConvertToAlphaNumber(int number, string style = CssConstants.UpperAlpha)
        {
            if (number == 0)
                return string.Empty;

            if (style.Equals(CssConstants.LowerGreek, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToGreekNumber(number);
            }
            else if (style.Equals(CssConstants.LowerRoman, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToRomanNumbers(number, true);
            }
            else if (style.Equals(CssConstants.UpperRoman, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToRomanNumbers(number, false);
            }
            else if (style.Equals(CssConstants.Armenian, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToSpecificNumbers(number, _armenianDigitsTable);
            }
            else if (style.Equals(CssConstants.Georgian, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToSpecificNumbers(number, _georgianDigitsTable);
            }
            else if (style.Equals(CssConstants.Hebrew, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToSpecificNumbers(number, _hebrewDigitsTable);
            }
            else if (style.Equals(CssConstants.Hiragana, StringComparison.InvariantCultureIgnoreCase) || style.Equals(CssConstants.HiraganaIroha, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToSpecificNumbers2(number, _hiraganaDigitsTable);
            }
            else if (style.Equals(CssConstants.Katakana, StringComparison.InvariantCultureIgnoreCase) || style.Equals(CssConstants.KatakanaIroha, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConvertToSpecificNumbers2(number, _satakanaDigitsTable);
            }
            else
            {
                var lowercase = style.Equals(CssConstants.LowerAlpha, StringComparison.InvariantCultureIgnoreCase) || style.Equals(CssConstants.LowerLatin, StringComparison.InvariantCultureIgnoreCase);
                return ConvertToEnglishNumber(number, lowercase);
            }
        }

        /// <summary>
        /// Convert the given integer into alphabetic numeric format (D, AU, etc.)
        /// </summary>
        /// <param name="number">the number to convert</param>
        /// <param name="lowercase">is to use lowercase</param>
        /// <returns>the roman number string</returns>
        private static string ConvertToEnglishNumber(int number, bool lowercase)
        {
            var sb = string.Empty;
            int alphStart = lowercase ? 97 : 65;
            while (number > 0)
            {
                var n = number % 26 - 1;
                if (n >= 0)
                {
                    sb = (Char)(alphStart + n) + sb;
                    number = number / 26;
                }
                else
                {
                    sb = (Char)(alphStart + 25) + sb;
                    number = (number - 1) / 26;
                }
            }

            return sb;
        }

        /// <summary>
        /// Convert the given integer into alphabetic numeric format (alpha, AU, etc.)
        /// </summary>
        /// <param name="number">the number to convert</param>
        /// <returns>the roman number string</returns>
        private static string ConvertToGreekNumber(int number)
        {
            var sb = string.Empty;
            while (number > 0)
            {
                var n = number % 24 - 1;
                if (n > 16)
                    n++;
                if (n >= 0)
                {
                    sb = (Char)(945 + n) + sb;
                    number = number / 24;
                }
                else
                {
                    sb = (Char)(945 + 24) + sb;
                    number = (number - 1) / 25;
                }
            }

            return sb;
        }

        /// <summary>
        /// Convert the given integer into roman numeric format (II, VI, IX, etc.)
        /// </summary>
        /// <param name="number">the number to convert</param>
        /// <param name="lowercase">if to use lowercase letters for roman digits</param>
        /// <returns>the roman number string</returns>
        private static string ConvertToRomanNumbers(int number, bool lowercase)
        {
            var sb = string.Empty;
            for (int i = 1000, j = 3; i > 0; i /= 10, j--)
            {
                int digit = number / i;
                sb += string.Format(_romanDigitsTable[j, digit]);
                number -= digit * i;
            }
            return lowercase ? sb.ToLower() : sb;
        }

        /// <summary>
        /// Convert the given integer into given alphabet numeric system.
        /// </summary>
        /// <param name="number">the number to convert</param>
        /// <param name="alphabet">the alphabet system to use</param>
        /// <returns>the number string</returns>
        private static string ConvertToSpecificNumbers(int number, string[,] alphabet)
        {
            int level = 0;
            var sb = string.Empty;
            while (number > 0 && level < alphabet.GetLength(0))
            {
                var n = number % 10;
                if (n > 0)
                    sb = alphabet[level, number % 10 - 1].ToString(CultureInfo.InvariantCulture) + sb;
                number /= 10;
                level++;
            }
            return sb;
        }

        /// <summary>
        /// Convert the given integer into given alphabet numeric system.
        /// </summary>
        /// <param name="number">the number to convert</param>
        /// <param name="alphabet">the alphabet system to use</param>
        /// <returns>the number string</returns>
        private static string ConvertToSpecificNumbers2(int number, string[] alphabet)
        {
            for (int i = 20; i > 0; i--)
            {
                if (number > 49 * i - i + 1)
                    number++;
            }

            var sb = string.Empty;
            while (number > 0)
            {
                sb = alphabet[Math.Max(0, number % 49 - 1)].ToString(CultureInfo.InvariantCulture) + sb;
                number /= 49;
            }
            return sb;
        }
    }
}