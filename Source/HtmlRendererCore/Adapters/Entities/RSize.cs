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

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities
{
    /// <summary>
    /// Stores an ordered pair of floating-point numbers, typically the width and height of a rectangle.
    /// </summary>
    public struct RSize
    {
        #region Fields and Consts

        /// <summary>
        ///     Gets a <see cref="RSize" /> structure that has a
        ///     <see
        ///         cref="RSize.Height" />
        ///     and
        ///     <see
        ///         cref="RSize.Width" />
        ///     value of 0.
        /// </summary>
        /// <returns>
        ///     A <see cref="RSize" /> structure that has a
        ///     <see
        ///         cref="RSize.Height" />
        ///     and
        ///     <see
        ///         cref="RSize.Width" />
        ///     value of 0.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public static readonly RSize Empty = new RSize();

        private double _height;
        private double _width;

        #endregion


        /// <summary>
        ///     Initializes a new instance of the <see cref="RSize" /> structure from the specified existing
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </summary>
        /// <param name="size">
        ///     The <see cref="RSize" /> structure from which to create the new
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </param>
        public RSize(RSize size)
        {
            _width = size._width;
            _height = size._height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RSize" /> structure from the specified <see cref="RPoint" /> structure.
        /// </summary>
        /// <param name="pt">The <see cref="RPoint" /> structure from which to initialize this <see cref="RSize" /> structure.</param>
        public RSize(RPoint pt)
        {
            _width = pt.X;
            _height = pt.Y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RSize" /> structure from the specified dimensions.
        /// </summary>
        /// <param name="width">
        ///     The width component of the new <see cref="RSize" /> structure.
        /// </param>
        /// <param name="height">
        ///     The height component of the new <see cref="RSize" /> structure.
        /// </param>
        public RSize(double width, double height)
        {
            _width = width;
            _height = height;
        }

        /// <summary>
        ///     Gets a value that indicates whether this <see cref="RSize" /> structure has zero width and height.
        /// </summary>
        /// <returns>
        ///     This property returns true when this <see cref="RSize" /> structure has both a width and height of zero; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public bool IsEmpty
        {
            get
            {
                if (Math.Abs(_width) < 0.0001)
                    return Math.Abs(_height) < 0.0001;
                else
                    return false;
            }
        }

        /// <summary>
        ///     Gets or sets the horizontal component of this <see cref="RSize" /> structure.
        /// </summary>
        /// <returns>
        ///     The horizontal component of this <see cref="RSize" /> structure, typically measured in pixels.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        ///     Gets or sets the vertical component of this <see cref="RSize" /> structure.
        /// </summary>
        /// <returns>
        ///     The vertical component of this <see cref="RSize" /> structure, typically measured in pixels.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        ///     Converts the specified <see cref="RSize" /> structure to a
        ///     <see cref="RPoint" /> structure.
        /// </summary>
        /// <returns>The <see cref="RPoint" /> structure to which this operator converts.</returns>
        /// <param name="size">The <see cref="RSize" /> structure to be converted
        /// </param>
        public static explicit operator RPoint(RSize size)
        {
            return new RPoint(size.Width, size.Height);
        }

        /// <summary>
        ///     Adds the width and height of one <see cref="RSize" /> structure to the width and height of another
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     A <see cref="RSize" /> structure that is the result of the addition operation.
        /// </returns>
        /// <param name="sz1">
        ///     The first <see cref="RSize" /> structure to add.
        /// </param>
        /// <param name="sz2">
        ///     The second <see cref="RSize" /> structure to add.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static RSize operator +(RSize sz1, RSize sz2)
        {
            return Add(sz1, sz2);
        }

        /// <summary>
        ///     Subtracts the width and height of one <see cref="RSize" /> structure from the width and height of another
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     A <see cref="RSize" /> that is the result of the subtraction operation.
        /// </returns>
        /// <param name="sz1">
        ///     The <see cref="RSize" /> structure on the left side of the subtraction operator.
        /// </param>
        /// <param name="sz2">
        ///     The <see cref="RSize" /> structure on the right side of the subtraction operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static RSize operator -(RSize sz1, RSize sz2)
        {
            return Subtract(sz1, sz2);
        }

        /// <summary>
        ///     Tests whether two <see cref="RSize" /> structures are equal.
        /// </summary>
        /// <returns>
        ///     This operator returns true if <paramref name="sz1" /> and <paramref name="sz2" /> have equal width and height; otherwise, false.
        /// </returns>
        /// <param name="sz1">
        ///     The <see cref="RSize" /> structure on the left side of the equality operator.
        /// </param>
        /// <param name="sz2">
        ///     The <see cref="RSize" /> structure on the right of the equality operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator ==(RSize sz1, RSize sz2)
        {
            if (Math.Abs(sz1.Width - sz2.Width) < 0.001)
                return Math.Abs(sz1.Height - sz2.Height) < 0.001;
            else
                return false;
        }

        /// <summary>
        ///     Tests whether two <see cref="RSize" /> structures are different.
        /// </summary>
        /// <returns>
        ///     This operator returns true if <paramref name="sz1" /> and <paramref name="sz2" /> differ either in width or height; false if
        ///     <paramref
        ///         name="sz1" />
        ///     and <paramref name="sz2" /> are equal.
        /// </returns>
        /// <param name="sz1">
        ///     The <see cref="RSize" /> structure on the left of the inequality operator.
        /// </param>
        /// <param name="sz2">
        ///     The <see cref="RSize" /> structure on the right of the inequality operator.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator !=(RSize sz1, RSize sz2)
        {
            return !(sz1 == sz2);
        }

        /// <summary>
        ///     Adds the width and height of one <see cref="RSize" /> structure to the width and height of another
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     A <see cref="RSize" /> structure that is the result of the addition operation.
        /// </returns>
        /// <param name="sz1">
        ///     The first <see cref="RSize" /> structure to add.
        /// </param>
        /// <param name="sz2">
        ///     The second <see cref="RSize" /> structure to add.
        /// </param>
        public static RSize Add(RSize sz1, RSize sz2)
        {
            return new RSize(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
        }

        /// <summary>
        ///     Subtracts the width and height of one <see cref="RSize" /> structure from the width and height of another
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     A <see cref="RSize" /> structure that is a result of the subtraction operation.
        /// </returns>
        /// <param name="sz1">
        ///     The <see cref="RSize" /> structure on the left side of the subtraction operator.
        /// </param>
        /// <param name="sz2">
        ///     The <see cref="RSize" /> structure on the right side of the subtraction operator.
        /// </param>
        public static RSize Subtract(RSize sz1, RSize sz2)
        {
            return new RSize(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
        }

        /// <summary>
        ///     Tests to see whether the specified object is a <see cref="RSize" /> structure with the same dimensions as this
        ///     <see
        ///         cref="RSize" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     This method returns true if <paramref name="obj" /> is a <see cref="RSize" /> and has the same width and height as this
        ///     <see
        ///         cref="RSize" />
        ///     ; otherwise, false.
        /// </returns>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" /> to test.
        /// </param>
        /// <filterpriority>1</filterpriority>
        public override bool Equals(object obj)
        {
            if (!(obj is RSize))
                return false;
            var sizeF = (RSize)obj;
            if (Math.Abs(sizeF.Width - Width) < 0.001 && Math.Abs(sizeF.Height - Height) < 0.001)
                return sizeF.GetType() == GetType();
            else
                return false;
        }

        /// <summary>
        ///     Returns a hash code for this <see cref="RSize" /> structure.
        /// </summary>
        /// <returns>
        ///     An integer value that specifies a hash value for this <see cref="RSize" /> structure.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///     Converts a <see cref="RSize" /> structure to a <see cref="RPoint" /> structure.
        /// </summary>
        /// <returns>
        ///     Returns a <see cref="RPoint" /> structure.
        /// </returns>
        public RPoint ToPointF()
        {
            return (RPoint)this;
        }

        /// <summary>
        ///     Creates a human-readable string that represents this <see cref="RSize" /> structure.
        /// </summary>
        /// <returns>
        ///     A string that represents this <see cref="RSize" /> structure.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        public override string ToString()
        {
            return "{Width=" + _width + ", Height=" + _height + "}";
        }
    }
}