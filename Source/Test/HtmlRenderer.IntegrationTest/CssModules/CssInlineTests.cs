using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.CssModules;

[DoNotParallelize]
[TestClass]
public sealed class CssInlineTests
{
    private const double Delta = 1.0;

    [TestMethod]
    [Spec("css-writing-modes-4", "#replaced-baselines")]
    [Spec("css-inline-3", "#baseline-tables")]
    public void InlineBlock_IsAlignedByTheBaselineOfItsLine()
    {
        var root = Layout("<div style='width:300px'>x <span style='display:inline-block;padding-top:20px'>y</span></div>");

        Assert.AreEqual(Word(root, "y").Bottom, Word(root, "x").Bottom, Delta);
        Assert.IsGreaterThan(19, Word(root, "x").Top);
    }

    [Ignore("Gap, see Spec/css-inline-3.yml")]
    [TestMethod]
    [Spec("css-inline-3", "#baseline-tables")]
    public void InlineBlock_IsAlignedByTheBaselineOfItsLastLine()
    {
        var root = Layout("<div style='width:300px'>x <span style='display:inline-block'>y<br>z</span></div>");

        Assert.AreEqual(Word(root, "z").Bottom, Word(root, "x").Bottom, Delta);
    }

    [Ignore("Gap, see Spec/css-writing-modes-4.yml")]
    [TestMethod]
    [Spec("css-writing-modes-4", "#bidi-atomic-inlines")]
    public void InlineBlock_InAnRtlBlock_IsReorderedLikeAWord()
    {
        var root = Layout("<div style='direction:rtl;width:300px'><span id='b' style='display:inline-block;width:20px;height:10px'></span> אבג</div>");

        Assert.IsGreaterThan(Word(root, "אבג").Right, LayoutHarness.FindById(root, "b")!.Location.X);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body)).Item1;

    private static CssRect Word(CssBox root, string text) =>
        LayoutHarness.Descendants(root).SelectMany(b => b.Words).First(w => w.Text == text);
}
