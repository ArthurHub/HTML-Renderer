using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Collects layout's output into the immutable <see cref="FragmentTree"/>. This is the first-cut
    /// version, sized for a single fragmentainer covering the whole document (no break tokens are ever
    /// produced yet) - a stepping stone that reproduces PeachPDF's own pre-fragmentation "single walk over
    /// the finished box tree" era, on top of which real multi-pass resumption is added next. It
    /// deliberately does not port PeachPDF's full <c>FragmentEmitter</c> (nested fragmentainers, row
    /// displacement/slicing, continuation shells - none of which this port needs yet).
    /// </summary>
    internal sealed class FragmentEmitter
    {
        private readonly HtmlContainerInt _container;

        internal FragmentEmitter(HtmlContainerInt container)
        {
            _container = container;
        }

        /// <summary>
        /// Materializes the immutable <see cref="FragmentTree"/> from the box tree as it stands right now.
        /// Layout must have already finished - this reads geometry, it does not compute any.
        /// </summary>
        internal FragmentTree Finish()
        {
            var root = _container.Root;
            if (root == null || _container.ActualSize.Height <= 0)
                return new FragmentTree(new List<FragmentainerFragment>(0));

            var rect = new RRect(RPoint.Empty, _container.ActualSize);
            var geometry = new PageBandGeometry(0, rect.Height, _container.MarginTop, _container.MarginRight, _container.MarginBottom, _container.MarginLeft);
            var rootFragment = BuildBoxFragment(root, fragmentainerIndex: 0);
            var fragmentainer = new FragmentainerFragment(rect, SlotIndex: 0, geometry, LocalOriginY: 0, rootFragment);

            return new FragmentTree(new List<FragmentainerFragment> { fragmentainer });
        }

        /// <summary>
        /// Builds one <see cref="BoxFragment"/> for <paramref name="box"/> and, recursively, for every
        /// descendant - the whole box tree, unconditionally. Display/visibility is a paint-time concern
        /// (<c>display: none</c>/<c>visibility: hidden</c> boxes still get a fragment; the painter skips
        /// drawing them), matching PeachPDF's separation of "layout states a structural fact" from
        /// "paint decides how to use it".
        /// </summary>
        private BoxFragment BuildBoxFragment(CssBox box, int fragmentainerIndex)
        {
            var rect = box.Bounds;

            var lines = new List<LineFragment>();
            if (box.Rectangles.Count == 0)
            {
                lines.Add(new LineFragment(rect, null, TrivialSlice(rect)));
            }
            else
            {
                foreach (var pair in box.Rectangles)
                {
                    lines.Add(new LineFragment(pair.Value, pair.Key, TrivialSlice(pair.Value)));
                }
            }

            var words = new List<TextFragment>(box.Words.Count);
            foreach (var word in box.Words)
            {
                words.Add(new TextFragment(word.Rectangle, word));
            }

            var children = new List<BoxFragment>(box.Boxes.Count);
            foreach (var child in box.Boxes)
            {
                children.Add(BuildBoxFragment(child, fragmentainerIndex));
            }

            return new BoxFragment(
                rect,
                box,
                fragmentainerIndex,
                OriginY: box.Location.Y,
                WholeBoxRect: rect,
                IsFixed: box.IsFixed,
                IsFirstFragment: true,
                IsLastFragment: true,
                IsMonolithic: MonolithicContent.IsMonolithic(box),
                lines,
                words,
                children,
                OverflowClip: null);
        }

        /// <summary>
        /// A no-op <see cref="SliceGeometry"/> for a rectangle that is whole in its one fragmentainer -
        /// every edge is a real box edge, since nothing straddles a break yet.
        /// </summary>
        private static SliceGeometry TrivialSlice(RRect rect) => new(rect, rect, HasLeftEdge: true, HasRightEdge: true);
    }
}
