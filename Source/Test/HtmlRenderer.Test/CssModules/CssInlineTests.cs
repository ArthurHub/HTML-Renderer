using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssInlineTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("css-inline-3", "#paint-order")]
    public void InlineBoxes_PaintInDocumentOrder()
    {
        var g = Paint("<div><span style='background:rgb(255,0,0)'>a</span><span style='background:rgb(0,0,255);margin-left:-5px'>b</span></div>");

        Assert.IsLessThan(RectIndex(g, RColor.FromArgb(0, 0, 255)), RectIndex(g, RColor.FromArgb(255, 0, 0)));
    }

    [TestMethod]
    [Spec("css-inline-3", "#dominant-baseline-property")]
    [DataRow("auto")]
    [DataRow("alphabetic")]
    [DataRow("central")]
    [DataRow("hanging")]
    public void DominantBaseline_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("dominant-baseline", value));
    }

    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    [DataRow("baseline")]
    [DataRow("sub")]
    [DataRow("super")]
    [DataRow("top")]
    [DataRow("text-top")]
    [DataRow("middle")]
    [DataRow("bottom")]
    [DataRow("text-bottom")]
    [DataRow("5px")]
    [DataRow("10%")]
    public void VerticalAlign_Css2Values_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("vertical-align", value));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#transverse-alignment")]
    [DataRow("first")]
    [DataRow("central")]
    [DataRow("last top")]
    public void VerticalAlign_Level3Values_AreParsed(string value)
    {
        Assert.AreEqual(value, Value("vertical-align", value));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#baseline-source")]
    [DataRow("auto")]
    [DataRow("first")]
    [DataRow("last")]
    public void BaselineSource_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("baseline-source", value));
    }

    [TestMethod]
    [Spec("css-inline-3", "#alignment-baseline-property")]
    [DataRow("baseline")]
    [DataRow("middle")]
    [DataRow("text-top")]
    public void AlignmentBaseline_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("alignment-baseline", value));
    }

    [TestMethod]
    [Spec("css-inline-3", "#baseline-shift-property")]
    [DataRow("5px")]
    [DataRow("sub")]
    [DataRow("top")]
    public void BaselineShift_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("baseline-shift", value));
    }

    [TestMethod]
    [Spec("css-inline-3", "#line-height-property")]
    [DataRow("normal")]
    [DataRow("1.5")]
    [DataRow("20px")]
    [DataRow("150%")]
    public void LineHeight_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("line-height", value));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#line-height-property")]
    [DataRow("-1px")]
    [DataRow("-1")]
    public void LineHeight_RejectsNegativeValues(string value)
    {
        Assert.AreEqual(string.Empty, Value("line-height", value));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#text-edges")]
    public void LineFitEdge_IsParsed()
    {
        Assert.AreEqual("cap alphabetic", Value("line-fit-edge", "cap alphabetic"));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#text-box-trim")]
    public void TextBoxTrim_TrimsTheHalfLeadingAboveTheFirstLine()
    {
        var plain = Layout("<div id='x' style='line-height:40px'>a</div>");
        var trimmed = Layout("<div id='x' style='line-height:40px;text-box-trim:trim-start'>a</div>");

        Assert.IsLessThan(LayoutHarness.FindById(plain, "x")!.Size.Height, LayoutHarness.FindById(trimmed, "x")!.Size.Height);
    }

    [TestMethod]
    [Spec("css-inline-3", "#text-box-shorthand")]
    [Spec("css-inline-3", "#text-box-edge")]
    public void TextBox_IsParsed()
    {
        Assert.AreEqual("trim-both cap alphabetic", Value("text-box", "trim-both cap alphabetic"));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#line-fill")]
    public void InlineSizing_IsParsed()
    {
        Assert.AreEqual("stretch", Value("inline-sizing", "stretch"));
    }

    [TestMethod]
    [Spec("css-inline-3", "#sizing-drop-initials")]
    [DataRow("3")]
    [DataRow("3 2")]
    public void InitialLetter_IsParsed(string value)
    {
        Assert.AreEqual(value, Value("initial-letter", value));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#aligning-initial-letter")]
    public void InitialLetterAlign_IsParsed()
    {
        Assert.AreEqual("alphabetic", Value("initial-letter-align", "alphabetic"));
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#initial-letter-wrapping")]
    public void InitialLetterWrap_IsParsed()
    {
        Assert.AreEqual("first", Value("initial-letter-wrap", "first"));
    }

    private static CssBox Layout(string body) =>
        LayoutHarness.Layout($"<html><head></head><body style='margin:0'>{body}</body></html>", 800, 600).Root;

    private static RecordingGraphics Paint(string body)
    {
        var container = new HtmlContainerInt(new MockAdapter()) { MaxSize = new RSize(800, 600), PageSize = new RSize(800, 600) };
        container.SetHtml($"<html><head></head><body style='margin:0'>{body}</body></html>");
        var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);
        container.PerformPaint(graphics);
        return graphics;
    }

    private static int RectIndex(RecordingGraphics g, RColor color)
    {
        var index = g.Log.FindIndex(e => e is RecordingGraphics.DrawRectCall r && r.Color == color);
        Assert.IsGreaterThanOrEqualTo(0, index, $"No rectangle painted in {color}");
        return index;
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
