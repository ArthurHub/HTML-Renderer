using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class SheetTests
{
    [TestMethod]
    public void CssSheetBareSemicolonInsideBlockIsDiscarded()
    {
        // CSS Syntax 3 §5.5.5 discards the ';'. CSS 2.1 §4.2 would drop ".b" with the malformed statement.
        var sheet = ParseStyleSheet(@"
            @media screen {
                .a { color: red; }
                ;
                .b { color: blue; }
            }");
        Assert.AreEqual(1, sheet.Rules.Length);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual(2, media.Rules.Length);
        Assert.AreEqual(".a", ((StyleRule)media.Rules[0]).SelectorText);
        Assert.AreEqual(".b", ((StyleRule)media.Rules[1]).SelectorText);
    }

    [TestMethod]
    public void CssSheetBareSemicolonInsideBlockDoesNotSwallowFollowingRule()
    {
        var sheet = ParseStyleSheet(@"
            @media screen { .a { color: red; } ; }
            @media print { .b { color: blue; } }");
        Assert.AreEqual(2, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        Assert.AreEqual(RuleType.Media, sheet.Rules[1].Type);
    }

    [TestMethod]
    public void CssSheetMultipleBareSemicolonsInsideBlockAreDiscarded()
    {
        var sheet = ParseStyleSheet(@"@media screen { ;;.a { color: red; };; }");
        Assert.AreEqual(1, sheet.Rules.Length);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual(1, media.Rules.Length);
        Assert.AreEqual(".a", ((StyleRule)media.Rules[0]).SelectorText);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetBareTopLevelSemicolonInvalidatesTheFollowingRule()
    {
        // Unlike inside a block, a top-level ';' becomes part of the next rule's prelude.
        // Acid2 relies on this to drop ".parser { height: 3em; }".
        var sheet = ParseStyleSheet(@".a { color: red; } ; .b { color: blue; }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(".a", ((StyleRule)sheet.Rules[0]).SelectorText);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetOnEofDuringRuleWithoutSemicolon()
    {
        var sheet = ParseStyleSheet(@"
h1 {
 color: red;
 font-weight: bold");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var h1 = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("h1", h1.SelectorText);
        Assert.AreEqual("rgb(255, 0, 0)", h1.Style.Color);
        Assert.AreEqual("bold", h1.Style.FontWeight);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#comments")]
    public void CssSheet1WithDoubleMarkedCommentFromIssue93()
    {
        var sheet = ParseStyleSheet(@"
            /**special css**/
            .dis-none { display: none;}
            .dis { display: block; }
            /*common css*/
            .dis2 { display: block; }
            ");
        Assert.AreEqual(3, sheet.Rules.Length);
        Assert.AreEqual(".dis-none { display: none }", sheet.Rules[0].Text);
        Assert.AreEqual(".dis { display: block }", sheet.Rules[1].Text);
        Assert.AreEqual(".dis2 { display: block }", sheet.Rules[2].Text);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#comments")]
    public void CssSheet2WithDoubleMarkedCommentFromIssue93()
    {
        var sheet = ParseStyleSheet(@"
            /**special css**/
            .dis-none { display: none;}
            .dis { display: block; }
            ");
        Assert.AreEqual(2, sheet.Rules.Length);
        Assert.AreEqual(".dis-none { display: none }", sheet.Rules[0].Text);
        Assert.AreEqual(".dis { display: block }", sheet.Rules[1].Text);
    }

    [TestMethod]
    public void CssSheetSerializeListStyleNone()
    {
        const string cssSrc = ".T1 {list-style:NONE}";
        const string expected = ".T1 { list-style: none }";
        var stylesheet = ParseStyleSheet(cssSrc);
        var text = stylesheet.ToCss();
        Assert.AreEqual(expected, text);
    }

    [TestMethod]
    public void CssSheetSerializeBorder1pxOutset()
    {
        const string cssSrc = ".T2 { border:1px  outset }";
        const string expected = ".T2 { border: 1px outset }";
        var stylesheet = ParseStyleSheet(cssSrc);
        var text = stylesheet.ToCss();
        Assert.AreEqual(expected, text);
    }

    [TestMethod]
    public void CssSheetSerializeBorder1pxSolidWithColor()
    {
        const string cssSrc = "#rule1 { border: 1px solid #BBCCEB; border-top: none }";
        const string expected = "#rule1 { border-right: 1px solid rgb(187, 204, 235); border-bottom: 1px solid rgb(187, 204, 235); border-left: 1px solid rgb(187, 204, 235); border-top: none }";
        var stylesheet = ParseStyleSheet(cssSrc);
        var text = stylesheet.ToCss();
        Assert.AreEqual(expected, text);
    }

    [TestMethod]
    public void CssSheetSerializeBackgroundWithUrlPositionRepeatX()
    {
        const string cssSrc = "#rule2 { background:url(/_static/img/bx_tile.gif) top left repeat-x; }";
        const string expected = "#rule2 { background: url(\"/_static/img/bx_tile.gif\") top left repeat-x }";
        var stylesheet = ParseStyleSheet(cssSrc);
        var text = stylesheet.ToCss();
        Assert.AreEqual(expected, text);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#vendor-keywords")]
    public void CssSheetIgnoreVendorPrefixes()
    {
        var css = @".something {
  -o-border-radius: 5px;
  -webkit-border-radius: 5px;
  border-radius: 5px;
  display: -webkit-box;
  display: -webkit-flex;
  display: -ms-flexbox;
  display: flex;
  background: -webkit-linear-gradient(red, green);
  background: linear-gradient(red, green);
}";
        var stylesheet = ParseStyleSheet(css);
        Assert.AreEqual(1, stylesheet.Rules.Length);
        var style = stylesheet.Rules[0] as StyleRule;
        Assert.IsNotNull(style);
        Assert.AreEqual(13, style.Style.Length);
    }

    [TestMethod]
    public void CssSheetSimpleStyleRuleStringification()
    {
        var css = @"html { font-family: sans-serif }";
        var stylesheet = ParseStyleSheet(css);
        Assert.AreEqual(1, stylesheet.Rules.Length);
        var rule = stylesheet.Rules[0];
        Assert.IsInstanceOfType<StyleRule>(rule);
        Assert.AreEqual(css, rule.Text);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetCloseStringsEndOfLine()
    {
        var sheet = ParseStyleSheet(@"p {
        color: green;
        font-family: 'Courier New Times
        color: red;
        color: green;
      }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetOnEofDuringRuleWithinString()
    {
        var sheet = ParseStyleSheet(@"
#something {
 content: 'hi there");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var id = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("#something", id.SelectorText);
        Assert.AreEqual("\"hi there\"", id.Style.Content);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetOnEofDuringAtMediaRuleWithinString()
    {
        var sheet = ParseStyleSheet(@"  @media screen {
    p:before { content: 'Hello");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("screen", media.Media.MediaText);
        Assert.AreEqual(1, media.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(media.Rules[0]);
        var p = (StyleRule)media.Rules[0];
        Assert.AreEqual("p::before", p.SelectorText);
        Assert.AreEqual("\"Hello\"", p.Style.Content);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetIgnoreUnknownProperty()
    {
        var sheet = ParseStyleSheet(@"h1 { color: red; rotation: 70minutes }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var h1 = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("h1", h1.SelectorText);
        Assert.AreEqual(1, h1.Style.Length);
        Assert.AreEqual("color", h1.Style[0]);
        Assert.AreEqual("rgb(255, 0, 0)", h1.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetInvalidStatementRulesetUnexpectedAtKeyword()
    {
        var sheet = ParseStyleSheet(@"p @here {color: red}");
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetInvalidStatementAtRuleUnexpectedAtKeyword()
    {
        var sheet = ParseStyleSheet(@"@foo @bar;");
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetInvalidStatementRulesetUnexpectedRightBrace()
    {
        var sheet = ParseStyleSheet(@"}} {{ - }}");
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetInvalidStatementRulesetUnexpectedRightBraceWithValidQualifiedRule()
    {
        var sheet = ParseStyleSheet(@"}} {{ - }}
#hi { color: green; }");
        Assert.AreEqual(1, sheet.Rules.Length);
        var style = sheet.Rules[0] as StyleRule;
        Assert.IsNotNull(style);
        Assert.AreEqual("#hi", style.SelectorText);
        Assert.AreEqual(1, style.Style.Length);
        Assert.AreEqual("rgb(0, 128, 0)", style.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetInvalidStatementRulesetUnexpectedRightParenthesis()
    {
        var sheet = ParseStyleSheet(@") ( {} ) p {color: red }");
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetInvalidStatementRulesetUnexpectedRightParenthesisWithValidQualifiedRule()
    {
        var sheet = ParseStyleSheet(@") {} p {color: green }");
        Assert.AreEqual(1, sheet.Rules.Length);
        var style = sheet.Rules[0] as StyleRule;
        Assert.IsNotNull(style);
        Assert.AreEqual("p", style.SelectorText);
        Assert.AreEqual(1, style.Style.Length);
        Assert.AreEqual("rgb(0, 128, 0)", style.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetIgnoreUnknownAtRule()
    {
        var sheet = ParseStyleSheet(@"@three-dee {
  @background-lighting {
    azimuth: 30deg;
    elevation: 190deg;
  }
  h1 { color: red }
}
h1 { color: blue }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var h1 = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("h1", h1.SelectorText);
        Assert.AreEqual(1, h1.Style.Length);
        Assert.AreEqual("color", h1.Style[0]);
        Assert.AreEqual("rgb(0, 0, 255)", h1.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetKeepValidValueFloat()
    {
        var sheet = ParseStyleSheet(@"img { float: left }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var img = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("img", img.SelectorText);
        Assert.AreEqual(1, img.Style.Length);
        Assert.AreEqual("float", img.Style[0]);
        Assert.AreEqual("left", img.Style.Float);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetIgnoreInvalidValueFloat()
    {
        var sheet = ParseStyleSheet(@"img { float: left here }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var img = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("img", img.SelectorText);
        Assert.AreEqual(0, img.Style.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetIgnoreInvalidValueBackground()
    {
        var sheet = ParseStyleSheet(@"img { background: ""red"" }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var img = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("img", img.SelectorText);
        Assert.AreEqual(0, img.Style.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetIgnoreInvalidValueBorderWidth()
    {
        var sheet = ParseStyleSheet(@"img { border-width: 3 }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var img = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("img", img.SelectorText);
        Assert.AreEqual(0, img.Style.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetWellformedDeclaration()
    {
        var sheet = ParseStyleSheet(@"p { color:green; }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetMalformedDeclarationMissingColon()
    {
        var sheet = ParseStyleSheet(@"p { color:green; color }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetMalformedDeclarationMissingColonWithRecovery()
    {
        var sheet = ParseStyleSheet(@"p { color:red;   color; color:green }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetMalformedDeclarationMissingValue()
    {
        var sheet = ParseStyleSheet(@"p { color:green; color: }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetMalformedDeclarationUnexpectedTokens()
    {
        var sheet = ParseStyleSheet(@"p { color:green; color{;color:maroon} }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#parsing-errors")]
    public void CssSheetMalformedDeclarationUnexpectedTokensWithRecovery()
    {
        var sheet = ParseStyleSheet(@"p { color:red;   color{;color:maroon}; color:green }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var p = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("p", p.SelectorText);
        Assert.AreEqual(1, p.Style.Length);
        Assert.AreEqual("color", p.Style[0]);
        Assert.AreEqual("rgb(0, 128, 0)", p.Style.Color);
    }

    [TestMethod]
    public void CssCreateValueListConformal()
    {
        var valueString = "24px 12px 6px";
        var list = ParseValue(valueString);
        Assert.AreEqual(5, list.Count);
        Assert.AreEqual("24px", list[0].ToValue());
        Assert.AreEqual(" ", list[1].ToValue());
        Assert.AreEqual("12px", list[2].ToValue());
        Assert.AreEqual(" ", list[3].ToValue());
        Assert.AreEqual("6px", list[4].ToValue());
    }

    [TestMethod]
    public void CssCreateValueListNonConformal()
    {
        var valueString = "  24px  12px 6px  13px ";
        var list = ParseValue(valueString);
        Assert.AreEqual(7, list.Count);
        Assert.AreEqual("24px", list[0].ToValue());
        Assert.AreEqual(" ", list[1].ToValue());
        Assert.AreEqual("12px", list[2].ToValue());
        Assert.AreEqual(" ", list[3].ToValue());
        Assert.AreEqual("6px", list[4].ToValue());
        Assert.AreEqual(" ", list[5].ToValue());
        Assert.AreEqual("13px", list[6].ToValue());
    }

    [TestMethod]
    public void CssCreateValueListEmpty()
    {
        var valueString = "";
        var value = ParseValue(valueString);
        Assert.IsNull(value);
    }

    [TestMethod]
    public void CssCreateValueListSpaces()
    {
        var valueString = "  ";
        var value = ParseValue(valueString);
        Assert.IsNull(value);
    }

    [TestMethod]
    public void CssCreateValueListIllegal()
    {
        var valueString = " , ";
        var list = ParseValue(valueString);
        Assert.AreEqual(1, list.Count);
    }

    [TestMethod]
    public void CssCreateMultipleValues()
    {
        var valueString = "Arial, Verdana, Helvetica, Sans-Serif";
        var list = ParseValue(valueString);
        Assert.AreEqual(10, list.Count);
        Assert.AreEqual("Arial", list[0].Data);
        Assert.AreEqual("Verdana", list[3].Data);
        Assert.AreEqual("Helvetica", list[6].Data);
        Assert.AreEqual("Sans-Serif", list[9].Data);
    }

    [TestMethod]
    public void CssCreateMultipleValueLists()
    {
        var valueString = "Arial 10pt bold, Verdana 12pt italic";
        var list = ParseValue(valueString);
        Assert.AreEqual(12, list.Count);
        Assert.AreEqual("Arial", list[0].ToValue());
        Assert.AreEqual("Verdana", list[7].ToValue());
        Assert.AreEqual("10pt", list[2].ToValue());
        Assert.AreEqual("12pt", list[9].ToValue());
        Assert.AreEqual("bold", list[4].ToValue());
        Assert.AreEqual("italic", list[11].ToValue());
    }

    [TestMethod]
    public void CssCreateMultipleValuesNonConformal()
    {
        var valueString = "  Arial  ,  Verdana  ,Helvetica,Sans-Serif   ";
        var list = ParseValue(valueString);
        Assert.AreEqual(10, list.Count);
        Assert.AreEqual("Arial", list[0].ToValue());
        Assert.AreEqual("Verdana", list[3].ToValue());
        Assert.AreEqual("Helvetica", list[6].ToValue());
        Assert.AreEqual("Sans-Serif", list[9].ToValue());
    }

    [TestMethod]
    public void CssColorBlack()
    {
        var valueString = "#000000";
        var value = ParseValue(valueString);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public void CssColorRed()
    {
        var valueString = "#FF0000";
        var value = ParseValue(valueString);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public void CssColorMixedShort()
    {
        var valueString = "#07C";
        var value = ParseValue(valueString);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public void CssColorGreenShort()
    {
        var valueString = "#00F";
        var value = ParseValue(valueString);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public void CssColorRedShort()
    {
        var valueString = "#F00";
        var value = ParseValue(valueString);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public void CssRgbaFunction()
    {
        var names = new[] { "border-top-color", "border-right-color", "border-bottom-color", "border-left-color" };
        var decls = ParseDeclarations("border-color: rgba(82, 168, 236, 0.8)");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);
            Assert.IsFalse(decl.IsImportant);
        }
    }

    [TestMethod]
    public void CssMarginAll()
    {
        var names = new[] { "margin-top", "margin-right", "margin-bottom", "margin-left" };
        var decls = ParseDeclarations("margin: 20px;");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);
            Assert.IsFalse(decl.IsImportant);
            Assert.AreEqual("20px", decl.Value);
        }
    }

    [TestMethod]
    public void CssMarginAllImportant()
    {
        var names = new[] { "margin-top", "margin-right", "margin-bottom", "margin-left" };
        var decls = ParseDeclarations("margin: 20px !important;");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);
            Assert.IsTrue(decl.IsImportant);
            Assert.AreEqual("20px", decl.Value);
        }
    }

    [TestMethod]
    public void CssMarginImportantShorhandFollowedByNotImportantLonghand()
    {
        var names = new[] { "margin-top", "margin-right", "margin-bottom", "margin-left" };
        var decls = ParseDeclarations("margin: 5px !important; margin-left: 3px;");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);
            Assert.IsTrue(decl.IsImportant);
            Assert.AreEqual("5px", decl.Value);
        }
    }

    [TestMethod]
    public void CssMarginImportantLonghandFollowedByNotImportantShorthand()
    {
        var names = new[] { "margin-left", "margin-top", "margin-right", "margin-bottom" };
        var decls = ParseDeclarations("margin-left: 5px !important; margin: 3px;");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);

            if (i == 0)
            {
                Assert.IsTrue(decl.IsImportant);
                Assert.AreEqual("5px", decl.Value);
            }
            else
            {
                Assert.IsFalse(decl.IsImportant);
                Assert.AreEqual("3px", decl.Value);
            }
        }
    }

    [TestMethod]
    public void CssMarginNotImportantShorhandFollowedByImportantLonghand()
    {
        var names = new[] { "margin-top", "margin-right", "margin-bottom", "margin-left" };
        var decls = ParseDeclarations("margin: 5px; margin-left: 3px !important;");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);

            if (i < 3)
            {
                Assert.IsFalse(decl.IsImportant);
                Assert.AreEqual("5px", decl.Value);
            }
            else
            {
                Assert.IsTrue(decl.IsImportant);
                Assert.AreEqual("3px", decl.Value);
            }
        }
    }

    [TestMethod]
    public void CssMarginNotImportantLonghandFollowedByImportantShorthand()
    {
        var names = new[] { "margin-top", "margin-right", "margin-bottom", "margin-left" };
        var decls = ParseDeclarations("margin-left: 5px; margin: 3px !important;");
        Assert.IsNotNull(decls);
        Assert.AreEqual(4, decls.Length);

        for (int i = 0; i < decls.Length; i++)
        {
            var propertyName = decls[i];
            var decl = decls.GetProperty(propertyName);
            Assert.AreEqual(names[i], decl.Name);
            Assert.AreEqual(propertyName, decl.Name);
            Assert.IsTrue(decl.IsImportant);
            Assert.AreEqual("3px", decl.Value);
        }
    }

    [TestMethod]
    public void CssSeveralFontFamily()
    {
        var prop = ParseDeclaration("font-family: \"Helvetica Neue\", Helvetica, Arial, sans-serif");
        Assert.AreEqual("font-family", prop.Name);
        Assert.IsFalse(prop.IsImportant);
        Assert.AreEqual("\"Helvetica Neue\", Helvetica, Arial, sans-serif", prop.Value);
    }

    [TestMethod]
    [Ignore("font-variant-caps, -ligatures, -numeric and -east-asian aren't properties here, so font expands to 7 longhands, not 10")]
    public void CssFontWithSlashAndContent()
    {
        var decl = ParseDeclarations("font: bold 1em/2em monospace; content: \" (\" attr(href) \")\"");
        Assert.IsNotNull(decl);
        Assert.AreEqual(11, decl.Length);

        Assert.AreEqual("bold 1em / 2em monospace", decl.GetPropertyValue("font"));

        var content = decl.GetProperty("content");
        Assert.AreEqual("content", content.Name);
        Assert.IsFalse(content.IsImportant);
        Assert.AreEqual("\" (\" attr(href) \")\"", content.Value);
    }

    [TestMethod]
    public void CssBackgroundWebkitGradient()
    {
        var background = ParseDeclaration("background: -webkit-gradient(linear, left top, left bottom, color-stop(0%, #FFA84C), color-stop(100%, #FF7B0D))");
        Assert.IsNotNull(background);
        Assert.AreEqual("background", background.Name);
        Assert.IsFalse(background.IsImportant);
        Assert.IsFalse(background.HasValue);
    }

    [TestMethod]
    public void CssBackgroundColorRgba()
    {
        var background = ParseDeclaration("background-color: rgba(255, 123, 13, 1)");
        Assert.AreEqual("background-color", background.Name);
        Assert.IsFalse(background.IsImportant);
        Assert.AreEqual("rgba(255, 123, 13, 1)", background.Value);
    }

    [TestMethod]
    public void CssFontWithFraction()
    {
        var font = ParseDeclaration("font:bold 40px/1.13 'PT Sans Narrow', sans-serif");
        Assert.AreEqual("font", font.Name);
        Assert.IsFalse(font.IsImportant);
    }

    [TestMethod]
    public void TextShadow()
    {
        var textShadow = ParseDeclaration("text-shadow: 0 0 10px #000");
        Assert.AreEqual("text-shadow", textShadow.Name);
        Assert.IsFalse(textShadow.IsImportant);
    }

    [TestMethod]
    public void CssBackgroundWithImage()
    {
        var background = ParseDeclaration("background:url(../images/ribbon.svg) no-repeat");
        Assert.AreEqual("background", background.Name);
        Assert.IsFalse(background.IsImportant);
    }

    [TestMethod]
    public void CssContentWithCounter()
    {
        var content = ParseDeclaration("content:counter(paging, decimal-leading-zero)");
        Assert.AreEqual("content", content.Name);
        Assert.IsFalse(content.IsImportant);
    }

    [TestMethod]
    public void CssBackgroundColorRgb()
    {
        var backgroundColor = ParseDeclaration("background-color: rgb(245, 0, 111)");
        Assert.AreEqual("background-color", backgroundColor.Name);
        Assert.IsFalse(backgroundColor.IsImportant);
    }

    [TestMethod]
    public void CssImportSheet()
    {
        var rule = "@import url(fonts.css);";
        var decl = ParseRule(rule);
        Assert.IsNotNull(decl);
        Assert.IsInstanceOfType<ImportRule>(decl);
        var importRule = (ImportRule)decl;
        Assert.AreEqual("fonts.css", importRule.Href);
    }

    [TestMethod]
    public void CssContentEscaped()
    {
        var content = ParseDeclaration("content:'\005E'");
        Assert.AreEqual("content", content.Name);
        Assert.IsFalse(content.IsImportant);
    }

    [TestMethod]
    public void CssContentCounter()
    {
        var content = ParseDeclaration("content:counter(list)'.'");
        Assert.AreEqual("content", content.Name);
        Assert.IsFalse(content.IsImportant);
    }

    [TestMethod]
    public void CssTransformTranslate()
    {
        var transform = ParseDeclaration("transform:translateY(-50%)");
        Assert.AreEqual("transform", transform.Name);
        Assert.IsFalse(transform.IsImportant);
    }

    [TestMethod]
    public void CssBoxShadowMultiline()
    {
        var boxShadow = ParseDeclaration(@"
        box-shadow:
				0 0 0 10px rgba(60, 61, 64, 0.6),
				0 0 50px #3C3D40;");
        Assert.AreEqual("box-shadow", boxShadow.Name);
        Assert.IsFalse(boxShadow.IsImportant);
    }

    [TestMethod]
    public void CssDisplayBlock()
    {
        var display = ParseDeclaration("display:block");
        Assert.AreEqual("display", display.Name);
        Assert.IsFalse(display.IsImportant);
        Assert.AreEqual("block", display.Value);
    }

    [TestMethod]
    public void CssSheetWithDataUrlAsBackgroundImage()
    {
        var sheet = ParseStyleSheet(".App_Header_ .logo { background-image: url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEcAAAAcCAMAAAAEJ1IZAAAABGdBTUEAALGPC/xhBQAAVAI/VAI/VAI/VAI/VAI/VAI/VAAAA////AI/VRZ0U8AAAAFJ0Uk5TYNV4S2UbgT/Gk6uQt585w2wGXS0zJO2lhGttJK6j4YqZSobH1AAAAAElFTkSuQmCC\"); background-size: 71px 28px; background-position: 0 19px; width: 71px; }");
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        var rule = sheet.Rules[0] as StyleRule;
        Assert.IsNotNull(rule);
        Assert.AreEqual(4, rule.Style.Length);
        Assert.AreEqual(".App_Header_ .logo", rule.SelectorText);
        var decl = rule.Style;
        Assert.AreEqual("url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEcAAAAcCAMAAAAEJ1IZAAAABGdBTUEAALGPC/xhBQAAVAI/VAI/VAI/VAI/VAI/VAI/VAAAA////AI/VRZ0U8AAAAFJ0Uk5TYNV4S2UbgT/Gk6uQt585w2wGXS0zJO2lhGttJK6j4YqZSobH1AAAAAElFTkSuQmCC\")", decl.BackgroundImage);
        Assert.AreEqual("71px 28px", decl.BackgroundSize);
        Assert.AreEqual("0 19px", decl.BackgroundPosition);
        Assert.AreEqual("71px", decl.Width);
    }

    [TestMethod]
    public void CssSheetFromStreamWeirdBytesLeadingToInfiniteLoop()
    {
        var bs = new byte[8];
        bs[0] = 239;
        bs[1] = 187;
        bs[2] = 191;
        bs[3] = 117;
        bs[4] = 43;
        bs[5] = 63;
        bs[6] = 63;
        bs[7] = 63;

        using var memoryStream = new MemoryStream(bs, false);
        var sheet = memoryStream.ToCssStylesheet();
    }

    [TestMethod]
    public void CssSheetFromStreamOnlyZerosAvailable()
    {
        var bs = new byte[7180];

        using var memoryStream = new MemoryStream(bs, false);
        var sheet = memoryStream.ToCssStylesheet();
        Assert.IsNotNull(sheet);
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    public void CssSheetFromStringWithQuestionMarksLeadingToInfiniteLoop()
    {
        var sheet = "U+???\0".ToCssStylesheet();
        Assert.IsNotNull(sheet);
        Assert.AreEqual(0, sheet.Rules.Length);
    }

    [TestMethod]
    public void CssDefaultSheetSupportsRoundTripping()
    {
        var originalSourceCode = @"p.info {
	font-family: arial, sans-serif;
	line-height: 150%;
	margin-left: 2em;
	padding: 1em;
	border: 3px solid red;
	background-color: #f89;
	display: inline-block;
}
p.info span {
	font-weight: bold;
}
p.info span::after {
	content: ': ';
}";
        var initialSheet = originalSourceCode.ToCssStylesheet();
        var initialSourceCode = initialSheet.ToCss();
        var finalSheet = initialSourceCode.ToCssStylesheet();
        var finalSourceCode = finalSheet.ToCss();
        Assert.AreEqual(initialSourceCode, finalSourceCode);
        Assert.AreEqual(initialSheet.Rules.Length, finalSheet.Rules.Length);
    }

    [TestMethod]
    public void CssParseSheetWithStyleMediaAndStyleRule()
    {
        var sheet = ParseStyleSheet(@".mobile,.tablet{display:none;} @media only screen and(max-width:51.875em){.tablet{display:block;}} .disp {display:block;}");
        Assert.AreEqual(3, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Style, sheet.Rules[0].Type);
        Assert.AreEqual(RuleType.Media, sheet.Rules[1].Type);
        Assert.AreEqual(RuleType.Style, sheet.Rules[2].Type);
    }

    [TestMethod]
    public void CssParseSheetWithMediaAndTwoStyleRules()
    {
        var sheet = ParseStyleSheet(@"@media only screen and(max-width:51.875em){.tablet{display:block;}} .mobile,.tablet{display:none;} .disp {display:block;}");
        Assert.AreEqual(3, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        Assert.AreEqual(RuleType.Style, sheet.Rules[1].Type);
        Assert.AreEqual(RuleType.Style, sheet.Rules[2].Type);
    }

    [TestMethod]
    public void CssParseSheetWithTwoStyleAndMediaRule()
    {
        var sheet = ParseStyleSheet(@".mobile,.tablet{display:none;} .disp {display:block;} @media only screen and(max-width:51.875em){.tablet{display:block;}}");
        Assert.AreEqual(3, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Style, sheet.Rules[0].Type);
        Assert.AreEqual(RuleType.Style, sheet.Rules[1].Type);
        Assert.AreEqual(RuleType.Media, sheet.Rules[2].Type);
    }

    // The Timeout on this and the two tests after it is a hang guard, not a performance bound: each
    // of these inputs once sent the parser into a loop it never left, and the only assertion the
    // timeout makes is that parsing terminates at all. Each parse takes well under a millisecond, so
    // the value is ~4 orders of magnitude of headroom and says nothing about how fast parsing is.
    [TestMethod]
    [Timeout(10000)]
    public async Task CssParseSheetWithAtAndCommentDoesNotTakeForever()
    {
        var sheet = await Task.Run(() => ParseStyleSheet(@"
            h3 {color: yellow;
            @media print {
                h3 {color: black; }
                }
            "));
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Style, sheet.Rules[0].Type);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task CssParseSheetWithAtAndCommentDoesNotTakeForever2()
    {
        var sheet = await Task.Run(() => ParseStyleSheet(@"
:root {
    --layout: {
    }
    --layout-horizontal: {
        @apply (--layout);
    }
}"));
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Style, sheet.Rules[0].Type);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task CssParseSheetWithAtAndCommentDoesNotTakeForever3()
    {
        var sheet = await Task.Run(() => ParseStyleSheet(@"
@media (max-width: 991px) {
    body {
        background-color: #013668;
    }
    ;
}

@media (max-width: 991px) {
    body {
        background: #FFF;
    }
}"));
        // The stray ';' inside the first @media block is an empty statement (a no-op) - it must
        // not swallow the second, independently valid @media rule that follows.
        Assert.AreEqual(2, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        Assert.AreEqual(RuleType.Media, sheet.Rules[1].Type);
    }

    [TestMethod]
    public void CssParseImportStatementWithNoMediaTextFollowedByStyle()
    {
        var src = "@import url(import3.css); p { color : #f00; }";
        var sheet = ParseStyleSheet(src);
        Assert.AreEqual(2, sheet.Rules.Length);
        var import = sheet.Rules[0] as ImportRule;
        var style = sheet.Rules[1] as StyleRule;
        Assert.IsNotNull(import);
        Assert.IsNotNull(style);
        Assert.AreEqual(0, import.Media.Length);
        Assert.AreEqual("", import.Media.MediaText);
        Assert.AreEqual("import3.css", import.Href);
        Assert.AreEqual("p", style.Selector.Text);
        Assert.AreEqual(1, style.Style.Length);
    }

    [TestMethod]
    public void CssParseMediaRuleWithInvalidMediumEntities()
    {
        var src = "@media only screen and (min--moz-device-pixel-ratio:1.5),only screen and (-o-min-device-pixel-ratio:3/2),only screen and (-webkit-min-device-pixel-ratio:1.5),only screen and (min-device-pixel-ratio:1.5){.favicon{background-image:url('../img/favicons-sprite32.png?v=1b9547cf9cee3350a5b4875951e3e552');background-size:16px 5634px}}";
        var sheet = ParseStyleSheet(src);
        Assert.AreEqual(1, sheet.Rules.Length);
        var media = (MediaRule)sheet.Rules[0];
        Assert.IsNotNull(media);
        Assert.AreEqual(1, media.Media.Length);
        Assert.AreEqual(1, media.Rules.Length);
        Assert.AreEqual("only screen and (min-device-pixel-ratio: 1.5)", media.ConditionText);
    }

    [TestMethod]
    public void CssParseStyleWithInvalidSurrogatePair()
    {
        var src = @"span.berschrift2Zchn
{mso-style-name:""\00DCberschrift 2 Zchn"";
mso-style-priority:9;
mso-style-link:""\00DCberschrift 2"";
font-family:""Cambria"",""serif"";
color:#4F81BD;
font-weight:bold;}";
        var sheet = ParseStyleSheet(src);
        Assert.AreEqual(1, sheet.Rules.Length);
        var style = sheet.Rules[0] as StyleRule;
        Assert.IsNotNull(style);
        Assert.AreEqual("span.berschrift2Zchn", style.SelectorText);
        Assert.AreEqual(3, style.Style.Length);
    }

    [TestMethod]
    public void CssParseMsViewPortWithoutOptions()
    {
        var css = "@-ms-viewport{width:device-width} .dsip { display: block; }";
        var doc = ParseStyleSheet(css);
        var result = doc.ToCss();
        Assert.AreEqual(".dsip { display: block }", result);
    }

    [TestMethod]
    public void CssParseMsViewPortWithUnknownRules()
    {
        var css = "@-ms-viewport{width:device-width} .dsip { display: block; }";
        var doc = ParseStyleSheet(css, true, true, true, true);
        var result = doc.ToCss();
        Assert.AreEqual($"@-ms-viewport{{width:device-width}}{Environment.NewLine}.dsip {{ display: block }}", result);
    }

    [TestMethod]
    public void CssParseMediaAndMsViewPortWithoutOptions()
    {
        var css = "@media screen and (max-width: 400px) {  @-ms-viewport { width: 320px; }  }  .dsip { display: block; }";
        var doc = ParseStyleSheet(css);
        var result = doc.ToCss();
        Assert.AreEqual($"@media screen and (max-width: 400px) {{ }}{Environment.NewLine}.dsip {{ display: block }}", result);
    }

    [TestMethod]
    public void CssParseMediaAndMsViewPortWithUnknownRules()
    {
        var css = "@media screen and (max-width: 400px) {  @-ms-viewport { width: 320px; }  }  .dsip { display: block; }";
        var doc = ParseStyleSheet(css, true, true, true, true);
        var result = doc.ToCss();
        Assert.AreEqual($"@media screen and (max-width: 400px) {{ @-ms-viewport {{ width: 320px; }} }}{Environment.NewLine}.dsip {{ display: block }}", result);
    }

    [TestMethod]
    public void CssStyleSheetInsertAndDeleteShouldWork()
    {
        var parser = new StylesheetParser();
        var s = new Stylesheet(parser);
        Assert.AreEqual(0, s.Rules.Length);

        s.Insert("a {color: blue}", 0);
        Assert.AreEqual(1, s.Rules.Length);

        s.Insert("a *:first-child, a img {border: none}", 1);
        Assert.AreEqual(2, s.Rules.Length);

        s.RemoveAt(1);
        Assert.AreEqual(1, s.Rules.Length);

        s.RemoveAt(0);
        Assert.AreEqual(0, s.Rules.Length);
    }

    [TestMethod]
    [Spec("css21", "syndata.html#comments")]
    public void CssStyleSheetShouldIgnoreHtmlCommentTokens()
    {
        var parser = new StylesheetParser();
        var source = "<!-- body { font-family: Verdana } div.hidden { display: none } -->";
        var sheet = parser.Parse(source);
        Assert.AreEqual(2, sheet.Rules.Length);

        Assert.AreEqual(RuleType.Style, sheet.Rules[0].Type);
        var body = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("body", body.SelectorText);
        Assert.AreEqual(1, body.Style.Length);
        Assert.AreEqual("Verdana", body.Style.FontFamily);

        Assert.AreEqual(RuleType.Style, sheet.Rules[1].Type);
        var div = (StyleRule)sheet.Rules[1];
        Assert.AreEqual("div.hidden", div.SelectorText);
        Assert.AreEqual(1, div.Style.Length);
        Assert.AreEqual("none", div.Style.Display);
    }

    [TestMethod]
    public void CssStyleSheetInsertShouldSetParentStyleSheetCorrectly()
    {
        var parser = new StylesheetParser();
        var s = new Stylesheet(parser);
        s.Insert("a {color: blue}", 0);
        Assert.AreEqual(s, s.Rules[0].Owner);
    }

    [TestMethod]
    public void CssStyleSheetWithoutCommentsButStoringTrivia()
    {
        var parser = new StylesheetParser();
        const string source = ".foo { color: red; } @media print { #myid { color: green; } }";
        var sheet = parser.Parse(source);
        var comments = sheet.GetComments();
        Assert.AreEqual(0, comments.Count());
    }

    [TestMethod]
    public void CssStyleSheetWithCommentInDeclaration()
    {
        var parser = new StylesheetParser(preserveComments: true);
        const string source = ".foo { /*test*/ color: red;/*test*/ } @media print { #myid { color: green; } }";
        var sheet = parser.Parse(source);
        var comments = sheet.GetComments();
        Assert.AreEqual(2, comments.Count());

        foreach (var comment in comments)
        {
            Assert.AreEqual("test", comment.Data);
        }
    }

    [TestMethod]
    public void CssStyleSheetWithCommentInRule()
    {
        var parser = new StylesheetParser(preserveComments: true);
        const string source = ".foo { color: red; } @media print { /*test*/ #myid { color: green; } /*test*/ }";
        var sheet = parser.Parse(source);
        var comments = sheet.GetComments();
        Assert.AreEqual(2, comments.Count());

        foreach (var comment in comments)
        {
            Assert.AreEqual("test", comment.Data);
        }
    }

    [TestMethod]
    public void CssStyleSheetWithCommentInMedia()
    {
        var parser = new StylesheetParser(preserveComments: true);
        var source = ".foo { color: red; } @media all /*test*/ and /*test*/ (min-width: 701px) /*test*/ { #myid { color: green; } }";
        var sheet = parser.Parse(source);
        var comments = sheet.GetComments();
        Assert.AreEqual(3, comments.Count());

        foreach (var comment in comments)
        {
            Assert.AreEqual("test", comment.Data);
        }
    }

    [TestMethod]
    public void CssStyleSheetSimpleRoundtrip()
    {
        var parser = new StylesheetParser(preserveComments: true);
        const string source = ".foo { color: red; } @media all /*test*/ and /*test*/ (min-width: 701px) /*test*/ { #myid { color: green; } }";
        var sheet = parser.Parse(source);
        var roundtrip = sheet.StylesheetText.Text;
        Assert.AreEqual(source, roundtrip);
    }

    [TestMethod]
    public void CssStyleSheetSelectorsGetAll()
    {
        var parser = new StylesheetParser();

        const string source = ".foo { } #bar { } @media all { div { } a > b { } @media print { script[type] { } } }";
        var sheet = parser.Parse(source);
        var roundtrip = sheet.StylesheetText.Text;
        Assert.AreEqual(source, roundtrip);
        var selectors = sheet.GetAll<ISelector>();
        Assert.AreEqual(5, selectors.Count());
        var mediaRules = sheet.GetAll<MediaRule>();
        Assert.AreEqual(2, mediaRules.Count());
        var descendentSelector = selectors.Skip(3).First();
        Assert.AreEqual("a>b", descendentSelector.Text);
        Assert.AreEqual("a > b ", descendentSelector.StylesheetText.Text);
    }

    [TestMethod]
    public void CssColorFunctionsMixAllShouldWork()
    {
        var parser = new StylesheetParser();
        const string source = @"
.rgbNumber { color: rgb(255, 128, 0); }
.rgbPercent { color: rgb(100%, 50%, 0%); }
.rgbaNumber { color: rgba(255, 128, 0, 0.0); }
.rgbaPercent { color: rgba(100%, 50%, 0%, 0.0); }
.hsl { color: hsl(120, 100%, 50%); }
.hslAngle { color: hsl(120deg, 100%, 50%); }
.hsla { color: hsla(120, 100%, 50%, 0.25); }
.hslaAngle { color: hsla(120deg, 100%, 50%, 0.25); }
.grayNumber { color: gray(128); }
.grayPercent { color: gray(50%); }
.grayPercentAlpha { color: gray(50%, 0.5); }
.hwb { color: hwb(120, 60%, 20%); }
.hwbAngle { color: hwb(120deg, 60%, 20%); }
.hwbAlpha { color: hwb(120, 10%, 50%, 0.5); }
.hwbAngleAlpha { color: hwb(120deg, 10%, 50%, 0.5); }";
        var sheet = parser.Parse(source);
        Assert.AreEqual(15, sheet.Rules.Length);

        var rgbNumber = ((StyleRule)sheet.Rules[0]).Style.Color;
        var rgbPercent = ((StyleRule)sheet.Rules[1]).Style.Color;
        var rgbaNumber = ((StyleRule)sheet.Rules[2]).Style.Color;
        var rgbaPercent = ((StyleRule)sheet.Rules[3]).Style.Color;
        var hsl = ((StyleRule)sheet.Rules[4]).Style.Color;
        var hslAngle = ((StyleRule)sheet.Rules[5]).Style.Color;
        var hsla = ((StyleRule)sheet.Rules[6]).Style.Color;
        var hslaAngle = ((StyleRule)sheet.Rules[7]).Style.Color;
        var grayNumber = ((StyleRule)sheet.Rules[8]).Style.Color;
        var grayPercent = ((StyleRule)sheet.Rules[9]).Style.Color;
        var grayPercentAlpha = ((StyleRule)sheet.Rules[10]).Style.Color;
        var hwb = ((StyleRule)sheet.Rules[11]).Style.Color;
        var hwbAngle = ((StyleRule)sheet.Rules[12]).Style.Color;
        var hwbAlpha = ((StyleRule)sheet.Rules[13]).Style.Color;
        var hwbAngleAlpha = ((StyleRule)sheet.Rules[14]).Style.Color;

        Assert.IsNotNull(rgbNumber);
        Assert.IsNotNull(rgbPercent);
        Assert.IsNotNull(rgbaNumber);
        Assert.IsNotNull(rgbaPercent);
        Assert.IsNotNull(hsl);
        Assert.IsNotNull(hslAngle);
        Assert.IsNotNull(hsla);
        Assert.IsNotNull(hslaAngle);
        Assert.IsNotNull(grayNumber);
        Assert.IsNotNull(grayPercent);
        Assert.IsNotNull(grayPercentAlpha);
        Assert.IsNotNull(hwb);
        Assert.IsNotNull(hwbAngle);
        Assert.IsNotNull(hwbAlpha);
        Assert.IsNotNull(hwbAngleAlpha);
    }

    [TestMethod]
    public void ShouldBeAbleToPreserveDuplicateProperties()
    {
        var sheet = ParseStyleSheet(@"
h1 {
 color: red;
 color: some-invalid-color;",
        tolerateInvalidValues: true,
        preserveDuplicateProperties: true);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var h1 = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("h1", h1.SelectorText);
        var props = h1.Style.Children.OfType<Property>().ToList();
        Assert.AreEqual("rgb(255, 0, 0)", props[0].Value);
        Assert.AreEqual("some-invalid-color", props[1].Value);
    }

    [TestMethod]
    public void Parse_ZIndex_Out_Of_Range()
    {
        var sheet = ParseStyleSheet(".style{ z-index: 99999999999999999;}");
    }

    [TestMethod]
    public void CanHandleGreaterThanSelectorWithNoFollowingSpace()
    {
        var sheet = ParseStyleSheet(@"#collapse-button >#icon{ }");
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var rule = (StyleRule)sheet.Rules[0];
        Assert.IsInstanceOfType<ComplexSelector>(rule.Selector);
        var selector = (ComplexSelector)rule.Selector;

        var parts = selector.ToList();
        Assert.AreEqual(2, parts.Count());
        Assert.IsInstanceOfType<IdSelector>(parts[0].Selector);
        Assert.IsInstanceOfType<IdSelector>(parts[1].Selector);
    }
}
