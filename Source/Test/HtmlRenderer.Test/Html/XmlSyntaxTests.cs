using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class XmlSyntaxTests
{
    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "xhtml.html#parsing-xhtml-documents")]
    public void XmlDocument_CdataSectionIsText()
    {
        Assert.AreEqual("a<b", Text(Xml("<p id=\"x\"><![CDATA[a<b]]></p>")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "xhtml.html#parsing-xhtml-documents")]
    public void XmlDocument_PrefixedElementIsInTheHtmlNamespace()
    {
        Assert.AreEqual("block", Xml("<h:p xmlns:h=\"http://www.w3.org/1999/xhtml\" id=\"x\">a</h:p>").Display);
    }

    /// <summary>Lays out an XHTML document with the body markup and returns the box with id "x".</summary>
    private static CssBox Xml(string body)
    {
        var (root, _) = LayoutHarness.Layout(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title>t</title></head><body>" + body + "</body></html>");
        return LayoutHarness.FindById(root, "x")!;
    }

    private static string Text(CssBox box) =>
        string.Concat(LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text)).Trim();
}
