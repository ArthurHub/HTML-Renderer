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
    ///     Represents an ordered pair of floating-point x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public struct RPoint
    {
        /// <summary>
        ///     Represents a new instance of the <see cref="RPoint" /> class with member data left uninitialized.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public static readonly RPoint Empty = new RPoint();

        private double _x;
        private double _y;

        static RPoint()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RPoint" /> class with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal position of the point. </param>
        /// <param name="y">The vertical position of the point. </param>
        public RPoint(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="RPoint" /> is empty.
        /// </summary>
        /// <returns>
        ///     true if both <see cref="RPoint.X" /> and
        ///     <see
        ///         cref="RPoint.Y" />
        ///     are 0; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public bool IsEmpty
        {
            get
            {
                if (Math.Abs(_x - 0.0) < 0.001)
                    return Math.Abs(_y - 0.0) < 0.001;
                else
                    return false;
            }
        }

        /// <summary>
        ///     Gets or sets the x-coordinate of this <see cref="RPoint" />.
        /// </summary>
        /// <returns>
        ///     The x-coordinate of this <see cref="RPoint" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        ///     Gets or sets the y-coordinate of this <see cref="RPoint" />.
        /// </summary>
        /// <returns>
        ///     The y-coordinate of this <see cref="RPoint" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        ///     Translates the <see cref="RPoint" /> by the specified
        ///     <see
        ///         cref="T:System.Drawing.SizeF" />
        ///     .
        /// </summary>
        /// <returns>
        ///     The translated <see cref="RPoint" />.
        /// </returns>
        /// <param name="pt">
        ///     The <see cref="RPoint" /> to translate.
        /// </param>
        /// <param name="sz">
        ///     The <see cref="T:System.Drawing.SizeF" /> that specifies the numbers to add to the x- and y-coordinates of the
        ///     <see
        ///         cref="RPoint" />
        ///     .
        /// </param>
        public static RPoint operator +(RPoint pt, RSize sz)
        {
            return Add(pt, sz);
        }

        /// <summary>
        ///     Translates a <see cref="RPoint" /> by the negative of a specified
        ///     <see
        ///         cref="T:System.Drawing.SizeF" />
        ///     .
        /// </summary>
        /// <returns>
        ///     The translated <see cref="RPoint" />.
        /// </returns>
        /// <param name="pt">
        ///     The <see cref="RPoint" /> to translate.
        /// </param>
        /// <param name="sz">
        ///     The <see cref="T:System.Drawing.SizeF" /> that specifies the numbers to subtract from the coordinates of
        ///     <paramref
        ///         name="pt" />
        ///     .
        /// </param>
        public static RPoint operator -(RPoint pt, RSize sz)
        {
            return Subtract(pt, sz);
        }

        /// <summary>
        ///     Compares two <see cref="RPoint" /> structures. The result specifies whether the values of the
        ///     <see
        ///         cref="RPoint.X" />
        ///     and <see cref="RPoint.Y" /> properties of the two
        ///     <see
        ///         cref="RPoint" />
        ///     structures are equal.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="RPoint.X" /> and
        ///     <see
        ///         cref="RPoint.Y" />
        ///     values of the left and right
        ///     <see
        ///         cref="RPoint" />
        ///     structures are equal; otherwise, false.
        /// </returns>
        /// <param name="left">
        ///     A <see cref="RPoint" /> to compare.
        /// </param>
        /// <param name="right">
        ///     A <see cref="RPoint" /> to compare.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator ==(RPoint left, RPoint right)
        {
            if (left.X == right.X)
                return left.Y == right.Y;
            else
                return false;
        }

        /// <summary>
        ///     Determines whether the coordinates of the specified points are not equal.
        /// </summary>
        /// <returns>
        ///     true to indicate the <see cref="RPoint.X" /> and
        ///     <see
        ///         cref="RPoint.Y" />
        ///     values of <paramref name="left" /> and
        ///     <paramref
        ///         name="right" />
        ///     are not equal; otherwise, false.
        /// </returns>
        /// <param name="left">
        ///     A <see cref="RPoint" /> to compare.
        /// </param>
        /// <param name="right">
        ///     A <see cref="RPoint" /> to compare.
        /// </param>
        /// <filterpriority>3</filterpriority>
        public static bool operator !=(RPoint left, RPoint right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     Translates a given <see cref="RPoint" /> by a specified
        ///     <see
        ///         cref="T:System.Drawing.SizeF" />
        ///     .
        /// </summary>
        /// <returns>
        ///     The translated <see cref="RPoint" />.
        /// </returns>
        /// <param name="pt">
        ///     The <see cref="RPoint" /> to translate.
        /// </param>
        /// <param name="sz">
        ///     The <see cref="T:System.Drawing.SizeF" /> that specifies the numbers to add to the coordinates of
        ///     <paramref
        ///         name="pt" />
        ///     .
        /// </param>
        public static RPoint Add(RPoint pt, RSize sz)
        {
            return new RPoint(pt.X + sz.Width, pt.Y + sz.Height);
        }

        /// <summary>
        ///     Translates a <see cref="RPoint" /> by the negative of a specified size.
        /// </summary>
        /// <returns>
        ///     The translated <see cref="RPoint" />.
        /// </returns>
        /// <param name="pt">
        ///     The <see cref="RPoint" /> to translate.
        /// </param>
        /// <param name="sz">
        ///     The <see cref="T:System.Drawing.SizeF" /> that specifies the numbers to subtract from the coordinates of
        ///     <paramref
        ///         name="pt" />
        ///     .
        /// </param>
        public static RPoint Subtract(RPoint pt, RSize sz)
        {
            return new RPoint(pt.X - sz.Width, pt.Y - sz.Height);
        }

        /// <summary>
        ///     Specifies whether this <see cref="RPoint" /> contains the same coordinates as the specified
        ///     <see
        ///         cref="T:System.Object" />
        ///     .
        /// </summary>
        /// <returns>
        ///     This method returns true if <paramref name="obj" /> is a <see cref="RPoint" /> and has the same coordinates as this
        ///     <see
        ///         cref="T:System.Drawing.Point" />
        ///     .
        /// </returns>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" /> to test.
        /// </param>
        /// <filterpriority>1</filterpriority>
        public override bool Equals(object obj)
        {
            if (!(obj is RPoint))
                return false;
            var pointF = (RPoint)obj;
            if (pointF.X == X && pointF.Y == Y)
                return pointF.GetType().Equals(GetType());
            else
                return false;
        }

        /// <summary>
        ///     Returns a hash code for this <see cref="RPoint" /> structure.
        /// </summary>
        /// <returns>
        ///     An integer value that specifies a hash value for this <see cref="RPoint" /> structure.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///     Converts this <see cref="RPoint" /> to a human readable string.
        /// </summary>
        /// <returns>
        ///     A string that represents this <see cref="RPoint" />.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string ToString()
        {
            return string.Format("{{X={0}, Y={1}}}", new object[]
            {
                _x,
                _y
            });
        }
    }
}