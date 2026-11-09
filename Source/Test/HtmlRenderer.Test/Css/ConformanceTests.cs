using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ConformanceTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    private static RColor ColorOf(string head, MockAdapter? adapter = null)
    {
        var html = $"<html><head>{head}</head><body><p id=x>a</p></body></html>";
        var (root, _) = LayoutHarness.Layout(html, adapter: adapter);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box.ActualColor;
    }

    private static string DataUri(string mediaType, string content) =>
        $"data:{mediaType};base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

    [TestMethod]
    [Spec("css21", "conform.html#conformance")]
    public void MissingStyleSheet_TheOthersStillApply()
    {
        Assert.AreEqual(Green, ColorOf("<link rel=stylesheet href='missing.css'><style>p { color: green }</style>"));
    }

    [TestMethod]
    [Spec("css21", "conform.html#conformance")]
    public void DeclarationWithAnInvalidValue_IsIgnored()
    {
        Assert.AreEqual(Green, ColorOf("<style>p { color: green } p { color: nonsense }</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void RulesForAnotherMediaType_AreIgnored()
    {
        Assert.AreEqual(Green, ColorOf("<style>p { color: green } @media print { p { color: red } }</style>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "conform.html#conformance")]
    public void UserStyleSheet_ImportantBeatsAuthorImportant()
    {
        var adapter = new MockAdapter();
        var container = new HtmlContainerInt(adapter) { MaxSize = new RSize(1000, 4000) };
        container.SetHtml(
            "<html><head><style>p { color: red !important }</style></head><body><p id=x>a</p></body></html>",
            CssData.Parse(adapter, "p { color: green !important }")).GetAwaiter().GetResult();
        using var graphics = new RecordingGraphics();
        container.PerformLayout(graphics);

        Assert.AreEqual(Green, LayoutHarness.FindById(container.Root!, "x")!.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "conform.html#text-css")]
    public void TextCssResource_IsUsedAsAStyleSheet()
    {
        Assert.AreEqual(Green, ColorOf($"<link rel=stylesheet href='{DataUri("text/css", "p { color: green }")}'>"));
    }

    [TestMethod]
    [Spec("css21", "conform.html#text-css")]
    public void ResourceOfAnotherContentType_IsNotUsedAsAStyleSheet()
    {
        Assert.AreNotEqual(Green, ColorOf($"<link rel=stylesheet href='{DataUri("text/plain", "p { color: green }")}'>"));
    }
}
