using HtmlRenderer.Test.TestSupport;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class SelectorMatchingTests
{
    private const string Marked = "rgb(1, 2, 3)";

    private static bool Matches(string selector, string body) =>
        Box($"<style>{selector} {{ color: rgb(1, 2, 3) }}</style>{body}").Color == Marked;

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics-other.html#case-sensitivity-of-the-css-'attr()'-function")]
    public void Attr_MatchesAttributeNamesCaseInsensitively()
    {
        var box = Box("<style>p::before { content: attr(TITLE) }</style><p id=x title=t>a</p>");

        Assert.AreEqual("t", box.Boxes.Single(b => b.IsBeforePseudoElement).Text);
    }

    [TestMethod]
    [Spec("html", "semantics-other.html#case-sensitivity-of-selectors")]
    [DataRow("P", "<p id=x>a</p>")]
    [DataRow("[TITLE]", "<p id=x title=a>a</p>")]
    [DataRow("[type=TEXT]", "<p id=x type=text>a</p>")]
    [DataRow("[align=LEFT]", "<p id=x align=left>a</p>")]
    public void ElementNames_AttributeNames_AndListedAttributeValues_MatchCaseInsensitively(string selector, string body)
    {
        Assert.IsTrue(Matches(selector, body));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics-other.html#case-sensitivity-of-selectors")]
    [DataRow("[title=Foo]", "<p id=x title=foo>a</p>")]
    [DataRow("[class=Foo]", "<p id=x class=foo>a</p>")]
    public void OtherAttributeValues_MatchCaseSensitively(string selector, string body)
    {
        Assert.IsFalse(Matches(selector, body));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics-other.html#case-sensitivity-of-selectors")]
    [DataRow("#X")]
    [DataRow(".Foo")]
    public void IdsAndClasses_MatchCaseSensitivelyInNoQuirksMode(string selector)
    {
        var (root, _) = LayoutHarness.Layout(
            $"<!DOCTYPE html><html><head><style>{selector} {{ color: rgb(1, 2, 3) }}</style></head>" +
            "<body><p id=x class=foo>a</p></body></html>");

        Assert.AreNotEqual(Marked, LayoutHarness.FindById(root, "x")!.Color);
    }

    [TestMethod]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Link_MatchesAnchorsWithAnHref()
    {
        Assert.IsTrue(Matches(":link", "<a id=x href=#a>a</a>"));
        Assert.IsFalse(Matches(":link", "<a id=x>a</a>"));
    }

    [TestMethod]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    [DataRow(":focus", "<input id=x>")]
    [DataRow(":target", "<p id=x>a</p>")]
    [DataRow(":modal", "<dialog id=x open>a</dialog>")]
    [DataRow(":popover-open", "<p id=x popover>a</p>")]
    [DataRow(":autofill", "<input id=x>")]
    [DataRow(":user-invalid", "<input id=x required>")]
    [DataRow(":playing", "<video id=x autoplay></video>")]
    public void PseudoClassesForUserInteractionAndScripts_NeverMatch(string selector, string body)
    {
        Assert.IsFalse(Matches(selector, body));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    public void Link_MatchesAreasWithAnHref()
    {
        Assert.IsTrue(Matches(":link", "<map><area id=x href=#a></map>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics-other.html#pseudo-classes")]
    [DataRow(":defined", "<p id=x>a</p>")]
    [DataRow(":enabled", "<input id=x>")]
    [DataRow(":disabled", "<input id=x disabled>")]
    [DataRow(":disabled", "<fieldset disabled><input id=x></fieldset>")]
    [DataRow(":checked", "<input id=x type=checkbox checked>")]
    [DataRow(":checked", "<select><option id=x selected>a</option></select>")]
    [DataRow(":indeterminate", "<progress id=x></progress>")]
    [DataRow(":default", "<input id=x type=radio checked>")]
    [DataRow(":placeholder-shown", "<input id=x placeholder=a>")]
    [DataRow(":required", "<input id=x required>")]
    [DataRow(":optional", "<input id=x>")]
    [DataRow(":read-only", "<p id=x>a</p>")]
    [DataRow(":read-write", "<textarea id=x></textarea>")]
    [DataRow(":valid", "<input id=x>")]
    [DataRow(":invalid", "<input id=x required>")]
    [DataRow(":in-range", "<input id=x type=number min=1 max=5 value=3>")]
    [DataRow(":out-of-range", "<input id=x type=number min=1 max=5 value=9>")]
    [DataRow(":dir(ltr)", "<p id=x>a</p>")]
    [DataRow(":heading", "<h1 id=x>a</h1>")]
    [DataRow(":open", "<details id=x open>a</details>")]
    [DataRow(":paused", "<video id=x></video>")]
    [DataRow(":muted", "<video id=x muted></video>")]
    public void PseudoClassesForTheDocumentsState_Match(string selector, string body)
    {
        Assert.IsTrue(Matches(selector, body));
    }
}
