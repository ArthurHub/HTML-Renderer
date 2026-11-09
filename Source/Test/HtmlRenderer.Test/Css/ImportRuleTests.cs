using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class ImportRuleTests
{
    private static ImportRule NewImportRule(string cssText)
    {
        var parser = new StylesheetParser();
        var rule = new ImportRule(parser) { Text = cssText };
        return rule;
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithNonQuotedUrl()
    {
        var source = "@import url(button.css);";
        var rule = NewImportRule(source);
        Assert.AreEqual("button.css", rule.Href);
        Assert.AreEqual("", rule.Media.MediaText);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithDoubleQuotedUrl()
    {
        var source = "@import url(\"button.css\");";
        var rule = NewImportRule(source);
        Assert.AreEqual("button.css", rule.Href);
        Assert.AreEqual("", rule.Media.MediaText);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithSingleQuotedUrl()
    {
        var source = "@import url('button.css');";
        var rule = NewImportRule(source);
        Assert.AreEqual("button.css", rule.Href);
        Assert.AreEqual("", rule.Media.MediaText);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithDoubleQuotedStringAsUrl()
    {
        var source = "@import \"button.css\";";
        var rule = NewImportRule(source);
        Assert.AreEqual("button.css", rule.Href);
        Assert.AreEqual("", rule.Media.MediaText);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithSingleQuotedStringAsUrl()
    {
        var source = "@import 'button.css';";
        var rule = NewImportRule(source);
        Assert.AreEqual("button.css", rule.Href);
        Assert.AreEqual("", rule.Media.MediaText);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithUrlAndAllMedia()
    {
        var media = "all";
        var source = "@import url(size/medium.css) " + media + ";";
        var rule = NewImportRule(source);
        Assert.AreEqual("size/medium.css", rule.Href);
        Assert.AreEqual(media, rule.Media.MediaText);
        Assert.AreEqual(1, rule.Media.Length);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#at-import")]
    public void CssImportWithUrlAndComplicatedMedia()
    {
        var media = "screen and (color), projection and (min-color: 256)";
        var source = "@import url(old.css) " + media + ";";
        var rule = NewImportRule(source);
        Assert.AreEqual("old.css", rule.Href);
        Assert.AreEqual(media, rule.Media.MediaText);
        Assert.AreEqual(2, rule.Media.Length);
    }
}
