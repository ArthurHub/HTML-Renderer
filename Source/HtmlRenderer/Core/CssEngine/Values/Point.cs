// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal struct Point
    {
        /// <summary>
        ///     Gets the (50%, 50%) point.
        /// </summary>
        public static readonly Point Center = new Point(Length.Half, Length.Half);

        /// <summary>
        ///     Gets the (0, 0) point.
        /// </summary>
        public static readonly Point LeftTop = new Point(Length.Zero, Length.Zero);

        /// <summary>
        ///     Gets the (100%, 0) point.
        /// </summary>
        public static readonly Point RightTop = new Point(Length.Full, Length.Zero);

        /// <summary>
        ///     Gets the (100%, 100%) point.
        /// </summary>
        public static readonly Point RightBottom = new Point(Length.Full, Length.Full);

        /// <summary>
        ///     Gets the (0, 100%) point.
        /// </summary>
        public static readonly Point LeftBottom = new Point(Length.Zero, Length.Full);

        public Point(Length x, Length y)
        {
            X = x;
            Y = y;
        }

        public Length X { get; }
        public Length Y { get; }
    }
}