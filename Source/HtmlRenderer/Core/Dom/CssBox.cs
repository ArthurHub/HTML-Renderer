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
using System.Collections.Generic;
using System.Globalization;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Fragmentation;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Represents a CSS Box of text or replaced elements.
    /// </summary>
    /// <remarks>
    /// The Box can contains other boxes, that's the way that the CSS Tree
    /// is composed.
    /// 
    /// To know more about boxes visit CSS spec:
    /// http://www.w3.org/TR/CSS21/box.html
    /// </remarks>
    internal class CssBox : CssBoxProperties, IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// the parent css box of this css box in the hierarchy
        /// </summary>
        private CssBox _parentBox;

        /// <summary>
        /// the root container for the hierarchy
        /// </summary>
        protected HtmlContainerInt _htmlContainer;

        /// <summary>
        /// the html tag that is associated with this css box, null if anonymous box
        /// </summary>
        private readonly HtmlTag _htmltag;

        private readonly List<CssRect> _boxWords = new List<CssRect>();
        private readonly List<CssBox> _boxes = new List<CssBox>();
        private readonly List<CssLineBox> _lineBoxes = new List<CssLineBox>();
        private readonly List<CssLineBox> _parentLineBoxes = new List<CssLineBox>();
        private readonly Dictionary<CssLineBox, RRect> _rectangles = new Dictionary<CssLineBox, RRect>();

        /// <summary>
        /// the inner text of the box
        /// </summary>
        private string _text;

        /// <summary>
        /// Do not use or alter this flag
        /// </summary>
        /// <remarks>
        /// Flag that indicates that CssTable algorithm already made fixes on it.
        /// </remarks>
        internal bool _tableFixed;

        protected bool _wordsSizeMeasured;
        private CssBox _listItemBox;

        /// <summary>
        /// The synthetic list-item marker box, if this box has one - not part of <see cref="Boxes"/>
        /// (it has no parent box), so it is otherwise unreachable by a tree walk.
        /// </summary>
        internal CssBox ListItemBox
        {
            get { return _listItemBox; }
        }

        /// <summary>
        /// For a table box only: detached clones of the table's own &lt;thead&gt; rows, one set per
        /// continuation page the table's body spans (css-tables-3 6.2's repeated headers) - not part
        /// of <see cref="Boxes"/> (so re-running table layout can never mistake them for real body
        /// content), rebuilt from scratch on every layout pass by <see cref="Fragmentation.TableHeaderRepeat"/>.
        /// Null when the table has no header or never crosses a page boundary.
        /// </summary>
        internal List<CssBox> RepeatedHeaderRows { get; set; }

        /// <summary>
        /// The resumption record this box should re-enter its own child loop with this pass, seeded by
        /// the parent's <see cref="ResumeAt"/> call right before invoking this box's layout - null for a
        /// box entered fresh this pass (no earlier pass stopped inside it). See <see cref="BreakToken"/>'s
        /// own doc comment for the chain shape.
        /// </summary>
        private BreakToken _incomingToken;

        /// <summary>
        /// A pre-decided document-Y top this box must place itself at this pass, rather than deriving one
        /// from its previous sibling - set only for a box being placed for the first time after an earlier
        /// pass requested a break before it (<see cref="RequestedBreakBeforeTop"/>). Must not be re-derived:
        /// re-deriving it would reach the same "doesn't fit" conclusion and request a break before itself
        /// again, forever.
        /// </summary>
        private double? _resumeTopOverride;

        /// <summary>
        /// Set by this box's own child-loop right after a child's layout call returns with either
        /// <see cref="RequestedBreakBeforeTop"/> set (wrapped as an <c>IsBreakBefore</c> link) or its own
        /// <see cref="PendingBreakToken"/> set (wrapped as a continuation link) - the mechanism that lets a
        /// break discovered arbitrarily deep in the tree reach <see cref="HtmlContainerInt"/>'s pass loop:
        /// every ancestor's own child loop checks this immediately after its child's layout call returns,
        /// and if set, stops laying out further siblings this pass and reflects the same fact to its own
        /// parent. Reset to null at the top of every <see cref="PerformLayoutImp"/> call.
        /// </summary>
        internal BreakToken PendingBreakToken { get; private set; }

        /// <summary>
        /// Set by this box's own layout when a forced <c>break-before</c>/<c>break-after</c> means it
        /// cannot be placed this pass at all - the box performs no further layout work and returns
        /// immediately, leaving its parent's child loop to notice this (right after the layout call
        /// returns) and stop, wrapping <see cref="RequestedBreakBeforeSlot"/>/this value into a
        /// <c>BlockBreakToken(IsBreakBefore: true)</c>. Reset to null at the top of every
        /// <see cref="PerformLayoutImp"/> call.
        /// </summary>
        internal double? RequestedBreakBeforeTop { get; private set; }

        /// <summary>The pagination slot <see cref="RequestedBreakBeforeTop"/> falls in.</summary>
        internal int RequestedBreakBeforeSlot { get; private set; }

        private CssLineBox _firstHostingLineBox;
        private CssLineBox _lastHostingLineBox;

        /// <summary>
        /// handler for loading background image
        /// </summary>
        private ImageLoadHandler _imageLoadHandler;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parentBox">optional: the parent of this css box in html</param>
        /// <param name="tag">optional: the html tag associated with this css box</param>
        public CssBox(CssBox parentBox, HtmlTag tag)
        {
            if (parentBox != null)
            {
                _parentBox = parentBox;
                _parentBox.Boxes.Add(this);
            }
            _htmltag = tag;
        }

        /// <summary>
        /// Gets the HtmlContainer of the Box.
        /// WARNING: May be null.
        /// </summary>
        public HtmlContainerInt HtmlContainer
        {
            get { return _htmlContainer ?? (_htmlContainer = _parentBox != null ? _parentBox.HtmlContainer : null); }
            set { _htmlContainer = value; }
        }

        /// <summary>
        /// Gets or sets the parent box of this box
        /// </summary>
        public CssBox ParentBox
        {
            get { return _parentBox; }
            set
            {
                //Remove from last parent
                if (_parentBox != null)
                    _parentBox.Boxes.Remove(this);

                _parentBox = value;

                //Add to new parent
                if (value != null)
                    _parentBox.Boxes.Add(this);
            }
        }

        /// <summary>
        /// Gets the children boxes of this box
        /// </summary>
        public List<CssBox> Boxes
        {
            get { return _boxes; }
        }

        /// <summary>
        /// Is the box is of "br" element.
        /// </summary>
        public bool IsBrElement
        {
            get {
                return _htmltag != null && _htmltag.Name.Equals("br", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Is the box "Display" is one of the table-row-group/table-header-group/table-footer-group values.
        /// </summary>
        public bool IsTableRowGroupBox
        {
            get { return Display == CssConstants.TableRowGroup || Display == CssConstants.TableHeaderGroup || Display == CssConstants.TableFooterGroup; }
        }

        /// <summary>
        /// Is this box a synthesized <c>::before</c> pseudo-element (see <see cref="CssData"/>'s selector-matching synthesis).
        /// </summary>
        public bool IsBeforePseudoElement { get; set; }

        /// <summary>
        /// Is this box a synthesized <c>::after</c> pseudo-element (see <see cref="CssData"/>'s selector-matching synthesis).
        /// </summary>
        public bool IsAfterPseudoElement { get; set; }

        /// <summary>
        /// Is this box a synthesized <c>::before</c>/<c>::after</c> pseudo-element.
        /// </summary>
        public bool IsPseudoElement
        {
            get { return IsBeforePseudoElement || IsAfterPseudoElement; }
        }

        /// <summary>
        /// is the box "Display" is "Inline", is this is an inline box and not block.
        /// </summary>
        public bool IsInline
        {
            get { return (Display == CssConstants.Inline || Display == CssConstants.InlineBlock) && !IsBrElement; }
        }

        /// <summary>
        /// is the box "Display" is "Block", is this is an block box and not inline.
        /// </summary>
        public bool IsBlock
        {
            get { return Display == CssConstants.Block; }
        }

        /// <summary>
        /// Is this box floated (float: left or float: right).
        /// </summary>
        public bool IsFloated
        {
            get { return Float == CssConstants.Left || Float == CssConstants.Right; }
        }

        /// <summary>
        /// True for a box removed from normal flow: floated, or absolutely/fixed positioned. Ported
        /// from PeachPDF's CssBox.IsOutOfFlow - used to find a container's last IN-FLOW child (e.g.
        /// <see cref="MarginBottomCollapse"/>), since an out-of-flow box doesn't contribute to a
        /// non-BFC-establishing container's own auto-height (CSS 2.1 10.6.3/10.6.7).
        /// </summary>
        public bool IsOutOfFlow
        {
            get { return IsFloated || Position == CssConstants.Absolute || Position == CssConstants.Fixed; }
        }

        /// <summary>
        /// Is the css box clickable (by default only "a" elements that are actual hyperlinks - i.e.
        /// have an "href" - are clickable; an "a" used only as a named anchor/target has no href and
        /// is not clickable, matching the same "href" gate CSS uses for the :link pseudo-class).
        /// </summary>
        public virtual bool IsClickable
        {
            get { return HtmlTag != null && HtmlTag.Name == HtmlConstants.A && HtmlTag.HasAttribute("href"); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance or one of its parents has Position = fixed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fixed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsFixed
        {
            get
            {
                if (Position == CssConstants.Fixed)
                    return true;

                if (this.ParentBox == null)
                    return false;

                CssBox parent = this;

                while (!(parent.ParentBox == null || parent == parent.ParentBox))
                {
                    parent = parent.ParentBox;

                    if (parent.Position == CssConstants.Fixed)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Get the href link of the box (by default get "href" attribute)
        /// </summary>
        public virtual string HrefLink
        {
            get { return GetAttribute(HtmlConstants.Href); }
        }

        /// <summary>
        /// Gets the containing block-box of this box. (The nearest parent box with display=block)
        /// </summary>
        public CssBox ContainingBlock
        {
            get
            {
                if (ParentBox == null)
                {
                    return this; //This is the initial containing block.
                }

                var box = ParentBox;
                while (!box.IsBlock &&
                       box.Display != CssConstants.ListItem &&
                       box.Display != CssConstants.Table &&
                       box.Display != CssConstants.TableCell &&
                       box.ParentBox != null)
                {
                    box = box.ParentBox;
                }

                //Comment this following line to treat always superior box as block
                if (box == null)
                    throw new Exception("There's no containing block on the chain");

                return box;
            }
        }

        /// <summary>
        /// Gets the HTMLTag that hosts this box
        /// </summary>
        public HtmlTag HtmlTag
        {
            get { return _htmltag; }
        }

        /// <summary>
        /// Gets if this box represents an image
        /// </summary>
        public bool IsImage
        {
            get { return Words.Count == 1 && Words[0].IsImage; }
        }

        /// <summary>
        /// Tells if the box is empty or contains just blank spaces
        /// </summary>
        public bool IsSpaceOrEmpty
        {
            get
            {
                if ((Words.Count != 0 || Boxes.Count != 0) && (Words.Count != 1 || !Words[0].IsSpaces))
                {
                    foreach (CssRect word in Words)
                    {
                        if (!word.IsSpaces)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the inner text of the box
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _boxWords.Clear();
            }
        }

        /// <summary>
        /// Gets the line-boxes of this box (if block box)
        /// </summary>
        internal List<CssLineBox> LineBoxes
        {
            get { return _lineBoxes; }
        }

        /// <summary>
        /// This box's actual rendered top, for page-index comparisons against an already-laid-out box -
        /// <see cref="CssBoxProperties.Location"/>'s Y for a block container, but the first line's actual
        /// top for an inline-only box. <c>Location</c> is committed once, before content layout runs, and
        /// <see cref="Fragmentation.InlineFragmentation.ApplyLineBreaking"/> never updates it even though
        /// it can move the box's one-and-only line (or first of several) to an entirely different page -
        /// a single-line paragraph pushed whole onto the next page by orphans/widows is the case that
        /// actually surfaces this: <c>Location.Y</c> stays wherever the box was originally positioned,
        /// silently wrong for any caller using it to ask "which page does this box's content start on."
        /// </summary>
        internal double EffectiveTop => _lineBoxes.Count > 0 ? _lineBoxes[0].LineTop : Location.Y;

        /// <summary>
        /// Gets the linebox(es) that contains words of this box (if inline)
        /// </summary>
        internal List<CssLineBox> ParentLineBoxes
        {
            get { return _parentLineBoxes; }
        }

        /// <summary>
        /// Gets the rectangles where this box should be painted
        /// </summary>
        internal Dictionary<CssLineBox, RRect> Rectangles
        {
            get { return _rectangles; }
        }

        /// <summary>
        /// Gets the BoxWords of text in the box
        /// </summary>
        internal List<CssRect> Words
        {
            get { return _boxWords; }
        }

        /// <summary>
        /// Gets the first word of the box
        /// </summary>
        internal CssRect FirstWord
        {
            get { return Words[0]; }
        }

        /// <summary>
        /// Gets or sets the first linebox where content of this box appear
        /// </summary>
        internal CssLineBox FirstHostingLineBox
        {
            get { return _firstHostingLineBox; }
            set { _firstHostingLineBox = value; }
        }

        /// <summary>
        /// Gets or sets the last linebox where content of this box appear
        /// </summary>
        internal CssLineBox LastHostingLineBox
        {
            get { return _lastHostingLineBox; }
            set { _lastHostingLineBox = value; }
        }

        /// <summary>
        /// Create new css box for the given parent with the given html tag.<br/>
        /// </summary>
        /// <param name="tag">the html tag to define the box</param>
        /// <param name="parent">the box to add the new box to it as child</param>
        /// <returns>the new box</returns>
        public static CssBox CreateBox(HtmlTag tag, CssBox parent = null)
        {
            ArgChecker.AssertArgNotNull(tag, "tag");

            if (tag.Name == HtmlConstants.Img)
            {
                return new CssBoxImage(parent, tag);
            }
            else if (tag.Name == HtmlConstants.Iframe)
            {
                return new CssBoxFrame(parent, tag);
            }
            else if (tag.Name == HtmlConstants.Hr)
            {
                return new CssBoxHr(parent, tag);
            }
            else
            {
                return new CssBox(parent, tag);
            }
        }

        /// <summary>
        /// Create new css box for the given parent with the given optional html tag and insert it either
        /// at the end or before the given optional box.<br/>
        /// If no html tag is given the box will be anonymous.<br/>
        /// If no before box is given the new box will be added at the end of parent boxes collection.<br/>
        /// If before box doesn't exists in parent box exception is thrown.<br/>
        /// </summary>
        /// <remarks>
        /// To learn more about anonymous inline boxes visit: http://www.w3.org/TR/CSS21/visuren.html#anonymous
        /// </remarks>
        /// <param name="parent">the box to add the new box to it as child</param>
        /// <param name="tag">optional: the html tag to define the box</param>
        /// <param name="before">optional: to insert as specific location in parent box</param>
        /// <returns>the new box</returns>
        public static CssBox CreateBox(CssBox parent, HtmlTag tag = null, CssBox before = null)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            var newBox = new CssBox(parent, tag);
            newBox.InheritStyle();
            if (before != null)
            {
                newBox.SetBeforeBox(before);
            }
            return newBox;
        }

        /// <summary>
        /// Create new css block box.
        /// </summary>
        /// <returns>the new block box</returns>
        public static CssBox CreateBlock()
        {
            var box = new CssBox(null, null);
            box.Display = CssConstants.Block;
            return box;
        }

        /// <summary>
        /// Create new css block box for the given parent with the given optional html tag and insert it either
        /// at the end or before the given optional box.<br/>
        /// If no html tag is given the box will be anonymous.<br/>
        /// If no before box is given the new box will be added at the end of parent boxes collection.<br/>
        /// If before box doesn't exists in parent box exception is thrown.<br/>
        /// </summary>
        /// <remarks>
        /// To learn more about anonymous block boxes visit CSS spec:
        /// http://www.w3.org/TR/CSS21/visuren.html#anonymous-block-level
        /// </remarks>
        /// <param name="parent">the box to add the new block box to it as child</param>
        /// <param name="tag">optional: the html tag to define the box</param>
        /// <param name="before">optional: to insert as specific location in parent box</param>
        /// <returns>the new block box</returns>
        public static CssBox CreateBlock(CssBox parent, HtmlTag tag = null, CssBox before = null)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            var newBox = CreateBox(parent, tag, before);
            newBox.Display = CssConstants.Block;
            return newBox;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.
        /// </summary>
        /// <param name="g">Device context to use</param>
        public void PerformLayout(RGraphics g)
        {
            try
            {
                PerformLayoutImp(g);
            }
            catch (Exception ex)
            {
                HtmlContainer.ReportError(HtmlRenderErrorType.Layout, "Exception in box layout", ex);
            }
        }

        /// <summary>
        /// Seeds this box's resumption state for the upcoming <see cref="PerformLayout"/> call - called
        /// by a parent's child loop right before re-entering a box on a break token's resume path (or by
        /// <see cref="HtmlContainerInt"/> on the document root at the start of every pass). Both parameters
        /// default to null/absent for a box being entered fresh this pass.
        /// </summary>
        /// <param name="token">
        /// how this box should resume its own child/content loop - <see cref="_incomingToken"/>. Null both
        /// for a genuinely fresh box and for a box being placed for the first time via
        /// <paramref name="resumeTopOverride"/> (nothing to resume into, since it was never entered before).
        /// </param>
        /// <param name="resumeTopOverride">
        /// a pre-decided top this box must place itself at, bypassing its own natural-position derivation
        /// - <see cref="_resumeTopOverride"/>.
        /// </param>
        internal void ResumeAt(BreakToken token, double? resumeTopOverride = null)
        {
            _incomingToken = token;
            _resumeTopOverride = resumeTopOverride;
        }

        /// <summary>
        /// Whether a forced break here could actually be deferred to (and resumed in) a later pass -
        /// false anywhere inside a table cell's subtree. <see cref="CssLayoutEngineTable"/>'s row loop
        /// calls <c>cell.PerformLayout</c> directly, the same way it always has, and does not participate
        /// in the <see cref="PendingBreakToken"/> bubbling protocol an ordinary block-child loop does (see
        /// that property's doc comment) - a table row is not itself laid out via that loop, so nothing
        /// would ever read a cell's own <see cref="PendingBreakToken"/> and turn it into a real pass
        /// boundary. Deferring anyway would leave the deferred content measured but never positioned
        /// (its <see cref="PerformLayoutImp"/> call returns before reaching <c>CreateLineBoxes</c>/the
        /// block-child loop, yet nothing ever resumes it) - found as a real regression while
        /// investigating table fragmentation, once R1's forced-break deferral existed to trigger it.
        /// </summary>
        private bool CanDeferToLaterPass()
        {
            for (var box = this; box != null; box = box.ParentBox)
            {
                if (box.Display == CssConstants.TableCell)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Set this box in
        /// </summary>
        /// <param name="before"></param>
        public void SetBeforeBox(CssBox before)
        {
            int index = _parentBox.Boxes.IndexOf(before);
            if (index < 0)
                throw new Exception("before box doesn't exist on parent");

            _parentBox.Boxes.Remove(this);
            _parentBox.Boxes.Insert(index, this);
        }

        /// <summary>
        /// Move all child boxes from <paramref name="fromBox"/> to this box.
        /// </summary>
        /// <param name="fromBox">the box to move all its child boxes from</param>
        public void SetAllBoxes(CssBox fromBox)
        {
            foreach (var childBox in fromBox._boxes)
                childBox._parentBox = this;

            _boxes.AddRange(fromBox._boxes);
            fromBox._boxes.Clear();
        }

        /// <summary>
        /// Splits the text into words and saves the result
        /// </summary>
        public void ParseToWords()
        {
            _boxWords.Clear();

            int startIdx = 0;
            bool preserveSpaces = WhiteSpace == CssConstants.Pre || WhiteSpace == CssConstants.PreWrap;
            bool respoctNewline = preserveSpaces || WhiteSpace == CssConstants.PreLine || IsBrElement;
            while (startIdx < _text.Length)
            {
                while (startIdx < _text.Length && _text[startIdx] == '\r')
                    startIdx++;

                if (startIdx < _text.Length)
                {
                    var endIdx = startIdx;
                    while (endIdx < _text.Length && char.IsWhiteSpace(_text[endIdx]) && _text[endIdx] != '\n')
                        endIdx++;

                    if (endIdx > startIdx)
                    {
                        if (preserveSpaces)
                            _boxWords.Add(new CssRectWord(this, HtmlUtils.DecodeHtml(_text.Substring(startIdx, endIdx - startIdx)), false, false));
                    }
                    else
                    {
                        endIdx = startIdx;
                        while (endIdx < _text.Length && !char.IsWhiteSpace(_text[endIdx]) && _text[endIdx] != '-' && WordBreak != CssConstants.BreakAll && !CommonUtils.IsAsianCharecter(_text[endIdx]))
                            endIdx++;

                        if (endIdx < _text.Length && (_text[endIdx] == '-' || WordBreak == CssConstants.BreakAll || CommonUtils.IsAsianCharecter(_text[endIdx])))
                            endIdx++;

                        if (endIdx > startIdx)
                        {
                            var hasSpaceBefore = !preserveSpaces && (startIdx > 0 && _boxWords.Count == 0 && char.IsWhiteSpace(_text[startIdx - 1]));
                            var hasSpaceAfter = !preserveSpaces && (endIdx < _text.Length && char.IsWhiteSpace(_text[endIdx]));
                            _boxWords.Add(new CssRectWord(this, HtmlUtils.DecodeHtml(_text.Substring(startIdx, endIdx - startIdx)), hasSpaceBefore, hasSpaceAfter));
                        }
                    }

                    // create new-line word so it will effect the layout
                    if (endIdx < _text.Length && _text[endIdx] == '\n')
                    {
                        endIdx++;
                        if (respoctNewline)
                            _boxWords.Add(new CssRectWord(this, "\n", false, false));
                    }

                    startIdx = endIdx;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (_imageLoadHandler != null)
                _imageLoadHandler.Dispose();

            foreach (var childBox in Boxes)
            {
                childBox.Dispose();
            }
        }


        #region Private Methods

        /// <summary>
        /// Computes this box's used height, ported from PeachPDF's CssLayoutEngine.GetBoxHeight -
        /// scoped to the height features this engine supports (no aspect-ratio, no absolutely-
        /// positioned top+bottom fill, no per-page height: PeachPDF features this engine never had).
        /// PeachPDF's Size.Height is content-only, so its GetBoxHeight starts from
        /// ActualBoxSizingHeight (Size.Height + padding/border) as the auto-height baseline and can
        /// return null for an indeterminate percentage height, leaving ApplyHeight's Math.Max to
        /// preserve whatever content-driven value is already there. This engine's Size.Height is
        /// already the border-box measure (see <see cref="ActualRight"/>), so the equivalent baseline
        /// is Size.Height itself, and there is no null case to thread through - an indeterminate
        /// percentage height simply falls back to that same baseline directly.
        /// </summary>
        private double GetBoxHeight()
        {
            double height = Size.Height;

            if (CssValueParser.IsValidLength(Height) && !(Height.EndsWith("%") && !ContainingBlock.IsHeightCalculated))
            {
                height = CssValueParser.ParseLength(Height, ContainingBlock.Size.Height, this) + ActualBoxSizeIncludedHeight;
            }

            if (CssValueParser.IsValidLength(MinHeight) && (ContainingBlock.IsHeightCalculated || !MinHeight.EndsWith("%")))
            {
                var minHeight = CssValueParser.ParseLength(MinHeight, ContainingBlock.Size.Height, this) + ActualBoxSizeIncludedHeight;
                if (minHeight > height)
                {
                    height = minHeight;
                }
            }

            return height;
        }

        /// <summary>
        /// Applies this box's used height (CSS 2.1 10.5/10.6.3/10.7) to <see cref="CssBoxProperties.ActualBottom"/>,
        /// and computes whether this box counts as height-calculated for a descendant's percentage
        /// height resolution. Ported from PeachPDF's CssLayoutEngine.ApplyHeight.
        /// <para>
        /// The assignment below is unconditional (not the plain <c>Math.Max(ActualBottom, ...)</c>
        /// this replaced) - a no-op for auto height, since <see cref="GetBoxHeight"/>'s baseline is
        /// already ActualBottom's own current value, but a real correction for a definite height,
        /// able to shrink ActualBottom back down, which Math.Max alone never could. That correction is
        /// what fixes a real bug this exposed: <see cref="CssLayoutEngine.FlowBox"/>'s own "handle
        /// height setting" step (for a nested atomic inline-block) pushes a block's own maxBottom out
        /// to the block's full ActualHeight measured from its content top, and CreateLineBoxes then
        /// adds that same block's bottom padding/border again on top - inflating ActualBottom by one
        /// extra padding+border box for any floated or block-level box with both an explicit height
        /// and non-zero padding/border (e.g. every floated box in the W3C ACID1 test). A definite
        /// height here authoritatively overwrites that inflated value instead of merely preserving it.
        /// </para>
        /// </summary>
        private void ApplyHeight()
        {
            var height = GetBoxHeight();
            ActualBottom = Location.Y + height;

            var isRootWithPageHeight = ParentBox == null && HtmlContainer != null;
            var isDefiniteHeight = CssValueParser.IsValidLength(Height) && (ContainingBlock.IsHeightCalculated || !Height.EndsWith("%"));
            IsHeightCalculated = isRootWithPageHeight || isDefiniteHeight;

            if (CssValueParser.IsValidLength(MaxHeight) && MaxHeight != CssConstants.None
                && (ContainingBlock.IsHeightCalculated || !MaxHeight.EndsWith("%")))
            {
                var maxHeight = CssValueParser.ParseLength(MaxHeight, ContainingBlock.Size.Height, this) + ActualBoxSizeIncludedHeight;
                var maxBottom = Location.Y + maxHeight;

                if (ActualBottom > maxBottom)
                {
                    ActualBottom = maxBottom;

                    if (CssValueParser.IsValidLength(MinHeight) && (ContainingBlock.IsHeightCalculated || !MinHeight.EndsWith("%")))
                    {
                        var minHeight = CssValueParser.ParseLength(MinHeight, ContainingBlock.Size.Height, this) + ActualBoxSizeIncludedHeight;
                        var minBottom = Location.Y + minHeight;

                        if (ActualBottom < minBottom)
                        {
                            ActualBottom = minBottom;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.<br/>
        /// </summary>
        /// <param name="g">Device context to use</param>
        protected virtual void PerformLayoutImp(RGraphics g)
        {
            // Pass-scoped signal state - stale values from an earlier pass must never leak into this one.
            PendingBreakToken = null;
            RequestedBreakBeforeTop = null;
            RequestedBreakBeforeSlot = 0;

            if (Display != CssConstants.None)
            {
                RectanglesReset();
                MeasureWordsSize(g);
            }

            if (IsBlock || Display == CssConstants.ListItem || Display == CssConstants.Table || Display == CssConstants.InlineTable || Display == CssConstants.TableCell)
            {
                // Because their width and height are set by CssTable
                if (Display != CssConstants.TableCell && Display != CssConstants.Table)
                {
                    double availableWidth = ContainingBlock.Size.Width
                                   - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight
                                   - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth;
                    double width = availableWidth;
                    bool explicitWidth = Width != CssConstants.Auto && !string.IsNullOrEmpty(Width);

                    if (explicitWidth)
                    {
                        // CSS 2.1 10.2: a percentage/length `width` resolves against the containing block's
                        // content width. Per CSS Box Sizing 3, that resolved value is this box's CONTENT
                        // width under the default `box-sizing: content-box` - its border-box width (what
                        // Size.Width holds throughout this engine) additionally includes this box's own
                        // padding+border (ActualBoxSizeIncludedWidth is 0 for `box-sizing: border-box`,
                        // where the resolved value already IS the border-box width).
                        width = CssValueParser.ParseLength(Width, availableWidth, this) + ActualBoxSizeIncludedWidth;
                    }
                    else if (IsFloated || Position == CssConstants.Absolute)
                    {
                        // CSS 2.1 10.3.5/10.3.7: a floated box, or an absolutely positioned box with no
                        // explicit width (the common case - both `left`/`right` auto), shrinks to fit its
                        // content instead of taking the full containing-block width like an ordinary block.
                        // (The full §10.3.7 seven-case width-auto-resolution algorithm - solving width from
                        // explicit left+right+margins - is not implemented; this covers the shrink-to-fit
                        // case PeachPDF's own Acid2 regression tests exercise.) GetMinMaxWidth already
                        // returns border-box-inclusive bounds (its own padding/border baked in), so no
                        // box-sizing adjustment is needed here.
                        double minWidth, maxWidth;
                        GetMinMaxWidth(out minWidth, out maxWidth);
                        width = Math.Min(Math.Max(minWidth, width), maxWidth);
                    }

                    Size = new RSize(width, Size.Height);

                    if (!explicitWidth && !IsFloated)
                    {
                        // CSS 2.1 10.3.3: only a plain (non-floated) width:auto block absorbs its own
                        // margin into the available fill width to become its used (border-box) width - an
                        // explicit width, or a floated shrink-to-fit width, is the used width as-is; margin
                        // is separate space outside the border box (already accounted for via this box's
                        // Location.X), not a further reduction of Size.Width on top of that.
                        Size = new RSize(width - ActualMarginLeft - ActualMarginRight, Size.Height);
                    }
                }

                if (Display != CssConstants.TableCell)
                {
                    var prevSibling = DomUtils.GetPreviousSibling(this);
                    double left;
                    double top;

                    if (Position == CssConstants.Fixed)
                    {
                        // Computed here (not eagerly from the Left/Top property setters, which used to
                        // race ahead of ActualMarginLeft/Top and ContainingBlock/HtmlContainer being ready
                        // and cache a margin-less Location that never got recomputed) so margin is always
                        // resolved against a fully-set-up box, matching every other positioning scheme.
                        var fixedLocation = GetActualLocation(Left, Top);
                        left = fixedLocation.X;
                        top = fixedLocation.Y;
                        Location = fixedLocation;
                        ActualBottom = top;
                    }
                    else
                    {
                        left = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth;
                        // StaticBottom (not ActualBottom): a relatively-positioned previous sibling's visual
                        // offset must not drag this box down with it (CSS 2.1 9.4.3 - relative positioning
                        // "has no effect on the position of any other box"). Ported from PeachPDF.
                        var baseTopWithoutMargin = (prevSibling == null && ParentBox != null ? ParentBox.ClientTop : ParentBox == null ? Location.Y : 0) + (prevSibling != null ? prevSibling.StaticBottom + prevSibling.ActualBorderBottomWidth : 0);

                        if (_incomingToken != null && ReferenceEquals(_incomingToken.Box, this))
                        {
                            // Resuming this box's own interrupted child/content loop, not placing it fresh
                            // - css-break-3 §2 gives a box one inline position across all its fragments, so
                            // there is nothing to re-derive here; Location already holds it from the pass
                            // that placed this box originally.
                            top = Location.Y;
                        }
                        else if (_resumeTopOverride.HasValue)
                        {
                            // A break-before target an earlier pass already decided (see
                            // RequestedBreakBeforeTop's doc comment) - must not be re-derived.
                            top = _resumeTopOverride.Value;
                        }
                        else if (BlockFragmentation.TryGetForcedBreakTarget(this, prevSibling, baseTopWithoutMargin, out var breakSlot, out var breakTop))
                        {
                            // css-break-3 5.2 preserves a box's own top margin at a FORCED break (unlike an
                            // unforced one, where BlockFragmentation.ResolveBlockTop truncates it to avoid
                            // paginating through blank space) - breakTop itself is the page's own content
                            // top (TryGetForcedBreakTarget's own contract, kept a pure boundary value so its
                            // slot/target stay meaningful on their own), so the margin is added here, once,
                            // at the point it becomes this box's actual placement.
                            var breakTopWithMargin = breakTop + MarginTopCollapse(prevSibling);

                            if (CanDeferToLaterPass())
                            {
                                // A forced break-before/after applies and this is a genuinely fresh entry
                                // (no resume state of any kind) - defer this box (and everything after it
                                // in its parent's child loop) to a later pass entirely, rather than
                                // positioning it now.
                                RequestedBreakBeforeSlot = breakSlot;
                                RequestedBreakBeforeTop = breakTopWithMargin;
                                return;
                            }

                            // Deferring would never actually be resumed here (see CanDeferToLaterPass) -
                            // place immediately at the target instead, matching how forced breaks worked
                            // before real pass-based deferral existed. Not ideal (this content doesn't
                            // get a fresh fragmentainer pass the way top-level content does), but correct
                            // rather than silently measured-but-never-positioned.
                            top = breakTopWithMargin;
                        }
                        else
                        {
                            top = BlockFragmentation.ResolveBlockTop(this, prevSibling, baseTopWithoutMargin);
                        }

                        Location = new RPoint(left, top);
                        ActualBottom = top;

                        // static position committed above; float/clear now overwrite Location using that
                        // static position as input. No-op for boxes that are neither floated nor clearing.
                        CssLayoutEngine.FloatBox(this);

                        // CSS 2.1 §9.4.3/§10.3.7: position:relative/absolute apply on top of the static
                        // position just committed above. Ported from PeachPDF's CssBox.CommitBlockChildOffset
                        // (adapted: this fork places a box within its own PerformLayoutImp rather than a
                        // parent placing its child, and position:fixed's own offset - which never runs
                        // through this static-flow branch at all, see the Position==Fixed arm above - is
                        // instead resolved by CssBoxProperties.GetActualLocation).
                        if (Position == CssConstants.Relative)
                        {
                            // Purely visual (§9.4.3): the offset is recorded separately (RelativeOffsetX/Y)
                            // so StaticBottom can back it out again for margin-collapse/sibling-placement
                            // consumers - "the effect of relative positioning on ... the box's parent's or
                            // following siblings' layout is nil".
                            var offsetX = ResolveNearFarOffset(this, Left, Right, ActualWidth);
                            var offsetY = ResolveNearFarOffset(this, Top, Bottom, ActualHeight);

                            RelativeOffsetX = offsetX;
                            RelativeOffsetY = offsetY;
                            Location = new RPoint(Location.X + offsetX, Location.Y + offsetY);
                            ActualBottom = Location.Y;
                        }
                        else if (Position == CssConstants.Absolute)
                        {
                            var nearestPositionedAncestor = DomUtils.GetNearestPositionedAncestor(this);
                            var leftIsAuto = string.IsNullOrEmpty(Left) || Left == CssConstants.Auto;
                            var rightIsAuto = string.IsNullOrEmpty(Right) || Right == CssConstants.Auto;

                            // left/top are measured from the containing block's PADDING edge (ClientLeft/
                            // ClientTop), not its border-box edge, and the box's own margin still applies
                            // on top of that offset (CSS 2.1 §10.3.7). When left is auto but right is set,
                            // anchor off the containing block's right edge instead - this box's own
                            // border-box width (Size.Width) is already resolved by this point (the shrink-
                            // to-fit/explicit-width computation above), unlike its height (see the
                            // top/bottom case, resolved later in this method once ApplyHeight has run).
                            var absLeft = !leftIsAuto
                                ? nearestPositionedAncestor.ClientLeft + ActualMarginLeft
                                  + ResolveOffsetOrZero(this, Left, nearestPositionedAncestor.ActualWidth)
                                : !rightIsAuto
                                    ? nearestPositionedAncestor.ClientLeft + nearestPositionedAncestor.ActualWidth
                                      - ActualMarginRight - ResolveOffsetOrZero(this, Right, nearestPositionedAncestor.ActualWidth) - Size.Width
                                    : nearestPositionedAncestor.ClientLeft + ActualMarginLeft;

                            var absTop = nearestPositionedAncestor.ClientTop + ActualMarginTop
                                         + ResolveOffsetOrZero(this, Top, nearestPositionedAncestor.ActualHeight);

                            Location = new RPoint(absLeft, absTop);
                            ActualBottom = Location.Y;
                        }
                    }
                }

                //If we're talking about a table here..
                if (Display == CssConstants.Table || Display == CssConstants.InlineTable)
                {
                    CssLayoutEngineTable.PerformLayout(g, this);
                }
                else
                {
                    //If there's just inline boxes, create LineBoxes
                    if (DomUtils.ContainsInlinesOnly(this))
                    {
                        ActualBottom = Location.Y;
                        CssLayoutEngine.CreateLineBoxes(g, this); //This will automatically set the bottom of this block
                        InlineFragmentation.ApplyLineBreaking(this);
                    }
                    else if (_boxes.Count > 0)
                    {
                        // Resuming our OWN child loop (as opposed to a fresh entry) if the incoming token
                        // names this box - ResumeChildIndex says which child to pick back up at; every
                        // child before it already has a finished fragment from an earlier pass and is
                        // never touched again.
                        var resumeToken = _incomingToken as BlockBreakToken;
                        var resumingHere = resumeToken != null && ReferenceEquals(resumeToken.Box, this);
                        var startIndex = resumingHere ? resumeToken.ResumeChildIndex : 0;

                        for (var i = startIndex; i < Boxes.Count; i++)
                        {
                            var childBox = Boxes[i];

                            if (i == startIndex && resumingHere)
                            {
                                if (resumeToken.IsBreakBefore)
                                    childBox.ResumeAt(null, resumeToken.ResumeTopOverride);
                                else
                                    childBox.ResumeAt(resumeToken.ChildToken);
                            }

                            childBox.PerformLayout(g);

                            if (childBox.RequestedBreakBeforeTop.HasValue)
                            {
                                // Child declined to be placed this pass at all - stop here too, so this
                                // box's own parent bubbles the same fact upward (see PendingBreakToken's
                                // doc comment for how this reaches HtmlContainerInt's pass loop).
                                PendingBreakToken = new BlockBreakToken(
                                    this, childBox.RequestedBreakBeforeSlot, i, null, true, childBox.RequestedBreakBeforeTop);
                                return;
                            }

                            // Checked BEFORE RelocateIfNeeded, not after: a child whose own child loop
                            // stopped mid-way (a nested forced break) never reached its epilogue, so its
                            // ActualBottom/Location only reflect a partial pass - RelocateIfNeeded's
                            // straddle test would read meaningless geometry if run on it.
                            if (BubbleChildPendingToken(childBox, i))
                                return;

                            BlockFragmentation.RelocateIfNeeded(g, childBox);

                            // RelocateIfNeeded's own relayout (see its doc comment) can itself surface a
                            // break nested inside the relocated child's subtree - e.g. a forced break
                            // inside a break-inside:avoid container - so check again.
                            if (BubbleChildPendingToken(childBox, i))
                                return;

                            BlockFragmentation.EnforceKeepWithNext(g, childBox);

                            // Same reasoning as above: EnforceKeepWithNext's own relayout of childBox can
                            // itself surface a nested break.
                            if (BubbleChildPendingToken(childBox, i))
                                return;
                        }
                        ActualRight = CalculateActualRight();

                        // CSS 2.1 10.6.3/10.6.7: a plain (non-BFC-establishing) box's own auto-height
                        // doesn't contribute from out-of-flow (floated/absolutely-positioned) children
                        // at all - if EVERY child is out-of-flow (e.g. a <ul> whose only children are
                        // floated <li>s), there is no in-flow content to measure, so ActualBottom is
                        // left exactly as the static-position commit above already set it (its own
                        // top, i.e. zero content height) rather than being pulled down to match a
                        // floated child's position. Ported from PeachPDF's CssBox.PerformLayoutImp
                        // (`if (Boxes.Any(b => !b.IsOutOfFlow)) { ActualBottom = MarginBottomCollapse(); }`).
                        if (Boxes.Exists(b => !b.IsOutOfFlow))
                        {
                            ActualBottom = MarginBottomCollapse();
                        }
                    }
                }
            }
            else
            {
                var prevSibling = DomUtils.GetPreviousSibling(this);
                if (prevSibling != null)
                {
                    if (Location == RPoint.Empty)
                        Location = prevSibling.Location;
                    ActualBottom = prevSibling.ActualBottom;
                }
            }
            ApplyHeight();

            if (Position == CssConstants.Absolute && Display != CssConstants.TableCell)
            {
                var topIsAuto = string.IsNullOrEmpty(Top) || Top == CssConstants.Auto;
                var bottomIsAuto = string.IsNullOrEmpty(Bottom) || Bottom == CssConstants.Auto;

                if (topIsAuto && !bottomIsAuto)
                {
                    // The top/bottom counterpart of the left/right shrink-to-fit-anchoring case above:
                    // unlike width, this box's own height is only known now, after ApplyHeight has run
                    // (auto height depends on this box's own already-laid-out content) - so the
                    // bottom-anchored case can't resolve at the same point the left/right one does, and
                    // is instead corrected here by shifting the whole subtree (OffsetTop, the same deep-
                    // move helper break relocation/table-header repetition already use) once this box's
                    // final height is known. CSS 2.1 §10.3.7.
                    //
                    // The ancestor's own ClientBottom/ActualBottom is NOT usable here: this box is still
                    // laying out as one of the ancestor's descendants, so the ancestor's own ApplyHeight
                    // (which sets ActualBottom, run only after ALL of its children finish) has not run yet
                    // either. ActualHeight, unlike ActualBottom, resolves directly from the ancestor's own
                    // explicit Height CSS string without depending on that - so the ancestor's content-box
                    // bottom edge is derived from Location.Y + ActualHeight instead.
                    var nearestPositionedAncestor = DomUtils.GetNearestPositionedAncestor(this);
                    var ancestorBorderBoxBottom = nearestPositionedAncestor.Location.Y + nearestPositionedAncestor.ActualHeight;
                    var ancestorClientBottom = ancestorBorderBoxBottom
                                                - nearestPositionedAncestor.ActualPaddingBottom
                                                - nearestPositionedAncestor.ActualBorderBottomWidth;
                    var offsetBottom = ResolveOffsetOrZero(this, Bottom, nearestPositionedAncestor.ActualHeight);
                    var targetBottom = ancestorClientBottom - ActualMarginBottom - offsetBottom;
                    var deltaY = targetBottom - ActualBottom;

                    // ActualBottom is computed (Location.Y + Size.Height, see CssBoxProperties.ActualBottom),
                    // so shifting Location.Y via OffsetTop already moves it by the same delta - no separate
                    // update needed (and adding one double-counts the shift).
                    if (deltaY != 0)
                    {
                        OffsetTop(deltaY);
                    }
                }
            }

            CreateListItemBox(g);

            if (!IsFixed)
            {
                var actualWidth = Math.Max(GetMinimumWidth() + GetWidthMarginDeep(this), Size.Width < 90999 ? ActualRight - HtmlContainer.Root.Location.X : 0);
                HtmlContainer.ActualSize = CommonUtils.Max(HtmlContainer.ActualSize, new RSize(actualWidth, ActualBottom - HtmlContainer.Root.Location.Y));
            }
        }

        /// <summary>
        /// If <paramref name="childBox"/> stopped somewhere inside its own content/child loop this pass,
        /// wraps its token in a link naming this box (at <paramref name="childIndex"/>) and sets it as
        /// this box's own <see cref="PendingBreakToken"/>, for the caller to stop laying out any further
        /// siblings and return. See <see cref="PendingBreakToken"/>'s doc comment for how this bubbling
        /// reaches <see cref="HtmlContainerInt"/>'s pass loop.
        /// </summary>
        private bool BubbleChildPendingToken(CssBox childBox, int childIndex)
        {
            if (childBox.PendingBreakToken == null)
                return false;

            PendingBreakToken = new BlockBreakToken(
                this, childBox.PendingBreakToken.ResumeSlotIndex, childIndex, childBox.PendingBreakToken, false, null);
            return true;
        }

        /// <summary>
        /// Assigns words its width and height
        /// </summary>
        /// <param name="g"></param>
        internal virtual void MeasureWordsSize(RGraphics g)
        {
            if (!_wordsSizeMeasured)
            {
                if (ActualBackgroundImage is CssImage.Url urlImage && _imageLoadHandler == null)
                {
                    _imageLoadHandler = new ImageLoadHandler(HtmlContainer, OnImageLoadComplete);
                    _imageLoadHandler.LoadImage(urlImage.Href, HtmlTag != null ? HtmlTag.Attributes : null);
                }

                MeasureWordSpacing(g);

                if (Words.Count > 0)
                {
                    foreach (var boxWord in Words)
                    {
                        boxWord.Width = boxWord.Text != "\n" ? g.MeasureString(boxWord.Text, ActualFont).Width : 0;
                        boxWord.Height = ActualFont.Height;
                    }
                }

                _wordsSizeMeasured = true;
            }
        }

        /// <summary>
        /// Get the parent of this css properties instance.
        /// </summary>
        /// <returns></returns>
        protected override sealed CssBoxProperties GetParent()
        {
            return _parentBox;
        }

        /// <summary>
        /// Gets the index of the box to be used on a (ordered) list
        /// </summary>
        /// <returns></returns>
        private int GetIndexForList()
        {
            bool reversed = !string.IsNullOrEmpty(ParentBox.GetAttribute("reversed"));
            int index;
            if (!int.TryParse(ParentBox.GetAttribute("start"), out index))
            {
                if (reversed)
                {
                    index = 0;
                    foreach (CssBox b in ParentBox.Boxes)
                    {
                        if (b.Display == CssConstants.ListItem)
                            index++;
                    }
                }
                else
                {
                    index = 1;
                }
            }

            foreach (CssBox b in ParentBox.Boxes)
            {
                // An explicit <li value="N"> (WHATWG HTML presentational hint) sets the running count to
                // N for that item; later items (without their own explicit value) continue incrementing
                // from there, matching every browser's actual behavior.
                int explicitValue;
                if (b.Display == CssConstants.ListItem && int.TryParse(b.GetAttribute("value"), out explicitValue))
                {
                    index = explicitValue;
                }

                if (b.Equals(this))
                    return index;

                if (b.Display == CssConstants.ListItem)
                    index += reversed ? -1 : 1;
            }

            return index;
        }

        /// <summary>
        /// Creates the <see cref="_listItemBox"/>
        /// </summary>
        /// <param name="g"></param>
        private void CreateListItemBox(RGraphics g)
        {
            if (Display == CssConstants.ListItem && ListStyleType != CssConstants.None)
            {
                if (_listItemBox == null)
                {
                    _listItemBox = new CssBox(null, null);
                    _listItemBox.InheritStyle(this);
                    _listItemBox.Display = CssConstants.Inline;
                    _listItemBox.HtmlContainer = HtmlContainer;

                    if (ListStyleType.Equals(CssConstants.Disc, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _listItemBox.Text = "•";
                    }
                    else if (ListStyleType.Equals(CssConstants.Circle, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _listItemBox.Text = "o";
                    }
                    else if (ListStyleType.Equals(CssConstants.Square, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Was "♠" (U+2660 BLACK SPADE SUIT) - not what CSS2.1 §12.5.1's list-style-type:
                        // square means at all (a filled square marker, the ♣/♦/♠ card-suit glyph was
                        // presumably a typo/copy-paste of a nearby symbol).
                        _listItemBox.Text = "▪"; // U+25AA BLACK SMALL SQUARE
                    }
                    else if (ListStyleType.Equals(CssConstants.Decimal, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _listItemBox.Text = GetIndexForList().ToString(CultureInfo.InvariantCulture) + ".";
                    }
                    else if (ListStyleType.Equals(CssConstants.DecimalLeadingZero, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _listItemBox.Text = GetIndexForList().ToString("00", CultureInfo.InvariantCulture) + ".";
                    }
                    else
                    {
                        _listItemBox.Text = CommonUtils.ConvertToAlphaNumber(GetIndexForList(), ListStyleType) + ".";
                    }

                    _listItemBox.ParseToWords();

                    _listItemBox.PerformLayoutImp(g);
                    _listItemBox.Size = new RSize(_listItemBox.Words[0].Width, _listItemBox.Words[0].Height);
                }
                _listItemBox.Words[0].Left = Location.X - _listItemBox.Size.Width - 5;
                _listItemBox.Words[0].Top = Location.Y + ActualPaddingTop; // +FontAscent;
            }
        }

        /// <summary>
        /// Searches for the first word occurrence inside the box, on the specified linebox
        /// </summary>
        /// <param name="b"></param>
        /// <param name="line"> </param>
        /// <returns></returns>
        internal CssRect FirstWordOccourence(CssBox b, CssLineBox line)
        {
            if (b.Words.Count == 0 && b.Boxes.Count == 0)
            {
                return null;
            }

            if (b.Words.Count > 0)
            {
                foreach (CssRect word in b.Words)
                {
                    if (line.Words.Contains(word))
                    {
                        return word;
                    }
                }
                return null;
            }
            else
            {
                foreach (CssBox bb in b.Boxes)
                {
                    CssRect w = FirstWordOccourence(bb, line);

                    if (w != null)
                    {
                        return w;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the specified Attribute, returns string.Empty if no attribute specified
        /// </summary>
        /// <param name="attribute">Attribute to retrieve</param>
        /// <returns>Attribute value or string.Empty if no attribute specified</returns>
        internal string GetAttribute(string attribute)
        {
            return GetAttribute(attribute, string.Empty);
        }

        /// <summary>
        /// Gets the value of the specified attribute of the source HTML tag.
        /// </summary>
        /// <param name="attribute">Attribute to retrieve</param>
        /// <param name="defaultValue">Value to return if attribute is not specified</param>
        /// <returns>Attribute value or defaultValue if no attribute specified</returns>
        internal string GetAttribute(string attribute, string defaultValue)
        {
            return HtmlTag != null ? HtmlTag.TryGetAttribute(attribute, defaultValue) : defaultValue;
        }

        /// <summary>
        /// Gets the minimum width that the box can be.<br/>
        /// The box can be as thin as the longest word plus padding.<br/>
        /// The check is deep thru box tree.<br/>
        /// </summary>
        /// <returns>the min width of the box</returns>
        internal double GetMinimumWidth()
        {
            double maxWidth = 0;
            CssRect maxWidthWord = null;
            GetMinimumWidth_LongestWord(this, ref maxWidth, ref maxWidthWord);

            double padding = 0f;
            if (maxWidthWord != null)
            {
                var box = maxWidthWord.OwnerBox;
                while (box != null)
                {
                    padding += box.ActualBorderRightWidth + box.ActualPaddingRight + box.ActualBorderLeftWidth + box.ActualPaddingLeft;
                    box = box != this ? box.ParentBox : null;
                }
            }

            return maxWidth + padding;
        }

        /// <summary>
        /// Gets the longest word (in width) inside the box, deeply.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="maxWidth"> </param>
        /// <param name="maxWidthWord"> </param>
        /// <returns></returns>
        private static void GetMinimumWidth_LongestWord(CssBox box, ref double maxWidth, ref CssRect maxWidthWord)
        {
            if (box.Words.Count > 0)
            {
                foreach (CssRect cssRect in box.Words)
                {
                    if (cssRect.Width > maxWidth)
                    {
                        maxWidth = cssRect.Width;
                        maxWidthWord = cssRect;
                    }
                }
            }
            else
            {
                foreach (CssBox childBox in box.Boxes)
                    GetMinimumWidth_LongestWord(childBox, ref maxWidth, ref maxWidthWord);
            }
        }

        /// <summary>
        /// Get the total margin value (left and right) from the given box to the given end box.<br/>
        /// </summary>
        /// <param name="box">the box to start calculation from.</param>
        /// <returns>the total margin</returns>
        private static double GetWidthMarginDeep(CssBox box)
        {
            double sum = 0f;
            if (box.Size.Width > 90999 || (box.ParentBox != null && box.ParentBox.Size.Width > 90999))
            {
                while (box != null)
                {
                    sum += box.ActualMarginLeft + box.ActualMarginRight;
                    box = box.ParentBox;
                }
            }
            return sum;
        }

        /// <summary>
        /// Gets the maximum bottom of the boxes inside the startBox
        /// </summary>
        /// <param name="startBox"></param>
        /// <param name="currentMaxBottom"></param>
        /// <returns></returns>
        internal double GetMaximumBottom(CssBox startBox, double currentMaxBottom)
        {
            foreach (var line in startBox.Rectangles.Keys)
            {
                currentMaxBottom = Math.Max(currentMaxBottom, startBox.Rectangles[line].Bottom);
            }

            foreach (var b in startBox.Boxes)
            {
                currentMaxBottom = Math.Max(currentMaxBottom, GetMaximumBottom(b, currentMaxBottom));
            }

            return currentMaxBottom;
        }

        /// <summary>
        /// Get the <paramref name="minWidth"/> and <paramref name="maxWidth"/> width of the box content.<br/>
        /// </summary>
        /// <param name="minWidth">The minimum width the content must be so it won't overflow (largest word + padding).</param>
        /// <param name="maxWidth">The total width the content can take without line wrapping (with padding).</param>
        internal void GetMinMaxWidth(out double minWidth, out double maxWidth)
        {
            double min = 0f;
            double maxSum = 0f;
            double paddingSum = 0f;
            double marginSum = 0f;
            GetMinMaxSumWords(this, ref min, ref maxSum, ref paddingSum, ref marginSum);

            maxWidth = paddingSum + maxSum;
            minWidth = paddingSum + (min < 90999 ? min : 0);
        }

        /// <summary>
        /// Get the <paramref name="min"/> and <paramref name="maxSum"/> of the box words content and <paramref name="paddingSum"/>.<br/>
        /// </summary>
        /// <param name="box">the box to calculate for</param>
        /// <param name="min">the width that allows for each word to fit (width of the longest word)</param>
        /// <param name="maxSum">the max width a single line of words can take without wrapping</param>
        /// <param name="paddingSum">the total amount of padding the content has </param>
        /// <param name="marginSum"></param>
        /// <returns></returns>
        private static void GetMinMaxSumWords(CssBox box, ref double min, ref double maxSum, ref double paddingSum, ref double marginSum)
        {
            double? oldSum = null;

            // paddingSum must be scoped per "line" the same way maxSum is (see the oldSum save/restore
            // below) - it represents the border/padding belonging to the WIDEST line found so far, not a
            // running total across every sibling's own unrelated line. Without oldPaddingSum, a block
            // box's own border/padding (and every descendant's, recursively) permanently accumulated into
            // paddingSum and was never reset between siblings - e.g. a content-bearing box followed by
            // border-only siblings summed all their unrelated border/padding into one shrink-to-fit width
            // instead of using only the widest line's own padding. Ported from PeachPDF's GetMinMaxSumWords.
            double? oldPaddingSum = null;

            // not inline (block) boxes start a new line so we need to reset the max sum
            if (box.Display != CssConstants.Inline && box.Display != CssConstants.TableCell && box.WhiteSpace != CssConstants.NoWrap)
            {
                oldSum = maxSum;
                maxSum = marginSum;
                oldPaddingSum = paddingSum;
                paddingSum = 0;
            }

            // add the padding 
            paddingSum += box.ActualBorderLeftWidth + box.ActualBorderRightWidth + box.ActualPaddingRight + box.ActualPaddingLeft;


            // for tables the padding also contains the spacing between cells
            if (box.Display == CssConstants.Table)
                paddingSum += CssLayoutEngineTable.GetTableSpacing(box);

            if (box.Words.Count > 0)
            {
                // calculate the min and max sum for all the words in the box
                foreach (CssRect word in box.Words)
                {
                    maxSum += word.FullWidth + (word.HasSpaceBefore ? word.OwnerBox.ActualWordSpacing : 0);
                    min = Math.Max(min, word.Width);
                }

                // remove the last word padding
                if (box.Words.Count > 0 && !box.Words[box.Words.Count - 1].HasSpaceAfter)
                    maxSum -= box.Words[box.Words.Count - 1].ActualWordSpacing;
            }
            else
            {
                // recursively on all the child boxes
                for (int i = 0; i < box.Boxes.Count; i++)
                {
                    CssBox childBox = box.Boxes[i];
                    marginSum += childBox.ActualMarginLeft + childBox.ActualMarginRight;

                    //maxSum += childBox.ActualMarginLeft + childBox.ActualMarginRight;
                    var maxSumBeforeChild = maxSum;
                    GetMinMaxSumWords(childBox, ref min, ref maxSum, ref paddingSum, ref marginSum);

                    // This walk otherwise never consults a box's own explicit CSS `width` at all - only
                    // literal word/text content. That's usually fine (explicit width constrains layout
                    // AFTER content is measured) but breaks down for a child whose only real sizing
                    // signal IS an explicit width with no word content to measure (e.g. a solid-color
                    // box). A plain absolute length (not a percentage, which would read this box's own
                    // not-yet-final ActualWidth) is folded in as an explicit floor for this line's
                    // running total. Excludes a non-replaced inline box (Display:Inline with no Words of
                    // its own): per CSS2.1 10.3.3, `width` has no effect on a non-replaced inline-level
                    // box. A child that starts its OWN new "line" must have its explicit width combined
                    // via Math.Max against maxSum, NOT added to maxSumBeforeChild - which already
                    // reflects whatever an earlier, unrelated block-level sibling contributed and must
                    // compete for "widest line wins", not accumulate. Ported from PeachPDF.
                    if (CssValueParser.IsValidLength(childBox.Width) && !childBox.Width.EndsWith("%")
                        && !(childBox.Display == CssConstants.Inline && childBox.Words.Count == 0))
                    {
                        var explicitContentWidth = CssValueParser.ParseLength(childBox.Width, 0, childBox);
                        var childStartsNewLine = childBox.Display != CssConstants.Inline
                            && childBox.Display != CssConstants.TableCell && childBox.WhiteSpace != CssConstants.NoWrap;
                        maxSum = childStartsNewLine
                            ? Math.Max(maxSum, explicitContentWidth)
                            : Math.Max(maxSum, maxSumBeforeChild + explicitContentWidth);
                        min = Math.Max(min, explicitContentWidth);
                    }

                    marginSum -= childBox.ActualMarginLeft + childBox.ActualMarginRight;
                }
            }

            // max sum (and its matching padding contribution) is the max of all the lines in the box
            if (oldSum.HasValue)
            {
                maxSum = Math.Max(maxSum, oldSum.Value);
                paddingSum = Math.Max(paddingSum, oldPaddingSum!.Value);
            }
        }

        /// <summary>
        /// Gets if this box has only inline siblings (including itself)
        /// </summary>
        /// <returns></returns>
        internal bool HasJustInlineSiblings()
        {
            return ParentBox != null && DomUtils.ContainsInlinesOnly(ParentBox);
        }

        /// <summary>
        /// Gets the rectangles where inline box will be drawn. See Remarks for more info.
        /// </summary>
        /// <returns>Rectangles where content should be placed</returns>
        /// <remarks>
        /// Inline boxes can be split across different LineBoxes, that's why this method
        /// Delivers a rectangle for each LineBox related to this box, if inline.
        /// </remarks>
        /// <summary>
        /// Inherits inheritable values from parent.
        /// </summary>
        internal new void InheritStyle(CssBox box = null, bool everything = false)
        {
            base.InheritStyle(box ?? ParentBox, everything);
        }

        /// <summary>
        /// Gets the result of collapsing the vertical margins of the two boxes
        /// </summary>
        /// <param name="prevSibling">the previous box under the same parent</param>
        /// <returns>Resulting top margin</returns>
        internal double MarginTopCollapse(CssBoxProperties prevSibling)
        {
            double value;
            if (prevSibling != null)
            {
                value = Math.Max(prevSibling.ActualMarginBottom, ActualMarginTop);
                CollapsedMarginTop = value;
            }
            else if (_parentBox != null && ActualPaddingTop < 0.1 && ActualPaddingBottom < 0.1 && _parentBox.ActualPaddingTop < 0.1 && _parentBox.ActualPaddingBottom < 0.1)
            {
                value = Math.Max(0, ActualMarginTop - Math.Max(_parentBox.ActualMarginTop, _parentBox.CollapsedMarginTop));
            }
            else
            {
                value = ActualMarginTop;
            }

            // fix for hr tag
            if (value < 0.1 && HtmlTag != null && HtmlTag.Name == "hr")
            {
                value = GetEmHeight() * 1.1f;
            }

            return value;
        }

        /// <summary>
        /// Calculate the actual right of the box by the actual right of the child boxes if this box actual right is not set.
        /// </summary>
        /// <returns>the calculated actual right value</returns>
        private double CalculateActualRight()
        {
            if (ActualRight > 90999)
            {
                var maxRight = 0d;
                foreach (var box in Boxes)
                {
                    maxRight = Math.Max(maxRight, box.ActualRight + box.ActualMarginRight);
                }
                return maxRight + ActualPaddingRight + ActualMarginRight + ActualBorderRightWidth;
            }
            else
            {
                return ActualRight;
            }
        }

        /// <summary>
        /// Gets the result of collapsing the vertical margins of the two boxes
        /// </summary>
        /// <returns>Resulting bottom margin</returns>
        private double MarginBottomCollapse()
        {
            // Only called when at least one in-flow child exists (see the caller's IsOutOfFlow gate),
            // so this is guaranteed to find one - an out-of-flow (floated/absolutely-positioned) child
            // doesn't contribute to this box's own auto-height (CSS 2.1 10.6.3/10.6.7) and must not be
            // used as "the last child" here, matching PeachPDF's own MarginBottomCollapse
            // (`Boxes.Last(b => !b.IsOutOfFlow)`).
            var lastInFlowBox = _boxes.FindLast(b => !b.IsOutOfFlow);

            double margin = 0;
            if (ParentBox != null && ParentBox.Boxes.IndexOf(this) == ParentBox.Boxes.Count - 1 && _parentBox.ActualMarginBottom < 0.1)
            {
                var lastChildBottomMargin = lastInFlowBox.ActualMarginBottom;
                margin = Height == "auto" ? Math.Max(ActualMarginBottom, lastChildBottomMargin) : lastChildBottomMargin;
            }
            // StaticBottom (not ActualBottom): a relatively-positioned last child's own visual offset must
            // not widen this box's auto height (CSS 2.1 9.4.3). Ported from PeachPDF's MarginBottomCollapse.
            return Math.Max(ActualBottom, lastInFlowBox.StaticBottom + margin + ActualPaddingBottom + ActualBorderBottomWidth);
        }

        /// <summary>
        /// Deeply offsets the top of the box and its contents
        /// </summary>
        /// <param name="amount"></param>
        /// <remarks>
        /// A real gap found while auditing this port's fragmentation engine against PeachPDF a second
        /// time: this box's own <see cref="Rectangles"/> entry for a line was kept in sync, but the
        /// line's OWN mirror of the same value (<see cref="CssLineBox.Rectangles"/>, keyed the other way
        /// around) was not - the two are separate dictionaries updated by separate call sites
        /// (<see cref="CssLineBox.ShiftLine"/> keeps both in sync when a line-level shift initiates the
        /// move; this method didn't when a box-level shift does). <see cref="CssLineBox.LineTop"/>/
        /// <c>LineBottom</c> - and therefore <see cref="EffectiveTop"/> for any inline-only box, since it
        /// reads them - went stale after this method ran, even though <see cref="Location"/> (this
        /// method's own last statement) was correctly updated. Confirmed by directly inspecting both
        /// dictionaries after a real <c>EnforceKeepWithNext</c> run-shift: <c>Location.Y</c> reflected the
        /// new position while <c>EffectiveTop</c> still reported the old one.
        /// </remarks>
        internal void OffsetTop(double amount)
        {
            List<CssLineBox> lines = new List<CssLineBox>();
            foreach (CssLineBox line in Rectangles.Keys)
                lines.Add(line);

            foreach (CssLineBox line in lines)
            {
                RRect r = Rectangles[line];
                var shifted = new RRect(r.X, r.Y + amount, r.Width, r.Height);
                Rectangles[line] = shifted;
                line.Rectangles[this] = shifted;
            }

            foreach (CssRect word in Words)
            {
                word.Top += amount;
            }

            foreach (CssBox b in Boxes)
            {
                b.OffsetTop(amount);
            }

            if (_listItemBox != null)
                _listItemBox.OffsetTop(amount);

            Location = new RPoint(Location.X, Location.Y + amount);
        }

        /// <summary>
        /// Paints the background of the box
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="rect">the bounding rectangle to draw in</param>
        /// <param name="isFirst">is it the first rectangle of the element</param>
        /// <param name="isLast">is it the last rectangle of the element</param>
        internal void PaintBackground(RGraphics g, RRect rect, bool isFirst, bool isLast)
        {
            if (rect.Width > 0 && rect.Height > 0)
            {
                RBrush brush = null;

                var backgroundImage = ActualBackgroundImage;
                if (backgroundImage is CssImage.LinearGradient linearGradient)
                {
                    brush = CssImagePainter.GetLinearGradientBrush(g, rect, linearGradient.Gradient);
                }
                else if (RenderUtils.IsColorVisible(ActualBackgroundColor))
                {
                    brush = g.GetSolidBrush(ActualBackgroundColor);
                }

                var rad = ComputeRadii(rect);

                if (brush != null)
                {
                    // TODO:a handle it correctly (tables background)
                    // if (isLast)
                    //  rectangle.Width -= ActualWordSpacing + CssUtils.GetWordEndWhitespace(ActualFont);

                    RGraphicsPath roundrect = null;
                    if (rad.IsRounded)
                    {
                        roundrect = RenderUtils.GetRoundRect(g, rect, rad.TLX, rad.TLY, rad.TRX, rad.TRY, rad.BRX, rad.BRY, rad.BLX, rad.BLY);
                    }

                    Object prevMode = null;
                    if (HtmlContainer != null && !HtmlContainer.AvoidGeometryAntialias && rad.IsRounded)
                    {
                        prevMode = g.SetAntiAliasSmoothingMode();
                    }

                    if (roundrect != null)
                    {
                        g.DrawPath(brush, roundrect);
                    }
                    else
                    {
                        g.DrawRectangle(brush, Math.Ceiling(rect.X), Math.Ceiling(rect.Y), rect.Width, rect.Height);
                    }

                    g.ReturnPreviousSmoothingMode(prevMode);

                    if (roundrect != null)
                        roundrect.Dispose();
                    brush.Dispose();
                }

                if (_imageLoadHandler != null && _imageLoadHandler.Image != null && isFirst)
                {
                    BackgroundImageDrawHandler.DrawBackgroundImage(g, this, _imageLoadHandler, rect);
                }
            }
        }

        /// <summary>
        /// Paints one word at <paramref name="wordRect"/>, its final paint position - the caller decides
        /// where that is (fragment-tree-local geometry, offset the same way <see cref="PaintBackground"/>'s
        /// line rects already are; there is no live-tree geometry read here, only style/selection state).
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="word">the word to paint</param>
        /// <param name="wordRect">the word's final paint rectangle</param>
        internal void PaintWord(RGraphics g, CssRect word, RRect wordRect)
        {
            if (word.IsLineBreak)
                return;

            var clip = g.GetClip();
            clip.Intersect(wordRect);
            if (clip == RRect.Empty)
                return;

            var isRtl = Direction == CssConstants.Rtl;
            var wordPoint = new RPoint(wordRect.X, wordRect.Y);
            if (word.Selected)
            {
                // handle paint selected word background and with partial word selection
                var wordLine = DomUtils.GetCssLineBoxByWord(word);
                var left = word.SelectedStartOffset > -1 ? word.SelectedStartOffset : (wordLine.Words[0] != word && word.HasSpaceBefore ? -ActualWordSpacing : 0);
                var padWordRight = word.HasSpaceAfter && !wordLine.IsLastSelectedWord(word);
                var width = word.SelectedEndOffset > -1 ? word.SelectedEndOffset : word.Width + (padWordRight ? ActualWordSpacing : 0);
                var rect = new RRect(wordRect.X + left, wordRect.Y, width - left, wordLine.LineHeight);

                g.DrawRectangle(GetSelectionBackBrush(g, false), rect.X, rect.Y, rect.Width, rect.Height);

                if (HtmlContainer.SelectionForeColor != RColor.Empty && (word.SelectedStartOffset > 0 || word.SelectedEndIndexOffset > -1))
                {
                    g.PushClipExclude(rect);
                    g.DrawString(word.Text, ActualFont, ActualColor, wordPoint, new RSize(word.Width, word.Height), isRtl);
                    g.PopClip();
                    g.PushClip(rect);
                    g.DrawString(word.Text, ActualFont, GetSelectionForeBrush(), wordPoint, new RSize(word.Width, word.Height), isRtl);
                    g.PopClip();
                }
                else
                {
                    g.DrawString(word.Text, ActualFont, GetSelectionForeBrush(), wordPoint, new RSize(word.Width, word.Height), isRtl);
                }
            }
            else
            {
                g.DrawString(word.Text, ActualFont, ActualColor, wordPoint, new RSize(word.Width, word.Height), isRtl);
            }
        }

        /// <summary>
        /// Paints the text decoration (underline/strike-through/over-line)
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="rectangle"> </param>
        /// <param name="isFirst"> </param>
        /// <param name="isLast"> </param>
        internal void PaintDecoration(RGraphics g, RRect rectangle, bool isFirst, bool isLast)
        {
            if (string.IsNullOrEmpty(TextDecoration) || TextDecoration == CssConstants.None)
                return;

            double x1 = rectangle.X;
            if (isFirst)
                x1 += ActualPaddingLeft + ActualBorderLeftWidth;

            double x2 = rectangle.Right;
            if (isLast)
                x2 -= ActualPaddingRight + ActualBorderRightWidth;

            var bottomInset = ActualPaddingBottom - ActualBorderBottomWidth;
            var pen = g.GetPen(ActualColor);
            pen.Width = 1;
            pen.DashStyle = RDashStyle.Solid;

            // text-decoration-line may list several space-separated line keywords (e.g. "underline
            // overline") - draw each one present rather than only the first/only value.
            foreach (var line in TextDecoration.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                double y;
                if (line == CssConstants.Underline)
                {
                    y = Math.Round(rectangle.Top + ActualFont.UnderlineOffset);
                }
                else if (line == CssConstants.LineThrough)
                {
                    y = rectangle.Top + rectangle.Height / 2f;
                }
                else if (line == CssConstants.Overline)
                {
                    y = rectangle.Top;
                }
                else
                {
                    continue;
                }

                y -= bottomInset;
                g.DrawLine(pen, x1, y, x2, y);
            }
        }

        /// <summary>
        /// Offsets the rectangle of the specified linebox by the specified gap,
        /// and goes deep for rectangles of children in that linebox.
        /// </summary>
        /// <param name="lineBox"></param>
        /// <param name="gap"></param>
        internal void OffsetRectangle(CssLineBox lineBox, double gap)
        {
            if (Rectangles.ContainsKey(lineBox))
            {
                var r = Rectangles[lineBox];
                Rectangles[lineBox] = new RRect(r.X, r.Y + gap, r.Width, r.Height);
            }
        }

        /// <summary>
        /// Resets the <see cref="Rectangles"/> array
        /// </summary>
        internal void RectanglesReset()
        {
            _rectangles.Clear();
        }

        /// <summary>
        /// On image load process complete with image request refresh for it to be painted.
        /// </summary>
        /// <param name="image">the image loaded or null if failed</param>
        /// <param name="rectangle">the source rectangle to draw in the image (empty - draw everything)</param>
        /// <param name="async">is the callback was called async to load image call</param>
        private void OnImageLoadComplete(RImage image, RRect rectangle, bool async)
        {
            if (image != null && async)
                HtmlContainer.RequestRefresh(false);
        }

        /// <summary>
        /// Get brush for the text depending if there is selected text color set.
        /// </summary>
        protected RColor GetSelectionForeBrush()
        {
            return HtmlContainer.SelectionForeColor != RColor.Empty ? HtmlContainer.SelectionForeColor : ActualColor;
        }

        /// <summary>
        /// Get brush for selection background depending if it has external and if alpha is required for images.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="forceAlpha">used for images so they will have alpha effect</param>
        protected RBrush GetSelectionBackBrush(RGraphics g, bool forceAlpha)
        {
            var backColor = HtmlContainer.SelectionBackColor;
            if (backColor != RColor.Empty)
            {
                if (forceAlpha && backColor.A > 180)
                    return g.GetSolidBrush(RColor.FromArgb(180, backColor.R, backColor.G, backColor.B));
                else
                    return g.GetSolidBrush(backColor);
            }
            else
            {
                return g.GetSolidBrush(CssUtils.DefaultSelectionBackcolor);
            }
        }

        protected override RFont GetCachedFont(string fontFamily, double fsize, RFontStyle st, int weight, int stretch, int? codepoint)
        {
            return HtmlContainer.Adapter.GetFont(fontFamily, fsize, st, weight, stretch, codepoint);
        }

        /// <summary>See <see cref="CssBoxProperties.GetFirstNonWhitespaceCodepoint"/>.</summary>
        protected override int? GetFirstNonWhitespaceCodepoint()
        {
            if (string.IsNullOrEmpty(_text))
                return null;

            for (var i = 0; i < _text.Length; i++)
            {
                if (char.IsWhiteSpace(_text[i]))
                    continue;

                if (char.IsHighSurrogate(_text[i]) && i + 1 < _text.Length && char.IsLowSurrogate(_text[i + 1]))
                    return char.ConvertToUtf32(_text[i], _text[i + 1]);

                return _text[i];
            }

            return null;
        }

        protected override RColor GetActualColor(string colorStr)
        {
            return HtmlContainer.CssParser.ParseColor(colorStr);
        }

        protected override CssImage GetActualBackgroundImageValue(string value)
        {
            return HtmlContainer.CssParser.ParseBackgroundImage(value);
        }

        protected override RPoint GetActualLocation(string X, string Y)
        {
            // position:fixed's own left/top offset resolves against the page/viewport size (CSS 2.1
            // §10.1: the initial containing block) and, like every other positioning scheme, the box's own
            // margin still applies on top of that offset. Ported from PeachPDF's CommitBlockChildOffset
            // Fixed branch (PeachPDF does not consult right/bottom for position:fixed either).
            var left = ActualMarginLeft + ResolveOffsetOrZero(this, X, this.HtmlContainer.PageSize.Width);
            var top = ActualMarginTop + ResolveOffsetOrZero(this, Y, this.HtmlContainer.PageSize.Height);
            return new RPoint(left, top);
        }

        /// <summary>
        /// CSS 2.1 §9.4.3's near/far offset resolution for one axis: the near offset (<c>left</c>/<c>top</c>)
        /// wins when set; if it's <c>auto</c> and the far offset (<c>right</c>/<c>bottom</c>) isn't, the far
        /// offset applies with its sign flipped; if both are <c>auto</c>, the offset is 0. Ported from
        /// PeachPDF's CssBox.ResolveNearFarOffset.
        /// </summary>
        private static double ResolveNearFarOffset(CssBox box, string near, string far, double basis)
        {
            var nearIsAuto = string.IsNullOrEmpty(near) || near == CssConstants.Auto;
            var farIsAuto = string.IsNullOrEmpty(far) || far == CssConstants.Auto;

            if (!nearIsAuto)
            {
                return CssValueParser.ParseLength(near, basis, box);
            }

            if (!farIsAuto)
            {
                return -CssValueParser.ParseLength(far, basis, box);
            }

            return 0;
        }

        /// <summary>
        /// Resolves a single <c>left</c>/<c>top</c>/<c>right</c>/<c>bottom</c> offset for the absolute/fixed
        /// positioning branches, where the counterpart edge is never consulted (unlike the relative-
        /// positioning near/far resolution in <see cref="ResolveNearFarOffset"/>) - an <c>auto</c> offset
        /// simply contributes 0. Ported from PeachPDF's CssBox.ResolveOffsetOrZero.
        /// </summary>
        private static double ResolveOffsetOrZero(CssBox box, string offset, double basis)
        {
            return offset != CssConstants.Auto && !string.IsNullOrEmpty(offset)
                ? CssValueParser.ParseLength(offset, basis, box)
                : 0;
        }

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var tag = HtmlTag != null ? string.Format("<{0}>", HtmlTag.Name) : "anon";

            if (IsBlock)
            {
                return string.Format("{0}{1} Block {2}, Children:{3}", ParentBox == null ? "Root: " : string.Empty, tag, FontSize, Boxes.Count);
            }
            else if (Display == CssConstants.None)
            {
                return string.Format("{0}{1} None", ParentBox == null ? "Root: " : string.Empty, tag);
            }
            else
            {
                return string.Format("{0}{1} {2}: {3}", ParentBox == null ? "Root: " : string.Empty, tag, Display, Text);
            }
        }

        #endregion
    }
}