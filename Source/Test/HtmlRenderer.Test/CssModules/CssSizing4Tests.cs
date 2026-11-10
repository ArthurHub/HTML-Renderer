using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssSizing4Tests
{
    private const double Delta = 0.5;

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [DataRow("size:100px 40px", 100, 40)]
    [DataRow("size:100px", 100, 100)]
    [Spec("css-sizing-4", "#sizing-properties")]
    public void SizeShorthands_SetWidthAndHeight(string style, double width, double height)
    {
        var root = Layout($"<div style='width:300px'><div id='b' style='{style}'></div></div>");

        Assert.AreEqual(width, Box(root, "b").Size.Width, Delta);
        Assert.AreEqual(height, Box(root, "b").Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [DataRow("width: contain")]
    [DataRow("width: fit-content(100px)")]
    [DataRow("height: calc-size(auto, size + 10px)")]
    [Spec("css-sizing-4", "#sizing-values")]
    public void NewSizingValues_AreAccepted(string declaration)
    {
        Assert.IsTrue(Accepted(declaration));
    }

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [Spec("css-sizing-4", "#sizing-values")]
    public void FitContentFunction_ClampsBetweenMinAndMaxContent()
    {
        var root = Layout("<div style='width:300px'><div id='b' style='width:fit-content(100px)'>aaaa bbbb cccc dddd eeee ffff</div></div>");

        Assert.AreEqual(100, Box(root, "b").Size.Width, Delta);
    }

    [TestMethod]
    [DataRow("aspect-ratio: 16 / 9")]
    [DataRow("aspect-ratio: auto 2")]
    [Spec("css-sizing-4", "#aspect-ratio")]
    public void AspectRatio_IsAccepted(string declaration)
    {
        Assert.IsTrue(Accepted(declaration));
    }

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [Spec("css-sizing-4", "#aspect-ratio")]
    [Spec("css-sizing-4", "#aspect-ratio-automatic")]
    public void AspectRatio_SizesTheAutoHeightFromTheWidth()
    {
        var root = Layout("<div id='b' style='width:100px;aspect-ratio:2/1'></div>");

        Assert.AreEqual(50, Box(root, "b").Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [DataRow("contain-intrinsic-size: 100px 50px")]
    [DataRow("contain-intrinsic-width: auto 100px")]
    [Spec("css-sizing-4", "#intrinsic-size-override")]
    public void ContainIntrinsicSize_IsAccepted(string declaration)
    {
        Assert.IsTrue(Accepted(declaration));
    }

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [Spec("css-sizing-4", "#responsive-iframes")]
    public void FrameSizing_IsAccepted()
    {
        Assert.IsTrue(Accepted("frame-sizing: content-height"));
    }

    [Ignore("Gap, see Spec/css-sizing-4.yml")]
    [TestMethod]
    [Spec("css-sizing-4", "#intrinsic-contribution-override")]
    public void MinIntrinsicSizing_IsAccepted()
    {
        Assert.IsTrue(Accepted("min-intrinsic-sizing: zero-if-scroll"));
    }

    private static bool Accepted(string declaration) => CssConstructionFunctions.ParseDeclarations(declaration).Length > 0;

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body)).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
