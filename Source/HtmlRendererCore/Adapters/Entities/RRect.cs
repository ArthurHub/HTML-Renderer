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
    /// Stores a set of four floating-point numbers that represent the location and size of a rectangle. 
    /// </summary>
    public struct RRect
    {
        #region Fields and Consts

        /// <summary>
        ///     Represents an instance of the <see cref="RRect" /> class with its members uninitialized.
        /// </summary>
        public static readonly RRect Empty = new RRect();

        private double _height;
        private double _width;

        private double _x;
        private double _y;

        #endregion


        /// <summary>
        ///     Initializes a new instance of the <see cref="RRect" /> class with the specified location and size.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle. </param>
        /// <param name="width">The width of the rectangle. </param>
        /// <param name="height">The height of the rectangle. </param>
        public RRect(double x, double y, double width, double height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RRect" /> class with the specified location and size.
        /// </summary>
        /// <param name="location">A <see cref="RPoint" /> that represents the upper-left corner of the rectangular region.</param>
        /// <param name="size">A <see cref="RSize" /> that represents the width and height of the rectangular region.</param>
        public RRect(RPoint location, RSize size)
        {
            _x = location.X;
            _y = location.Y;
            _width = size.Width;
            _height = size.Height;
        }

        /// <summary>
        /// Gets or sets the coordinates of the upper-left corner of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>A <see cref="RPoint" /> that represents the upper-left corner of this <see cref="RRect" /> structure.</returns>
        public RPoint Location
        {
            get { return new RPoint(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of this <see cref="RRect" />.
        /// </summary>
        /// <returns>A <see cref="RSize" /> that represents the width and height of this <see cref="RRect" /> structure.</returns>
        public RSize Size
        {
            get { return new RSize(Width, Height); }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        /// <summary>
        ///     Gets or sets the x-coordinate of the upper-left corner of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The x-coordinate of the upper-left corner of this <see cref="RRect" /> structure.
        /// </returns>
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        ///     Gets or sets the y-coordinate of the upper-left corner of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The y-coordinate of the upper-left corner of this <see cref="RRect" /> structure.
        /// </returns>
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        ///     Gets or sets the width of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The width of this <see cref="RRect" /> structure.
        /// </returns>
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        ///     Gets or sets the height of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The height of this <see cref="RRect" /> structure.
        /// </returns>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        ///     Gets the x-coordinate of the left edge of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The x-coordinate of the left edge of this <see cref="RRect" /> structure.
        /// </returns>
        public double Left
        {
            get { return X; }
        }

        /// <summary>
        ///     Gets the y-coordinate of the top edge of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The y-coordinate of the top edge of this <see cref="RRect" /> structure.
        /// </returns>
        public double Top
        {
            get { return Y; }
        }

        /// <summary>
        ///     Gets the x-coordinate that is the sum of <see cref="RRect.X" /> and
        ///     <see
        ///         cref="RRect.Width" />
        ///     of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The x-coordinate that is the sum of <see cref="RRect.X" /> and
        ///     <see
        ///         cref="RRect.Width" />
        ///     of this <see cref="RRect" /> structure.
        /// </returns>
        public double Right
        {
            get { return X + Width; }
        }

        /// <summary>
        ///     Gets the y-coordinate that is the sum of <see cref="RRect.Y" /> and
        ///     <see
        ///         cref="RRect.Height" />
        ///     of this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     The y-coordinate that is the sum of <see cref="RRect.Y" /> and
        ///     <see
        ///         cref="RRect.Height" />
        ///     of this <see cref="RRect" /> structure.
        /// </returns>
        public double Bottom
        {
            get { return Y + Height; }
        }

        /// <summary>
        ///     Tests whether the <see cref="RRect.Width" /> or
        ///     <see
        ///         cref="RRect.Height" />
        ///     property of this <see cref="RRect" /> has a value of zero.
        /// </summary>
        /// <returns>
        ///     This property returns true if the <see cref="RRect.Width" /> or
        ///     <see
        ///         cref="RRect.Height" />
        ///     property of this <see cref="RRect" /> has a value of zero; otherwise, false.
        /// </returns>
        public bool IsEmpty
        {
            get
            {
                if (Width > 0.0)
                    return Height <= 0.0;
                else
                    return true;
            }
        }

        /// <summary>
        ///     Tests whether two <see cref="RRect" /> structures have equal location and size.
        /// </summary>
        /// <returns>
        ///     This operator returns true if the two specified <see cref="RRect" /> structures have equal
        ///     <see cref="RRect.X" />, <see cref="RRect.Y" />, <see cref="RRect.Width" />, and <see cref="RRect.Height" /> properties.
        /// </returns>
        /// <param name="left">
        ///     The <see cref="RRect" /> structure that is to the left of the equality operator.
        /// </param>
        /// <param name="right">
        ///     The <see cref="RRect" /> structure that is to the right of the equality operator.
        /// </param>
        public static bool operator ==(RRect left, RRect right)
        {
            if (Math.Abs(left.X - right.X) < 0.001 && Math.Abs(left.Y - right.Y) < 0.001 && Math.Abs(left.Width - right.Width) < 0.001)
                return Math.Abs(left.Height - right.Height) < 0.001;
            else
                return false;
        }

        /// <summary>
        ///     Tests whether two <see cref="RRect" /> structures differ in location or size.
        /// </summary>
        /// <returns>
        ///     This operator returns true if any of the <see cref="RRect.X" /> ,
        ///     <see cref="RRect.Y" />, <see cref="RRect.Width" />, or <see cref="RRect.Height" />
        ///     properties of the two <see cref="RRect" /> structures are unequal; otherwise false.
        /// </returns>
        /// <param name="left">
        ///     The <see cref="RRect" /> structure that is to the left of the inequality operator.
        /// </param>
        /// <param name="right">
        ///     The <see cref="RRect" /> structure that is to the right of the inequality operator.
        /// </param>
        public static bool operator !=(RRect left, RRect right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     Creates a <see cref="RRect" /> structure with upper-left corner and lower-right corner at the specified locations.
        /// </summary>
        /// <returns>
        ///     The new <see cref="RRect" /> that this method creates.
        /// </returns>
        /// <param name="left">The x-coordinate of the upper-left corner of the rectangular region. </param>
        /// <param name="top">The y-coordinate of the upper-left corner of the rectangular region. </param>
        /// <param name="right">The x-coordinate of the lower-right corner of the rectangular region. </param>
        /// <param name="bottom">The y-coordinate of the lower-right corner of the rectangular region. </param>
        public static RRect FromLTRB(double left, double top, double right, double bottom)
        {
            return new RRect(left, top, right - left, bottom - top);
        }

        /// <summary>
        ///     Tests whether <paramref name="obj" /> is a <see cref="RRect" /> with the same location and size of this
        ///     <see cref="RRect" />.
        /// </summary>
        /// <returns>
        ///     This method returns true if <paramref name="obj" /> is a <see cref="RRect" /> and its X, Y, Width, and Height properties are equal to the corresponding properties of this
        ///     <see cref="RRect" />; otherwise, false.
        /// </returns>
        /// <param name="obj">
        ///     The <see cref="T:System.Object" /> to test.
        /// </param>
        public override bool Equals(object obj)
        {
            if (!(obj is RRect))
                return false;
            var rectangleF = (RRect)obj;
            if (Math.Abs(rectangleF.X - X) < 0.001 && Math.Abs(rectangleF.Y - Y) < 0.001 && Math.Abs(rectangleF.Width - Width) < 0.001)
                return Math.Abs(rectangleF.Height - Height) < 0.001;
            else
                return false;
        }

        /// <summary>
        ///     Determines if the specified point is contained within this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     This method returns true if the point defined by <paramref name="x" /> and <paramref name="y" /> is contained within this
        ///     <see cref="RRect" />
        ///     structure; otherwise false.
        /// </returns>
        /// <param name="x">The x-coordinate of the point to test. </param>
        /// <param name="y">The y-coordinate of the point to test. </param>
        public bool Contains(double x, double y)
        {
            if (X <= x && x < X + Width && Y <= y)
                return y < Y + Height;
            else
                return false;
        }

        /// <summary>
        ///     Determines if the specified point is contained within this <see cref="RRect" /> structure.
        /// </summary>
        /// <returns>
        ///     This method returns true if the point represented by the <paramref name="pt" /> parameter is contained within this
        ///     <see cref="RRect" />
        ///     structure; otherwise false.
        /// </returns>
        /// <param name="pt">The <see cref="RPoint" /> to test.</param>
        public bool Contains(RPoint pt)
        {
            return Contains(pt.X, pt.Y);
        }

        /// <summary>
        ///     Determines if the rectangular region represented by <paramref name="rect" /> is entirely contained within this
        ///     <see cref="RRect" />
        ///     structure.
        /// </summary>
        /// <returns>
        ///     This method returns true if the rectangular region represented by <paramref name="rect" /> is entirely contained within the rectangular region represented by this
        ///     <see cref="RRect" />
        ///     ; otherwise false.
        /// </returns>
        /// <param name="rect">
        ///     The <see cref="RRect" /> to test.
        /// </param>
        public bool Contains(RRect rect)
        {
            if (X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y)
                return rect.Y + rect.Height <= Y + Height;
            else
                return false;
        }

        /// <summary>
        ///     Inflates this <see cref="RRect" /> structure by the specified amount.
        /// </summary>
        /// <param name="x">
        ///     The amount to inflate this <see cref="RRect" /> structure horizontally.
        /// </param>
        /// <param name="y">
        ///     The amount to inflate this <see cref="RRect" /> structure vertically.
        /// </param>
        public void Inflate(double x, double y)
        {
            X -= x;
            Y -= y;
            Width += 2f * x;
            Height += 2f * y;
        }

        /// <summary>
        ///     Inflates this <see cref="RRect" /> by the specified amount.
        /// </summary>
        /// <param name="size">The amount to inflate this rectangle. </param>
        public void Inflate(RSize size)
        {
            Inflate(size.Width, size.Height);
        }

        /// <summary>
        ///     Creates and returns an inflated copy of the specified <see cref="RRect" /> structure. The copy is inflated by the specified amount. The original rectangle remains unmodified.
        /// </summary>
        /// <returns>
        ///     The inflated <see cref="RRect" />.
        /// </returns>
        /// <param name="rect">
        ///     The <see cref="RRect" /> to be copied. This rectangle is not modified.
        /// </param>
        /// <param name="x">The amount to inflate the copy of the rectangle horizontally. </param>
        /// <param name="y">The amount to inflate the copy of the rectangle vertically. </param>
        public static RRect Inflate(RRect rect, double x, double y)
        {
            RRect rectangleF = rect;
            rectangleF.Inflate(x, y);
            return rectangleF;
        }

        /// <summary>
        ///     Replaces this <see cref="RRect" /> structure with the intersection of itself and the specified
        ///     <see
        ///         cref="RRect" />
        ///     structure.
        /// </summary>
        /// <param name="rect">The rectangle to intersect. </param>
        public void Intersect(RRect rect)
        {
            RRect rectangleF = Intersect(rect, this);
            X = rectangleF.X;
            Y = rectangleF.Y;
            Width = rectangleF.Width;
            Height = rectangleF.Height;
        }

        /// <summary>
        ///     Returns a <see cref="RRect" /> structure that represents the intersection of two rectangles. If there is no intersection, and empty
        ///     <see
        ///         cref="RRect" />
        ///     is returned.
        /// </summary>
        /// <returns>
        ///     A third <see cref="RRect" /> structure the size of which represents the overlapped area of the two specified rectangles.
        /// </returns>
        /// <param name="a">A rectangle to intersect. </param>
        /// <param name="b">A rectangle to intersect. </param>
        public static RRect Intersect(RRect a, RRect b)
        {
            double x = Math.Max(a.X, b.X);
            double num1 = Math.Min(a.X + a.Width, b.X + b.Width);
            double y = Math.Max(a.Y, b.Y);
            double num2 = Math.Min(a.Y + a.Height, b.Y + b.Height);
            if (num1 >= x && num2 >= y)
                return new RRect(x, y, num1 - x, num2 - y);
            else
                return Empty;
        }

        /// <summary>
        ///     Determines if this rectangle intersects with <paramref name="rect" />.
        /// </summary>
        /// <returns>
        ///     This method returns true if there is any intersection.
        /// </returns>
        /// <param name="rect">The rectangle to test. </param>
        public bool IntersectsWith(RRect rect)
        {
            if (rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height)
                return Y < rect.Y + rect.Height;
            else
                return false;
        }

        /// <summary>
        ///     Creates the smallest possible third rectangle that can contain both of two rectangles that form a union.
        /// </summary>
        /// <returns>
        ///     A third <see cref="RRect" /> structure that contains both of the two rectangles that form the union.
        /// </returns>
        /// <param name="a">A rectangle to union. </param>
        /// <param name="b">A rectangle to union. </param>
        public static RRect Union(RRect a, RRect b)
        {
            double x = Math.Min(a.X, b.X);
            double num1 = Math.Max(a.X + a.Width, b.X + b.Width);
            double y = Math.Min(a.Y, b.Y);
            double num2 = Math.Max(a.Y + a.Height, b.Y + b.Height);
            return new RRect(x, y, num1 - x, num2 - y);
        }

        /// <summary>
        ///     Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="pos">The amount to offset the location. </param>
        public void Offset(RPoint pos)
        {
            Offset(pos.X, pos.Y);
        }

        /// <summary>
        ///     Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="x">The amount to offset the location horizontally. </param>
        /// <param name="y">The amount to offset the location vertically. </param>
        public void Offset(double x, double y)
        {
            X += x;
            Y += y;
        }

        /// <summary>
        ///     Gets the hash code for this <see cref="RRect" /> structure. For information about the use of hash codes, see Object.GetHashCode.
        /// </summary>
        /// <returns>The hash code for this <see cref="RRect" /></returns>
        public override int GetHashCode()
        {
            return (int)(uint)X ^ ((int)(uint)Y << 13 | (int)((uint)Y >> 19)) ^ ((int)(uint)Width << 26 | (int)((uint)Width >> 6)) ^ ((int)(uint)Height << 7 | (int)((uint)Height >> 25));
        }

        /// <summary>
        /// Converts the Location and Size of this <see cref="RRect" /> to a human-readable string.
        /// </summary>
        /// <returns>
        /// A string that contains the position, width, and height of this <see cref="RRect" /> structure for example, "{X=20, Y=20, Width=100, Height=50}".
        /// </returns>
        public override string ToString()
        {
            return "{X=" + X + ",Y=" + Y + ",Width=" + Width + ",Height=" + Height + "}";
        }
    }
}