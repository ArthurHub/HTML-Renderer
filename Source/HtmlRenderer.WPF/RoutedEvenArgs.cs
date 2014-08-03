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

using System.Windows;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.WPF
{
    /// <summary>
    /// Handler for HTML renderer routed events.
    /// </summary>
    /// <param name="args">the event arguments object</param>
    /// <typeparam name="T">the type of the routed events args data</typeparam>
    public delegate void RoutedEventHandler<T>(object sender, RoutedEvenArgs<T> args) where T : class;

    /// <summary>
    /// HTML Renderer routed event arguments containing event data.
    /// </summary>
    public sealed class RoutedEvenArgs<T> : RoutedEventArgs where T : class
    {
        /// <summary>
        /// the argument data of the routed event
        /// </summary>
        private readonly T _data;

        public RoutedEvenArgs(RoutedEvent routedEvent, T data)
            : base(routedEvent)
        {
            ArgChecker.AssertArgNotNull(data, "args");
            _data = data;
        }

        public RoutedEvenArgs(RoutedEvent routedEvent, object source, T data)
            : base(routedEvent, source)
        {
            ArgChecker.AssertArgNotNull(data, "args");
            _data = data;
        }

        /// <summary>
        /// the argument data of the routed event
        /// </summary>
        public T Data
        {
            get { return _data; }
        }

        public override string ToString()
        {
            return string.Format("RoutedEventArgs({0})", _data);
        }
    }
}