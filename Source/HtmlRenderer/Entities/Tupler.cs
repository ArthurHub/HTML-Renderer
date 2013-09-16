// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

namespace HtmlRenderer.Entities
{
    /// <summary>
    /// Used to hold two items
    /// </summary>
    internal struct Tupler<T1,T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tupler(T1 item1 = default(T1), T2 item2 = default(T2))
            : this()
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    /// <summary>
    /// Used to hold three items
    /// </summary>
    internal struct Tupler<T1, T2, T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public Tupler(T1 item1 = default(T1), T2 item2 = default(T2), T3 item3 = default(T3))
            : this()
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
}
