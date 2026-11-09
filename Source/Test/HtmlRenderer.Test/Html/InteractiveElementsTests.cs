using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class InteractiveElementsTests
{
    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text));

    [TestMethod]
    [Spec("html", "interactive-elements.html#the-details-element")]
    [Spec("html", "interactive-elements.html#the-summary-element")]
    public void OpenDetails_ShowsTheSummaryAndTheContent()
    {
        Assert.AreEqual("sa", Text(Box("<details id=x open><summary>s</summary>a</details>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interactive-elements.html#the-details-element")]
    public void ClosedDetails_ShowsOnlyTheSummary()
    {
        var withContent = Box("<div id=x><details><summary>s</summary><p>a</p><p>b</p></details></div>");
        var withoutContent = Box("<div id=x><details><summary>s</summary></details></div>");

        Assert.AreEqual(withoutContent.Size.Height, withContent.Size.Height);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interactive-elements.html#the-details-element")]
    public void Details_WithoutASummary_GetsADefaultLegend()
    {
        StringAssert.StartsWith(Text(Box("<details id=x>a</details>")), "Details");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interactive-elements.html#the-details-element")]
    public void Details_InANameGroup_OnlyTheFirstOpenOneStaysOpen()
    {
        const string html =
            "<style>details[open] p { color: rgb(1, 2, 3) }</style>" +
            "<details name=g open><p id=a>a</p></details><details name=g open><p id=b>b</p></details>";

        Assert.AreEqual("rgb(1, 2, 3)", Box(html, "a").Color);
        Assert.AreNotEqual("rgb(1, 2, 3)", Box(html, "b").Color);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interactive-elements.html#the-summary-element")]
    public void Summary_HasADisclosureMarker()
    {
        Assert.AreEqual("disclosure-closed", Box("<details><summary id=x>s</summary></details>").ListStyleType);
    }

    [TestMethod]
    [Spec("html", "interactive-elements.html#the-dialog-element")]
    public void OpenDialog_ShowsItsContent()
    {
        Assert.AreEqual("a", Text(Box("<dialog id=x open>a</dialog>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interactive-elements.html#the-dialog-element")]
    public void ClosedDialog_IsNotShown()
    {
        Assert.AreEqual(0, Box("<div id=x><dialog>a</dialog></div>").Size.Height);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "interactive-elements.html#the-dialog-element")]
    public void OpenDialog_IsAnAbsolutelyPositionedBox()
    {
        Assert.AreEqual("absolute", Box("<dialog id=x open>a</dialog>").Position);
    }
}
