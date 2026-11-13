using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssWritingModesTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css-writing-modes-4", "#direction")]
    [DataRow("ltr")]
    [DataRow("rtl")]
    public void Direction_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("direction", value));
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#direction")]
    public void DirAttribute_SetsTheDirection()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div dir='rtl'><p id='p'>a</p></div>"));

        Assert.AreEqual("rtl", LayoutHarness.FindById(root, "p")!.Direction);
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#unicode-bidi")]
    [DataRow("normal")]
    [DataRow("embed")]
    [DataRow("isolate")]
    [DataRow("bidi-override")]
    [DataRow("isolate-override")]
    [DataRow("plaintext")]
    public void UnicodeBidi_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("unicode-bidi", value));
    }

    [Ignore("Gap, see Spec/css-writing-modes-4.yml")]
    [TestMethod]
    [Spec("css-writing-modes-4", "#unicode-bidi")]
    [Spec("css-writing-modes-4", "#bidi-para-direction")]
    public void Plaintext_TakesTheParagraphDirectionFromTheText()
    {
        var root = Layout("<div style='direction:rtl;unicode-bidi:plaintext;width:300px'>one two</div>");

        Assert.IsLessThan(Word(root, "two").Left, Word(root, "one").Left);
    }

    [Ignore("Gap, see Spec/css-writing-modes-4.yml")]
    [TestMethod]
    [Spec("css-writing-modes-4", "#unicode-bidi")]
    [Spec("css-writing-modes-4", "#bidi-control-codes")]
    public void IsolatedLtrSpan_InAnRtlBlock_KeepsItsWordOrder()
    {
        var root = Layout("<div style='direction:rtl;width:300px'>אבג <span style='direction:ltr;unicode-bidi:isolate'>one two</span> דהו</div>");

        Assert.IsLessThan(Word(root, "two").Left, Word(root, "one").Left);
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#bidi-atomic-inlines")]
    public void Image_InAnRtlBlock_IsReorderedLikeAWord()
    {
        var root = Layout("<div style='direction:rtl;width:300px'><img id='i' style='width:20px;height:10px'> אבג</div>");
        var image = LayoutHarness.FindById(root, "i")!.Words.Single();

        Assert.IsGreaterThan(Word(root, "אבג").Right, image.Left);
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#block-flow")]
    [DataRow("horizontal-tb")]
    [DataRow("vertical-rl")]
    [DataRow("vertical-lr")]
    [DataRow("sideways-rl")]
    [DataRow("sideways-lr")]
    public void WritingMode_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("writing-mode", value));
    }

    [Ignore("Gap, see Spec/css-writing-modes-4.yml")]
    [TestMethod]
    [Spec("css-writing-modes-4", "#block-flow")]
    [Spec("css-writing-modes-4", "#vertical-layout")]
    public void VerticalRl_StacksBlocksFromRightToLeft()
    {
        var root = Layout("<div style='writing-mode:vertical-rl;width:200px;height:100px'><div id='a' style='width:20px'></div><div id='b' style='width:20px'></div></div>");

        Assert.IsLessThan(LayoutHarness.FindById(root, "a")!.Location.X, LayoutHarness.FindById(root, "b")!.Location.X);
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#text-orientation")]
    [DataRow("mixed")]
    [DataRow("upright")]
    [DataRow("sideways")]
    public void TextOrientation_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-orientation", value));
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#text-combine-upright")]
    [DataRow("none")]
    [DataRow("all")]
    public void TextCombineUpright_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("text-combine-upright", value));
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#logical-to-physical")]
    [Spec("css-logical-1", "#margin-properties")]
    public void FlowRelativeMargins_MapToThePhysicalSidesInHorizontalLtr()
    {
        var box = Styled("#x{margin-block-start:1px;margin-inline-end:2px;margin-block-end:3px;margin-inline-start:4px}");

        Assert.AreEqual(1, box.ActualMarginTop, Delta);
        Assert.AreEqual(2, box.ActualMarginRight, Delta);
        Assert.AreEqual(3, box.ActualMarginBottom, Delta);
        Assert.AreEqual(4, box.ActualMarginLeft, Delta);
    }

    [Ignore("Gap, see Spec/css-writing-modes-4.yml")]
    [TestMethod]
    [Spec("css-writing-modes-4", "#logical-to-physical")]
    [Spec("css-logical-1", "#margin-properties")]
    public void MarginInlineStart_IsTheRightMarginInRtl()
    {
        var box = Styled("#x{direction:rtl;margin-inline-start:4px}");

        Assert.AreEqual(4, box.ActualMarginRight, Delta);
        Assert.AreEqual(0, box.ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Spec("css-logical-1", "#box")]
    public void PhysicalMargin_StaysOnItsSideInRtl()
    {
        var root = Layout("<div style='direction:rtl;width:200px'><div id='x' style='margin-left:30px;height:10px'></div></div>");
        var box = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual(30, box.Location.X, Delta);
        Assert.AreEqual(170, box.Size.Width, Delta);
    }

    [TestMethod]
    [Spec("css-writing-modes-4", "#line-mappings")]
    public void TextAlignLeft_IsTheLineLeftInRtl()
    {
        var root = Layout("<div style='direction:rtl;text-align:left;width:200px'>אבג</div>");

        Assert.AreEqual(0, Word(root, "אבג").Left, Delta);
    }

    private static CssBox Layout(string body) =>
        LayoutHarness.Layout($"<html><head></head><body style='margin:0'>{body}</body></html>", 800, 600).Root;

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>", 800, 600);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static CssRect Word(CssBox root, string text)
    {
        var word = LayoutHarness.Descendants(root).SelectMany(b => b.Words).FirstOrDefault(w => w.Text == text);
        Assert.IsNotNull(word, $"No word '{text}'");
        return word;
    }

    private static string Value(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        return sheet.Rules.OfType<StyleRule>().Single().Style.GetPropertyValue(property);
    }
}
