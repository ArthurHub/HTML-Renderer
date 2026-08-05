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
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
    /// <summary>
    /// Parser to parse CSS stylesheet source string into CSS objects.<br/>
    /// Derived from the same CSS-OM lineage as the vendored engine, adapted to stay fully synchronous
    /// (HTML-Renderer's whole SetHtml/DomParser pipeline is synchronous, and <see cref="StylesheetLoadHandler"/>
    /// already does its own blocking I/O) - the recursive @import-walk logic is kept, just without
    /// any Task/await plumbing.
    /// </summary>
    internal sealed class CssParser
    {
        #region Fields and Consts

        /// <summary>
        /// The platform adapter.
        /// </summary>
        private readonly RAdapter _adapter;

        /// <summary>
        /// Utility for value parsing.
        /// </summary>
        private readonly CssValueParser _valueParser;

        /// <summary>
        /// Used to resolve @import references (loads via the same synchronous, event-overridable
        /// path as &lt;link rel=stylesheet&gt;). Null when this parser is used outside of a live
        /// document parse (e.g. the public <see cref="CssData.Parse"/> convenience API) - in that
        /// case @import rules are silently skipped, since there is no container to resolve them through.
        /// </summary>
        private readonly HtmlContainerInt _htmlContainer;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public CssParser(RAdapter adapter)
            : this(adapter, null)
        {
        }

        /// <summary>
        /// Init.
        /// </summary>
        public CssParser(RAdapter adapter, HtmlContainerInt htmlContainer)
        {
            ArgChecker.AssertArgNotNull(adapter, "global");

            _valueParser = new CssValueParser(adapter);
            _adapter = adapter;
            _htmlContainer = htmlContainer;
        }

        /// <summary>
        /// Parse the given stylesheet source to a <see cref="CssData"/>.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed stylesheet is combined with a
        /// clone of the default (UA) css data (as defined by W3) - a clone so the adapter's cached
        /// default data is never mutated by this call. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="stylesheet">raw css stylesheet to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the CSS data with parsed CSS objects (never null)</returns>
        public CssData ParseStyleSheet(string stylesheet, bool combineWithDefault)
        {
            var cssData = combineWithDefault ? _adapter.DefaultCssData.Clone() : new CssData();
            if (!string.IsNullOrEmpty(stylesheet))
            {
                ParseStyleSheet(cssData, stylesheet);
            }
            return cssData;
        }

        /// <summary>
        /// Parse raw CSS text into a <see cref="Stylesheet"/> object model (tokenizer + selector/value
        /// grammar), with no cascade/box concerns at all - the thinnest possible entry point into the
        /// vendored parser.
        /// </summary>
        public static Stylesheet ParseStyleSheetText(string stylesheet)
        {
            var parser = new StylesheetParser();
            return parser.Parse(stylesheet);
        }

        /// <summary>
        /// Parse the given stylesheet source and add its rules into the given css data (author origin),
        /// recursively resolving @import rules (guarded against circular imports).
        /// </summary>
        /// <param name="cssData">the CSS data to fill with parsed CSS objects</param>
        /// <param name="stylesheet">raw css stylesheet to parse</param>
        /// <param name="baseUri">
        /// The resolved absolute URI this stylesheet was loaded from (e.g. a &lt;link&gt; href), so that
        /// relative @import references inside it resolve against its own location rather than the
        /// document's base. Null for inline &lt;style&gt; content or caller-supplied CSS text.
        /// </param>
        public void ParseStyleSheet(CssData cssData, string stylesheet, Uri baseUri = null)
        {
            if (!string.IsNullOrEmpty(stylesheet))
            {
                ParseStyle(cssData, stylesheet, baseUri, new HashSet<string>());
            }
        }

        /// <summary>
        /// Parse a single inline style="" attribute value into its one resulting style rule, by
        /// wrapping it in a synthetic universal-selector rule and parsing it through the same
        /// tokenizer/grammar as a full stylesheet - so inline style values get exactly the same value
        /// parsing (shorthands, calc(), var(), etc.) as anything from a &lt;style&gt; block.
        /// </summary>
        /// <param name="declarations">the raw declaration list, e.g. "color: red; font-weight: bold"</param>
        /// <returns>the single parsed style rule, or null if the declaration block was empty/invalid</returns>
        public IStyleRule ParseInlineStyle(string declarations)
        {
            var stylesheet = ParseStyleSheetText("* { " + declarations + " }");
            return stylesheet.StyleRules.FirstOrDefault();
        }

        /// <summary>
        /// Parses a color value in CSS style; e.g. #ff0000, red, rgb(255,0,0), rgb(100%, 0, 0)
        /// </summary>
        /// <param name="colorStr">color string value to parse</param>
        /// <returns>color value</returns>
        public RColor ParseColor(string colorStr)
        {
            return _valueParser.GetActualColor(colorStr);
        }

        /// <summary>
        /// Checks if the given color value can be resolved to an actual color.
        /// </summary>
        public bool IsColorValid(string colorValue)
        {
            return _valueParser.IsColorValid(colorValue);
        }

        /// <summary>
        /// Parses a "background-image" value into a url() reference or a linear-gradient() - see
        /// <see cref="CssValueParser.ParseImage"/>.
        /// </summary>
        public CssImage ParseBackgroundImage(string value)
        {
            return _valueParser.ParseImage(value);
        }

        /// <summary>
        /// Parse a complex font family css property to check if it contains multiple fonts and if the font exists.<br/>
        /// returns the font family name to use or 'inherit' if failed.
        /// </summary>
        /// <param name="value">the font-family value to parse</param>
        /// <returns>parsed font-family value</returns>
        public string ParseFontFamily(string value)
        {
            int start = 0;

            while (start < value.Length)
            {
                while (start < value.Length && (char.IsWhiteSpace(value[start]) || value[start] == ',' || value[start] == '\'' || value[start] == '"'))
                    start++;
                var end = value.IndexOf(',', start);
                if (end < 0)
                    end = value.Length;
                var adjEnd = end - 1;
                while (adjEnd > start && (char.IsWhiteSpace(value[adjEnd]) || value[adjEnd] == '\'' || value[adjEnd] == '"'))
                    adjEnd--;

                if (adjEnd >= start)
                {
                    var font = value.Substring(start, adjEnd - start + 1);
                    if (_adapter.IsFontExists(font))
                        return font;
                }

                start = end + 1;
            }

            return CssConstants.Inherit;
        }

        #region Private methods

        private void ParseStyle(CssData data, string stylesheetText, Uri baseUri, HashSet<string> visitedImportUris)
        {
            var stylesheet = ParseStyleSheetText(stylesheetText);
            stylesheet.BaseUri = baseUri;

            // Guards against circular @import chains (A imports B imports A) recursing forever; also
            // stops a stylesheet from importing itself directly.
            if (baseUri != null)
            {
                visitedImportUris.Add(baseUri.AbsoluteUri);
            }

            var hasReachedNonImportRules = false;

            foreach (var rule in stylesheet.Children)
            {
                var importRule = rule as IImportRule;
                if (importRule != null && !hasReachedNonImportRules)
                {
                    if (importRule.Href == null || _htmlContainer == null) continue;

                    // Relative references inside an already-loaded stylesheet resolve against that
                    // stylesheet's own location, not the document's base.
                    var resolvedHref = ResolveImportHref(importRule.Href, baseUri);

                    string importedContent;
                    CssData importedCssData;
                    StylesheetLoadHandler.LoadStylesheet(_htmlContainer, resolvedHref, null, out importedContent, out importedCssData);

                    if (!string.IsNullOrEmpty(importedContent))
                    {
                        var importedUri = CommonUtils.TryGetUri(resolvedHref);
                        if (importedUri == null || visitedImportUris.Add(importedUri.AbsoluteUri))
                        {
                            ParseStyle(data, importedContent, importedUri, visitedImportUris);
                        }
                    }
                    else if (importedCssData != null)
                    {
                        data.Combine(importedCssData);
                    }
                }
                else if (!(rule is LayerStatementRule))
                {
                    // A bare `@layer a, b;` statement is allowed before @import (CSS Cascade 5 §6.4.1), so
                    // it must not close the @import prologue - otherwise every following @import is skipped.
                    hasReachedNonImportRules = true;
                }
            }

            data.Stylesheets.Add(stylesheet);
        }

        /// <summary>
        /// Resolves an @import href against the importing stylesheet's own base URI (if any), so
        /// nested @import chains resolve relative references correctly instead of against the
        /// document's base - matching browser behavior for fetched CSS.
        /// </summary>
        private static string ResolveImportHref(string href, Uri baseUri)
        {
            if (baseUri != null)
            {
                Uri combined;
                if (Uri.TryCreate(baseUri, href, out combined))
                    return combined.AbsoluteUri;
            }
            return href;
        }

        #endregion
    }
}
