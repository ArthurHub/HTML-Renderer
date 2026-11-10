using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Network;
using static HtmlRenderer.Test.Html.EmbeddedTestSupport;
using static HtmlRenderer.Test.Html.HtmlTestSupport;

namespace HtmlRenderer.Test.Html;

[TestClass]
public sealed class InfrastructureTests
{
    private const string DocumentUrl = "https://example.com/doc/index.html";

    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    [TestMethod]
    [Spec("html", "infrastructure.html#extensibility-2")]
    public void UnknownElementsAndAttributes_AreStyledByCss()
    {
        var box = Box("<style>foo[bar=baz] { display: block; color: green }</style><foo id=x bar=baz>a</foo>");

        Assert.AreEqual("block", box.Display);
        Assert.AreEqual(Green, box.ActualColor);
    }

    [TestMethod]
    [Spec("html", "infrastructure.html#xml")]
    public void HtmlElements_MatchTheHtmlNamespace()
    {
        Assert.AreEqual(Green, Box("<style>@namespace url(http://www.w3.org/1999/xhtml); div#x { color: green }</style><div id=x>a</div>").ActualColor);
    }

    [TestMethod]
    [Spec("html", "urls-and-fetching.html#resolving-urls")]
    [DataRow("a.png", "https://example.com/doc/a.png")]
    [DataRow("../img/a.png", "https://example.com/img/a.png")]
    [DataRow("/a.png", "https://example.com/a.png")]
    [DataRow("//cdn.example.org/a.png", "https://cdn.example.org/a.png")]
    [DataRow("?v=1", "https://example.com/doc/index.html?v=1")]
    [DataRow("a b.png", "https://example.com/doc/a%20b.png")]
    [DataRow("ä.png", "https://example.com/doc/%C3%A4.png")]
    [DataRow("http://other.example/a.png", "http://other.example/a.png")]
    [DataRow(" a.png ", "https://example.com/doc/a.png")]
    [DataRow("img\\a.png", "https://example.com/doc/img/a.png")]
    public void Url_IsParsedRelativeToTheDocument(string src, string expected)
    {
        CollectionAssert.AreEqual(new[] { expected }, Load($"<img src='{src}'>").Requested);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "urls-and-fetching.html#resolving-urls")]
    [DataRow("a\n.png")]
    [DataRow("a\t.png")]
    public void TabsAndNewlines_AreRemoved(string src)
    {
        CollectionAssert.AreEqual(new[] { "https://example.com/doc/a.png" }, Load($"<img src='{src}'>").Requested);
    }

    [TestMethod]
    [Spec("html", "urls-and-fetching.html#content-type-sniffing")]
    public void Image_IsDecodedWhateverItsContentType()
    {
        var doc = Load("<img id=x src=a.png>", new() { ["https://example.com/doc/a.png"] = "not a png" }, "text/plain");

        Assert.IsNotNull(ImageOf(doc.Box("x")));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "urls-and-fetching.html#content-type-sniffing")]
    [Spec("html", "links.html#link-type-stylesheet")]
    [DataRow("text/cssx")]
    [DataRow("application/x-text/css")]
    public void StyleSheet_OfAnotherEssenceThanTextCss_IsNotApplied(string contentType)
    {
        var doc = Load(
            "<link rel=stylesheet href=a.css><p id=x>a</p>",
            new() { ["https://example.com/doc/a.css"] = "p { color: green }" },
            contentType);

        Assert.AreNotEqual(Green, doc.Box("x").ActualColor);
    }

    [TestMethod]
    [Spec("html", "urls-and-fetching.html#lazy-loading-attributes")]
    public void LazyImage_IsLoadedWithoutScripting()
    {
        var doc = Load("<img id=x loading=lazy src=a.png style='margin-top: 10000px'>", new() { ["https://example.com/doc/a.png"] = "png" });

        CollectionAssert.AreEqual(new[] { "https://example.com/doc/a.png" }, doc.Requested);
        Assert.IsNotNull(ImageOf(doc.Box("x")));
    }

    private static Document Load(string body, Dictionary<string, string>? resources = null, string contentType = "text/css")
    {
        var loader = new StubLoader(resources ?? [], contentType);
        var container = new HtmlContainerInt(new MockAdapter { NetworkLoader = loader }) { MaxSize = new RSize(1000, 4000), AvoidAsyncImagesLoading = true, AvoidImagesLateLoading = true };
        container.SetHtml($"<!DOCTYPE html><html><head></head><body>{body}</body></html>").GetAwaiter().GetResult();
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
