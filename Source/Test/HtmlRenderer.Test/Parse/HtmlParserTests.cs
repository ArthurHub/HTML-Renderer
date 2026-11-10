using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Parse;

[TestClass]
public sealed class HtmlParserTests
{
    private static CssBox Find(CssBox box, string tagName)
    {
        if (box.HtmlTag?.Name == tagName)
            return box;

        foreach (var child in box.Boxes)
        {
            var found = Find(child, tagName);
            if (found != null)
                return found;
        }

        return null!;
    }

    private static string Text(CssBox box) =>
        box.HtmlTag == null && box.Text != null ? box.Text : string.Concat(box.Boxes.Select(Text));

    [TestMethod]
    [DataRow("<body><noscript>Enable JS</noscript></body>")]
    [DataRow("<html><head><noscript>Enable JS</noscript></head><body></body></html>")]
    public void Noscript_TextIsParsedAsText(string html)
    {
        var noscript = Find(HtmlParser.ParseDocument(html), "noscript");

        Assert.AreEqual("Enable JS", Text(noscript));
    }

    [TestMethod]
    public void Noscript_MarkupKeepsItsStructure()
    {
        var noscript = Find(HtmlParser.ParseDocument("<noscript><p><b>a</b><i>b</i></p>c</noscript>"), "noscript");

        Assert.AreEqual(2, noscript.Boxes.Count);
        Assert.AreEqual("p", noscript.Boxes[0].HtmlTag?.Name);
        Assert.AreEqual("ab", Text(noscript.Boxes[0]));
        Assert.AreEqual("c", Text(noscript.Boxes[1]));
    }
}
