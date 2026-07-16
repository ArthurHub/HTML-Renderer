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
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// Holds parsed stylesheets (UA default + author + inline), and provides origin-aware,
    /// specificity-ordered selector matching against the box tree.<br/>
    /// </summary>
    /// <remarks>
    /// Ported from PeachPDF's Html/Core/CssData.cs (itself a fork of Tyler Brinks' ExCSS), adapted to
    /// HTML-Renderer's CssBox/HtmlTag shape. One deliberate omission from the source: the
    /// <c>::marker</c> pseudo-element box-synthesis path is removed - <c>::before</c>/<c>::after</c>
    /// synthesis is kept, but this codebase has no <c>CssBox.IsMarkerPseudoElement</c> concept and no
    /// consumer for it (list markers are painted procedurally, not via a synthesized box).
    /// </remarks>
    public sealed class CssData
    {
        /// <summary>
        /// The parsed stylesheets that make up this CssData, in the order they were added (UA default
        /// first, then author/`&lt;link&gt;`/`&lt;style&gt;` stylesheets in document order). Each carries its own
        /// <see cref="Stylesheet.IsUserAgent"/> flag so origin-aware cascade phases can be resolved.
        /// </summary>
        internal List<Stylesheet> Stylesheets { get; } = new List<Stylesheet>();

        /// <summary>
        /// Init.
        /// </summary>
        internal CssData()
        {
        }

        /// <summary>
        /// Parse the given stylesheet to <see cref="CssData"/> object.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed css blocks are added to the
        /// default css data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="adapter">Platform adapter</param>
        /// <param name="stylesheet">the stylesheet source to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the parsed css data</returns>
        public static CssData Parse(RAdapter adapter, string stylesheet, bool combineWithDefault = true)
        {
            var parser = new CssParser(adapter);
            return parser.ParseStyleSheet(stylesheet, combineWithDefault);
        }

        /// <summary>
        /// Combine this CSS data's stylesheets with <paramref name="other"/>'s.
        /// </summary>
        /// <param name="other">the CSS data to combine with</param>
        public void Combine(CssData other)
        {
            ArgChecker.AssertArgNotNull(other, "other");
            Stylesheets.AddRange(other.Stylesheets);
        }

        /// <summary>
        /// Create a shallow copy of this css data - the returned instance has its own <see cref="Stylesheets"/>
        /// list (so appending a new stylesheet to the clone doesn't affect the original), but the
        /// <see cref="Stylesheet"/> instances themselves are shared (they're never mutated after parsing).
        /// </summary>
        /// <returns>cloned object</returns>
        public CssData Clone()
        {
            var clone = new CssData();
            clone.Stylesheets.AddRange(Stylesheets);
            return clone;
        }

        // --- Rule index -----------------------------------------------------------------------
        //
        // Matching every stylesheet rule against every CssBox in the document (the naive approach)
        // is O(rules x boxes) and dominates cascade cost on large documents. Real browser engines
        // avoid this by bucketing rules by the "subject" simple selector (the one that must match the
        // box itself, e.g. the tag/class/id) so a box only needs to test the handful of rules that
        // could plausibly match it, instead of the whole stylesheet. DoesSelectorMatch remains the
        // source of truth for whether a rule actually matches - the index only narrows the candidates.
        //
        // Built lazily, once, the first time this CssData's rules are queried. Safe because by the
        // time CascadeApplyStyles starts querying rules for the box tree, DomParser has already
        // finished building/cloning CssData from <style>/<link> tags, so Stylesheets doesn't change
        // during the walk.
        private enum SelectorBucketKind { Tag, Class, Id, Universal }

        // DocumentOrder is assigned while walking stylesheets (recursing into @media blocks in
        // place) so that, regardless of which bucket a rule is later retrieved through, the true
        // source order - including across the plain-rule/@media boundary - can be reconstructed
        // for specificity tie-breaking (see GetStyleRulesByOrigin). EnclosingMedia carries the
        // chain of @media conditions (outermost first) a rule was nested under, so media matching
        // doesn't need a separate unindexed scan.
        private readonly struct IndexedRule
        {
            public IndexedRule(IStyleRule rule, bool isUserAgent, int documentOrder, MediaList[] enclosingMedia)
            {
                Rule = rule;
                IsUserAgent = isUserAgent;
                DocumentOrder = documentOrder;
                EnclosingMedia = enclosingMedia;
            }

            public IStyleRule Rule { get; }
            public bool IsUserAgent { get; }
            public int DocumentOrder { get; }
            public MediaList[] EnclosingMedia { get; }
        }

        private Dictionary<string, List<IndexedRule>> _tagIndex;
        private Dictionary<string, List<IndexedRule>> _classIndex;
        private Dictionary<string, List<IndexedRule>> _idIndex;
        private List<IndexedRule> _universalRules;

        private void EnsureIndex()
        {
            if (_universalRules != null) return;

            var tagIndex = new Dictionary<string, List<IndexedRule>>(StringComparer.InvariantCultureIgnoreCase);
            var classIndex = new Dictionary<string, List<IndexedRule>>(StringComparer.InvariantCultureIgnoreCase);
            var idIndex = new Dictionary<string, List<IndexedRule>>(StringComparer.InvariantCultureIgnoreCase);
            var universal = new List<IndexedRule>();
            var keys = new List<(SelectorBucketKind Kind, string Key)>();
            var order = 0;

            foreach (var stylesheet in Stylesheets)
            {
                IndexRules(stylesheet.Rules, stylesheet.IsUserAgent, null, tagIndex, classIndex, idIndex, universal, keys, ref order);
            }

            _tagIndex = tagIndex;
            _classIndex = classIndex;
            _idIndex = idIndex;
            _universalRules = universal;
        }

        /// <summary>
        /// Walks a rule list in true document order, recursing into <c>@media</c> blocks (any
        /// nesting depth) in place, and buckets every style rule found - assigning each an
        /// ever-increasing <see cref="IndexedRule.DocumentOrder"/> and recording the chain of
        /// enclosing media conditions it's nested under, if any.
        /// </summary>
        private static void IndexRules(
            IEnumerable<IRule> rules,
            bool isUserAgent,
            MediaList[] enclosingMedia,
            Dictionary<string, List<IndexedRule>> tagIndex,
            Dictionary<string, List<IndexedRule>> classIndex,
            Dictionary<string, List<IndexedRule>> idIndex,
            List<IndexedRule> universal,
            List<(SelectorBucketKind Kind, string Key)> keys,
            ref int order)
        {
            foreach (var rule in rules)
            {
                if (rule is IStyleRule styleRule)
                {
                    var indexedRule = new IndexedRule(styleRule, isUserAgent, order++, enclosingMedia);

                    keys.Clear();
                    CollectIndexKeys(styleRule.Selector, keys);

                    foreach (var (kind, key) in keys)
                    {
                        Dictionary<string, List<IndexedRule>> bucket;
                        switch (kind)
                        {
                            case SelectorBucketKind.Tag: bucket = tagIndex; break;
                            case SelectorBucketKind.Class: bucket = classIndex; break;
                            case SelectorBucketKind.Id: bucket = idIndex; break;
                            default: bucket = null; break;
                        }

                        if (bucket == null)
                        {
                            universal.Add(indexedRule);
                        }
                        else
                        {
                            List<IndexedRule> list;
                            if (!bucket.TryGetValue(key, out list))
                                bucket[key] = list = new List<IndexedRule>();
                            list.Add(indexedRule);
                        }
                    }
                }
                else if (rule is IMediaRule mediaRule)
                {
                    MediaList[] nestedMedia = enclosingMedia == null
                        ? new[] { mediaRule.Media }
                        : enclosingMedia.Concat(new[] { mediaRule.Media }).ToArray();
                    IndexRules(mediaRule.Rules, isUserAgent, nestedMedia, tagIndex, classIndex, idIndex, universal, keys, ref order);
                }
            }
        }

        /// <summary>
        /// True if every level of a rule's enclosing <c>@media</c> chain matches <paramref name="media"/>
        /// (nesting is conjunctive - all levels must match), or if the rule isn't nested in any
        /// <c>@media</c> block at all.
        /// </summary>
        private static bool EnclosingMediaMatches(MediaList[] enclosingMedia, string media)
        {
            if (enclosingMedia == null) return true;

            foreach (var mediaList in enclosingMedia)
            {
                var anyMatches = false;
                foreach (var medium in mediaList)
                {
                    var typeMatches = medium.Type == media || medium.Type == "all";
                    if (medium.IsInverse ? !typeMatches : typeMatches)
                    {
                        anyMatches = true;
                        break;
                    }
                }

                if (!anyMatches) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines which bucket(s) a rule's selector should be indexed under, based on the simple
        /// selector that must match the box itself (for <see cref="ComplexSelector"/>, that's its
        /// rightmost/subject selector - see the reversal in <see cref="DoesSelectorMatch(ComplexSelector, CssBox)"/>).
        /// A <see cref="ListSelector"/> (comma-separated) contributes one key per alternative, since
        /// matching any alternative matches the rule.
        /// </summary>
        private static void CollectIndexKeys(ISelector selector, List<(SelectorBucketKind Kind, string Key)> keys)
        {
            if (selector is ListSelector listSelector)
            {
                foreach (var sub in listSelector)
                    CollectIndexKeys(sub, keys);
            }
            else if (selector is ComplexSelector complexSelector)
            {
                var last = complexSelector.LastOrDefault().Selector;
                if (last != null)
                    CollectIndexKeys(last, keys);
                else
                    keys.Add((SelectorBucketKind.Universal, string.Empty));
            }
            else if (selector is CompoundSelector compoundSelector)
            {
                // Pseudo-element and :first-child subjects need the box's own HtmlTag/ParentBox
                // re-derived at match time (see DoesSelectorMatch), including for synthesized
                // pseudo-element boxes whose HtmlTag is null - the tag/class/id index can't
                // safely represent that, so fall back to a full scan for these.
                if (compoundSelector.Any(s => s is PseudoElementSelector || s is FirstChildSelector))
                {
                    keys.Add((SelectorBucketKind.Universal, string.Empty));
                    return;
                }

                IdSelector id = null;
                ClassSelector cls = null;
                TypeSelector type = null;

                foreach (var member in compoundSelector)
                {
                    if (member is IdSelector idSel) id = id ?? idSel;
                    else if (member is ClassSelector clsSel) cls = cls ?? clsSel;
                    else if (member is TypeSelector typeSel) type = type ?? typeSel;
                }

                if (id != null) keys.Add((SelectorBucketKind.Id, id.Id));
                else if (cls != null) keys.Add((SelectorBucketKind.Class, cls.Class));
                else if (type != null) keys.Add((SelectorBucketKind.Tag, type.Name));
                else keys.Add((SelectorBucketKind.Universal, string.Empty));
            }
            else if (selector is TypeSelector typeSelector)
            {
                keys.Add((SelectorBucketKind.Tag, typeSelector.Name));
            }
            else if (selector is ClassSelector classSelector)
            {
                keys.Add((SelectorBucketKind.Class, classSelector.Class));
            }
            else if (selector is IdSelector idSelector)
            {
                keys.Add((SelectorBucketKind.Id, idSelector.Id));
            }
            else
            {
                // AllSelector, bare pseudo-class/element/attribute selectors, etc.
                keys.Add((SelectorBucketKind.Universal, string.Empty));
            }
        }

        /// <summary>
        /// All style rules (any origin) matching <paramref name="box"/> under <paramref name="media"/>,
        /// ordered by specificity then document order.
        /// </summary>
        internal IEnumerable<IStyleRule> GetStyleRules(string media, CssBox box)
        {
            return GetStyleRulesByOrigin(media, box, null);
        }

        /// <summary>
        /// Only the user-agent (default stylesheet) rules matching <paramref name="box"/>.
        /// </summary>
        internal IEnumerable<IStyleRule> GetUserAgentStyleRules(string media, CssBox box)
        {
            return GetStyleRulesByOrigin(media, box, true);
        }

        /// <summary>
        /// Only the author (non-default) rules matching <paramref name="box"/>.
        /// </summary>
        internal IEnumerable<IStyleRule> GetAuthorStyleRules(string media, CssBox box)
        {
            return GetStyleRulesByOrigin(media, box, false);
        }

        private IEnumerable<IStyleRule> GetStyleRulesByOrigin(string media, CssBox box, bool? userAgentOnly)
        {
            EnsureIndex();

            // Rules matched via the index below can, in rare cases (a comma-separated selector list
            // whose alternatives land in different buckets, e.g. "div, .foo" matching a <div class="foo">),
            // be reachable through more than one bucket. Dedup so callers never see the same rule twice.
            HashSet<IStyleRule> seen = null;
            var matched = new List<IndexedRule>();

            Action<IndexedRule> collect = indexed =>
            {
                if (userAgentOnly.HasValue && indexed.IsUserAgent != userAgentOnly.Value) return;
                if (!EnclosingMediaMatches(indexed.EnclosingMedia, media)) return;
                if (!DoesSelectorMatch(indexed.Rule.Selector, box)) return;
                seen = seen ?? new HashSet<IStyleRule>();
                if (!seen.Add(indexed.Rule)) return;
                matched.Add(indexed);
            };

            foreach (var indexed in _universalRules)
                collect(indexed);

            if (box.HtmlTag != null)
            {
                List<IndexedRule> tagRules;
                if (_tagIndex.TryGetValue(box.HtmlTag.Name, out tagRules))
                {
                    foreach (var indexed in tagRules)
                        collect(indexed);
                }

                if (box.HtmlTag.Attributes != null)
                {
                    string classAttr;
                    if (box.HtmlTag.Attributes.TryGetValue("class", out classAttr) && classAttr.Length > 0)
                    {
                        foreach (var className in classAttr.Split(' '))
                        {
                            if (className.Length == 0) continue;
                            List<IndexedRule> classRules;
                            if (_classIndex.TryGetValue(className, out classRules))
                                foreach (var indexed in classRules)
                                    collect(indexed);
                        }
                    }

                    string idAttr;
                    List<IndexedRule> idRules;
                    if (box.HtmlTag.Attributes.TryGetValue("id", out idAttr) && idAttr.Length > 0
                        && _idIndex.TryGetValue(idAttr, out idRules))
                    {
                        foreach (var indexed in idRules)
                            collect(indexed);
                    }
                }
            }

            // Stable sort by specificity, tie-broken by DocumentOrder (true source order, including
            // across the plain-rule/@media boundary, assigned once up front by IndexRules) - equal-
            // specificity rules keep true document order for correct source-order tiebreaking.
            return matched
                .OrderBy(indexed => GetMatchedSpecificity(indexed.Rule.Selector, box))
                .ThenBy(indexed => indexed.DocumentOrder)
                .Select(indexed => indexed.Rule);
        }

        /// <summary>
        /// The effective specificity of a matched rule's selector for cascade-ordering purposes,
        /// relative to a specific matched box. This deliberately differs from calling
        /// <c>selector.Specificity</c> directly for a top-level <see cref="ListSelector"/> (comma-
        /// separated rule selector, e.g. "h1, h2 {}"): per spec a selector list used as a whole
        /// rule's selector is cascade-equivalent to separate rules, so the specificity that governs
        /// THIS box's cascade is whichever one alternative actually matched it - not every
        /// alternative's static max (which is what <see cref="ListSelector.Specificity"/> reports,
        /// correctly, for its OTHER use as an argument to :is()/:not()/:has()).
        /// </summary>
        private static Priority GetMatchedSpecificity(ISelector selector, CssBox box)
        {
            var list = selector as ListSelector;
            if (list != null)
            {
                return list
                    .Where(s => DoesSelectorMatch(s, box))
                    .Select(s => s.Specificity)
                    .DefaultIfEmpty(Priority.Zero)
                    .Max();
            }

            return selector.Specificity;
        }

        private static bool DoesSelectorMatch(ISelector selector, CssBox box)
        {
            if (selector is AllSelector) return true;
            if (selector is ListSelector listSelector) return DoesSelectorMatch(listSelector, box);
            if (selector is TypeSelector typeSelector) return DoesSelectorMatch(typeSelector, box);
            if (selector is ComplexSelector complexSelector) return DoesSelectorMatch(complexSelector, box);
            if (selector is CompoundSelector compoundSelector) return DoesSelectorMatch(compoundSelector, box);
            if (selector is PseudoElementSelector pseudoElementSelector) return DoesSelectorMatch(pseudoElementSelector, box);
            if (selector is PseudoClassSelector pseudoClassSelector) return DoesSelectorMatch(pseudoClassSelector, box);
            if (selector is AttrMatchSelector attrMatchSelector) return DoesSelectorMatch(attrMatchSelector, box);
            if (selector is ClassSelector classSelector) return DoesSelectorMatch(classSelector, box);
            if (selector is IdSelector idSelector) return DoesSelectorMatch(idSelector, box);
            if (selector is AttrAvailableSelector attrAvailableSelector) return DoesSelectorMatch(attrAvailableSelector, box);
            if (selector is AttrContainsSelector attrContainsSelector) return DoesSelectorMatch(attrContainsSelector, box);
            if (selector is AttrListSelector attrListSelector) return DoesSelectorMatch(attrListSelector, box);
            if (selector is AttrBeginsSelector attrBeginsSelector) return DoesSelectorMatch(attrBeginsSelector, box);
            if (selector is AttrEndsSelector attrEndsSelector) return DoesSelectorMatch(attrEndsSelector, box);
            if (selector is AttrHyphenSelector attrHyphenSelector) return DoesSelectorMatch(attrHyphenSelector, box);
            if (selector is ChildSelector childSelector) return DoesSelectorMatch(childSelector, box);
            if (selector is OnlyChildSelector onlyChildSelector) return DoesSelectorMatch(onlyChildSelector, box);
            if (selector is OnlyOfTypeSelector onlyOfTypeSelector) return DoesSelectorMatch(onlyOfTypeSelector, box);
            if (selector is NotSelector notSelector) return DoesSelectorMatch(notSelector, box);
            if (selector is MatchesSelector matchesSelector) return DoesSelectorMatch(matchesSelector, box);
            if (selector is HasSelector hasSelector) return DoesSelectorMatch(hasSelector, box);
            return false;
        }

        private static bool DoesSelectorMatch(ListSelector listSelector, CssBox box)
        {
            return listSelector.Any(selector => DoesSelectorMatch(selector, box));
        }

        private static bool DoesSelectorMatch(CompoundSelector compoundSelector, CssBox box)
        {
            if (box == null)
            {
                return false;
            }

            var lastSelector = compoundSelector.Last();

            // Structural pseudo-classes (ChildSelector subtypes, OnlyChildSelector, OnlyOfTypeSelector)
            // are matched against `box` itself here, same as every other compound member - their own
            // DoesSelectorMatch overload re-derives sibling scope from `box` as needed, so no special
            // handling is required for them beyond the plain path below.
            var pseudoElementSelector = lastSelector as PseudoElementSelector;
            if (pseudoElementSelector == null)
                return compoundSelector.All(selector => DoesSelectorMatch(selector, box));

            var referenceBox = box.IsPseudoElement ? box.ParentBox : box;

            var isMatchWithoutPseudoElement = compoundSelector
                .Where(x => !(x is PseudoElementSelector))
                .All(selector => DoesSelectorMatch(selector, referenceBox));

            if (!isMatchWithoutPseudoElement) return false;

            if (box.IsPseudoElement)
            {
                return DoesSelectorMatch(pseudoElementSelector, box);
            }

            // Only ::before/::after synthesize a box here - ::marker is intentionally not ported
            // (no CssBox.IsMarkerPseudoElement concept in this codebase; list markers are painted
            // procedurally, not via a synthesized box - see PaintListStyleShapeMarker and friends).
            if (pseudoElementSelector.Name == CssConstants.Before && !box.Boxes.Any(b => b.IsBeforePseudoElement))
            {
                var beforePseudoBox = new CssBox(box, null);
                beforePseudoBox.IsBeforePseudoElement = true;

                beforePseudoBox.InheritStyle(box);
                box.Boxes.Remove(beforePseudoBox);
                box.Boxes.Insert(0, beforePseudoBox);
            }
            else if (pseudoElementSelector.Name == CssConstants.After && !box.Boxes.Any(b => b.IsAfterPseudoElement))
            {
                var afterPseudoBox = new CssBox(box, null);
                afterPseudoBox.IsAfterPseudoElement = true;

                afterPseudoBox.InheritStyle(box);
                box.Boxes.Remove(afterPseudoBox);
                box.Boxes.Add(afterPseudoBox);
            }

            return false;
        }

        private static bool DoesSelectorMatch(TypeSelector typeSelector, CssBox box)
        {
            return box != null && box.HtmlTag != null && typeSelector.Name.Equals(box.HtmlTag.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool DoesSelectorMatch(ClassSelector classSelector, CssBox box)
        {
            string classNames;
            if (box != null && box.HtmlTag != null && box.HtmlTag.Attributes != null && box.HtmlTag.Attributes.TryGetValue("class", out classNames))
            {
                return classNames.Split(' ').Any(className =>
                    className.Equals(classSelector.Class, StringComparison.InvariantCultureIgnoreCase));
            }

            return false;
        }

        private static bool DoesSelectorMatch(IdSelector idSelector, CssBox box)
        {
            string id;
            if (box != null && box.HtmlTag != null && box.HtmlTag.Attributes != null && box.HtmlTag.Attributes.TryGetValue("id", out id))
            {
                return id.Equals(idSelector.Id, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrAvailableSelector attrAvailableSelector, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrAvailableSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrMatchSelector attrMatchSelector, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrMatchSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    return attribute.Value.Equals(attrMatchSelector.Value, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrListSelector attrListSelector, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrListSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    var attributeValues = attribute.Value.Split(' ').Where(x => x.Length > 0).ToArray();

                    return attributeValues.Any(value => value.Equals(attrListSelector.Value, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrContainsSelector attrContainsSelector, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null)
            {
                return false;
            }

            foreach (var attribute in box.HtmlTag.Attributes)
            {
                if (attribute.Key.Equals(attrContainsSelector.Attribute, StringComparison.InvariantCultureIgnoreCase))
                {
                    return attribute.Value.IndexOf(attrContainsSelector.Value, StringComparison.InvariantCultureIgnoreCase) >= 0;
                }
            }

            return false;
        }

        private static bool DoesSelectorMatch(AttrBeginsSelector s, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null) return false;
            foreach (var attr in box.HtmlTag.Attributes)
                if (attr.Key.Equals(s.Attribute, StringComparison.InvariantCultureIgnoreCase))
                    return attr.Value.StartsWith(s.Value, StringComparison.InvariantCultureIgnoreCase);
            return false;
        }

        private static bool DoesSelectorMatch(AttrEndsSelector s, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null) return false;
            foreach (var attr in box.HtmlTag.Attributes)
                if (attr.Key.Equals(s.Attribute, StringComparison.InvariantCultureIgnoreCase))
                    return attr.Value.EndsWith(s.Value, StringComparison.InvariantCultureIgnoreCase);
            return false;
        }

        private static bool DoesSelectorMatch(AttrHyphenSelector s, CssBox box)
        {
            if (box == null || box.HtmlTag == null || box.HtmlTag.Attributes == null) return false;
            foreach (var attr in box.HtmlTag.Attributes)
                if (attr.Key.Equals(s.Attribute, StringComparison.InvariantCultureIgnoreCase))
                    return attr.Value.Equals(s.Value, StringComparison.InvariantCultureIgnoreCase)
                        || attr.Value.StartsWith(s.Value + "-", StringComparison.InvariantCultureIgnoreCase);
            return false;
        }

        private static bool DoesSelectorMatch(PseudoClassSelector pseudoClassSelector, CssBox box)
        {
            if (box == null) return false;

            // ":hover" is matched structurally (ignoring live mouse-hover state, which this engine
            // doesn't track at cascade time) so DomParser.AssignCssBlock can intercept the match and
            // divert it to HtmlContainerInt.AddHoverBox instead of applying it - preserving the
            // "matched but inert" behavior this engine has always had for :hover (see DomParser).
            if (pseudoClassSelector.Class == "hover") return true;

            // ":root" always matches the document's <html> element - this engine has no separate
            // CssBox.IsRoot flag, so match on tag name directly (correct for any HTML document).
            if (pseudoClassSelector.Class == "root")
                return box.HtmlTag != null && box.HtmlTag.Name.Equals("html", StringComparison.InvariantCultureIgnoreCase);

            return pseudoClassSelector.Class == "link" && box.IsClickable;
        }

        private static bool DoesSelectorMatch(PseudoElementSelector pseudoElementSelector, CssBox box)
        {
            if (box == null)
            {
                return false;
            }

            if (pseudoElementSelector.Name == CssConstants.Before) return box.IsBeforePseudoElement;
            if (pseudoElementSelector.Name == CssConstants.After) return box.IsAfterPseudoElement;
            return false;
        }

        /// <summary>
        /// Matches the six structural "An+B" pseudo-classes (:nth-child, :nth-last-child,
        /// :nth-of-type, :nth-last-of-type, :nth-column, :nth-last-column - including their bare-ident
        /// forms like :first-child, wired to Step=0/Offset=1 by PseudoClassSelectorFactory).
        /// Each variant differs only in (a) which sibling scope to count within and (b) whether to
        /// count from the start or the end; the actual "does this position satisfy An+B" test is
        /// shared via <see cref="MatchesAnPlusB"/>.
        /// </summary>
        private static bool DoesSelectorMatch(ChildSelector childSelector, CssBox box)
        {
            if (box == null || box.HtmlTag == null)
            {
                return false;
            }

            if (childSelector is FirstColumnSelector || childSelector is LastColumnSelector)
            {
                return DoesColumnSelectorMatch(childSelector, box, childSelector is LastColumnSelector);
            }

            var parentBox = DomUtils.GetNearestParentElementBox(box);
            if (parentBox == null)
            {
                return false;
            }

            IEnumerable<CssBox> siblingBoxes = parentBox.Boxes.Where(b => b.HtmlTag != null);

            var sameTypeOnly = childSelector is FirstTypeSelector || childSelector is LastTypeSelector;
            if (sameTypeOnly)
            {
                siblingBoxes = siblingBoxes.Where(b =>
                    b.HtmlTag.Name.Equals(box.HtmlTag.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            // CSS4 "of <selector>" extension (:nth-child(An+B of S)/:nth-last-child(An+B of S)).
            // Kind defaults to AllSelector (matches unconditionally) whenever no "of" clause was
            // written, and the parser only ever allows a non-default Kind on FirstChildSelector/
            // LastChildSelector - so this filter is always a no-op for the four other subtypes.
            // Applying it unconditionally is simpler than special-casing which subtypes can have it.
            siblingBoxes = siblingBoxes.Where(b => DoesSelectorMatch(childSelector.Kind, b));

            var siblings = siblingBoxes.ToList();
            var index = siblings.IndexOf(box);
            if (index < 0)
            {
                return false;
            }

            var fromEndPosition = childSelector is LastChildSelector || childSelector is LastTypeSelector;
            var position = fromEndPosition ? siblings.Count - index : index + 1;

            return MatchesAnPlusB(position, childSelector.Step, childSelector.Offset);
        }

        /// <summary>
        /// Matches :nth-column()/:nth-last-column() against the cell's occupied column position(s)
        /// within its own row. Only accounts for colspan of preceding cells in the SAME row.
        /// A colspan-N cell occupies N columns; per spec this matches if ANY occupied column position
        /// satisfies the An+B formula, not just the cell's starting column.
        /// </summary>
        private static bool DoesColumnSelectorMatch(ChildSelector childSelector, CssBox box, bool fromEnd)
        {
            var row = box.ParentBox;
            if (row == null)
            {
                return false;
            }

            var cellsInRow = row.Boxes.Where(b => b.HtmlTag != null).ToList();
            var columnIndex = 0;
            var totalColumns = 0;
            var found = false;

            foreach (var cell in cellsInRow)
            {
                if (cell == box)
                {
                    columnIndex = totalColumns;
                    found = true;
                }

                totalColumns += GetColSpan(cell);
            }

            if (!found)
            {
                return false;
            }

            var boxSpan = GetColSpan(box);
            for (var column = columnIndex; column < columnIndex + boxSpan; column++)
            {
                var position = fromEnd ? totalColumns - column : column + 1;
                if (MatchesAnPlusB(position, childSelector.Step, childSelector.Offset))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads a box's "colspan" HTML attribute (default 1 if absent/invalid). Deliberately
        /// duplicated from <see cref="Dom.CssLayoutEngineTable"/>'s private equivalent rather than
        /// shared - this runs at cascade time, before the layout-phase state that method's column
        /// bookkeeping otherwise depends on exists.
        /// </summary>
        private static int GetColSpan(CssBox box)
        {
            var value = box.GetAttribute("colspan", "1");
            int colSpan;
            return !int.TryParse(value, out colSpan) || colSpan < 1 ? 1 : colSpan;
        }

        /// <summary>
        /// The standard CSS "An+B" existence test: does there exist a non-negative integer n such that
        /// position == step*n + offset? position is always the 1-based index within whatever sibling
        /// scope the caller has already resolved.
        /// </summary>
        private static bool MatchesAnPlusB(int position, int step, int offset)
        {
            if (step == 0)
            {
                return position == offset;
            }

            var diff = position - offset;
            return diff % step == 0 && (step > 0 ? diff >= 0 : diff <= 0);
        }

        private static bool DoesSelectorMatch(OnlyChildSelector _, CssBox box)
        {
            if (box == null || box.HtmlTag == null)
            {
                return false;
            }

            var parentBox = DomUtils.GetNearestParentElementBox(box);
            return parentBox != null && parentBox.Boxes.Count(b => b.HtmlTag != null) == 1;
        }

        private static bool DoesSelectorMatch(OnlyOfTypeSelector _, CssBox box)
        {
            if (box == null || box.HtmlTag == null)
            {
                return false;
            }

            var parentBox = DomUtils.GetNearestParentElementBox(box);
            if (parentBox == null)
            {
                return false;
            }

            return parentBox.Boxes.Count(b =>
                b.HtmlTag != null &&
                b.HtmlTag.Name.Equals(box.HtmlTag.Name, StringComparison.InvariantCultureIgnoreCase)) == 1;
        }

        private static bool DoesSelectorMatch(NotSelector notSelector, CssBox box)
        {
            return box != null && !DoesSelectorMatch(notSelector.Inner, box);
        }

        private static bool DoesSelectorMatch(MatchesSelector matchesSelector, CssBox box)
        {
            return DoesSelectorMatch(matchesSelector.Inner, box);
        }

        private static bool DoesSelectorMatch(HasSelector hasSelector, CssBox box)
        {
            return box != null && HasDescendantMatch(box, hasSelector.Inner);
        }

        /// <summary>
        /// Recursively searches box's descendants (at any depth) for one matching inner, short-
        /// circuiting on the first hit. Backs :has()'s default (descendant) relative-selector form;
        /// leading-combinator forms (":has(&gt; x)"/"+"/"~") aren't supported.
        /// </summary>
        private static bool HasDescendantMatch(CssBox box, ISelector inner)
        {
            foreach (var child in box.Boxes)
            {
                if (DoesSelectorMatch(inner, child)) return true;
                if (HasDescendantMatch(child, inner)) return true;
            }

            return false;
        }

        private static bool DoesSelectorMatch(ComplexSelector complexSelector, CssBox box)
        {
            var selectorsInReverse = complexSelector.Reverse().ToList();

            var isLowestItem = true;
            var currentRef = box;

            foreach (var selector in selectorsInReverse)
            {
                if (selector.Selector == null) return false;

                if (isLowestItem)
                {
                    if (!DoesSelectorMatch(selector.Selector, currentRef)) return false;
                    isLowestItem = false;
                    continue;
                }

                if (currentRef == null) return false;

                if (selector.Delimiter == ">")
                {
                    currentRef = currentRef.ParentBox;
                    if (!DoesSelectorMatch(selector.Selector, currentRef)) return false;
                }
                else if (selector.Delimiter == " " || selector.Delimiter == null)
                {
                    currentRef = currentRef.ParentBox;
                    while (currentRef != null && !DoesSelectorMatch(selector.Selector, currentRef))
                        currentRef = currentRef.ParentBox;
                    if (currentRef == null) return false;
                }
                else if (selector.Delimiter == "+")
                {
                    var parent = currentRef.ParentBox;
                    if (parent == null) return false;
                    var siblings = parent.Boxes.Where(b => b.HtmlTag != null).ToList();
                    var idx = siblings.IndexOf(currentRef);
                    if (idx <= 0) return false;
                    currentRef = siblings[idx - 1];
                    if (!DoesSelectorMatch(selector.Selector, currentRef)) return false;
                }
                else if (selector.Delimiter == "~")
                {
                    var parent = currentRef.ParentBox;
                    if (parent == null) return false;
                    var siblings = parent.Boxes.Where(b => b.HtmlTag != null).ToList();
                    var idx = siblings.IndexOf(currentRef);
                    if (idx <= 0) return false;
                    CssBox match = null;
                    for (var i = idx - 1; i >= 0; i--)
                        if (DoesSelectorMatch(selector.Selector, siblings[i])) { match = siblings[i]; break; }
                    if (match == null) return false;
                    currentRef = match;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
