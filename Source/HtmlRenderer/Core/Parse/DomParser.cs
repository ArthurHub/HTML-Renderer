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
using System.Linq;
using System.Text;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
    /// <summary>
    /// Handle css DOM tree generation from raw html and stylesheet.
    /// </summary>
    internal sealed class DomParser
    {
        #region Fields and Consts

        /// <summary>
        /// The media type this engine always cascades against. Non-"all" @media rules (e.g. "@media
        /// screen") are parsed and indexed but never actually queried/applied, matching this engine's
        /// pre-existing behavior (there has never been a live "what device/media are we rendering for"
        /// concept here) - not a regression introduced by this backport.
        /// </summary>
        private const string Media = "all";

        /// <summary>
        /// Parser for CSS
        /// </summary>
        private readonly CssParser _cssParser;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public DomParser(CssParser cssParser)
        {
            ArgChecker.AssertArgNotNull(cssParser, "cssParser");

            _cssParser = cssParser;
        }

        /// <summary>
        /// Generate css tree by parsing the given html and applying the given css style data on it.
        /// </summary>
        /// <param name="html">the html to parse</param>
        /// <param name="htmlContainer">the html container to use for reference resolve</param>
        /// <param name="cssData">the css data to use</param>
        /// <returns>the root of the generated tree</returns>
        public CssBox GenerateCssTree(string html, HtmlContainerInt htmlContainer, ref CssData cssData)
        {
            var root = HtmlParser.ParseDocument(html);
            if (root != null)
            {
                root.HtmlContainer = htmlContainer;

                bool cssDataChanged = false;
                CascadeParseStyles(root, htmlContainer, ref cssData, ref cssDataChanged);

                CascadeApplyStyles(root, cssData);

                SetTextSelectionStyle(htmlContainer, cssData);

                CorrectTextBoxes(root);

                CorrectImgBoxes(root);

                CorrectLineBreaksBlocks(root);

                CorrectInlineBoxesParent(root);

                CorrectBlockInsideInline(root);

                CorrectInlineBoxesParent(root);

                CorrectAnonymousTables(root);
            }
            return root;
        }


        #region Private methods

        /// <summary>
        /// Read styles defined inside the dom structure in links and style elements.<br/>
        /// If the html tag is "style" tag parse it content and add to the css data for all future tags parsing.<br/>
        /// If the html tag is "link" that point to style data parse it content and add to the css data for all future tags parsing.<br/>
        /// </summary>
        /// <param name="box">the box to parse style data in</param>
        /// <param name="htmlContainer">the html container to use for reference resolve</param>
        /// <param name="cssData">the style data to fill with found styles</param>
        /// <param name="cssDataChanged">check if the css data has been modified by the handled html not to change the base css data</param>
        private void CascadeParseStyles(CssBox box, HtmlContainerInt htmlContainer, ref CssData cssData, ref bool cssDataChanged)
        {
            if (box.HtmlTag != null)
            {
                // Check for the <link rel=stylesheet> tag
                if (box.HtmlTag.Name.Equals("link", StringComparison.CurrentCultureIgnoreCase) &&
                    box.GetAttribute("rel", string.Empty).Equals("stylesheet", StringComparison.CurrentCultureIgnoreCase))
                {
                    CloneCssData(ref cssData, ref cssDataChanged);
                    var href = box.GetAttribute("href", string.Empty);
                    string stylesheet;
                    CssData stylesheetData;
                    StylesheetLoadHandler.LoadStylesheet(htmlContainer, href, box.HtmlTag.Attributes, out stylesheet, out stylesheetData);
                    if (stylesheet != null)
                        _cssParser.ParseStyleSheet(cssData, stylesheet, CommonUtils.TryGetUri(href));
                    else if (stylesheetData != null)
                        cssData.Combine(stylesheetData);
                }

                // Check for the <style> tag
                if (box.HtmlTag.Name.Equals("style", StringComparison.CurrentCultureIgnoreCase) && box.Boxes.Count > 0)
                {
                    CloneCssData(ref cssData, ref cssDataChanged);
                    foreach (var child in box.Boxes)
                        _cssParser.ParseStyleSheet(cssData, child.Text);
                }
            }

            foreach (var childBox in box.Boxes)
            {
                CascadeParseStyles(childBox, htmlContainer, ref cssData, ref cssDataChanged);
            }
        }


        /// <summary>
        /// Applies style to all boxes in the tree via a spec-correct, origin-aware six-phase cascade:
        /// UA-normal, Author-normal, Inline-normal, Author-!important, Inline-!important, UA-!important
        /// (origin order reverses for the !important tier, per spec). Each phase's revert/revert-layer
        /// target is a snapshot of the box's state immediately before that phase ran.
        /// </summary>
        /// <param name="box">the box to apply the style to</param>
        /// <param name="cssData">the style data for the html</param>
        private void CascadeApplyStyles(CssBox box, CssData cssData)
        {
            // Every box's fields already start at their initial value (CssBoxProperties's private
            // field initializers, which CssDefaults.InitialValues was itself built to mirror exactly -
            // see CssDefaults.cs) - so unlike PeachPDF's cascade, there is no separate "set every
            // property to its initial value" pass here. Deliberately: a handful of this engine's
            // property setters (e.g. LineHeight, CssBoxProperties.cs:550) are eager - they immediately
            // resolve the assigned value to an absolute pixel string using the box's CURRENT font/size,
            // rather than storing it as-is for lazy resolution later. Calling every whitelisted
            // property's setter unconditionally on every box, before InheritStyle() has propagated a
            // real font-size down and before layout has given the box a real size, fires those eager
            // setters against not-yet-meaningful state and bakes in garbage (observed: line-height
            // resolving to "-0px", corrupting all downstream layout). CssDefaults.InitialValues is
            // still very much used - see GetInitialValue() calls below - just only consulted on demand
            // when a declaration literally says initial/unset/revert, never as a blanket pre-pass.

            // 1. Inherit inheritable properties from parent
            box.InheritStyle();

            // Regular property declarations whose value contains var(...) are deferred here and resolved
            // once, after the whole cascade (all phases below) has finished, so var() sees this box's
            // FINAL custom property values regardless of which phase declared them.
            var pendingVarProperties = new Dictionary<string, string>();

            // Anonymous layout-only wrapper boxes (HtmlTag == null, not a pseudo-element) never match
            // anything and are skipped, matching this engine's pre-existing behavior. Synthesized
            // ::before/::after boxes ARE queried despite also having HtmlTag == null - their whole
            // purpose is to receive the declarations from the rule whose selector synthesized them
            // (e.g. "li::before { content: ... }"), matched via CssData's PseudoElementSelector check.
            var canMatchRules = box.HtmlTag != null || box.IsPseudoElement;
            var uaRules = canMatchRules ? cssData.GetUserAgentStyleRules(Media, box).ToList() : new List<IStyleRule>();
            var authorRules = canMatchRules ? cssData.GetAuthorStyleRules(Media, box).ToList() : new List<IStyleRule>();

            // Inline style is parsed up front - parsing is pure text -> rule with no cascade side
            // effects, so hoisting it here (ahead of TranslateAttributes, which still runs at its
            // original point below) doesn't change behavior, and lets RulesUseRevertKeyword below see it too.
            IStyleRule inlineRule = null;
            if (box.HtmlTag != null && box.HtmlTag.HasAttribute("style"))
            {
                inlineRule = _cssParser.ParseInlineStyle(box.HtmlTag.TryGetAttribute("style"));
            }

            // The relatively expensive property/custom-property snapshots below are only ever read
            // back when a later phase's declaration is literally revert/revert-layer, so each is only
            // captured when something that will actually consult it uses one of those keywords.
            var authorUsesRevert = RulesUseRevertKeyword(authorRules);
            var uaUsesRevert = RulesUseRevertKeyword(uaRules);
            var inlineUsesRevert = inlineRule != null && RulesUseRevertKeyword(new[] { inlineRule });

            // Cascade precedence (lowest to highest, i.e. applied in this order so "last write wins"
            // naturally produces the correct winner): UA-normal, Author-normal (inline-normal wins ties
            // within this tier), Author-!important (inline-!important wins ties within this tier),
            // UA-!important (per spec, origin order REVERSES for !important, so it's applied - and wins
            // - last).

            // 3. UA normal
            AssignCssBlocks(box, uaRules, false, null, null, pendingVarProperties);
            var needsUaSnapshot = authorUsesRevert;
            var uaSnapshot = needsUaSnapshot ? CssUtils.SnapshotProperties(box) : null;
            var uaCustomSnapshot = needsUaSnapshot ? CssUtils.SnapshotCustomProperties(box) : null;

            // 4. Author normal
            AssignCssBlocks(box, authorRules, false, uaSnapshot, uaCustomSnapshot, pendingVarProperties);
            var needsAuthorNormalSnapshot = inlineUsesRevert || authorUsesRevert;
            var authorNormalSnapshot = needsAuthorNormalSnapshot ? CssUtils.SnapshotProperties(box) : null;
            var authorNormalCustomSnapshot = needsAuthorNormalSnapshot ? CssUtils.SnapshotCustomProperties(box) : null;

            if (box.HtmlTag != null)
            {
                TranslateAttributes(box.HtmlTag, box);
            }

            // 5. Inline normal; revert target is the author-normal-applied state
            var inlineNormalSnapshot = authorNormalSnapshot;
            var inlineNormalCustomSnapshot = authorNormalCustomSnapshot;
            if (inlineRule != null)
            {
                AssignCssBlock(box, inlineRule, false, authorNormalSnapshot, authorNormalCustomSnapshot, pendingVarProperties);
                var needsInlineNormalSnapshot = authorUsesRevert;
                inlineNormalSnapshot = needsInlineNormalSnapshot ? CssUtils.SnapshotProperties(box) : null;
                inlineNormalCustomSnapshot = needsInlineNormalSnapshot ? CssUtils.SnapshotCustomProperties(box) : null;
            }

            // 6. Author !important. Note this means an author-!important "revert" can roll back to a
            // value inline *normal* style just set, not all the way back to the UA snapshot - a
            // deliberate choice for mechanical consistency ("revert = state right before this phase")
            // rather than a stricter per-origin reading of the spec.
            AssignCssBlocks(box, authorRules, true, inlineNormalSnapshot, inlineNormalCustomSnapshot, pendingVarProperties);
            var needsAuthorImportantSnapshot = inlineUsesRevert || uaUsesRevert;
            var authorImportantSnapshot = needsAuthorImportantSnapshot ? CssUtils.SnapshotProperties(box) : null;
            var authorImportantCustomSnapshot = needsAuthorImportantSnapshot ? CssUtils.SnapshotCustomProperties(box) : null;

            // 7. Inline !important
            var afterInlineImportantSnapshot = authorImportantSnapshot;
            var afterInlineImportantCustomSnapshot = authorImportantCustomSnapshot;
            if (inlineRule != null)
            {
                AssignCssBlock(box, inlineRule, true, authorImportantSnapshot, authorImportantCustomSnapshot, pendingVarProperties);
                var needsAfterInlineImportantSnapshot = uaUsesRevert;
                afterInlineImportantSnapshot = needsAfterInlineImportantSnapshot ? CssUtils.SnapshotProperties(box) : null;
                afterInlineImportantCustomSnapshot = needsAfterInlineImportantSnapshot ? CssUtils.SnapshotCustomProperties(box) : null;
            }

            // 8. UA !important - applied globally last so it wins over everything else, per spec's
            // origin reversal for the !important tier.
            AssignCssBlocks(box, uaRules, true, afterInlineImportantSnapshot, afterInlineImportantCustomSnapshot, pendingVarProperties);

            // 9. Resolve var() references now that every custom property's final cascaded value is known
            ResolveDeferredVarProperties(box, pendingVarProperties);

            // cascade text decoration only to boxes that actually have text so it will be handled correctly.
            if (box.TextDecoration != String.Empty && box.Text == null)
            {
                foreach (var childBox in box.Boxes)
                    childBox.TextDecoration = box.TextDecoration;
                box.TextDecoration = string.Empty;
            }

            foreach (var childBox in box.Boxes)
            {
                CascadeApplyStyles(childBox, cssData);
            }
        }

        /// <summary>
        /// Assigns the given css style rules to the given css box, applying only the declarations whose
        /// !important flag matches <paramref name="importantPass"/>. A rule whose selector's subject
        /// includes ":hover" is diverted to <see cref="HtmlContainerInt.AddHoverBox"/> instead - matching
        /// this engine's pre-existing "matched but never actually applied" :hover behavior.
        /// </summary>
        private static void AssignCssBlocks(
            CssBox box,
            IEnumerable<IStyleRule> rules,
            bool importantPass,
            IDictionary<string, string> revertTarget,
            IDictionary<string, string> customPropertyRevertTarget,
            Dictionary<string, string> pendingVarProperties)
        {
            foreach (var rule in rules)
            {
                if (IsHoverRule(rule))
                {
                    if (!importantPass)
                    {
                        box.HtmlContainer.AddHoverBox(box, rule);
                    }
                    continue;
                }

                AssignCssBlock(box, rule, importantPass, revertTarget, customPropertyRevertTarget, pendingVarProperties);
            }
        }

        /// <summary>
        /// True if the rule's selector's subject (rightmost, in the case of a combinator chain) compound
        /// includes a ":hover" pseudo-class - e.g. "a:hover", ".foo:hover", or bare ":hover".
        /// </summary>
        private static bool IsHoverRule(IStyleRule rule)
        {
            return SelectorHasHover(rule.Selector);
        }

        private static bool SelectorHasHover(ISelector selector)
        {
            var pseudoClass = selector as PseudoClassSelector;
            if (pseudoClass != null) return pseudoClass.Class == "hover";

            var compound = selector as CompoundSelector;
            if (compound != null) return compound.Any(SelectorHasHover);

            var complex = selector as ComplexSelector;
            if (complex != null)
            {
                var last = complex.LastOrDefault().Selector;
                return last != null && SelectorHasHover(last);
            }

            var list = selector as ListSelector;
            if (list != null) return list.Any(SelectorHasHover);

            return false;
        }

        /// <summary>
        /// Checks whether any declaration in the given rules literally uses the revert/revert-layer
        /// keyword, so the (relatively expensive) property snapshot used as their revert target only
        /// needs to be captured when it can actually be consulted.
        /// </summary>
        private static bool RulesUseRevertKeyword(IEnumerable<IStyleRule> rules)
        {
            foreach (var rule in rules)
            {
                foreach (var prop in rule.Style)
                {
                    if (prop.Value == CssConstants.Revert || prop.Value == CssConstants.RevertLayer)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Assigns the given css style rule's declarations to the given css box, applying only the
        /// declarations whose !important flag matches <paramref name="importantPass"/> (the caller is
        /// expected to call this once per origin per pass - see <see cref="CascadeApplyStyles"/> - so
        /// that within a properly-ordered sequence of calls, "last write wins" alone produces the
        /// spec-correct result). Handles the CSS-wide keywords: inherit, initial, unset, revert,
        /// revert-layer. Custom property declarations (--foo) are routed to
        /// <see cref="AssignCustomPropertyDeclaration"/> instead. Regular declarations whose value
        /// contains var(...) are deferred into <paramref name="pendingVarProperties"/> rather than
        /// applied immediately - see <see cref="ResolveDeferredVarProperties"/> for why.
        /// </summary>
        private static void AssignCssBlock(
            CssBox box,
            IStyleRule stylesheetRule,
            bool importantPass,
            IDictionary<string, string> revertTarget,
            IDictionary<string, string> customPropertyRevertTarget,
            Dictionary<string, string> pendingVarProperties)
        {
            foreach (var prop in stylesheetRule.Style)
            {
                if (prop.IsImportant != importantPass)
                    continue;

                if (PropertyFactory.IsCustomPropertyName(prop.Name))
                {
                    AssignCustomPropertyDeclaration(box, prop, customPropertyRevertTarget);
                    continue;
                }

                string value;
                if (prop.Value == CssConstants.Inherit)
                {
                    value = box.ParentBox != null
                        ? CssUtils.GetPropertyValue(box.ParentBox, prop.Name)
                        : CssDefaults.GetInitialValue(prop.Name);
                }
                else if (prop.Value == CssConstants.Initial)
                {
                    value = CssDefaults.GetInitialValue(prop.Name);
                }
                else if (prop.Value == CssConstants.Unset)
                {
                    value = CssDefaults.InheritedProperties.Contains(prop.Name) && box.ParentBox != null
                        ? CssUtils.GetPropertyValue(box.ParentBox, prop.Name)
                        : CssDefaults.GetInitialValue(prop.Name);
                }
                else if (prop.Value == CssConstants.Revert || prop.Value == CssConstants.RevertLayer)
                {
                    string rv;
                    value = revertTarget != null && revertTarget.TryGetValue(prop.Name, out rv)
                        ? rv
                        : CssDefaults.GetInitialValue(prop.Name);
                }
                else
                {
                    value = prop.Value;
                }

                if (value == null) continue;

                if (value.IndexOf("var(", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Overwrites any earlier pending entry for this name (last write wins).
                    pendingVarProperties[prop.Name] = value;
                }
                else
                {
                    // A later plain value supersedes an earlier deferred var() value for the same property.
                    pendingVarProperties.Remove(prop.Name);
                    if (IsStyleOnElementAllowed(box, prop.Name, value))
                        CssUtils.SetPropertyValue(box, prop.Name, value);
                }
            }
        }

        /// <summary>
        /// Assigns a single custom property (--foo) declaration to the box's custom-property store.
        /// Unlike regular properties, custom properties are always inherited, their names are
        /// case-sensitive, and their global keywords resolve against the custom-property dictionary
        /// rather than the fixed, known-property switch used by <see cref="AssignCssBlock"/>. The stored
        /// value is left unresolved (it may itself contain var(...)) - resolution happens once, later, in
        /// <see cref="ResolveDeferredVarProperties"/>.
        /// </summary>
        private static void AssignCustomPropertyDeclaration(
            CssBox box,
            IProperty prop,
            IDictionary<string, string> customPropertyRevertTarget)
        {
            string rawValue;
            if (prop.Value == CssConstants.Inherit || prop.Value == CssConstants.Unset)
            {
                string pv;
                rawValue = box.ParentBox != null && box.ParentBox.CustomProperties != null &&
                           box.ParentBox.CustomProperties.TryGetValue(prop.Name, out pv)
                    ? pv
                    : null;
            }
            else if (prop.Value == CssConstants.Initial)
            {
                rawValue = null; // guaranteed-invalid value => property becomes absent
            }
            else if (prop.Value == CssConstants.Revert || prop.Value == CssConstants.RevertLayer)
            {
                string rv;
                rawValue = customPropertyRevertTarget != null && customPropertyRevertTarget.TryGetValue(prop.Name, out rv)
                    ? rv
                    : null;
            }
            else
            {
                rawValue = prop.Value;
            }

            if (box.CustomProperties == null)
                box.CustomProperties = new Dictionary<string, string>();

            if (rawValue != null)
                box.CustomProperties[prop.Name] = rawValue;
            else
                box.CustomProperties.Remove(prop.Name);
        }

        /// <summary>
        /// Result of attempting to resolve every var() reference in a declaration's value. Success is
        /// false only when a reference is "guaranteed-invalid" (no matching custom property, no
        /// fallback, or a cyclic reference) - per spec this invalidates the whole value.
        /// </summary>
        private struct VarResolution
        {
            public VarResolution(bool success, string value)
            {
                Success = success;
                Value = value;
            }

            public bool Success;
            public string Value;
        }

        /// <summary>
        /// Resolves every regular property deferred during the cascade's phases (see
        /// <see cref="AssignCssBlock"/>) now that this box's custom properties reflect their FINAL
        /// cascaded (though not yet var()-resolved) values. Resolution is graph-based and memoized per
        /// box, so multi-hop cyclic references are detected correctly regardless of which pending
        /// property triggers the lookup first.
        /// </summary>
        private static void ResolveDeferredVarProperties(CssBox box, Dictionary<string, string> pendingVarProperties)
        {
            if (pendingVarProperties.Count == 0) return;

            var resolvedCache = new Dictionary<string, string>();
            var resolving = new HashSet<string>();
            var cyclic = new HashSet<string>();

            foreach (var pending in pendingVarProperties)
            {
                var name = pending.Key;
                var rawValue = pending.Value;
                var result = SubstituteVarReferences(box, rawValue, resolvedCache, resolving, cyclic);
                var finalValue = result.Success ? result.Value : GetGuaranteedInvalidFallback(box, name);

                if (finalValue != null)
                    ApplyResolvedPropertyValue(box, name, finalValue);
            }
        }

        /// <summary>
        /// Applies a fully var()-resolved property value to the box. Shorthand properties are re-parsed
        /// through the real CSS-OM shorthand converter and expanded into longhands here, because
        /// var()-containing shorthands deliberately skip longhand expansion at parse time (their value
        /// can't be split into per-longhand slices until their var() references are resolved, which has
        /// only just happened here).
        /// </summary>
        private static void ApplyResolvedPropertyValue(CssBox box, string name, string value)
        {
            var reparsed = StylesheetParser.Default.ParseDeclaration(name + ": " + value);

            var shorthand = reparsed as ShorthandProperty;
            if (shorthand != null && shorthand.HasValue)
            {
                var longhands = PropertyFactory.Instance.CreateLonghandsFor(name);
                shorthand.Export(longhands);

                foreach (var longhand in longhands)
                {
                    if (longhand.HasValue && IsStyleOnElementAllowed(box, longhand.Name, longhand.Value))
                        CssUtils.SetPropertyValue(box, longhand.Name, longhand.Value);
                }

                return;
            }

            if (reparsed != null && reparsed.HasValue && IsStyleOnElementAllowed(box, name, reparsed.Value))
                CssUtils.SetPropertyValue(box, name, reparsed.Value);
        }

        /// <summary>
        /// Resolves every var(...) occurrence in <paramref name="value"/> to plain text. Quote-aware (so
        /// `content: "var(--x)"` is left untouched) and paren-depth-aware (so a fallback containing
        /// nested commas splits correctly).
        /// </summary>
        private static VarResolution SubstituteVarReferences(CssBox box, string value, Dictionary<string, string> resolvedCache, HashSet<string> resolving, HashSet<string> cyclic)
        {
            if (value.IndexOf("var(", StringComparison.OrdinalIgnoreCase) < 0)
                return new VarResolution(true, value);

            var sb = new StringBuilder();
            var i = 0;
            while (i < value.Length)
            {
                int argsStart, callEnd;
                if (TryMatchVarCall(value, i, out argsStart, out callEnd))
                {
                    string name, fallback;
                    SplitFirstTopLevelComma(value, argsStart, callEnd - 1, out name, out fallback);

                    string found;
                    if (TryResolveCustomProperty(box, name, resolvedCache, resolving, cyclic, out found))
                    {
                        sb.Append(found);
                    }
                    else if (fallback != null)
                    {
                        var fallbackResult = SubstituteVarReferences(box, fallback, resolvedCache, resolving, cyclic);
                        if (!fallbackResult.Success) return new VarResolution(false, null);
                        sb.Append(fallbackResult.Value);
                    }
                    else
                    {
                        return new VarResolution(false, null); // guaranteed-invalid
                    }

                    i = callEnd;
                }
                else if (value[i] == '"' || value[i] == '\'')
                {
                    var quoteEnd = SkipQuotedString(value, i);
                    sb.Append(value, i, quoteEnd - i);
                    i = quoteEnd;
                }
                else
                {
                    sb.Append(value[i]);
                    i++;
                }
            }

            return new VarResolution(true, sb.ToString());
        }

        /// <summary>
        /// Recursive + memoized: resolves box.CustomProperties[name]'s own var() references first (so a
        /// custom property may reference another custom property, in any declaration order), using
        /// `resolving` as a visited-set for cycle detection. `cyclic` permanently marks a property found
        /// to (directly or transitively) reference itself - distinct from plain "not found", since per
        /// spec a self-referencing property is guaranteed-invalid regardless of any fallback written
        /// inside its own definition.
        /// </summary>
        private static bool TryResolveCustomProperty(CssBox box, string name, Dictionary<string, string> resolvedCache, HashSet<string> resolving, HashSet<string> cyclic, out string value)
        {
            if (cyclic.Contains(name))
            {
                value = null;
                return false;
            }

            if (resolvedCache.TryGetValue(name, out value)) return true;

            string rawValue;
            if (box.CustomProperties == null || !box.CustomProperties.TryGetValue(name, out rawValue))
            {
                value = null;
                return false;
            }

            if (!resolving.Add(name))
            {
                cyclic.Add(name); // name is referenced while already being resolved - a cycle
                value = null;
                return false;
            }

            var result = SubstituteVarReferences(box, rawValue, resolvedCache, resolving, cyclic);
            resolving.Remove(name);

            if (cyclic.Contains(name) || !result.Success)
            {
                cyclic.Add(name);
                value = null;
                return false;
            }

            value = result.Value;
            resolvedCache[name] = value;
            return true;
        }

        /// <summary>
        /// Matches a case-insensitive "var(" starting at <paramref name="start"/> and scans forward for
        /// the matching close paren. On success, <paramref name="argsStart"/> is the index of the first
        /// argument character and <paramref name="callEnd"/> is the index just past the closing paren.
        /// </summary>
        private static bool TryMatchVarCall(string value, int start, out int argsStart, out int callEnd)
        {
            argsStart = 0;
            callEnd = 0;

            if (start + 4 > value.Length) return false;
            if (string.Compare(value, start, "var(", 0, 4, StringComparison.OrdinalIgnoreCase) != 0) return false;

            var depth = 1;
            var i = start + 4;
            while (i < value.Length)
            {
                var c = value[i];
                if (c == '"' || c == '\'')
                {
                    i = SkipQuotedString(value, i);
                    continue;
                }

                if (c == '(') depth++;
                else if (c == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        argsStart = start + 4;
                        callEnd = i + 1;
                        return true;
                    }
                }

                i++;
            }

            return false;
        }

        /// <summary>
        /// Splits a var() argument list at the first top-level (paren-depth 0, outside quotes) comma.
        /// The text before it is the custom property name (trimmed); everything after - commas and all -
        /// is the fallback, per spec.
        /// </summary>
        private static void SplitFirstTopLevelComma(string value, int start, int end, out string name, out string fallback)
        {
            var depth = 0;
            var i = start;
            while (i < end)
            {
                var c = value[i];
                if (c == '"' || c == '\'')
                {
                    i = SkipQuotedString(value, i);
                    continue;
                }

                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (c == ',' && depth == 0)
                {
                    name = value.Substring(start, i - start).Trim();
                    var fb = value.Substring(i + 1, end - (i + 1)).Trim();
                    fallback = fb.Length > 0 ? fb : null;
                    return;
                }

                i++;
            }

            name = value.Substring(start, end - start).Trim();
            fallback = null;
        }

        /// <summary>
        /// Advances past a quoted string literal starting at <paramref name="start"/> (which must point
        /// at the opening quote), honoring backslash escapes so an escaped quote doesn't end the string
        /// early. Returns the index just past the closing quote (or the string's end, if unterminated).
        /// </summary>
        private static int SkipQuotedString(string value, int start)
        {
            var quote = value[start];
            var i = start + 1;
            while (i < value.Length)
            {
                if (value[i] == '\\' && i + 1 < value.Length)
                {
                    i += 2;
                    continue;
                }

                if (value[i] == quote)
                    return i + 1;

                i++;
            }

            return i;
        }

        /// <summary>
        /// The value a property falls back to when a var() reference in it is guaranteed-invalid -
        /// reuses the same expression as the "unset" keyword arm in <see cref="AssignCssBlock"/>.
        /// </summary>
        private static string GetGuaranteedInvalidFallback(CssBox box, string propName)
        {
            return CssDefaults.InheritedProperties.Contains(propName) && box.ParentBox != null
                ? CssUtils.GetPropertyValue(box.ParentBox, propName)
                : CssDefaults.GetInitialValue(propName);
        }

        /// <summary>
        /// Set the selected text style (selection text color and background color) from any "::selection"
        /// rule found in the parsed stylesheets. This is a document-level lookup, not a per-box cascade
        /// one - there is no box for a "::selection" pseudo-element to match against.
        /// </summary>
        /// <param name="htmlContainer"> </param>
        /// <param name="cssData">the style data</param>
        private void SetTextSelectionStyle(HtmlContainerInt htmlContainer, CssData cssData)
        {
            htmlContainer.SelectionForeColor = RColor.Empty;
            htmlContainer.SelectionBackColor = RColor.Empty;

            foreach (var stylesheet in cssData.Stylesheets)
            {
                foreach (var rule in stylesheet.StyleRules)
                {
                    if (!SelectorIsSelectionPseudoElement(rule.Selector)) continue;

                    foreach (var prop in rule.Style)
                    {
                        if (prop.Name == "color")
                            htmlContainer.SelectionForeColor = _cssParser.ParseColor(prop.Value);
                        else if (prop.Name == "background-color")
                            htmlContainer.SelectionBackColor = _cssParser.ParseColor(prop.Value);
                    }
                }
            }
        }

        private static bool SelectorIsSelectionPseudoElement(ISelector selector)
        {
            var pseudoElement = selector as PseudoElementSelector;
            if (pseudoElement != null) return pseudoElement.Name == "selection";

            var compound = selector as CompoundSelector;
            if (compound != null) return compound.Any(SelectorIsSelectionPseudoElement);

            var complex = selector as ComplexSelector;
            if (complex != null)
            {
                var last = complex.LastOrDefault().Selector;
                return last != null && SelectorIsSelectionPseudoElement(last);
            }

            var list = selector as ListSelector;
            if (list != null) return list.Any(SelectorIsSelectionPseudoElement);

            return false;
        }

        /// <summary>
        /// Check if the given style is allowed to be set on the given css box.<br/>
        /// Used to prevent invalid CssBoxes creation like table with inline display style.
        /// </summary>
        /// <param name="box">the css box to assign css to</param>
        /// <param name="key">the style key to cehck</param>
        /// <param name="value">the style value to check</param>
        /// <returns>true - style allowed, false - not allowed</returns>
        private static bool IsStyleOnElementAllowed(CssBox box, string key, string value)
        {
            if (box.HtmlTag != null && key == HtmlConstants.Display)
            {
                switch (box.HtmlTag.Name)
                {
                    case HtmlConstants.Table:
                        return value == CssConstants.Table;
                    case HtmlConstants.Tr:
                        return value == CssConstants.TableRow;
                    case HtmlConstants.Tbody:
                        return value == CssConstants.TableRowGroup;
                    case HtmlConstants.Thead:
                        return value == CssConstants.TableHeaderGroup;
                    case HtmlConstants.Tfoot:
                        return value == CssConstants.TableFooterGroup;
                    case HtmlConstants.Col:
                        return value == CssConstants.TableColumn;
                    case HtmlConstants.Colgroup:
                        return value == CssConstants.TableColumnGroup;
                    case HtmlConstants.Td:
                    case HtmlConstants.Th:
                        return value == CssConstants.TableCell;
                    case HtmlConstants.Caption:
                        return value == CssConstants.TableCaption;
                }
            }
            return true;
        }

        /// <summary>
        /// Clone css data if it has not already been cloned.<br/>
        /// Used to preserve the base css data used when changed by style inside html.
        /// </summary>
        private static void CloneCssData(ref CssData cssData, ref bool cssDataChanged)
        {
            if (!cssDataChanged)
            {
                cssDataChanged = true;
                cssData = cssData.Clone();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="box"></param>
        private void TranslateAttributes(HtmlTag tag, CssBox box)
        {
            if (tag.HasAttributes())
            {
                foreach (string att in tag.Attributes.Keys)
                {
                    string value = tag.Attributes[att];

                    switch (att)
                    {
                        case HtmlConstants.Align:
                            if (value == HtmlConstants.Left || value == HtmlConstants.Center || value == HtmlConstants.Right || value == HtmlConstants.Justify)
                                box.TextAlign = value.ToLower();
                            else
                                box.VerticalAlign = value.ToLower();
                            break;
                        case HtmlConstants.Background:
                            box.BackgroundImage = value.ToLower();
                            break;
                        case HtmlConstants.Bgcolor:
                            box.BackgroundColor = value.ToLower();
                            break;
                        case HtmlConstants.Border:
                            if (!string.IsNullOrEmpty(value) && value != "0")
                                box.BorderLeftStyle = box.BorderTopStyle = box.BorderRightStyle = box.BorderBottomStyle = CssConstants.Solid;
                            box.BorderLeftWidth = box.BorderTopWidth = box.BorderRightWidth = box.BorderBottomWidth = TranslateLength(value);

                            if (tag.Name == HtmlConstants.Table)
                            {
                                if (value != "0")
                                    ApplyTableBorder(box, "1px");
                            }
                            else
                            {
                                box.BorderTopStyle = box.BorderLeftStyle = box.BorderRightStyle = box.BorderBottomStyle = CssConstants.Solid;
                            }
                            break;
                        case HtmlConstants.Bordercolor:
                            box.BorderLeftColor = box.BorderTopColor = box.BorderRightColor = box.BorderBottomColor = value.ToLower();
                            break;
                        case HtmlConstants.Cellspacing:
                            box.BorderSpacing = TranslateLength(value);
                            break;
                        case HtmlConstants.Cellpadding:
                            ApplyTablePadding(box, value);
                            break;
                        case HtmlConstants.Color:
                            box.Color = value.ToLower();
                            break;
                        case HtmlConstants.Dir:
                            box.Direction = value.ToLower();
                            break;
                        case HtmlConstants.Face:
                            box.FontFamily = _cssParser.ParseFontFamily(value);
                            break;
                        case HtmlConstants.Height:
                            box.Height = TranslateLength(value);
                            break;
                        case HtmlConstants.Hspace:
                            box.MarginRight = box.MarginLeft = TranslateLength(value);
                            break;
                        case HtmlConstants.Nowrap:
                            box.WhiteSpace = CssConstants.NoWrap;
                            break;
                        case HtmlConstants.Size:
                            if (tag.Name.Equals(HtmlConstants.Hr, StringComparison.OrdinalIgnoreCase))
                                box.Height = TranslateLength(value);
                            else if (tag.Name.Equals(HtmlConstants.Font, StringComparison.OrdinalIgnoreCase))
                                box.FontSize = value;
                            break;
                        case HtmlConstants.Valign:
                            box.VerticalAlign = value.ToLower();
                            break;
                        case HtmlConstants.Vspace:
                            box.MarginTop = box.MarginBottom = TranslateLength(value);
                            break;
                        case HtmlConstants.Width:
                            box.Width = TranslateLength(value);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts an HTML length into a Css length
        /// </summary>
        /// <param name="htmlLength"></param>
        /// <returns></returns>
        private static string TranslateLength(string htmlLength)
        {
            CssLength len = new CssLength(htmlLength);

            if (len.HasError)
            {
                return string.Format(NumberFormatInfo.InvariantInfo, "{0}px", htmlLength);
            }

            return htmlLength;
        }

        /// <summary>
        /// Cascades to the TD's the border spacified in the TABLE tag.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="border"></param>
        private static void ApplyTableBorder(CssBox table, string border)
        {
            SetForAllCells(table, cell =>
            {
                cell.BorderLeftStyle = cell.BorderTopStyle = cell.BorderRightStyle = cell.BorderBottomStyle = CssConstants.Solid;
                cell.BorderLeftWidth = cell.BorderTopWidth = cell.BorderRightWidth = cell.BorderBottomWidth = border;
            });
        }

        /// <summary>
        /// Cascades to the TD's the border spacified in the TABLE tag.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="padding"></param>
        private static void ApplyTablePadding(CssBox table, string padding)
        {
            var length = TranslateLength(padding);
            SetForAllCells(table, cell => cell.PaddingLeft = cell.PaddingTop = cell.PaddingRight = cell.PaddingBottom = length);
        }

        /// <summary>
        /// Execute action on all the "td" cells of the table.<br/>
        /// Handle if there is "theader" or "tbody" exists.
        /// </summary>
        /// <param name="table">the table element</param>
        /// <param name="action">the action to execute</param>
        private static void SetForAllCells(CssBox table, ActionInt<CssBox> action)
        {
            foreach (var l1 in table.Boxes)
            {
                foreach (var l2 in l1.Boxes)
                {
                    if (l2.HtmlTag != null && l2.HtmlTag.Name == "td")
                    {
                        action(l2);
                    }
                    else
                    {
                        foreach (var l3 in l2.Boxes)
                        {
                            action(l3);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Go over all the text boxes (boxes that have some text that will be rendered) and
        /// remove all boxes that have only white-spaces but are not 'preformatted' so they do not effect
        /// the rendered html.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectTextBoxes(CssBox box)
        {
            for (int i = box.Boxes.Count - 1; i >= 0; i--)
            {
                var childBox = box.Boxes[i];

                CssContentEngine.ApplyContent(childBox);

                if (childBox.Text != null)
                {
                    // is the box has text
                    var keepBox = !string.IsNullOrWhiteSpace(childBox.Text);

                    // is the box a "br" element (its Text is a forced "\n" line break, not real content)
                    keepBox = keepBox || childBox.IsBrElement;

                    // is the box is pre-formatted
                    keepBox = keepBox || childBox.WhiteSpace == CssConstants.Pre || childBox.WhiteSpace == CssConstants.PreWrap;

                    // is the box is only one in the parent
                    keepBox = keepBox || box.Boxes.Count == 1;

                    // is it a whitespace between two inline boxes
                    keepBox = keepBox || (i > 0 && i < box.Boxes.Count - 1 && box.Boxes[i - 1].IsInline && box.Boxes[i + 1].IsInline);

                    // is first/last box where is in inline box and it's next/previous box is inline
                    keepBox = keepBox || (i == 0 && box.Boxes.Count > 1 && box.Boxes[1].IsInline && box.IsInline) || (i == box.Boxes.Count - 1 && box.Boxes.Count > 1 && box.Boxes[i - 1].IsInline && box.IsInline);

                    if (keepBox)
                    {
                        // valid text box, parse it to words
                        childBox.ParseToWords();
                    }
                    else
                    {
                        // remove text box that has no
                        childBox.ParentBox.Boxes.RemoveAt(i);
                    }
                }
                else
                {
                    // recursive
                    CorrectTextBoxes(childBox);
                }
            }
        }

        /// <summary>
        /// Go over all image boxes and if its display style is set to block, put it inside another block but set the image to inline.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectImgBoxes(CssBox box)
        {
            for (int i = box.Boxes.Count - 1; i >= 0; i--)
            {
                var childBox = box.Boxes[i];
                if (childBox is CssBoxImage && childBox.Display == CssConstants.Block)
                {
                    var block = CssBox.CreateBlock(childBox.ParentBox, null, childBox);
                    childBox.ParentBox = block;
                    childBox.Display = CssConstants.Inline;
                }
                else
                {
                    // recursive
                    CorrectImgBoxes(childBox);
                }
            }
        }

        /// <summary>
        /// Correct the DOM tree recursively by turning "br" html boxes that should force a line break
        /// into a "\n" text word on the box itself, reusing the same forced-newline mechanism used for
        /// "white-space: pre/pre-line" content.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectLineBreaksBlocks(CssBox box)
        {
            foreach (var childBox in box.Boxes)
            {
                CorrectLineBreaksBlocks(childBox);
            }

            if (!box.IsBrElement) return;

            var previousSibling = DomUtils.GetPreviousSibling(box);
            if (previousSibling == null || previousSibling.IsBlock)
            {
                var nextSibling = DomUtils.GetFollowingSiblings(box, b => b.IsInline && !b.IsBrElement, true).FirstOrDefault();
                if (nextSibling == null)
                {
                    box.Text = "\n";
                    box.ParseToWords();
                }
            }
        }

        /// <summary>
        /// Correct DOM tree if there is block boxes that are inside inline blocks.<br/>
        /// Need to rearrange the tree so block box will be only the child of other block box.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectBlockInsideInline(CssBox box)
        {
            try
            {
                if (DomUtils.ContainsInlinesOnly(box) && !ContainsInlinesOnlyDeep(box))
                {
                    var tempRightBox = CorrectBlockInsideInlineImp(box);
                    while (tempRightBox != null)
                    {
                        // loop on the created temp right box for the fixed box until no more need (optimization remove recursion)
                        CssBox newTempRightBox = null;
                        if (DomUtils.ContainsInlinesOnly(tempRightBox) && !ContainsInlinesOnlyDeep(tempRightBox))
                            newTempRightBox = CorrectBlockInsideInlineImp(tempRightBox);

                        tempRightBox.ParentBox.SetAllBoxes(tempRightBox);
                        tempRightBox.ParentBox = null;
                        tempRightBox = newTempRightBox;
                    }
                }

                if (!DomUtils.ContainsInlinesOnly(box))
                {
                    foreach (var childBox in box.Boxes)
                    {
                        CorrectBlockInsideInline(childBox);
                    }
                }
            }
            catch (Exception ex)
            {
                box.HtmlContainer.ReportError(HtmlRenderErrorType.HtmlParsing, "Failed in block inside inline box correction", ex);
            }
        }

        /// <summary>
        /// Rearrange the DOM of the box to have block box with boxes before the inner block box and after.
        /// </summary>
        /// <param name="box">the box that has the problem</param>
        private static CssBox CorrectBlockInsideInlineImp(CssBox box)
        {
            if (box.Display == CssConstants.Inline)
                box.Display = CssConstants.Block;

            if (box.Boxes.Count > 1 || box.Boxes[0].Boxes.Count > 1)
            {
                var leftBlock = CssBox.CreateBlock(box);

                while (ContainsInlinesOnlyDeep(box.Boxes[0]))
                    box.Boxes[0].ParentBox = leftBlock;
                leftBlock.SetBeforeBox(box.Boxes[0]);

                var splitBox = box.Boxes[1];
                splitBox.ParentBox = null;

                CorrectBlockSplitBadBox(box, splitBox, leftBlock);

                // remove block that did not get any inner elements
                if (leftBlock.Boxes.Count < 1)
                    leftBlock.ParentBox = null;

                int minBoxes = leftBlock.ParentBox != null ? 2 : 1;
                if (box.Boxes.Count > minBoxes)
                {
                    // create temp box to handle the tail elements and then get them back so no deep hierarchy is created
                    var tempRightBox = CssBox.CreateBox(box, null, box.Boxes[minBoxes]);
                    while (box.Boxes.Count > minBoxes + 1)
                        box.Boxes[minBoxes + 1].ParentBox = tempRightBox;

                    return tempRightBox;
                }
            }
            else if (box.Boxes[0].Display == CssConstants.Inline)
            {
                box.Boxes[0].Display = CssConstants.Block;
            }

            return null;
        }

        /// <summary>
        /// Split bad box that has inline and block boxes into two parts, the left - before the block box
        /// and right - after the block box.
        /// </summary>
        /// <param name="parentBox">the parent box that has the problem</param>
        /// <param name="badBox">the box to split into different boxes</param>
        /// <param name="leftBlock">the left block box that is created for the split</param>
        private static void CorrectBlockSplitBadBox(CssBox parentBox, CssBox badBox, CssBox leftBlock)
        {
            CssBox leftbox = null;
            while (badBox.Boxes[0].IsInline && ContainsInlinesOnlyDeep(badBox.Boxes[0]))
            {
                if (leftbox == null)
                {
                    // if there is no elements in the left box there is no reason to keep it
                    leftbox = CssBox.CreateBox(leftBlock, badBox.HtmlTag);
                    leftbox.InheritStyle(badBox, true);
                }
                badBox.Boxes[0].ParentBox = leftbox;
            }

            var splitBox = badBox.Boxes[0];
            if (!ContainsInlinesOnlyDeep(splitBox))
            {
                CorrectBlockSplitBadBox(parentBox, splitBox, leftBlock);
                splitBox.ParentBox = null;
            }
            else
            {
                splitBox.ParentBox = parentBox;
            }

            if (badBox.Boxes.Count > 0)
            {
                CssBox rightBox;
                if (splitBox.ParentBox != null || parentBox.Boxes.Count < 3)
                {
                    rightBox = CssBox.CreateBox(parentBox, badBox.HtmlTag);
                    rightBox.InheritStyle(badBox, true);

                    if (parentBox.Boxes.Count > 2)
                        rightBox.SetBeforeBox(parentBox.Boxes[1]);

                    if (splitBox.ParentBox != null)
                        splitBox.SetBeforeBox(rightBox);
                }
                else
                {
                    rightBox = parentBox.Boxes[2];
                }

                rightBox.SetAllBoxes(badBox);
            }
            else if (splitBox.ParentBox != null && parentBox.Boxes.Count > 1)
            {
                splitBox.SetBeforeBox(parentBox.Boxes[1]);
                if (splitBox.HtmlTag != null && splitBox.HtmlTag.Name == "br" && (leftbox != null || leftBlock.Boxes.Count > 1))
                    splitBox.Display = CssConstants.Inline;
            }
        }

        /// <summary>
        /// Makes block boxes be among only block boxes and all inline boxes have block parent box.<br/>
        /// Inline boxes should live in a pool of Inline boxes only so they will define a single block.<br/>
        /// At the end of this process a block box will have only block siblings and inline box will have
        /// only inline siblings.
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectInlineBoxesParent(CssBox box)
        {
            if (ContainsVariantBoxes(box))
            {
                for (int i = 0; i < box.Boxes.Count; i++)
                {
                    if (box.Boxes[i].IsInline)
                    {
                        var newbox = CssBox.CreateBlock(box, null, box.Boxes[i++]);
                        while (i < box.Boxes.Count && box.Boxes[i].IsInline)
                        {
                            box.Boxes[i].ParentBox = newbox;
                        }
                    }
                }
            }

            if (!DomUtils.ContainsInlinesOnly(box))
            {
                foreach (var childBox in box.Boxes)
                {
                    CorrectInlineBoxesParent(childBox);
                }
            }
        }

        /// <summary>
        /// Check if the given box contains only inline child boxes in all subtree.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - only inline child boxes, false - otherwise</returns>
        private static bool ContainsInlinesOnlyDeep(CssBox box)
        {
            foreach (var childBox in box.Boxes)
            {
                if (!childBox.IsInline || !ContainsInlinesOnlyDeep(childBox))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the given box contains inline and block child boxes.
        /// </summary>
        /// <param name="box">the box to check</param>
        /// <returns>true - has variant child boxes, false - otherwise</returns>
        private static bool ContainsVariantBoxes(CssBox box)
        {
            bool hasBlock = false;
            bool hasInline = false;
            for (int i = 0; i < box.Boxes.Count && (!hasBlock || !hasInline); i++)
            {
                var isBlock = !box.Boxes[i].IsInline;
                hasBlock = hasBlock || isBlock;
                hasInline = hasInline || !isBlock;
            }

            return hasBlock && hasInline;
        }

        /// <summary>
        /// Corrects the missing elements in tables per https://www.w3.org/TR/CSS2/tables.html#anonymous-boxes
        /// </summary>
        /// <param name="box">the current box to correct its sub-tree</param>
        private static void CorrectAnonymousTables(CssBox box)
        {
            // 1. Remove irrelevant boxes
            CorrectAnonymousTablesRemoveIrrelevantBoxes(box);

            foreach (var childBox in box.Boxes.ToArray())
            {
                CorrectAnonymousTablesRemoveIrrelevantBoxes(childBox);
            }

            // 2. Generate missing child wrappers
            CorrectAnonymousTablesGenerateMissingChildWrappers(box);

            foreach (var childBox in box.Boxes.ToArray())
            {
                CorrectAnonymousTablesGenerateMissingChildWrappers(childBox);
            }

            // 3. Generate missing parents
            CorrectAnonymousTablesGenerateMissingParents(box);

            foreach (var childBox in box.Boxes.ToArray())
            {
                CorrectAnonymousTablesGenerateMissingParents(childBox);
            }

            foreach (var childBox in box.Boxes.ToArray())
            {
                CorrectAnonymousTables(childBox);
            }
        }

        private static void CorrectAnonymousTablesRemoveIrrelevantBoxes(CssBox box)
        {
            // 1.1 All child boxes of a 'table-column' parent are treated as if they had 'display: none'
            if (box.Display == CssConstants.TableColumn)
            {
                foreach (var childBox in box.Boxes)
                {
                    childBox.Display = CssConstants.None;
                }
            }

            // 1.2 If a child of a 'table-column-group' parent is not a 'table-column' box, it is treated as if it had 'display: none'.
            if (box.ParentBox != null && box.ParentBox.Display == CssConstants.TableColumnGroup && box.Display != CssConstants.TableColumn)
            {
                box.Display = CssConstants.None;
            }
        }

        private static void CorrectAnonymousTablesGenerateMissingChildWrappers(CssBox box)
        {
            // 2.1 If a child of a 'table'/'inline-table' box is not a proper table child, generate an anonymous 'table-row' box around it
            // and all consecutive siblings that are not proper table children.
            if (box.ParentBox != null && box.ParentBox.Display == CssConstants.Table)
            {
                if (!DomUtils.IsProperTableChild(box))
                {
                    var tableRowBox = new CssBox(box.ParentBox, null);
                    tableRowBox.Display = CssConstants.TableRow;
                    box.ParentBox = tableRowBox;
                }
            }

            // 2.2 If a child of a row group box is not a 'table-row' box, generate an anonymous 'table-row' box around it
            // and all consecutive siblings that are not 'table-row' boxes.
            if (box.ParentBox != null && box.ParentBox.IsTableRowGroupBox)
            {
                if (box.Display != CssConstants.TableRow)
                {
                    var tableRowBox = new CssBox(box.ParentBox, null);
                    tableRowBox.Display = CssConstants.TableRow;
                    box.ParentBox = tableRowBox;
                }
            }

            // 2.3 If a child of a 'table-row' box is not a 'table-cell', generate an anonymous 'table-cell' box around it
            // and all consecutive siblings that are not 'table-cell' boxes.
            if (box.ParentBox != null && box.ParentBox.Display == CssConstants.TableRow)
            {
                if (box.Display != CssConstants.TableCell)
                {
                    var followingMatchingSiblings = DomUtils.GetFollowingSiblings(box, sibling => sibling.Display == CssConstants.TableCell, true).ToList();

                    var tableCellBox = new CssBox(box.ParentBox, null);
                    tableCellBox.Display = CssConstants.TableCell;
                    box.ParentBox = tableCellBox;

                    followingMatchingSiblings.ForEach(sib => sib.ParentBox = tableCellBox);
                }
            }
        }

        private static void CorrectAnonymousTablesGenerateMissingParents(CssBox box)
        {
            // 3.1 For each 'table-cell' box in a sequence of consecutive internal table and 'table-caption' siblings, if its parent
            // is not a 'table-row' then generate an anonymous 'table-row' box around it and all consecutive 'table-cell' siblings.
            if (box.Display == CssConstants.TableCell)
            {
                if (box.ParentBox == null || box.ParentBox.Display != CssConstants.TableRow)
                {
                    var followingMatchingSiblings = DomUtils.GetFollowingSiblings(box, sibling => sibling.Display == CssConstants.TableCell, true).ToList();

                    var tableRowBox = new CssBox(box.ParentBox, null);
                    tableRowBox.Display = CssConstants.TableRow;
                    box.ParentBox = tableRowBox;

                    followingMatchingSiblings.ForEach(sib => sib.ParentBox = tableRowBox);
                }
            }

            // 3.2 For each proper table child in a sequence of consecutive proper table children, if it is misparented then generate
            // an anonymous 'table'/'inline-table' box around it and all consecutive proper-table-child siblings.
            if (DomUtils.IsProperTableChild(box))
            {
                var isMissingParent = box.ParentBox == null;
                var isParentNotTable = box.ParentBox == null || box.ParentBox.Display != CssConstants.Table;
                var isParentNotInlineTable = box.ParentBox == null || box.ParentBox.Display != CssConstants.InlineTable;

                var isMisparented = isMissingParent && isParentNotTable && isParentNotInlineTable;

                if (isMisparented)
                {
                    var parentDisplay = (box.ParentBox == null || box.ParentBox.IsBlock) ? CssConstants.Table : CssConstants.InlineTable;

                    var followingMatchingSiblings = DomUtils.GetFollowingSiblings(box, DomUtils.IsProperTableChild, true).ToList();

                    var tableBox = new CssBox(box.ParentBox, null);
                    tableBox.Display = parentDisplay;
                    box.ParentBox = tableBox;

                    followingMatchingSiblings.ForEach(sib => sib.ParentBox = tableBox);
                }
            }
        }

        #endregion
    }
}
