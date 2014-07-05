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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.Core;
using HtmlRenderer.WPF.Adapters;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
    /// Using <see cref="AutoSize"/> and <see cref="AutoSizeHeightOnly"/> client can control how the html content effects the
    /// size of the label. Either case scrollbars are never shown and html content outside of client bounds will be clipped.
    /// MaxWidth/MaxHeight and MinWidth/MinHeight with AutoSize can limit the max/min size of the control<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// </summary>
    /// <remarks>
    /// See <see cref="HtmlControlBase"/> for more info.
    /// </remarks>
    public class HtmlLabel : HtmlControlBase
    {
        #region Fields and Consts

        /// <summary>
        /// Automatically sets the size of the label by content size.
        /// </summary>
        protected bool _autoSize;

        /// <summary>
        /// is to handle auto size of the control height only
        /// </summary>
        protected bool _autoSizeHeight;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public HtmlLabel()
        {
            _autoSize = true;
            Background = Brushes.Transparent;
        }

        /// <summary>
        /// Automatically sets the size of the label by content size
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Layout")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Automatically sets the size of the label by content size.")]
        public bool AutoSize
        {
            get { return _autoSize; }
            set
            {
                _autoSize = value;
                if (value)
                {
                    _autoSizeHeight = false;
                    InvalidateMeasure();
                    InvalidateVisual();
                }
            }
        }

        /// <summary>
        /// Automatically sets the height of the label by content height (width is not effected).
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category("Layout")]
        [Description("Automatically sets the height of the label by content height (width is not effected)")]
        public virtual bool AutoSizeHeightOnly
        {
            get { return _autoSizeHeight; }
            set
            {
                _autoSizeHeight = value;
                if (value)
                {
                    AutoSize = false;
                    InvalidateMeasure();
                    InvalidateVisual();
                }
            }
        }


        #region Private methods

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (_htmlContainer != null)
            {
                using (var ig = new GraphicsAdapter())
                {
                    var size = new RSize(constraint.Width < Double.PositiveInfinity ? constraint.Width - BorderThickness.Left - BorderThickness.Right : 0,
                        constraint.Height < Double.PositiveInfinity ? constraint.Height - BorderThickness.Top - BorderThickness.Bottom : 0);
                    var minSize = new RSize(MinWidth < Double.PositiveInfinity ? MinWidth : 0, MinHeight < Double.PositiveInfinity ? MinHeight : 0);
                    var maxSize = new RSize(MaxWidth < Double.PositiveInfinity ? MaxWidth : 0, MaxHeight < Double.PositiveInfinity ? MaxHeight : 0);
                    var newSize = HtmlRendererUtils.Layout(ig, _htmlContainer.HtmlContainerInt, size, minSize, maxSize, AutoSize, AutoSizeHeightOnly);
                    constraint = new Size(newSize.Width + BorderThickness.Left + BorderThickness.Right, newSize.Height + BorderThickness.Top + BorderThickness.Bottom);
                }
            }

            return constraint;
        }

        #endregion
    }
}