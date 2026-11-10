using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class CssTextDecor4PaintIntegrationTests
{
    [TestMethod]
    [Spec("css-text-decor-4", "#line-position")]
    [Spec("css-text-decor-4", "#text-line-constancy")]
    [DataRow("font-size:30px")]
    [DataRow("vertical-align:super")]
    public void Underline_HasOnePositionPerLine(string style)
    {
        var lines = PageLines($"<div style='text-decoration:underline'>a <span style='{style}'>B</span> c</div>");

        Assert.IsGreaterThan(0, lines.Count);
        Assert.AreEqual(1, lines.Select(l => l.Y1).Distinct().Count());
    }

    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-spaces-property")]
    [DataRow("<div style='text-decoration:underline'>aaa bbb ccc</div>")]
    [DataRow("<span style='text-decoration:underline'>aaa bbb ccc</span>")]
    public void Underline_IsContinuousAcrossSpacesBetweenWords(string html)
    {
        AssertContinuous(PageLines(html));
    }

    [Ignore("Gap, see Spec/css-text-decor-4.yml")]
    [TestMethod]
    [Spec("css-text-decor-4", "#text-decoration-skip-spaces-property")]
    public void Underline_IsContinuousAcrossSpacesNextToAnElementBoundary()
    {
        AssertContinuous(PageLines("<div style='text-decoration:underline'>aaa <span>bbb</span> ccc</div>"));
    }

    private static void AssertContinuous(List<RecordingGraphics.DrawLineCall> lines)
    {
        lines = lines.OrderBy(l => l.X1).ToList();
        Assert.IsGreaterThan(0, lines.Count);
        for (var i = 1; i < lines.Count; i++)
            Assert.IsLessThanOrEqualTo(lines[i - 1].X2 + 0.5, lines[i].X1);
    }

    private static List<RecordingGraphics.DrawLineCall> PageLines(string bodyHtml)
    {
        var (_, container) = PaintHarness.Layout(PaintHarness.Wrap(bodyHtml));
        return PaintHarness.PaintPage(container).Log.OfType<RecordingGraphics.DrawLineCall>().ToList();
    }
}
