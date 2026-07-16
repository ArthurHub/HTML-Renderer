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

namespace TheArtOfDev.HtmlRenderer.Core
{
    internal static class CssDefaults
    {
        /// <summary>
        /// CSS Specification's Default Style Sheet for HTML 4
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/CSS21/sample.html
        /// </remarks>
        public const string DefaultStyleSheet = @"
        html, address,
        blockquote,
        body, dd, div,
        dl, dt, fieldset, form,
        frame, frameset,
        h1, h2, h3, h4,
        h5, h6, noframes,
        ol, p, ul, center,
        dir, menu, pre   { display: block }
        li              { display: list-item }
        head            { display: none }
        table           { display: table }
        tr              { display: table-row }
        thead           { display: table-header-group }
        tbody           { display: table-row-group }
        tfoot           { display: table-footer-group }
        col             { display: table-column }
        colgroup        { display: table-column-group }
        td, th          { display: table-cell }
        caption         { display: table-caption }
        th              { font-weight: bolder; text-align: center }
        caption         { text-align: center }
        body            { margin: 8px }
        h1              { font-size: 2em; margin: .67em 0 }
        h2              { font-size: 1.5em; margin: .75em 0 }
        h3              { font-size: 1.17em; margin: .83em 0 }
        h4, p,
        blockquote, ul,
        fieldset, form,
        ol, dl, dir,
        menu            { margin: 1.12em 0 }
        h5              { font-size: .83em; margin: 1.5em 0 }
        h6              { font-size: .75em; margin: 1.67em 0 }
        h1, h2, h3, h4,
        h5, h6, b,
        strong          { font-weight: bolder; }
        blockquote      { margin-left: 40px; margin-right: 40px }
        i, cite, em,
        var, address    { font-style: italic }
        pre, tt, code,
        kbd, samp       { font-family: monospace }
        pre             { white-space: pre }
        button, textarea,
        input, select   { display: inline-block }
        big             { font-size: 1.17em }
        small, sub, sup { font-size: .83em }
        sub             { vertical-align: sub }
        sup             { vertical-align: super }
        table           { border-spacing: 2px; }
        thead, tbody,
        tfoot, tr       { vertical-align: middle }
        td, th          { vertical-align: inherit }
        s, strike, del  { text-decoration: line-through }
        hr              { border: 1px inset; }
        ol, ul, dir,
        menu, dd        { margin-left: 40px }
        ol              { list-style-type: decimal }
        ol ul, ul ol,
        ul ul, ol ol    { margin-top: 0; margin-bottom: 0 }
        ol ul, ul ul   { list-style-type: circle }
        ul ul ul, 
        ol ul ul, 
        ul ol ul        { list-style-type: square }
        u, ins          { text-decoration: underline }
        br:before       { content: ""\A"" }
        :before, :after { white-space: pre-line }
        center          { text-align: center }
        :link, :visited { text-decoration: underline }
        :focus          { outline: thin dotted invert }

        /* Begin bidirectionality settings (do not change) */
        BDO[DIR=""ltr""]  { direction: ltr; unicode-bidi: bidi-override }
        BDO[DIR=""rtl""]  { direction: rtl; unicode-bidi: bidi-override }

        *[DIR=""ltr""]    { direction: ltr; unicode-bidi: embed }
        *[DIR=""rtl""]    { direction: rtl; unicode-bidi: embed }

        @media print {
          h1            { page-break-before: always }
          h1, h2, h3,
          h4, h5, h6    { page-break-after: avoid }
          ul, ol, dl    { page-break-before: avoid }
        }

