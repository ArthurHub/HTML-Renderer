using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssBox4Tests
{
    private const double Delta = 0.5;

    [TestMethod]
    [DataRow("background-clip: content-box")]
    [DataRow("background-origin: border-box")]
    [Spec("css-box-4", "#keywords")]
    public void VisualBoxKeywords_AreAccepted(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclarations(declaration).Length > 0);
    }

    [Ignore("Gap, see Spec/css-box-4.yml")]
    [TestMethod]
    [DataRow("margin-trim: block")]
    [DataRow("margin-trim: inline-start block-end")]
    [Spec("css-box-4", "#margin-trim")]
    public void MarginTrim_IsAccepted(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclarations(declaration).Length > 0);
    }

    [Ignore("Gap, see Spec/css-box-4.yml")]
    [TestMethod]
    [Spec("css-box-4", "#margin-trim-block")]
    public void MarginTrimBlock_TrimsTheFirstAndLastChildMargins()
    {
        var root = Layout("<div id='c' style='margin-trim:block;padding:1px 0'><div id='a' style='margin-top:20px;height:10px'></div><div style='margin-bottom:20px;height:10px'></div></div>");

        Assert.AreEqual(1, Box(root, "a").Location.Y - Box(root, "c").Location.Y, Delta);
        Assert.AreEqual(22, Box(root, "c").Size.Height, Delta);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body)).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
