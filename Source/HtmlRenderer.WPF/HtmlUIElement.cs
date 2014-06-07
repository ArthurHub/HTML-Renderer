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
using System.Windows;
using System.Windows.Media;
using HtmlRenderer.Core.Utils;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public class HtmlUIElement : UIElement
    {
        #region Fields/Consts

        /// <summary>
        /// Underline html container instance.
        /// </summary>
        private readonly HtmlContainer _htmlContainer;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public HtmlUIElement(HtmlContainer htmlContainer)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            _htmlContainer = htmlContainer;
        }

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override Size MeasureCore(Size constraint)
        {
            Size size = PerformHtmlLayout(constraint);

            // to handle if vertical scrollbar is appearing or disappearing
            if (_htmlContainer != null && Math.Abs(_htmlContainer.MaxSize.Width - constraint.Width) > 0.1)
            {
                size = PerformHtmlLayout(constraint);
            }

            return size;
        }

        /// <summary>
        /// Perform html container layout by the current panel client size.
        /// </summary>
        protected Size PerformHtmlLayout(Size constraint)
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.MaxSize = new Size(constraint.Width, 0);

                DrawingGroup dGroup = new DrawingGroup();
                using (var g = dGroup.Open())
                {
                    _htmlContainer.PerformLayout(g);
                }

                return _htmlContainer.ActualSize;
            }
            return Size.Empty;
        }



        /// <summary>
        /// Perform paint of the html in the control.
        /// </summary>
        protected override void OnRender(DrawingContext context)
        {
            if (_htmlContainer != null)
            {
                _htmlContainer.PerformPaint(context);

                // TODO:a handle if we need to refresh the pointer here
                // call mouse move to handle paint after scroll or html change affecting mouse cursor.
                //                var mp = PointToClient(MousePosition);
                //                _htmlContainer.HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
            }
        }
 
    }
}