        /* Not in the specification but necessary */
        a               { color: #0055BB; text-decoration:underline }
        table           { border-color:#dfdfdf; }
        td, th          { border-color:#dfdfdf; overflow: hidden; }
        style, title,
        script, link,
        meta, area,
        base, param     { display:none }
        hr              { border-top-color: #9A9A9A; border-left-color: #9A9A9A; border-bottom-color: #EEEEEE; border-right-color: #EEEEEE; }
        pre             { font-size: 10pt; margin-top: 15px; }
        
        /*This is the background of the HtmlToolTip*/
        .htmltooltip {
            border:solid 1px #767676;
            background-color:white;
            background-gradient:#E4E5F0;
            padding: 8px; 
            Font: 9pt Tahoma;
        }";

        /// <summary>
        /// The initial value of every property this engine recognizes (<see cref="Utils.CssUtils"/>'s exact
        /// whitelist), used to resolve the <c>initial</c>/<c>unset</c>/<c>revert</c>/<c>revert-layer</c>
        /// CSS-wide keywords in the six-phase cascade (<see cref="Parse.DomParser"/>). Deliberately matches
        /// this engine's own existing field defaults (<see cref="Dom.CssBoxProperties"/>'s private field
        /// initializers) rather than the CSS spec's initial values where the two differ (e.g. this engine's
        /// border-color default is "black", not the spec's "currentcolor"; text-align/text-decoration/
        /// list-style-image default to an empty string here, not "left"/"none"/"none") - so resolving
        /// "initial" produces exactly what an unstyled box already looks like today, not a new value.
        /// </summary>
        internal static readonly System.Collections.Generic.Dictionary<string, string> InitialValues =
            new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "background-color", "transparent" },
            { "background-gradient", "none" },
            { "background-gradient-angle", "90" },
            { "background-image", "none" },
            { "background-position", "0% 0%" },
            { "background-repeat", "repeat" },
            { "border-top-width", "medium" },
            { "border-right-width", "medium" },
            { "border-bottom-width", "medium" },
            { "border-left-width", "medium" },
            { "border-top-color", "black" },
            { "border-right-color", "black" },
            { "border-bottom-color", "black" },
            { "border-left-color", "black" },
            { "border-top-style", "none" },
            { "border-right-style", "none" },
            { "border-bottom-style", "none" },
            { "border-left-style", "none" },
            { "border-spacing", "0" },
            { "border-collapse", "separate" },
            { "color", "black" },
            { "content", "normal" },
            { "corner-nw-radius", "0" },
            { "corner-ne-radius", "0" },
            { "corner-se-radius", "0" },
            { "corner-sw-radius", "0" },
            { "corner-radius", "0" },
            { "empty-cells", "show" },
            { "direction", "ltr" },
            { "display", "inline" },
            { "font-size", "medium" },
            { "font-style", "normal" },
            { "font-variant", "normal" },
            { "font-weight", "normal" },
            { "float", "none" },
            { "height", "auto" },
            { "margin-bottom", "0" },
            { "margin-left", "0" },
            { "margin-right", "0" },
            { "margin-top", "0" },
            { "left", "auto" },
            { "line-height", "normal" },
            { "list-style-type", "disc" },
            { "list-style-image", "" },
            { "list-style-position", "outside" },
            { "list-style", "" },
            { "overflow", "visible" },
            { "padding-left", "0" },
            { "padding-bottom", "0" },
            { "padding-right", "0" },
            { "padding-top", "0" },
            { "page-break-inside", "auto" },
            { "text-align", "" },
            { "text-decoration", "" },
            { "text-indent", "0" },
            { "top", "auto" },
            { "position", "static" },
            { "vertical-align", "baseline" },
            { "width", "auto" },
            { "max-width", "none" },
            { "word-spacing", "normal" },
            { "word-break", "normal" },
            { "visibility", "visible" },
            { "white-space", "normal" },
        };

        /// <summary>
        /// Properties inherited from parent to child during cascade - kept exactly in sync with the set
        /// <see cref="Dom.CssBoxProperties.InheritStyle"/> already (independently) copies, since this is
        /// used only to decide whether the <c>unset</c> keyword should behave like <c>inherit</c> or
        /// <c>initial</c> for a given property, and the two must agree for "unset" to make sense.
        /// </summary>
        internal static readonly System.Collections.Generic.HashSet<string> InheritedProperties =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "border-spacing", "border-collapse",
            "color",
            "empty-cells",
            "white-space",
            "visibility",
            "text-indent", "text-align",
            "font-family", "font-size", "font-style", "font-variant", "font-weight",
            "list-style-image", "list-style-position", "list-style-type", "list-style",
            "line-height",
            "word-break",
            "direction",
        };

        /// <summary>
        /// Returns the initial value for the given property name, or null if this engine doesn't
        /// recognize the property at all.
        /// </summary>
        internal static string GetInitialValue(string propertyName)
        {
            string value;
            return InitialValues.TryGetValue(propertyName, out value) ? value : null;
        }
    }
}