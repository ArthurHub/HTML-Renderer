using System;
using System.Collections.Generic;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Fragments;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Collects layout's output into the immutable <see cref="FragmentTree"/>. Layout (see
    /// <see cref="BlockFragmentation"/>) already positions every box correctly across however many
    /// pages the document spans, in one continuous top-down pass with local relocation corrections -
    /// so unlike PeachPDF's pass-based emitter, this one does not need to collect per-pass output over
    /// multiple <c>EmitPass</c> calls. Its job is simpler: walk the already-finished box tree once per
    /// page band and bucket each box's rectangles into whichever band(s) they fall in, splitting a box
    /// that spans multiple pages into one <see cref="BoxFragment"/> per page it appears on.
    /// </summary>
    internal sealed class FragmentEmitter
    {
        private readonly HtmlContainerInt _container;

        internal FragmentEmitter(HtmlContainerInt container)
        {
            _container = container;
        }

        /// <summary>
        /// Materializes the immutable <see cref="FragmentTree"/> from the box tree as it stands right
        /// now. Layout must have already finished - this reads geometry, it does not compute any.
        /// </summary>
        internal FragmentTree Finish()
        {
            var root = _container.Root;
            if (root == null || _container.ActualSize.Height <= 0)
                return new FragmentTree(new List<FragmentainerFragment>(0));

            if (!_container.HasRealPageGrid)
            {
                // No bounded page grid (WinForms/WPF's continuous-scroll convention, or any container
                // that never set a real PageSize) - the whole document is one fragmentainer.
                var rect = new RRect(RPoint.Empty, _container.ActualSize);
                var band = new PageBand(0, rect.Height);
                var geometry = new PageBandGeometry(0, rect.Height, _container.MarginTop, _container.MarginRight, _container.MarginBottom, _container.MarginLeft);
                var rootFragment = BuildBoxFragment(root, 0, band);
                var fragmentainer = new FragmentainerFragment(rect, 0, geometry, 0, rootFragment);
                return new FragmentTree(new List<FragmentainerFragment> { fragmentainer });
            }

            // root.ActualBottom (Location.Y + Size.Height), not _container.ActualSize.Height: the
            // latter is document height *excluding* the root's own top offset (ActualSize.Height =
            // ActualBottom - Root.Location.Y, set at the end of CssBox.PerformLayoutImp/Epilogue), so
            // using it directly here as an absolute Y would double-subtract MarginTop inside
            // PageIndexOf and under-report the last slot whenever content's true bottom lands just
            // past a page boundary that ActualSize.Height alone doesn't yet cross.
            var lastSlot = _container.PageIndexOf(Math.Max(0, root.ActualBottom - Epsilon));
            var fragmentainers = new List<FragmentainerFragment>();

            // css-position-3, paged media: a fixed box's containing block is each page's own page area,
            // and it "is thus replicated on every page". Collected once - each fixed box's own Location
            // is already page-relative (CssBox never runs it through normal top-computing flow; see
            // CssBox.PerformLayoutImp's own Position==Fixed branch), so building its fragment against a
            // band starting at Y=0 (rather than this slot's real band top) localizes it to exactly that
            // same relative position on every page, unchanged.
            var fixedRoots = new List<CssBox>();
            CollectFixedRoots(root, fixedRoots);
            var fixedBand = new PageBand(0, _container.PageSize.Height);

            for (var slot = 0; slot <= lastSlot; slot++)
            {
                var bandTop = _container.PageTopOf(slot);
                var bandBottom = _container.PageBottomOf(slot);
                var band = new PageBand(bandTop, bandBottom);

                // CSS Paged Media 3 3.2: a page-slot no box has any content in is never materialized -
                // this falls out of the walk rather than being special-cased, since a box only gets
                // built into this fragmentainer at all when HasContentInBand finds something. Fixed
                // content deliberately does not itself justify materializing an otherwise content-empty
                // slot - matches this port's existing blank-page-skipping scope.
                if (!HasContentInBand(root, band))
                    continue;

                var rootFragment = BuildBoxFragment(root, slot, band);
                if (fixedRoots.Count > 0)
                {
                    var fixedFragments = fixedRoots
                        .Where(fixedRoot => HasContentInBand(fixedRoot, fixedBand))
                        .Select(fixedRoot => BuildBoxFragment(fixedRoot, slot, fixedBand))
                        .ToList();
                    if (fixedFragments.Count > 0)
                        rootFragment = rootFragment with { Children = rootFragment.Children.Concat(fixedFragments).ToList() };
                }

                var rect = new RRect(0, 0, _container.PageSize.Width, band.Height);
                var geometry = new PageBandGeometry(bandTop, band.Height, _container.MarginTop, _container.MarginRight, _container.MarginBottom, _container.MarginLeft);
                fragmentainers.Add(new FragmentainerFragment(rect, slot, geometry, bandTop, rootFragment));
            }

            return new FragmentTree(fragmentainers);
        }

        /// <summary>
        /// Finds every <c>position:fixed</c> box in the tree, at any nesting depth - each one gets its
        /// own independent repeat-per-page treatment in <see cref="Finish"/>, regardless of whether it's
        /// nested inside another fixed box (rare, but each still resolves its own page-relative position
        /// independently per css-position-3, so neither should be folded into the other's subtree).
        /// </summary>
        private static void CollectFixedRoots(CssBox box, List<CssBox> into)
        {
            foreach (var child in box.Boxes)
            {
                if (child.Position == CssConstants.Fixed)
                    into.Add(child);
                CollectFixedRoots(child, into);
            }
        }

        /// <summary>
        /// Whether <paramref name="box"/> or any descendant has some rectangle (its own decoration
        /// rects, a word, or a child's) overlapping <paramref name="band"/> - used both to decide
        /// whether a page-slot is content-empty (skip it) and whether a child belongs in this band's
        /// fragment at all.
        /// </summary>
        private bool HasContentInBand(CssBox box, PageBand band)
        {
            if (box.Rectangles.Count == 0)
            {
                if (Overlaps(box.Bounds, band)) return true;
            }
            else
            {
                foreach (var rect in box.Rectangles.Values)
                {
                    if (Overlaps(rect, band)) return true;
                }
            }

            foreach (var word in box.Words)
            {
                if (Overlaps(word.Rectangle, band)) return true;
            }

            foreach (var child in box.Boxes)
            {
                // A fixed box is handled separately when there's a real page grid (see
                // CollectFixedRoots/Finish) - it repeats identically on every page rather than
                // belonging to whichever band its own (page-relative, not absolute) coordinates would
                // otherwise overlap. Without a real page grid (WinForms/WPF continuous-scroll, one
                // fragmentainer for the whole document) it stays in the normal walk unchanged - "stays
                // put" there is a paint-time scroll-offset suppression, not a repeat-per-page concern.
                if (_container.HasRealPageGrid && child.Position == CssConstants.Fixed) continue;
                if (HasContentInBand(child, band)) return true;
            }

            if (box.ListItemBox != null && HasContentInBand(box.ListItemBox, band)) return true;

            if (box.RepeatedHeaderRows != null)
            {
                foreach (var repeatedRow in box.RepeatedHeaderRows)
                {
                    if (HasContentInBand(repeatedRow, band)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Builds one <see cref="BoxFragment"/> for the portion of <paramref name="box"/> falling in
        /// <paramref name="band"/>, recursively, for every descendant with content there. Coordinates
        /// are made fragmentainer-local (document Y - <paramref name="band"/>.Top) throughout.
        /// </summary>
        private BoxFragment BuildBoxFragment(CssBox box, int fragmentainerIndex, PageBand band)
        {
            var lines = new List<LineFragment>();
            if (box.Rectangles.Count == 0)
            {
                if (Overlaps(box.Bounds, band))
                {
                    var clipped = ToLocal(Clip(box.Bounds, band), band);
                    lines.Add(new LineFragment(clipped, null, TrivialSlice(clipped)));
                }
            }
            else
            {
                foreach (var pair in box.Rectangles)
                {
                    if (!Overlaps(pair.Value, band)) continue;
                    var clipped = ToLocal(Clip(pair.Value, band), band);
                    lines.Add(new LineFragment(clipped, pair.Key, TrivialSlice(clipped)));
                }
            }

            var words = new List<TextFragment>();
            foreach (var word in box.Words)
            {
                // A word is monolithic (css-break-3 4.1) - never sliced, only localized.
                if (Overlaps(word.Rectangle, band))
                    words.Add(new TextFragment(ToLocal(word.Rectangle, band), word));
            }

            var children = new List<BoxFragment>();
            foreach (var child in box.Boxes)
            {
                // See the matching check/comment in HasContentInBand.
                if (_container.HasRealPageGrid && child.Position == CssConstants.Fixed) continue;
                if (HasContentInBand(child, band))
                    children.Add(BuildBoxFragment(child, fragmentainerIndex, band));
            }

            if (box.RepeatedHeaderRows != null)
            {
                foreach (var repeatedRow in box.RepeatedHeaderRows)
                {
                    if (HasContentInBand(repeatedRow, band))
                        children.Add(BuildBoxFragment(repeatedRow, fragmentainerIndex, band));
                }
            }

            BoxFragment markerFragment = null;
            if (box.ListItemBox != null && HasContentInBand(box.ListItemBox, band))
                markerFragment = BuildBoxFragment(box.ListItemBox, fragmentainerIndex, band);

            var rect = ToLocal(Clip(box.Bounds, band), band);
            var wholeBoxRect = ToLocal(box.Bounds, band);

            var topSlot = _container.PageIndexOf(box.Location.Y);
            var bottomSlot = _container.PageIndexOf(Math.Max(box.Location.Y, box.ActualBottom - Epsilon));
            var thisSlot = _container.PageIndexOf(band.Top);
            var isFirstFragment = thisSlot <= topSlot;
            var isLastFragment = thisSlot >= bottomSlot;

            return new BoxFragment(
                rect,
                box,
                fragmentainerIndex,
                OriginY: box.Location.Y,
                WholeBoxRect: wholeBoxRect,
                IsFixed: box.IsFixed,
                IsFirstFragment: isFirstFragment,
                IsLastFragment: isLastFragment,
                IsMonolithic: MonolithicContent.IsMonolithic(box),
                lines,
                words,
                children,
                markerFragment,
                OverflowClip: null);
        }

        private const double Epsilon = 0.01;

        private static bool Overlaps(RRect rect, PageBand band) => rect.Top < band.Bottom && rect.Bottom > band.Top;

        private static RRect Clip(RRect rect, PageBand band)
        {
            var top = Math.Max(rect.Top, band.Top);
            var bottom = Math.Min(rect.Bottom, band.Bottom);
            return new RRect(rect.X, top, rect.Width, Math.Max(0, bottom - top));
        }

        private static RRect ToLocal(RRect rect, PageBand band) => new RRect(rect.X, rect.Y - band.Top, rect.Width, rect.Height);

        /// <summary>
        /// A no-op <see cref="SliceGeometry"/> - every edge is treated as a real box edge, since real
        /// box-decoration-break slicing (distinguishing a genuine break edge from a real box edge) is
        /// deferred until paint needs to draw a spanning box's borders correctly (Stage E2).
        /// </summary>
        private static SliceGeometry TrivialSlice(RRect rect) => new(rect, rect, HasLeftEdge: true, HasRightEdge: true);
    }
}
