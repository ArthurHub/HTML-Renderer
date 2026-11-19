using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.Test.Parse;

[TestClass]
public sealed class CssParserFontFamilyTests
{
    private static string ParseFontFamily(string value)
    {
        var adapter = new MockAdapter();
        adapter.AddFontFamily(new MockFontFamily("Foo Bar"));
        adapter.AddFontFamily(new MockFontFamily("Baz"));
        adapter.AddFontFamily(new MockFontFamily("X"));
        return new CssParser(adapter).ParseFontFamily(value);
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [DataRow("Baz", "Baz")]
    [DataRow("  Baz  ", "Baz")]
    [DataRow("Missing, Baz", "Baz")]
    [DataRow("Missing , Baz", "Baz")]
    [DataRow("Missing,Baz", "Baz")]
    [DataRow("'Missing', \"Foo Bar\"", "Foo Bar")]
    [DataRow("\"Foo Bar\" , Baz", "Foo Bar")]
    [DataRow("'Foo Bar'\t,Baz", "Foo Bar")]
    [DataRow("Missing, X", "X")]
    [DataRow("Foo Bar, Baz", "Foo Bar")]
    public void FirstAvailableFamily_IsUsed(string value, string expected)
    {
        Assert.AreEqual(expected, ParseFontFamily(value));
    }

    [TestMethod]
    [Spec("css21", "fonts.html#font-family-prop")]
    [DataRow("Missing")]
    [DataRow("Missing,")]
    [DataRow("Missing, ")]
    [DataRow("'Missing'")]
    [DataRow("Ba")]
    public void NoAvailableFamily_FallsBackToTheDefaultFont(string value)
    {
        Assert.AreEqual(CssConstants.DefaultFont, ParseFontFamily(value));
    }
}
