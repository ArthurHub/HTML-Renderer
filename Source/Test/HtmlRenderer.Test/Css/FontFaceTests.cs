using System.Text;
using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Network;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class FontFaceTests
{
    [TestMethod]
    [Spec("css-fonts-4", "#font-face-rule")]
    [Spec("css-fonts-4", "#src-desc")]
    [Spec("css-fonts-4", "#font-face-src-parsing")]
    [Spec("css-fonts-5", "#font-face-src-parsing")]
    public void FontFaceOpenSansWithSource()
    {
        var src = "@font-face{font-family:'Open Sans';src:url(fonts/OpenSans-Light.eot);src:local('Open Sans Light'),local('OpenSans-Light'),url(fonts/OpenSans-Light.ttf) format('truetype'),url(fonts/OpenSans-Light.woff) format('woff');font-style:normal}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<FontFaceRule>(sheet.Rules[0]);
        var fontface = (IFontFaceRule)sheet.Rules[0];
        Assert.AreEqual("\"Open Sans\"", fontface.Family);
        Assert.AreEqual("", fontface.Features);
        Assert.AreEqual("", fontface.Range);
        Assert.AreNotEqual("", fontface.Source);
        Assert.AreEqual("", fontface.Stretch);
        Assert.AreEqual("normal", fontface.Style);
        Assert.AreEqual("", fontface.Variant);
        Assert.AreEqual("", fontface.Weight);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#src-desc")]
    [Spec("css-fonts-4", "#font-face-src-parsing")]
    [Spec("css-fonts-5", "#font-face-src-parsing")]
    public void FontFaceWithWoff2Source()
    {
        var src = "@font-face{font-family:'Inter';src:url(fonts/Inter-Medium.woff2) format('woff2');font-weight:500}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<FontFaceRule>(sheet.Rules[0]);
        var fontface = (IFontFaceRule)sheet.Rules[0];
        Assert.AreEqual("\"Inter\"", fontface.Family);
        StringAssert.Contains(fontface.Source, "Inter-Medium.woff2");
        StringAssert.Contains(fontface.Source, "woff2");
        Assert.AreEqual("500", fontface.Weight);
    }

    [TestMethod]
    public void FontFaceOpenSansNoSource()
    {
        var src = "@font-face{font-family:'Open Sans';font-style:normal}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.IsNotNull(sheet);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<FontFaceRule>(sheet.Rules[0]);
        var fontface = (IFontFaceRule)sheet.Rules[0];
        Assert.AreEqual("\"Open Sans\"", fontface.Family);
        Assert.AreEqual("", fontface.Features);
        Assert.AreEqual("", fontface.Range);
        Assert.AreEqual("", fontface.Source);
        Assert.AreEqual("", fontface.Stretch);
        Assert.AreEqual("normal", fontface.Style);
        Assert.AreEqual("", fontface.Variant);
        Assert.AreEqual("", fontface.Weight);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#unicode-range-desc")]
    [DataRow("U+41-5A")]
    [DataRow("U+0-7F")]
    [DataRow("U+4??")]
    [DataRow("U+000041")]
    [DataRow("U+0025-00FF, U+4??")]
    [Spec("css-syntax-3", "#consume-unicode-range-value")]
    public void FontFaceUnicodeRangeIsRetained(string range)
    {
        var src = "@font-face{font-family:'Open Sans';unicode-range:" + range + "}";
        var sheet = CssConstructionFunctions.ParseStyleSheet(src);
        Assert.AreEqual(1, sheet.Rules.Length);
        var fontface = (IFontFaceRule)sheet.Rules[0];

        Assert.AreEqual(range, fontface.Range);
    }

    [TestMethod]
    [Spec("css-syntax-3", "#starts-a-unicode-range")]
    [Spec("css-syntax-3", "#consume-unicode-range-token")]
    [Spec("css-fonts-4", "#unicode-range-desc")]
    [DataRow("U+4??", "400", "4FF")]
    [DataRow("U+??????", "000000", "FFFFFF")]
    [DataRow("U+41", "41", "41")]
    [DataRow("U+41-5A", "41", "5A")]
    [DataRow("U+0-7F", "0", "7F")]
    public void UnicodeRangeTokenStartAndEnd(string source, string start, string end)
    {
        var lexer = new Lexer(new TextSource(source));
        var range = lexer.Get();
        Assert.IsInstanceOfType<RangeToken>(range);
        var rangeToken = (RangeToken)range;

        Assert.AreEqual(start, rangeToken.Start);
        Assert.AreEqual(end, rangeToken.End);
        Assert.AreEqual(TokenType.EndOfFile, lexer.Get().Type);
    }

    [TestMethod]
    [DataRow("url(f.ttf)", "f.ttf")]
    [DataRow("url('f.ttf')", "f.ttf")]
    [DataRow("url(\"f.ttf\")", "f.ttf")]
    [DataRow("url( \"f.ttf\" )", "f.ttf")]
    [DataRow(@"url(""f\"")g.ttf"")", "f%22)g.ttf")]
    [DataRow(@"url('f\'g.ttf')", "f'g.ttf")]
    [DataRow(@"url(f\(g\).ttf)", "f(g).ttf")]
    [DataRow(@"url(""f\67 .ttf"")", "fg.ttf")]
    [Spec("css-fonts-4", "#font-face-src-loading")]
    [Spec("css-values-4", "#relative-urls")]
    public void FontFaceInALinkedStyleSheet_ResolvesAgainstTheStyleSheetUrl(string src, string expected)
    {
        CollectionAssert.Contains(FontFaceRequests(src), "https://example.com/doc/css/" + expected);
    }

    private static List<string> FontFaceRequests(string src)
    {
        var loader = new StyleSheetLoader("@font-face { font-family: F; src: " + src + " }");
        var container = new HtmlContainerInt(new MockAdapter { NetworkLoader = loader }) { AvoidAsyncImagesLoading = true };

        container.SetHtml("<html><head><link rel=stylesheet href=css/fonts.css></head><body>a</body></html>").GetAwaiter().GetResult();

        return loader.Requests;
    }

    private sealed class StyleSheetLoader(string styleSheet) : RNetworkLoader
    {
        public List<string> Requests { get; } = [];

        public override RUri? BaseUri => new("https://example.com/doc/index.html");

        public override Task<string> GetPrimaryContents() => Task.FromResult(string.Empty);

        public override Task<RNetworkResponse?> GetResourceStream(RUri uri)
        {
            Requests.Add(uri.AbsoluteUri);
            if (uri.AbsoluteUri != "https://example.com/doc/css/fonts.css")
                return Task.FromResult<RNetworkResponse?>(null);

            var headers = new Dictionary<string, string[]> { ["Content-Type"] = ["text/css"] };
            return Task.FromResult<RNetworkResponse?>(new RNetworkResponse(new MemoryStream(Encoding.UTF8.GetBytes(styleSheet)), headers));
        }
    }
}
