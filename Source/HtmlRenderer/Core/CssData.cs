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
    /// Holds the parsed style sheets (UA default and author) and matches their rules against the
    /// box tree, ordered for the cascade.
    /// </summary>
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
        /// default (user agent) css data. If false only the data in the given stylesheet is returned.
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

        private List<MediaList[]> _mediaConditions;
        private Dictionary<string, List<IndexedRule>> _tagIndex;
        private Dictionary<string, List<IndexedRule>> _classIndex;
        private Dictionary<string, List<IndexedRule>> _idIndex;
        private List<IndexedRule> _universalRules;

        /// <summary>
        /// The distinct <c>@media</c> chains any indexed rule is nested under. Small (one entry per
        /// distinct <c>@media</c> block), and only used to test whether a change of device would change
        /// which rules apply - see <see cref="MediaOutcomeChanged"/>.
        /// </summary>
        private static List<MediaList[]> CollectMediaConditions(
            Dictionary<string, List<IndexedRule>> tagIndex,
            Dictionary<string, List<IndexedRule>> classIndex,
            Dictionary<string, List<IndexedRule>> idIndex,
            List<IndexedRule> universal)
        {
            var conditions = new List<MediaList[]>();

            Action<IEnumerable<IndexedRule>> collect = rules =>
            {
                foreach (var indexed in rules)
                {
                    if (indexed.EnclosingMedia == null) continue;
                    if (conditions.Contains(indexed.EnclosingMedia)) continue;   // reference compare: one array per block
                    conditions.Add(indexed.EnclosingMedia);
                }
            };

            foreach (var bucket in tagIndex.Values) collect(bucket);
            foreach (var bucket in classIndex.Values) collect(bucket);
            foreach (var bucket in idIndex.Values) collect(bucket);
            collect(universal);

            return conditions;
        }

        /// <summary>
        /// Whether moving from <paramref name="before"/> to <paramref name="after"/> changes the outcome
        /// of any <c>@media</c> condition in these stylesheets - i.e. whether the cascade needs to be
        /// re-run. False for a resize that doesn't cross any breakpoint, and for documents with no
        /// viewport-dependent <c>@media</c> at all (the common case), so the caller can skip the work.
        /// </summary>
        internal bool MediaOutcomeChanged(MediaQueryContext before, MediaQueryContext after)
        {
            EnsureIndex();

            foreach (var condition in _mediaConditions)
            {
                if (MediaQueryMatcher.Matches(condition, before) != MediaQueryMatcher.Matches(condition, after))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Every rule in a rule list, in document order, descending into nested style rules,
        /// <c>@layer</c>, <c>@media</c>, <c>@container</c> and, when its condition is true,
        /// <c>@supports</c>. <c>@media</c> conditions aren't evaluated here.
        /// </summary>
        internal static IEnumerable<IRule> FlattenRules(IEnumerable<IRule> rules)
        {
            foreach (var rule in rules)
            {
                yield return rule;

                if (rule is IStyleRule styleRule)
                {
                    foreach (var nested in FlattenRules(styleRule.NestedRules))
                        yield return nested;
                }
                else if (rule is ILayerRule layerRule)
                {
                    foreach (var nested in FlattenRules(layerRule.Rules))
                        yield return nested;
                }
                else if (rule is IMediaRule mediaRule)
                {
                    foreach (var nested in FlattenRules(mediaRule.Rules))
                        yield return nested;
                }
                else if (rule is IContainerRule containerRule)
                {
                    foreach (var nested in FlattenRules(containerRule.Rules))
                        yield return nested;
                }
                else if (rule is ISupportsRule supportsRule)
                {
                    if (supportsRule.Condition.Check())
                    {
                        foreach (var nested in FlattenRules(supportsRule.Rules))
                            yield return nested;
                    }
                }
            }
        }

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
            _mediaConditions = CollectMediaConditions(tagIndex, classIndex, idIndex, universal);
            _universalRules = universal;
        }

        /// <summary>
        /// Walks a rule list in true document order, recursing into <c>@media</c> blocks, <c>@layer</c>
        /// blocks, <c>@supports</c> blocks whose condition is true and CSS-Nesting child rules (any nesting depth) in place, and buckets every style rule
        /// found - assigning each an ever-increasing <see cref="IndexedRule.DocumentOrder"/> and recording
        /// the chain of enclosing media conditions it's nested under, if any.
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

                    // CSS Nesting 1 §3.4: nested rules aren't in Stylesheet.Rules. Indexed right after the
                    // parent in the same media context, so they come after it in order of appearance.
                    if (styleRule.NestedRules.Count > 0)
                    {
                        IndexRules(styleRule.NestedRules, isUserAgent, enclosingMedia, tagIndex, classIndex, idIndex, universal, keys, ref order);
                    }
                }
                else if (rule is IMediaRule mediaRule)
                {
                    MediaList[] nestedMedia = enclosingMedia == null
                        ? new[] { mediaRule.Media }
                        : enclosingMedia.Concat(new[] { mediaRule.Media }).ToArray();
                    IndexRules(mediaRule.Rules, isUserAgent, nestedMedia, tagIndex, classIndex, idIndex, universal, keys, ref order);
                }
                else if (rule is ILayerRule layerRule)
                {
                    // CSS Cascade 5 §6.4: layer order isn't tracked, the rules cascade as if unlayered
                    IndexRules(layerRule.Rules, isUserAgent, enclosingMedia, tagIndex, classIndex, idIndex, universal, keys, ref order);
                }
                else if (rule is ISupportsRule supportsRule)
                {
                    // css-conditional-3 #at-supports: the condition doesn't depend on the device, so it
                    // is decided once here instead of per box like @media.
                    if (supportsRule.Condition.Check())
                        IndexRules(supportsRule.Rules, isUserAgent, enclosingMedia, tagIndex, classIndex, idIndex, universal, keys, ref order);
                }
                // Everything else (@layer statements, @property, @font-palette-values, @container, ...)
                // contributes no style rules to the cascade.
            }
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
        internal IEnumerable<IStyleRule> GetStyleRules(MediaQueryContext media, CssBox box)
        {
            return GetStyleRulesByOrigin(media, box, null);
        }

        /// <summary>
        /// Only the user-agent (default stylesheet) rules matching <paramref name="box"/>.
        /// </summary>
        internal IEnumerable<IStyleRule> GetUserAgentStyleRules(MediaQueryContext media, CssBox box)
        {
            return GetStyleRulesByOrigin(media, box, true);
        }

        /// <summary>
        /// Only the author (non-default) rules matching <paramref name="box"/>.
        /// </summary>
        internal IEnumerable<IStyleRule> GetAuthorStyleRules(MediaQueryContext media, CssBox box)
        {
            return GetStyleRulesByOrigin(media, box, false);
        }

        private IEnumerable<IStyleRule> GetStyleRulesByOrigin(MediaQueryContext media, CssBox box, bool? userAgentOnly)
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
                if (!MediaQueryMatcher.Matches(indexed.EnclosingMedia, media)) return;
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

            // CSS 2.1 §6.4.1 steps 3 and 4
            return matched
                .OrderBy(indexed => GetMatchedSpecificity(indexed.Rule.Selector, box))
                .ThenBy(indexed => indexed.DocumentOrder)
                .Select(indexed => indexed.Rule);
        }

        /// <summary>
        /// The specificity of a matched rule for <paramref name="box"/>, CSS 2.1 §6.4.3. A rule's
        /// selector list counts with the highest matching selector, unlike
        /// <see cref="ListSelector.Specificity"/>, which is the static maximum used inside :is() and :not().
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

            // Matching the element creates the ::before/::after box, which then matches on its own
            // cascade pass. Other pseudo-elements get no box here.
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

            // CSS 2.1 §5.11.3. Matches whatever the mouse does, DomParser.AssignCssBlocks then hands
            // rules with :hover in the subject to HtmlContainerInt.AddHoverBox instead of applying them.
            if (pseudoClassSelector.Class == "hover") return true;

            if (pseudoClassSelector.Class == "root")
                return box.HtmlTag != null && box.HtmlTag.Name.Equals("html", StringComparison.InvariantCultureIgnoreCase);

            // CSS 2.1 §5.11.2. There's no history, every link is unvisited and :visited never matches.
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
        /// The An+B pseudo-classes, including the bare forms like :first-child (CSS 2.1 §5.11.1),
        /// which PseudoClassSelectorFactory creates with Step 0 and Offset 1.
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

            // The "of S" filter of :nth-child()/:nth-last-child(). Without it Kind is AllSelector.
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
        /// Reads a cell's colspan, 1 when it's missing or invalid (HTML §4.9.12.1).
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
