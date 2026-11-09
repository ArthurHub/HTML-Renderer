using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class MediaListTests
{
    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void SimpleScreenMediaList()
    {
        var source = @"@media screen {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("screen", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void MediaListAtIllegal()
    {
        var source = @"@media @screen {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void MediaListInterrupted()
    {
        var source = @"@media screen; h1 { color: green }";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[0]);
        var h1 = (StyleRule)sheet.Rules[0];
        Assert.AreEqual("h1", h1.SelectorText);
        var style = h1.Style;
        Assert.AreEqual("rgb(0, 128, 0)", style.Color);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void SimpleScreenTvMediaList()
    {
        var source = @"@media screen,tv {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("screen, tv", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(2, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    [Spec("css21", "media.html#at-media-rule")]
    public void SimpleScreenTvSpacesMediaList()
    {
        var source = @"@media              screen ,          tv {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("screen, tv", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(2, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void OnlyScreenTvMediaList()
    {
        var source = @"@media only screen,tv {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("only screen, tv", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(2, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void NotScreenTvMediaList()
    {
        var source = @"@media not screen,tv {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not screen, tv", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(2, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void FeatureMinWidthMediaList()
    {
        var source = @"@media (min-width:30px) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("(min-width: 30px)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void OnlyFeatureWidthMediaList()
    {
        var source = @"@media only (width: 640px) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("only (width: 640px)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void NotFeatureDeviceWidthMediaList()
    {
        var source = @"@media not (device-width: 640px) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not (device-width: 640px)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void AllFeatureMaxWidthMediaListMissingAnd()
    {
        var source = @"@media all (max-width:30px) {
    h1 { color: red }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void NoMediaQueryGivenSkip()
    {
        var source = @"@media {
    h1 { color: red }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void NotNoMediaTypeOrExpressionSkip()
    {
        var source = @"@media not {
    h1 { color: red }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void OnlyNoMediaTypeOrExpressionSkip()
    {
        var source = @"@media only {
    h1 { color: red }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void MediaFeatureMissingSkip()
    {
        var source = @"@media () {
    h1 { color: red }
}";

        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void MediaFeatureMissingSkipReadNext()
    {
        var source = @"@media () {
    h1 { color: red }
}
h1 { color: green }";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(2, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        Assert.IsInstanceOfType<StyleRule>(sheet.Rules[1]);
        var style = (StyleRule)sheet.Rules[1];
        Assert.AreEqual("rgb(0, 128, 0)", style.Style.Color);
        Assert.AreEqual("h1", style.SelectorText);
    }

    [TestMethod]
    public void FeatureMaxWidthMediaListMissingConnectedAnd()
    {
        var source = @"@media (max-width:30px) (min-width:10px) {
    h1 { color: red }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void TvScreenMediaListMissingComma()
    {
        var source = @"@media tv screen {
    h1 { color: red }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.AreEqual(RuleType.Media, sheet.Rules[0].Type);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("not all", media.ConditionText);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void AllFeatureMaxWidthMediaListWithAndKeyword()
    {
        var source = @"@media all and (max-width:30px) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("all and (max-width: 30px)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void FeatureAspectRatioMediaList()
    {
        var source = @"@media (aspect-ratio: 16/9) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("(aspect-ratio: 16/9)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void PrintFeatureMaxWidthAndMinDeviceWidthMediaList()
    {
        var source = @"@media print and (max-width:30px) and (min-device-width:100px) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("print and (max-width: 30px) and (min-device-width: 100px)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void AllFeatureMinWidthAndMinDeviceWidthScreenMediaList()
    {
        var source = @"@media all and (min-width:0) and (min-device-width:100px), screen {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("all and (min-width: 0) and (min-device-width: 100px), screen", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(2, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void ImplicitAllFeatureResolutionMediaList()
    {
        var source = @"@media (resolution:72dpi) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("(resolution: 72dpi)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void ImplicitAllFeatureMinResolutionAndMaxResolutionMediaList()
    {
        var source = @"@media (min-resolution:72dpi) and (max-resolution:140dpi) {
    h1 { color: green }
}";
        var sheet = ParseStyleSheet(source);
        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<MediaRule>(sheet.Rules[0]);
        var media = (MediaRule)sheet.Rules[0];
        Assert.AreEqual("(min-resolution: 72dpi) and (max-resolution: 140dpi)", media.Media.MediaText);
        var list = media.Media;
        Assert.AreEqual(1, list.Length);
        Assert.AreEqual(1, media.Rules.Length);
    }

    [TestMethod]
    public void CssMediaListApiWithAppendDeleteAndTextShouldWork()
    {
        var media = new[] { "handheld", "screen", "only screen and (max-device-width: 480px)" };
        var p = new StylesheetParser();
        var m = new MediaList(p);
        Assert.AreEqual(0, m.Length);

        m.Add(media[0]);
        m.Add(media[1]);
        m.Add(media[2]);

        m.Remove(media[1]);

        Assert.AreEqual(2, m.Length);
        Assert.AreEqual(media[0], m[0]);
        Assert.AreEqual(media[2], m[1]);
        Assert.AreEqual(string.Concat(media[0], ", ", media[2]), m.MediaText);
    }

    [TestMethod]
    public void CombinedConditionMediaQueriesLevel4()
    {
        const string source = @"/* Traditionelle Syntax */
@media (min-height: 500px) and (max-height: 800px) {
  /* Styles */
  h1 { color: rgb(255, 0, 0); }
}

/* Mit Vergleichsoperatoren */
@media (height >= 500px) and (height <= 800px) {
  /* Gleiche Styles */
  h1 { color: rgb(255, 0, 0); }
}";
        var result = ParseStyleSheet(source);
        Assert.AreEqual(source, result.StylesheetText.Text);
        Assert.AreEqual(2, result.Rules.Length);
        var rule1 = result.Rules[0] as MediaRule;
        var rule2 = result.Rules[1] as MediaRule;
        Assert.IsNotNull(rule1);
        Assert.IsNotNull(rule2);
        Assert.AreEqual("(min-height: 500px) and (max-height: 800px)", rule1.ConditionText);
        Assert.AreEqual("(height >= 500px) and (height <= 800px)", rule2.ConditionText);
        Assert.AreEqual("@media (min-height: 500px) and (max-height: 800px) { h1 { color: rgb(255, 0, 0) } }", rule1.ToCss());
        Assert.AreEqual("@media (height >= 500px) and (height <= 800px) { h1 { color: rgb(255, 0, 0) } }", rule2.ToCss());
    }
}
