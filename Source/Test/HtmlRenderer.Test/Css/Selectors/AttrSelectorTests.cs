using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css.Selectors;

[TestClass]
public sealed class AttrSelectorTests
{
    [TestMethod]
    public void FindAllAttrMatchSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("");
        var list = GetAttributeStyleRules<AttrMatchSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllAttrInListSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("~");
        var list = GetAttributeStyleRules<AttrListSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllAttrHyphenSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("|");
        var list = GetAttributeStyleRules<AttrHyphenSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllAttrBeginsSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("^");
        var list = GetAttributeStyleRules<AttrBeginsSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllAttrEndsSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("$");
        var list = GetAttributeStyleRules<AttrEndsSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllAttrContainsSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("*");
        var list = GetAttributeStyleRules<AttrContainsSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllAttrNotMatchSelectorsThatMatchAttributeName()
    {
        var sheet = GetAttributeStylesheet("!");
        var list = GetAttributeStyleRules<AttrNotMatchSelector>(sheet);
        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void BareAttributePresenceSelector_ResolvesToAttrAvailableSelector()
    {
        // A `[attr]` with no combinator/value falls through the factory's dispatch to the presence
        // selector (the switch `_` arm) - the one branch the combinator theory above doesn't cover.
        var sheet = CssConstructionFunctions.ParseStyleSheet(
            "[type] { background-color: #101010 } .sample-class[type] { background-color: #121212 }");

        var list = GetAttributeStyleRules<AttrAvailableSelector>(sheet);

        Assert.AreEqual(2, list.Count());
    }

    private static Stylesheet GetAttributeStylesheet(string combinator)
    {
        var css = "[type" + combinator + "='button'] { background-color: #101010 } .sample-class[type"
                  + combinator + "='input'] { background-color: #121212 }";

        return CssConstructionFunctions.ParseStyleSheet(css);
    }

    private static IEnumerable<IStyleRule> GetAttributeStyleRules<T>(Stylesheet sheet) where T : IAttrSelector
    {
        return sheet.StyleRules
            .Where(x =>
                (x.Selector is CompoundSelector selector &&
                 selector.Any(y => y is T { Attribute: "type" }))
                || x.Selector is T { Attribute: "type" }
            );
    }
}
