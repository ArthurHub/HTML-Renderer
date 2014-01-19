// Type: System.Drawing.Color
// Assembly: System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Drawing.dll

using System;
using System.Text;

namespace HtmlRenderer.Core.SysEntities
{
    /// <summary>
    ///     Represents an ARGB (alpha, red, green, blue) color.
    /// </summary>
    /// <filterpriority>1</filterpriority>
    /// <completionlist cref="T:HtmlRenderer.Core.SysEntities.Color" />
    public struct ColorInt
    {
        /// <summary>
        ///     Represents a color that is null.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public static readonly ColorInt Empty = new ColorInt();

        private readonly long _value;

        static ColorInt()
        {}

        private ColorInt(long value)
        {
            _value = value;
        }

        /// <summary>
        ///     Gets a system-defined color.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> representing a system-defined color.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public static ColorInt Transparent
        {
            get { return new ColorInt(0); }
        }

        /// <summary>
        ///     Gets a system-defined color that has an ARGB value of #FF000000.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> representing a system-defined color.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public static ColorInt Black
        {
            get { return FromArgb(0,0,0); }
        }

        /// <summary>
        ///     Gets a system-defined color that has an ARGB value of #FFFFFFFF.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> representing a system-defined color.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public static ColorInt White
        {
            get { return FromArgb(255,255,255); }
        }

        /// <summary>
        ///     Gets the red component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure.
        /// </summary>
        /// <returns>
        ///     The red component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public byte R
        {
            get { return (byte)( (ulong)( _value >> 16 ) & byte.MaxValue ); }
        }

        /// <summary>
        ///     Gets the green component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure.
        /// </summary>
        /// <returns>
        ///     The green component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public byte G
        {
            get { return (byte)((ulong)(_value >> 8) & byte.MaxValue); }
        }

        /// <summary>
        ///     Gets the blue component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure.
        /// </summary>
        /// <returns>
        ///     The blue component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public byte B
        {
            get { return (byte)((ulong)_value & byte.MaxValue); }
        }

        /// <summary>
        ///     Gets the alpha component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure.
        /// </summary>
        /// <returns>
        ///     The alpha component value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public byte A
        {
            get { return (byte)((ulong)(_value >> 24) & byte.MaxValue); }
        }

        /// <summary>
        ///     Specifies whether this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure is uninitialized.
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
        ///     Tests whether two specified <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structures are equivalent.
        /// </summary>
        /// <returns>
        ///     true if the two <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structures are equal; otherwise, false.
        /// </returns>
        /// <param name="left">
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that is to the left of the equality operator.
        /// </param>
        /// <param name="right">
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that is to the right of the equality operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator ==(ColorInt left, ColorInt right)
        {
            return left._value == right._value;
        }

        /// <summary>
        ///     Tests whether two specified <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structures are different.
        /// </summary>
        /// <returns>
        ///     true if the two <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structures are different; otherwise, false.
        /// </returns>
        /// <param name="left">
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that is to the left of the inequality operator.
        /// </param>
        /// <param name="right">
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that is to the right of the inequality operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator !=(ColorInt left, ColorInt right)
        {
            return !( left == right );
        }

        private static void CheckByte(int value)
        {
            if( value >= 0 && value <= byte.MaxValue )
                return;
            throw new ArgumentException("InvalidEx2BoundArgument");
        }

        private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
        {
            return (uint)( red << 16 | green << 8 | blue | alpha << 24 ) & (long)uint.MaxValue;
        }

        /// <summary>
        ///     Creates a <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure from a 32-bit ARGB value.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure that this method creates.
        /// </returns>
        /// <param name="argb">A value specifying the 32-bit ARGB value. </param>
        /// <filterpriority>1</filterpriority>
        public static ColorInt FromArgb(int argb)
        {
            return new ColorInt(argb & uint.MaxValue);
        }

        /// <summary>
        ///     Creates a <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure from the four ARGB component (alpha, red, green, and blue) values. Although this method allows a 32-bit value to be passed for each component, the value of each component is limited to 8 bits.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that this method creates.
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
            return new ColorInt(MakeArgb((byte)alpha, (byte)red, (byte)green, (byte)blue));
        }

        /// <summary>
        ///     Creates a <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure from the specified
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
        ///     structure, but with the new specified alpha value. Although this method allows a 32-bit value to be passed for the alpha value, the value is limited to 8 bits.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that this method creates.
        /// </returns>
        /// <param name="alpha">
        ///     The alpha value for the new <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. Valid values are 0 through 255.
        /// </param>
        /// <param name="baseColor">
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> from which to create the new
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
        ///     .
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="alpha" /> is less than 0 or greater than 255.
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public static ColorInt FromArgb(int alpha, ColorInt baseColor)
        {
            CheckByte(alpha);
            return new ColorInt(MakeArgb((byte)alpha, baseColor.R, baseColor.G, baseColor.B));
        }

