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

using System.ComponentModel;
using System.Windows;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.Core;
using HtmlRenderer.WPF.Adapters;
using HtmlRenderer.WPF.Utilities;

namespace HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
    /// Using <see cref="AutoSize"/> and <see cref="AutoSizeHeightOnly"/> client can control how the html content effects the
    /// size of the label. Either case scrollbars are never shown and html content outside of client bounds will be clipped.
    /// <see cref="MaximumSize"/> and <see cref="MinimumSize"/> with AutoSize can limit the max/min size of the control<br/>
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
        protected bool _autoSize = true;

        /// <summary>
        /// is to handle auto size of the control height only
        /// </summary>
        protected bool _autoSizeHeight;

        #endregion


        /// <summary>
        /// Automatically sets the size of the label by content size
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
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
                    var newSize = HtmlRendererUtils.Layout(ig, _htmlContainer.HtmlContainerInt, Utils.Convert(constraint), new RSize(MinWidth, MinHeight), new RSize(MaxWidth, MaxHeight), AutoSize, AutoSizeHeightOnly);
                    constraint = Utils.ConvertRound(newSize);
                }
            }

            return constraint;
        }

        #endregion
    }
}