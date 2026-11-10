using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Network;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssomTests
{
    private const string DocumentUrl = "https://example.com/doc/index.html";

    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    [TestMethod]
    [Spec("cssom-1", "#css-style-sheet-collections")]
    [DataRow("<style>p { color: red }</style><link rel=stylesheet href=green.css>")]
    [DataRow("<link rel=stylesheet href=red.css><style>p { color: green }</style>")]
    [DataRow("<link rel=stylesheet href=red.css><link rel=stylesheet href=green.css>")]
    public void StyleSheets_CascadeInTreeOrder(string head)
    {
        Assert.AreEqual(Green, ColorOf($"<!DOCTYPE html><html><head>{head}</head><body><p id=x>a</p></body></html>"));
    }

    [TestMethod]
    [Spec("cssom-1", "#css-style-sheet-collections")]
    public void PersistentStyleSheet_AppliesNextToTheTitledSet()
    {
        Assert.AreEqual(Green, ColorOf(
            "<!DOCTYPE html><html><head><style title=a>p { font-weight: bold }</style><style>p { color: green }</style></head>"
            + "<body><p id=x>a</p></body></html>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/cssom-1.yml")]
    [Spec("cssom-1", "#css-style-sheet-collections")]
    public void DefaultStylePragma_SelectsThePreferredStyleSheetSet()
    {
        Assert.AreEqual(Green, ColorOf(
            "<!DOCTYPE html><html><head><meta http-equiv=default-style content=b>"
            + "<style title=b>p { color: green }</style><style title=a>p { color: red }</style></head>"
            + "<body><p id=x>a</p></body></html>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/cssom-1.yml")]
    [Spec("cssom-1", "#fetching-css-style-sheets")]
    public void QuirksModeDocument_AppliesASameOriginStyleSheetOfAnotherContentType()
    {
        Assert.AreEqual(Green, ColorOf("<html><head><link rel=stylesheet href=green.css></head><body><p id=x>a</p></body></html>", "text/plain"));
    }

    private static RColor ColorOf(string html, string contentType = "text/css")
    {
        var resources = new Dictionary<string, string>
        {
            ["https://example.com/doc/green.css"] = "p { color: green }",
            ["https://example.com/doc/red.css"] = "p { color: red }",
        };
        var container = new HtmlContainerInt(new MockAdapter { NetworkLoader = new StubLoader(resources, contentType) })
        {
            MaxSize = new RSize(1000, 4000),
            AvoidAsyncImagesLoading = true
        };
        container.SetHtml(html).GetAwaiter().GetResult();
        using (var graphics = new RecordingGraphics())
            container.PerformLayout(graphics);

        return LayoutHarness.FindById(container.Root!, "x")!.ActualColor;
    }

    private sealed class StubLoader(Dictionary<string, string> resources, string contentType) : RNetworkLoader
    {
        public override RUri? BaseUri => new(DocumentUrl);

        public override Task<string> GetPrimaryContents() => Task.FromResult(string.Empty);

        public override Task<RNetworkResponse?> GetResourceStream(RUri uri)
        {
            if (!resources.TryGetValue(uri.AbsoluteUri, out var content))
                return Task.FromResult<RNetworkResponse?>(null);

            var headers = new Dictionary<string, string[]> { ["Content-Type"] = [contentType] };
            return Task.FromResult<RNetworkResponse?>(new RNetworkResponse(new MemoryStream(Encoding.UTF8.GetBytes(content)), headers));
        }
    }
}
