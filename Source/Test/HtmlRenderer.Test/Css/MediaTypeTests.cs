using System.Text;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class MediaTypeTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    private static CssBox Box(string head, string mediaType = "screen")
    {
        var html = $"<html><head>{head}</head><body><p id=x>a</p></body></html>";
        var (root, _) = LayoutHarness.Layout(html, adapter: new MockAdapter { MediaType = mediaType });
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }

    private static RColor ColorOf(string head, string mediaType = "screen") => Box(head, mediaType).ActualColor;

    private static string DataCss(string css) =>
        "data:text/css;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(css));

    [TestMethod]
    [Spec("css21", "media.html#media-sheets")]
    public void LinkWithMatchingMedia_Applies()
    {
        Assert.AreEqual(Green, ColorOf($"<link rel=stylesheet media=screen href='{DataCss("p { color: green }")}'>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "media.html#media-sheets")]
    public void LinkWithOtherMedia_IsIgnored()
    {
        Assert.AreNotEqual(Green, ColorOf($"<link rel=stylesheet media='print, handheld' href='{DataCss("p { color: green }")}'>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "media.html#media-sheets")]
    public void StyleWithOtherMedia_IsIgnored()
    {
        Assert.AreNotEqual(Green, ColorOf("<style media=print>p { color: green }</style>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "media.html#media-sheets")]
    public void ImportWithMatchingMedia_Applies()
    {
        Assert.AreEqual(Green, ColorOf($"<style>@import url({DataCss("p { color: green }")}) screen;</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    [DataRow("print", "10pt")]
    [DataRow("screen", "13px")]
    public void AtMedia_SelectsTheRulesForTheMediaType(string mediaType, string fontSize)
    {
        var box = Box("""
            <style>
              @media print { p { font-size: 10pt } }
              @media screen { p { font-size: 13px } }
              @media screen, print { p { color: green } }
            </style>
            """, mediaType);

        Assert.AreEqual(fontSize, box.FontSize);
        Assert.AreEqual(Green, box.ActualColor);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void RulesOutsideAtMedia_ApplyToAllMediaTypes()
    {
        Assert.AreEqual(Green, ColorOf("<style>p { color: green } @media screen { div { color: red } }</style>", "print"));
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void InvalidStatementInsideAtMedia_IsIgnored()
    {
        Assert.AreEqual(Green, ColorOf("<style>@media screen { p..x { color: red } p { color: green } }</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    [DataRow("braille")]
    [DataRow("embossed")]
    [DataRow("handheld")]
    [DataRow("print")]
    [DataRow("projection")]
    [DataRow("screen")]
    [DataRow("speech")]
    [DataRow("tty")]
    [DataRow("tv")]
    public void RecognizedMediaType_MatchesItself(string mediaType)
    {
        Assert.AreEqual(Green, ColorOf($"<style>@media {mediaType} {{ p {{ color: green }} }}</style>", mediaType));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    [DataRow("screen")]
    [DataRow("print")]
    public void All_MatchesEveryMediaType(string mediaType)
    {
        Assert.AreEqual(Green, ColorOf("<style>@media all { p { color: green } }</style>", mediaType));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    [DataRow("SCREEN")]
    [DataRow("Screen")]
    public void MediaTypeNames_AreCaseInsensitive(string name)
    {
        Assert.AreEqual(Green, ColorOf($"<style>@media {name} {{ p {{ color: green }} }}</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    public void MediaType_OtherThanTheRenderedOne_DoesNotMatch()
    {
        Assert.AreNotEqual(Green, ColorOf("<style>@media tv { p { color: green } }</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    [DataRow("holographic, screen")]
    [DataRow("screen, holographic")]
    public void UnknownMediaType_IsTreatedAsNotPresent(string mediaList)
    {
        Assert.AreEqual(Green, ColorOf($"<style>@media {mediaList} {{ p {{ color: green }} }}</style>"));
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css21.yml")]
    [Spec("css21", "media.html#media-types")]
    public void MalformedEntryInAMediaList_DoesNotDisableTheOthers()
    {
        Assert.AreEqual(Green, ColorOf("<style>@media screen, 3D { p { color: green } }</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    public void OnlyUnknownMediaTypes_DoNotMatch()
    {
        Assert.AreNotEqual(Green, ColorOf("<style>@media holographic { p { color: green } }</style>"));
    }

    [TestMethod]
    [Spec("css21", "media.html#media-types")]
    public void MalformedMediaType_InvalidatesTheStatement()
    {
        Assert.AreNotEqual(Green, ColorOf("<style>@media @screen { p { color: green } }</style>"));
    }
}
