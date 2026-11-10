using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Network;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class DocumentMetadataTests
{
    private const string DocumentUrl = "https://example.com/doc/index.html";

    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    [TestMethod]
    [Spec("html", "semantics.html#the-html-element")]
    public void Html_IsTheRootOfTheDocument()
    {
        var html = Load("<!DOCTYPE html><html id=x><head></head><body><p>a</p></body></html>").Box("x");

        Assert.IsNull(html.ParentBox?.HtmlTag);
        CollectionAssert.AreEqual(new[] { "head", "body" }, html.Boxes.Select(b => b.HtmlTag?.Name).ToArray());
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-head-element")]
    public void MetadataInTheHead_IsProcessed()
    {
        var doc = Load(
            "<title>t</title><meta name=author content=a><link rel=stylesheet href=a.css><style>p { font-weight: bold }</style>",
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "p { color: green }" });

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
        Assert.AreEqual("bold", doc.Box("x").FontWeight);
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-base-element")]
    [Spec("html", "urls-and-fetching.html#document-base-urls")]
    public void BaseHref_IsTheBaseForRelativeUrls()
    {
        var doc = Load("<base href='https://example.com/img/'>", "<img src=a.png>");

        CollectionAssert.Contains(doc.Requested, "https://example.com/img/a.png");
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-base-element")]
    [Spec("html", "urls-and-fetching.html#document-base-urls")]
    public void WithoutABase_RelativeUrlsResolveAgainstTheDocumentUrl()
    {
        var doc = Load("<link rel=stylesheet href=a.css>", "<img src=a.png>");

        CollectionAssert.Contains(doc.Requested, "https://example.com/doc/a.css");
        CollectionAssert.Contains(doc.Requested, "https://example.com/doc/a.png");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#the-base-element")]
    [Spec("html", "urls-and-fetching.html#document-base-urls")]
    public void BaseHref_IsTheBaseForStyleSheetLinks()
    {
        var doc = Load("<base href='https://example.com/css/'><link rel=stylesheet href=a.css>", "<p id=x>a</p>");

        CollectionAssert.Contains(doc.Requested, "https://example.com/css/a.css");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#the-base-element")]
    [Spec("html", "urls-and-fetching.html#document-base-urls")]
    public void RelativeBaseHref_IsResolvedAgainstTheDocumentUrl()
    {
        var doc = Load("<base href='img/'>", "<img src=a.png>");

        CollectionAssert.Contains(doc.Requested, "https://example.com/doc/img/a.png");
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#the-base-element")]
    [Spec("html", "urls-and-fetching.html#document-base-urls")]
    public void FirstBaseWithAnHref_IsUsed()
    {
        var doc = Load("<base target=_blank><base href='https://example.com/img/'>", "<img src=a.png>");

        CollectionAssert.Contains(doc.Requested, "https://example.com/img/a.png");
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-link-element")]
    [Spec("html", "semantics.html#fetching-and-processing-a-resource-from-a-link-element")]
    public void StyleSheetLink_IsFetchedAndApplied()
    {
        var doc = Load(
            "<link rel=stylesheet href=a.css><link rel=StyleSheet href=b.css>",
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "p { color: green }", ["https://example.com/doc/b.css"] = "p { font-weight: bold }" });

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
        Assert.AreEqual("bold", doc.Box("x").FontWeight);
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-link-element")]
    public void LinkWithAnotherRelation_IsNotFetched()
    {
        var doc = Load("<link rel=icon href=a.css><link href=b.css>", "<p id=x>a</p>");

        CollectionAssert.AreEqual(Array.Empty<string>(), doc.Requested);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#the-link-element")]
    public void RelWithSeveralKeywords_StillLinksTheStyleSheet()
    {
        var doc = Load(
            "<link rel='next stylesheet' href=a.css>",
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "p { color: green }" });

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#the-link-element")]
    [Spec("html", "semantics.html#the-style-element")]
    [Spec("cssom-1", "#css-style-sheet-collections")]
    [DataRow("<link rel=stylesheet title=a href=a.css><link rel=stylesheet title=b href=b.css>")]
    [DataRow("<style title=a>p { color: green }</style><style title=b>p { color: red }</style>")]
    public void StyleSheetWithAnotherTitle_ThanThePreferredSet_IsDisabled(string head)
    {
        var doc = Load(
            head,
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "p { color: green }", ["https://example.com/doc/b.css"] = "p { color: red }" });

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Spec("html", "semantics.html#processing-the-type-attribute")]
    [Spec("cssom-1", "#fetching-css-style-sheets")]
    [Spec("html", "urls-and-fetching.html#content-type-sniffing")]
    public void LinkedResourceOfAnotherContentType_IsNotApplied()
    {
        var doc = Load(
            "<link rel=stylesheet href=a.txt>",
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.txt"] = "p { color: green }" },
            contentType: "text/plain");

        Assert.AreNotEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#processing-the-type-attribute")]
    public void LinkWithAnUnsupportedType_IsNotFetched()
    {
        var doc = Load(
            "<link rel=stylesheet type=text/plain href=a.css>",
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "p { color: green }" });

        CollectionAssert.AreEqual(Array.Empty<string>(), doc.Requested);
    }

    [TestMethod]
    [Spec("html", "semantics.html#fetching-and-processing-a-resource-from-a-link-element")]
    public void MissingStyleSheet_IsSkipped()
    {
        var doc = Load("<link rel=stylesheet href=missing.css><style>p { color: green }</style>", "<p id=x>a</p>");

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#fetching-and-processing-a-resource-from-a-link-element")]
    [Spec("html", "semantics.html#the-style-element")]
    [DataRow("<link rel=stylesheet href=a.css>")]
    [DataRow("<style>@import url(b.css);</style>")]
    public void ImportedStyleSheet_IsLoaded(string head)
    {
        var doc = Load(
            head,
            "<p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "@import url(b.css);", ["https://example.com/doc/b.css"] = "p { color: green }" });

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-style-element")]
    public void StyleElement_AppliesWhereverItIs()
    {
        var doc = Load(
            "<style>p { color: green }</style>",
            "<p id=x>a</p><div><style>p { font-weight: bold }</style></div>");

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
        Assert.AreEqual("bold", doc.Box("x").FontWeight);
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-style-element")]
    public void StyleElement_TextWithALessThanSign_IsOneStyleSheet()
    {
        var doc = Load("<style>#a { --x: \"<\"; } p { color: green }</style>", "<p id=x>a</p>");

        Assert.AreEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Spec("html", "semantics.html#the-style-element")]
    [DataRow("")]
    [DataRow("text/css")]
    [DataRow("TEXT/CSS")]
    public void StyleElement_WithACssType_Applies(string type)
    {
        Assert.AreEqual(Green, Load($"<style type='{type}'>p {{ color: green }}</style>", "<p id=x>a</p>").Box("x").ActualColor);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "semantics.html#the-style-element")]
    [DataRow("text/plain")]
    [DataRow("text/css; charset=utf-8")]
    public void StyleElement_WithAnotherType_IsIgnored(string type)
    {
        Assert.AreNotEqual(Green, Load($"<style type='{type}'>p {{ color: green }}</style>", "<p id=x>a</p>").Box("x").ActualColor);
    }

    private static Document Load(string head, string body, Dictionary<string, string>? resources = null, string contentType = "text/css") =>
        Load($"<!DOCTYPE html><html><head>{head}</head><body>{body}</body></html>", resources, contentType);

    private static Document Load(string html, Dictionary<string, string>? resources = null, string contentType = "text/css")
    {
        var loader = new StubLoader(resources ?? [], contentType);
        var container = new HtmlContainerInt(new MockAdapter { NetworkLoader = loader }) { MaxSize = new RSize(1000, 4000), AvoidAsyncImagesLoading = true };
        container.SetHtml(html).GetAwaiter().GetResult();
        using (var graphics = new RecordingGraphics())
            container.PerformLayout(graphics);

        Assert.IsNotNull(container.Root);
        return new Document(container.Root!, loader.Requested);
    }

    private sealed record Document(CssBox Root, List<string> Requested)
    {
        public CssBox Box(string id)
        {
            var box = LayoutHarness.FindById(Root, id);
            Assert.IsNotNull(box, $"No box with id '{id}'");
            return box;
        }
    }

    /// <summary>Serves <c>DocumentUrl</c> as the document's URL and the given resources from memory.</summary>
    private sealed class StubLoader(Dictionary<string, string> resources, string contentType) : RNetworkLoader
    {
        public List<string> Requested { get; } = [];

        public override RUri? BaseUri => new(DocumentUrl);

        public override Task<string> GetPrimaryContents() => Task.FromResult(string.Empty);

        public override Task<RNetworkResponse?> GetResourceStream(RUri uri)
        {
            Requested.Add(uri.AbsoluteUri);
            if (!resources.TryGetValue(uri.AbsoluteUri, out var content))
                return Task.FromResult<RNetworkResponse?>(null);

            var headers = new Dictionary<string, string[]> { ["Content-Type"] = [contentType] };
            return Task.FromResult<RNetworkResponse?>(new RNetworkResponse(new MemoryStream(Encoding.UTF8.GetBytes(content)), headers));
        }
    }
}
