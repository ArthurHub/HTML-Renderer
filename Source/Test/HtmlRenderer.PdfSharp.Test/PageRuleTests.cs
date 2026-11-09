using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// An A4 page is 842pt tall, the 400pt block fits on one page unless the margins leave less than 400pt.
/// </summary>
// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class PageRuleTests
{
    private const string Block = "<div style='height:400pt'></div>";

    private static async Task<int> PageCount(string html, int marginTop = 0, int marginBottom = 0)
    {
        var config = new PdfGenerateConfig { PageSize = PageSize.A4, MarginTop = marginTop, MarginBottom = marginBottom };

        using var document = await PdfGenerator.GeneratePdf(html, config);

        return document.Pages.Count;
    }

    [TestMethod]
    [Spec("css21", "page.html#page-box")]
    public async Task PageArea_IsThePageMinusTheConfiguredMargins()
    {
        Assert.AreEqual(1, await PageCount($"<html><body style='margin:0'>{Block}</body></html>"));
        Assert.AreEqual(2, await PageCount($"<html><body style='margin:0'>{Block}</body></html>", 250, 250));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#page-margins")]
    [DataRow("@page { margin: 250pt 0 }")]
    [DataRow("@page { margin-top: 30%; margin-bottom: 30% }")]
    public async Task PageMargins_ShrinkThePageArea(string rule)
    {
        Assert.AreEqual(2, await PageCount($"<html><head><style>{rule}</style></head><body style='margin:0'>{Block}</body></html>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#page-selectors")]
    [DataRow("@page :first { margin-top: 500pt }")]
    [DataRow("@page :right { margin-top: 500pt }")]
    public async Task PageSelector_AppliesToTheFirstPage(string rule)
    {
        Assert.AreEqual(2, await PageCount($"<html><head><style>{rule}</style></head><body style='margin:0'>{Block}</body></html>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "page.html#page-cascade")]
    public async Task PageContext_FollowsTheCascade()
    {
        const string rules = "@page :first { margin-top: 500pt } @page { margin-top: 0 }";

        Assert.AreEqual(2, await PageCount($"<html><head><style>{rules}</style></head><body style='margin:0'>{Block}</body></html>"));
    }
}