        /// <summary>
        ///     Creates a <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure from the specified 8-bit color values (red, green, and blue). The alpha value is implicitly 255 (fully opaque). Although this method allows a 32-bit value to be passed for each color component, the value of each component is limited to 8 bits.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> that this method creates.
        /// </returns>
        /// <param name="red">
        ///     The red component value for the new <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. Valid values are 0 through 255.
        /// </param>
        /// <param name="green">
        ///     The green component value for the new <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. Valid values are 0 through 255.
        /// </param>
        /// <param name="blue">
        ///     The blue component value for the new <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. Valid values are 0 through 255.
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
        ///     Gets the hue-saturation-brightness (HSB) brightness value for this
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     The brightness of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. The brightness ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public float GetBrightness()
        {
            float num1 = R/(float)byte.MaxValue;
            float num2 = G/(float)byte.MaxValue;
            float num3 = B/(float)byte.MaxValue;
            float num4 = num1;
            float num5 = num1;
            if( num2 > (double)num4 )
                num4 = num2;
            if( num3 > (double)num4 )
                num4 = num3;
            if( num2 < (double)num5 )
                num5 = num2;
            if( num3 < (double)num5 )
                num5 = num3;
            return (float)( ( num4 + (double)num5 )/2.0 );
        }

        /// <summary>
        ///     Gets the hue-saturation-brightness (HSB) hue value, in degrees, for this
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     The hue, in degrees, of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. The hue is measured in degrees, ranging from 0.0 through 360.0, in HSB color space.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public float GetHue()
        {
            if( R == G && G == B )
                return 0.0f;
            float num1 = R/(float)byte.MaxValue;
            float num2 = G/(float)byte.MaxValue;
            float num3 = B/(float)byte.MaxValue;
            float num4 = 0.0f;
            float num5 = num1;
            float num6 = num1;
            if( num2 > (double)num5 )
                num5 = num2;
            if( num3 > (double)num5 )
                num5 = num3;
            if( num2 < (double)num6 )
                num6 = num2;
            if( num3 < (double)num6 )
                num6 = num3;
            float num7 = num5 - num6;
            if( num1 == (double)num5 )
                num4 = ( num2 - num3 )/num7;
            else if( num2 == (double)num5 )
                num4 = (float)( 2.0 + ( num3 - (double)num1 )/num7 );
            else if( num3 == (double)num5 )
                num4 = (float)( 4.0 + ( num1 - (double)num2 )/num7 );
            float num8 = num4*60f;
            if( num8 < 0.0 )
                num8 += 360f;
            return num8;
        }

        /// <summary>
        ///     Gets the hue-saturation-brightness (HSB) saturation value for this
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     The saturation of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public float GetSaturation()
        {
            float num1 = R/(float)byte.MaxValue;
            float num2 = G/(float)byte.MaxValue;
            float num3 = B/(float)byte.MaxValue;
            float num4 = 0.0f;
            float num5 = num1;
            float num6 = num1;
            if( num2 > (double)num5 )
                num5 = num2;
            if( num3 > (double)num5 )
                num5 = num3;
            if( num2 < (double)num6 )
                num6 = num2;
            if( num3 < (double)num6 )
                num6 = num3;
            if( num5 != (double)num6 )
                num4 = ( (double)num5 + (double)num6 )/2.0 > 0.5 ? (float)( ( num5 - (double)num6 )/( 2.0 - num5 - num6 ) ) : (float)( ( num5 - (double)num6 )/( num5 + (double)num6 ) );
            return num4;
        }

        /// <summary>
        ///     Gets the 32-bit ARGB value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure.
        /// </summary>
        /// <returns>
        ///     The 32-bit ARGB value of this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public int ToArgb()
        {
            return (int)_value;
        }

        /// <summary>
        ///     Converts this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure to a human-readable string.
        /// </summary>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder(32);
            stringBuilder.Append(GetType().Name);
            stringBuilder.Append(" [");
            if( _value != 0 )
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

        /// <summary>
        ///     Tests whether the specified object is a <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure and is equivalent to this
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="obj" /> is a <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure equivalent to this
        ///     <see
        ///         cref="T:HtmlRenderer.Core.SysEntities.Color" />
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
        ///     Returns a hash code for this <see cref="T:HtmlRenderer.Core.SysEntities.Color" /> structure.
        /// </summary>
        /// <returns>
        ///     An integer value that specifies the hash code for this <see cref="T:HtmlRenderer.Core.SysEntities.Color" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}