using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Content;

[DoNotParallelize]
[TestClass]
public sealed class ListItemCounterIntegrationTests
{
    [TestMethod]
    [DataRow(-3, "lower-roman", "-3.")]
    [DataRow(0, "upper-roman", "0.")]
    [DataRow(4000, "lower-roman", "4000.")]
    [DataRow(-3, "lower-alpha", "-3.")]
    [DataRow(0, "lower-greek", "0.")]
    public void OlStart_OutsideTheStyleRange_FallsBackToDecimal(int start, string style, string expected)
    {
        var html = LayoutHarness.Wrap($"<ol start='{start}' style='list-style-type:{style}'><li id='a'>x</li></ol>");
        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;

        // The marker breaks after the minus sign, so "-3." is two words
        Assert.AreEqual(expected, string.Concat(a.ListItemBox.Words.Select(w => w.Text)));
    }

    [TestMethod]
    [Spec("css21", "generate.html#list-style")]
    [Spec("css-lists-3", "#text-markers")]
    public void ListStyleTypeSquare_UsesAFilledSquare()
    {
        var html = LayoutHarness.Wrap("<ul style='list-style-type:square'><li id='a'>x</li></ul>");
        var (root, _) = LayoutHarness.Layout(html);
        var a = LayoutHarness.FindById(root, "a")!;

        Assert.AreEqual("▪", a.ListItemBox.Words[0].Text);
    }
}
