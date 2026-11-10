using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssContainTests
{
    private const double Delta = 0.5;

    [Ignore("Gap, see Spec/css-contain-2.yml")]
    [TestMethod]
    [DataRow("contain: strict")]
    [DataRow("contain: layout paint")]
    [DataRow("content-visibility: hidden")]
    [Spec("css-contain-2", "#contain-property")]
    [Spec("css-contain-2", "#content-visibility")]
    public void ContainmentProperties_AreAccepted(string declaration)
    {
        Assert.IsTrue(CssConstructionFunctions.ParseDeclarations(declaration).Length > 0);
    }

    [Ignore("Gap, see Spec/css-contain-2.yml")]
    [TestMethod]
    [Spec("css-contain-2", "#containment-size")]
    public void SizeContainment_SizesTheBoxAsIfEmpty()
    {
        var root = Layout("<div id='b' style='contain:size'><div style='height:50px'></div></div>");

        Assert.AreEqual(0, Box(root, "b").Size.Height, Delta);
    }

    [Ignore("Gap, see Spec/css-contain-2.yml")]
    [TestMethod]
    [Spec("css-contain-2", "#containment-layout")]
    public void LayoutContainment_IsTheContainingBlockOfAbsoluteDescendants()
    {
        var root = Layout("<div style='height:40px'></div><div id='c' style='contain:layout;height:50px'><div id='a' style='position:absolute;top:0;height:10px'></div></div>");

        Assert.AreEqual(Box(root, "c").Location.Y, Box(root, "a").Location.Y, Delta);
    }

    [Ignore("Gap, see Spec/css-contain-2.yml")]
    [TestMethod]
    [Spec("css-contain-2", "#content-visibility")]
    public void ContentVisibilityHidden_SkipsTheContents()
    {
        var root = Layout("<div id='b' style='content-visibility:hidden'><div style='height:50px'></div></div>");

        Assert.AreEqual(0, Box(root, "b").Size.Height, Delta);
    }

    private static CssBox Layout(string body) => LayoutHarness.Layout(LayoutHarness.Wrap(body)).Root;

    private static CssBox Box(CssBox root, string id) => LayoutHarness.FindById(root, id)!;
}
