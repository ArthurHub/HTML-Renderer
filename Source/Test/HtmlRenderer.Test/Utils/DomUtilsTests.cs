using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.Test.Utils;

[TestClass]
public sealed class DomUtilsTests
{
    [TestMethod]
    public void GetBoxById_FindsMatchingElement()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='outer'><span id='inner'>Text</span></div>"));

        var found = DomUtils.GetBoxById(root, "inner");

        Assert.IsNotNull(found);
        Assert.AreEqual("span", found!.HtmlTag!.Name);
    }

    [TestMethod]
    public void GetBoxById_UnknownId_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='outer'></div>"));

        Assert.IsNull(DomUtils.GetBoxById(root, "missing"));
    }

    [TestMethod]
    public void GetBoxById_NullOrEmptyId_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='outer'></div>"));

        Assert.IsNull(DomUtils.GetBoxById(root, null));
        Assert.IsNull(DomUtils.GetBoxById(root, string.Empty));
    }

    [TestMethod]
    public void FindParent_ReturnsParentOfAncestorMatchingTagName()
    {
        // FindParent walks up from `box` looking for an ancestor tagged `tagName`, then returns
        // *that ancestor's own parent* (not the matched ancestor itself) -- so searching for
        // "div" from a <span> nested one level inside a <div> returns the div's parent (<body>).
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><span id='inner'>Text</span></div>"));
        var span = DomUtils.GetBoxById(root, "inner")!;

        var parent = DomUtils.FindParent(root, "div", span);

        Assert.AreEqual("body", parent!.HtmlTag!.Name);
    }

    [TestMethod]
    public void FindParent_NullBox_ReturnsRoot()
    {
        // A null box goes straight to root, there's no separate "not found" result.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div></div>"));

        var parent = DomUtils.FindParent(root, "div", null);

        Assert.AreSame(root, parent);
    }

    [TestMethod]
    public void GetPreviousSibling_ReturnsPrecedingBox()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><p id='a'>A</p><p id='b'>B</p></div>"));
        var b = DomUtils.GetBoxById(root, "b")!;

        var previous = DomUtils.GetPreviousSibling(b);

        Assert.IsNotNull(previous);
        Assert.AreEqual("a", previous!.HtmlTag!.TryGetAttribute("id"));
    }

    [TestMethod]
    public void GetPreviousSibling_FirstChild_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><p id='a'>A</p></div>"));
        var a = DomUtils.GetBoxById(root, "a")!;

        Assert.IsNull(DomUtils.GetPreviousSibling(a));
    }

    [TestMethod]
    public void GetFollowingSiblings_ReturnsMatchingLaterSiblings()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><p id='a'>A</p><p id='b'>B</p><p id='c'>C</p></div>"));
        var a = DomUtils.GetBoxById(root, "a")!;

        var following = DomUtils.GetFollowingSiblings(a, _ => true, isConsecutive: false).ToList();

        Assert.AreEqual(2, following.Count);
    }

    [TestMethod]
    public void ContainsInlinesOnly_AllInlineChildren_ReturnsTrue()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='outer'><span>A</span><span>B</span></div>"));
        var div = DomUtils.GetBoxById(root, "outer")!;

        Assert.IsTrue(DomUtils.ContainsInlinesOnly(div));
    }

    [TestMethod]
    public void ContainsInlinesOnly_HasBlockChild_ReturnsFalse()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='outer'><p>Block</p></div>"));
        var div = DomUtils.GetBoxById(root, "outer")!;

        Assert.IsFalse(DomUtils.ContainsInlinesOnly(div));
    }

    [TestMethod]
    public void GetAllLinkBoxes_CollectsClickableVisibleBoxes()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div><a href='#'>Link</a><span>Not a link</span></div>"));

        var links = new System.Collections.Generic.List<CssBox>();
        DomUtils.GetAllLinkBoxes(root, links);

        Assert.IsTrue(links.Any(b => b.HtmlTag?.Name == "a"));
    }

    [TestMethod]
    public void GetCssBox_LocationInsideBounds_ReturnsABox()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='outer' style='width:100px;height:50px'><span id='inner'>Text</span></div>"));
        var outer = DomUtils.GetBoxById(root, "outer")!;
        var point = new RPoint(outer.Bounds.X + 1, outer.Bounds.Y + 1);

        var found = DomUtils.GetCssBox(root, point);

        Assert.IsNotNull(found);
    }

    [TestMethod]
    public void GetCssBox_InvisibleBox_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='hidden' style='visibility:hidden;height:20px'>x</div>"));
        var hidden = DomUtils.GetBoxById(root, "hidden")!;
        var point = new RPoint(hidden.Bounds.X + 1, hidden.Bounds.Y + 1);

        Assert.IsNull(DomUtils.GetCssBox(hidden, point));
    }

    [TestMethod]
    public void GetLinkBox_LocationOnClickableVisibleLink_ReturnsIt()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<a href='#'>Click</a>"));
        var link = LayoutHarness.Descendants(root).First(b => b.HtmlTag?.Name == "a");
        var word = FindFirstWord(link)!;
        var point = new RPoint(word.Rectangle.X + word.Rectangle.Width / 2, word.Rectangle.Y + word.Rectangle.Height / 2);

        var found = DomUtils.GetLinkBox(root, point);

        Assert.IsNotNull(found);
        Assert.AreEqual("a", found!.HtmlTag!.Name);
    }

    [TestMethod]
    public void GetLinkBox_LocationAwayFromAnyLink_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<a href='#'>Click</a>"));

        Assert.IsNull(DomUtils.GetLinkBox(root, new RPoint(-1000, -1000)));
    }

    [TestMethod]
    public void GetCssBoxWord_LocationOnVisibleWord_ReturnsWord()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Hello</p>"));
        var p = DomUtils.GetBoxById(root, "p")!;
        var word = FindFirstWord(p)!;
        var point = new RPoint(word.Rectangle.X + word.Rectangle.Width / 2, word.Rectangle.Y + word.Rectangle.Height / 2);

        Assert.IsNotNull(DomUtils.GetCssBoxWord(root, point));
    }

    [TestMethod]
    public void GetCssBoxWord_InvisibleBox_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p' style='visibility:hidden'>Hello</p>"));
        var p = DomUtils.GetBoxById(root, "p")!;
        var word = FindFirstWord(p)!;
        var point = new RPoint(word.Rectangle.X + word.Rectangle.Width / 2, word.Rectangle.Y + word.Rectangle.Height / 2);

        Assert.IsNull(DomUtils.GetCssBoxWord(p, point));
    }

    [TestMethod]
    public void GetCssLineBox_LocationOnLine_ReturnsLine()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Hello world</p>"));
        var p = DomUtils.GetBoxById(root, "p")!;
        var word = FindFirstWord(p)!;
        var point = new RPoint(word.Rectangle.X + word.Rectangle.Width / 2, word.Rectangle.Y + word.Rectangle.Height / 2);

        var line = DomUtils.GetCssLineBox(root, point);

        Assert.AreSame(p.LineBoxes[0], line);
    }

    [TestMethod]
    public void GetCssLineBox_LocationAboveAllContent_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='p'>Hello world</p>"));

        var line = DomUtils.GetCssLineBox(root, new RPoint(0, -1000));

        Assert.IsNull(line);
    }

    [TestMethod]
    public void GetCssLineBox_NullBox_ReturnsNull()
    {
        Assert.IsNull(DomUtils.GetCssLineBox(null, new RPoint(0, 0)));
    }

    [TestMethod]
    public void GetCssLineBox_TableCellOutsideBounds_ReturnsNull()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<table><tr><td id='cell'>Cell text</td></tr></table>"));
        var cell = DomUtils.GetBoxById(root, "cell")!;

        var line = DomUtils.GetCssLineBox(cell, new RPoint(-1000, -1000));

        Assert.IsNull(line);
    }

    [TestMethod]
    public void IsProperTableChild_TableRow_ReturnsTrue()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<table><tr id='row'><td>Cell</td></tr></table>"));
        var row = DomUtils.GetBoxById(root, "row")!;

        Assert.IsTrue(DomUtils.IsProperTableChild(row));
    }

    [TestMethod]
    public void IsProperTableChild_NonTableBox_ReturnsFalse()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<div id='plain'></div>"));
        var div = DomUtils.GetBoxById(root, "plain")!;

        Assert.IsFalse(DomUtils.IsProperTableChild(div));
    }

    // --- Helper ---

    private static CssRect? FindFirstWord(CssBox box)
    {
        if (box.Words.Count > 0) return box.Words[0];
        foreach (var child in box.Boxes)
        {
            var found = FindFirstWord(child);
            if (found is not null) return found;
        }
        return null;
    }
}
