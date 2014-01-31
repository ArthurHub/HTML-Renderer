// Type: System.Drawing.Color
// Assembly: System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Drawing.dll

using System;
using System.Text;

namespace HtmlRenderer.Entities
{
    /// <summary>
    /// Represents an ARGB (alpha, red, green, blue) color.
    /// </summary>
    public struct ColorInt
    {
        #region Fields and Consts

        /// <summary>
        ///     Represents a color that is null.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public static readonly ColorInt Empty = new ColorInt();

        private readonly long _value;

        #endregion


        private ColorInt(long value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets a system-defined color.
        /// </summary>
        public static ColorInt Transparent
        {
            get { return new ColorInt(0); }
        }

        /// <summary>
        ///     Gets a system-defined color that has an ARGB value of #FF000000.
        /// </summary>
        public static ColorInt Black
        {
            get { return FromArgb(0,0,0); }
        }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFFFF.
        /// </summary>
        public static ColorInt White
        {
            get { return FromArgb(255,255,255); }
        }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF5F5F5.
        /// </summary>
        public static ColorInt WhiteSmoke
        {
            get { return FromArgb(245, 245, 245); }
        }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFD3D3D3.
        /// </summary>
        public static ColorInt LightGray
        {
            get { return FromArgb(211, 211, 211); }
        }

        /// <summary>
        ///     Gets the red component value of this <see cref="ColorInt" /> structure.
        /// </summary>
        public byte R
        {
            get { return (byte)( (ulong)( _value >> 16 ) & byte.MaxValue ); }
        }

        /// <summary>
        ///     Gets the green component value of this <see cref="ColorInt" /> structure.
        /// </summary>
        public byte G
        {
            get { return (byte)((ulong)(_value >> 8) & byte.MaxValue); }
        }

        /// <summary>
        ///     Gets the blue component value of this <see cref="ColorInt" /> structure.
        /// </summary>
        public byte B
        {
            get { return (byte)((ulong)_value & byte.MaxValue); }
        }

        /// <summary>
        ///     Gets the alpha component value of this <see cref="ColorInt" /> structure.
        /// </summary>
        public byte A
        {
            get { return (byte)((ulong)(_value >> 24) & byte.MaxValue); }
        }

        /// <summary>
        ///     Specifies whether this <see cref="ColorInt" /> structure is uninitialized.
        /// </summary>
        /// <returns>
        ///     This property returns true if this color is uninitialized; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public bool IsEmpty
        {
            get { return _value == 0; }
        }

        /// <summary>
        ///     Tests whether two specified <see cref="ColorInt" /> structures are equivalent.
        /// </summary>
        /// <returns>
        ///     true if the two <see cref="ColorInt" /> structures are equal; otherwise, false.
        /// </returns>
        /// <param name="left">
        ///     The <see cref="ColorInt" /> that is to the left of the equality operator.
        /// </param>
        /// <param name="right">
        ///     The <see cref="ColorInt" /> that is to the right of the equality operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator ==(ColorInt left, ColorInt right)
        {
            return left._value == right._value;
        }

        /// <summary>
        ///     Tests whether two specified <see cref="ColorInt" /> structures are different.
        /// </summary>
        /// <returns>
        ///     true if the two <see cref="ColorInt" /> structures are different; otherwise, false.
        /// </returns>
        /// <param name="left">
        ///     The <see cref="ColorInt" /> that is to the left of the inequality operator.
        /// </param>
        /// <param name="right">
        ///     The <see cref="ColorInt" /> that is to the right of the inequality operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator !=(ColorInt left, ColorInt right)
        {
            return !( left == right );
        }

        /// <summary>
        ///     Creates a <see cref="ColorInt" /> structure from the four ARGB component (alpha, red, green, and blue) values. Although this method allows a 32-bit value to be passed for each component, the value of each component is limited to 8 bits.
        /// </summary>
        /// <returns>
        ///     The <see cref="ColorInt" /> that this method creates.
        /// </returns>
        /// <param name="alpha">The alpha component. Valid values are 0 through 255. </param>
        /// <param name="red">The red component. Valid values are 0 through 255. </param>
        /// <param name="green">The green component. Valid values are 0 through 255. </param>
        /// <param name="blue">The blue component. Valid values are 0 through 255. </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="alpha" />, <paramref name="red" />, <paramref name="green" />, or <paramref name="blue" /> is less than 0 or greater than 255.
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public static ColorInt FromArgb(int alpha, int red, int green, int blue)
        {
            CheckByte(alpha);
            CheckByte(red);
            CheckByte(green);
            CheckByte(blue);
            return new ColorInt((uint)(red << 16 | green << 8 | blue | alpha << 24) & (long)uint.MaxValue);
        }

        /// <summary>
        ///     Creates a <see cref="ColorInt" /> structure from the specified 8-bit color values (red, green, and blue). The alpha value is implicitly 255 (fully opaque). Although this method allows a 32-bit value to be passed for each color component, the value of each component is limited to 8 bits.
        /// </summary>
        /// <returns>
        ///     The <see cref="ColorInt" /> that this method creates.
        /// </returns>
        /// <param name="red">
        ///     The red component value for the new <see cref="ColorInt" />. Valid values are 0 through 255.
        /// </param>
        /// <param name="green">
        ///     The green component value for the new <see cref="ColorInt" />. Valid values are 0 through 255.
        /// </param>
        /// <param name="blue">
        ///     The blue component value for the new <see cref="ColorInt" />. Valid values are 0 through 255.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="red" />, <paramref name="green" />, or <paramref name="blue" /> is less than 0 or greater than 255.
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public static ColorInt FromArgb(int red, int green, int blue)
        {
            return FromArgb(byte.MaxValue, red, green, blue);
        }

        /// <summary>
        ///     Tests whether the specified object is a <see cref="ColorInt" /> structure and is equivalent to this
        ///     <see
        ///         cref="ColorInt" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="obj" /> is a <see cref="ColorInt" /> structure equivalent to this
        ///     <see
        ///         cref="ColorInt" />
        ///     structure; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to test. </param>
        /// <filterpriority>1</filterpriority>
        public override bool Equals(object obj)
        {
            if( obj is ColorInt )
            {
                var color = (ColorInt)obj;
                return _value == color._value;
            }
            return false;
        }

        /// <summary>
        ///     Returns a hash code for this <see cref="ColorInt" /> structure.
        /// </summary>
        /// <returns>
        ///     An integer value that specifies the hash code for this <see cref="ColorInt" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        ///     Converts this <see cref="ColorInt" /> structure to a human-readable string.
        /// </summary>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder(32);
            stringBuilder.Append(GetType().Name);
            stringBuilder.Append(" [");
            if (_value != 0)
            {
                stringBuilder.Append("A=");
                stringBuilder.Append(A);
                stringBuilder.Append(", R=");
                stringBuilder.Append(R);
                stringBuilder.Append(", G=");
                stringBuilder.Append(G);
                stringBuilder.Append(", B=");
                stringBuilder.Append(B);
            }
            else
                stringBuilder.Append("Empty");
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }


        #region Private methods

        private static void CheckByte(int value)
        {
            if( value >= 0 && value <= byte.MaxValue )
                return;
            throw new ArgumentException("InvalidEx2BoundArgument");
        }

        #endregion
    }
}