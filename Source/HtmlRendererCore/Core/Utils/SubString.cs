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

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Represents sub-string of a full string starting at specific location with a specific length.
    /// </summary>
    internal sealed class SubString
    {
        #region Fields and Consts

        /// <summary>
        /// the full string that this sub-string is part of
        /// </summary>
        private readonly string _fullString;

        /// <summary>
        /// the start index of the sub-string
        /// </summary>
        private readonly int _startIdx;

        /// <summary>
        /// the length of the sub-string starting at <see cref="_startIdx"/>
        /// </summary>
        private readonly int _length;

        #endregion


        /// <summary>
        /// Init sub-string that is the full string.
        /// </summary>
        /// <param name="fullString">the full string that this sub-string is part of</param>
        public SubString(string fullString)
        {
            ArgChecker.AssertArgNotNull(fullString, "fullString");

            _fullString = fullString;
            _startIdx = 0;
            _length = fullString.Length;
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="fullString">the full string that this sub-string is part of</param>
        /// <param name="startIdx">the start index of the sub-string</param>
        /// <param name="length">the length of the sub-string starting at <paramref name="startIdx"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="fullString"/> is null</exception>
        public SubString(string fullString, int startIdx, int length)
        {
            ArgChecker.AssertArgNotNull(fullString, "fullString");
            if (startIdx < 0 || startIdx >= fullString.Length)
                throw new ArgumentOutOfRangeException("startIdx", "Must within fullString boundries");
            if (length < 0 || startIdx + length > fullString.Length)
                throw new ArgumentOutOfRangeException("length", "Must within fullString boundries");

            _fullString = fullString;
            _startIdx = startIdx;
            _length = length;
        }

        /// <summary>
        /// the full string that this sub-string is part of
        /// </summary>
        public string FullString
        {
            get { return _fullString; }
        }

        /// <summary>
        /// the start index of the sub-string
        /// </summary>
        public int StartIdx
        {
            get { return _startIdx; }
        }

        /// <summary>
        /// the length of the sub-string starting at <see cref="_startIdx"/>
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Get string char at specific index.
        /// </summary>
        /// <param name="idx">the idx to get the char at</param>
        /// <returns>char at index</returns>
        public char this[int idx]
        {
            get
            {
                if (idx < 0 || idx > _length)
                    throw new ArgumentOutOfRangeException("idx", "must be within the string range");
                return _fullString[_startIdx + idx];
            }
        }

        /// <summary>
        /// Is the sub-string is empty string.
        /// </summary>
        /// <returns>true - empty string, false - otherwise</returns>
        public bool IsEmpty()
        {
            return _length < 1;
        }

        /// <summary>
        /// Is the sub-string is empty string or contains only whitespaces.
        /// </summary>
        /// <returns>true - empty or whitespace string, false - otherwise</returns>
        public bool IsEmptyOrWhitespace()
        {
            for (int i = 0; i < _length; i++)
            {
                if (!char.IsWhiteSpace(_fullString, _startIdx + i))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Is the sub-string contains only whitespaces (at least one).
        /// </summary>
        /// <returns>true - empty or whitespace string, false - otherwise</returns>
        public bool IsWhitespace()
        {
            if (_length < 1)
                return false;
            for (int i = 0; i < _length; i++)
            {
                if (!char.IsWhiteSpace(_fullString, _startIdx + i))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get a string of the sub-string.<br/>
        /// This will create a new string object!
        /// </summary>
        /// <returns>new string that is the sub-string represented by this instance</returns>
        public string CutSubstring()
        {
            return _length > 0 ? _fullString.Substring(_startIdx, _length) : string.Empty;
        }

        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length. 
        /// </summary>
        /// <param name="startIdx">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="length">The number of characters in the substring. </param>
        /// <returns>A String equivalent to the substring of length length that begins at startIndex in this instance, or 
        /// Empty if startIndex is equal to the length of this instance and length is zero. </returns>
        public string Substring(int startIdx, int length)
        {
            if (startIdx < 0 || startIdx > _length)
                throw new ArgumentOutOfRangeException("startIdx");
            if (length > _length)
                throw new ArgumentOutOfRangeException("length");
            if (startIdx + length > _length)
                throw new ArgumentOutOfRangeException("length");

            return _fullString.Substring(_startIdx + startIdx, length);
        }

        public override string ToString()
        {
            return string.Format("Sub-string: {0}", _length > 0 ? _fullString.Substring(_startIdx, _length) : string.Empty);
        }
    }
}