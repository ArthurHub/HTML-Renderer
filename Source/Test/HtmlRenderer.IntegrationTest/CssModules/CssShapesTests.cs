using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.CssModules;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class CssShapesTests
{
    [Ignore("Gap, see Spec/css-shapes-1.yml")]
    [TestMethod]
    [DataRow("inset(0 50% 0 0)")]
    [DataRow("polygon(0 0, 50% 0, 50% 100%, 0 100%)")]
    [Spec("css-shapes-1", "#shape-outside-property")]
    [Spec("css-shapes-1", "#relation-to-box-model-and-float-behavior")]
    public void ShapeOutside_NarrowsTheFloatArea(string shape)
    {
        var root = LayoutHarness.Layout(LayoutHarness.Wrap(
            $"<div style='width:300px'><div style='float:left;width:100px;height:100px;shape-outside:{shape}'></div>"
            + "<span id='t' style='display:inline-block;width:10px;height:10px'></span></div>"), maxWidth: 400).Root;

        Assert.AreEqual(50, LayoutHarness.FindById(root, "t")!.Rectangles.Values.Single().X, 0.5);
    }
}
