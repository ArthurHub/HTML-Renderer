using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Where lines end around &lt;br&gt;, compared against what browsers render. With the mock adapter every line
/// is 20px tall.
/// </summary>
[DoNotParallelize]
[TestClass]
public sealed class LineBreakLayoutIntegrationTests
{
    private const double Delta = 0.5;

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-br-element")]
    [DataRow("<div id='x'>first<br>second</div>", 40)]
    [DataRow("<div id='x'>first<br><br>second</div>", 60)]
    [DataRow("<div id='x'><span>first</span><br/><br/><span>second</span></div>", 60)]
    [DataRow("<div id='x'>a<br></div>", 20)]
    [DataRow("<div id='x'><br>b</div>", 40)]
    [DataRow("<div id='x'><br></div>", 20)]
    public void BlockHeight_MatchesTheNumberOfLines(string html, double expectedHeight)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));
        var box = LayoutHarness.FindById(root, "x")!;

        Assert.AreEqual(expectedHeight, box.ActualBottom - box.Location.Y, Delta);
    }

    [TestMethod]
    [Spec("html", "text-level-semantics.html#the-br-element")]
    public void BreakBetweenBlocks_LeavesOneEmptyLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='a'>a</div><br><div id='b'>b</div>"));
        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.Location.Y - a.ActualBottom, Delta);
    }

    [TestMethod]
    public void PreElement_DropsTheNewlineRightAfterItsStartTag()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<pre id='x'>\nline</pre><pre id='y'>line</pre>"));
        var withNewline = LayoutHarness.FindById(root, "x")!;
        var without = LayoutHarness.FindById(root, "y")!;

        Assert.AreEqual(without.ActualBottom - without.Location.Y, withNewline.ActualBottom - withNewline.Location.Y, Delta);
    }

    [TestMethod]
    public void TableCell_MaxContentWidth_IsTheWidestLine_NotAllLinesSummed()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<table><tr><td id='broken'>aaaa<br>bbbb</td></tr></table>"
            + "<table><tr><td id='single'>aaaa</td></tr></table>"));
        var broken = LayoutHarness.FindById(root, "broken")!;
        var single = LayoutHarness.FindById(root, "single")!;

        Assert.AreEqual(single.ActualWidth, broken.ActualWidth, Delta);
    }
}
