using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Network;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class MediaRefreshTests
{
    private const string StyleSheetUrl = "https://example.com/style.css";

    private static readonly RColor Red = RColor.FromArgb(255, 0, 0);
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    [TestMethod]
    [Spec("css-conditional-3", "#processing")]
    public void ResizingAcrossABreakpoint_AppliesTheRulesThatMatchNow()
    {
        var container = Load("<style>p { color: red } @media (max-width: 400px) { p { color: green } }</style>", new CountingLoader());

        Assert.AreEqual(Red, ColorAt(container, 800));
        Assert.AreEqual(Green, ColorAt(container, 300));
        Assert.AreEqual(Red, ColorAt(container, 800));
    }

    [TestMethod]
    [Spec("css-conditional-3", "#processing")]
    public void ResizingAcrossABreakpoint_DoesntLoadTheStyleSheetsAgain()
    {
        var loader = new CountingLoader { StyleSheet = "p { color: red } @media (max-width: 400px) { p { color: green } }" };
        var container = Load($"<link rel=stylesheet href='{StyleSheetUrl}'>", loader);

        Assert.AreEqual(Red, ColorAt(container, 800));
        Assert.AreEqual(Green, ColorAt(container, 300));
        Assert.AreEqual(1, loader.Requests.Count(r => r == StyleSheetUrl));
    }

    private static HtmlContainerInt Load(string head, CountingLoader loader)
    {
        var container = new HtmlContainerInt(new MockAdapter { NetworkLoader = loader }) { MaxSize = new RSize(800, 600) };
        container.SetHtmlAsync($"<html><head>{head}</head><body><p id=x>a</p></body></html>").GetAwaiter().GetResult();
        return container;
    }

    private static RColor ColorAt(HtmlContainerInt container, double width)
    {
        container.MaxSize = new RSize(width, 600);
        container.PerformLayout(new RecordingGraphics());

        var box = LayoutHarness.FindById(container.Root!, "x");
        Assert.IsNotNull(box);
        return box.ActualColor;
    }

    private sealed class CountingLoader : RNetworkLoader
    {
        public string StyleSheet { get; init; } = string.Empty;

        public List<string> Requests { get; } = [];

        public override RUri? BaseUri => new("https://example.com/index.html");

        public override Task<string> GetPrimaryContentsAsync() => Task.FromResult(string.Empty);

        public override Task<RNetworkResponse?> GetResourceStreamAsync(RUri uri)
        {
            Requests.Add(uri.AbsoluteUri);
            if (uri.AbsoluteUri != StyleSheetUrl)
                return Task.FromResult<RNetworkResponse?>(null);

            var headers = new Dictionary<string, string[]> { ["Content-Type"] = ["text/css"] };
            return Task.FromResult<RNetworkResponse?>(new RNetworkResponse(new MemoryStream(Encoding.UTF8.GetBytes(StyleSheet)), headers));
        }
    }
}
