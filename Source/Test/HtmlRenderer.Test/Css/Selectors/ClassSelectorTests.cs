using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css.Selectors;

[TestClass]
public sealed class ClassSelectorTests
{
    [TestMethod]
    public void FindAllClassSelectorsThatMatchClassName()
    {
        var css =
            ".sample-class { background-color: #101010 } .sample-class[type='input'] { background-color: #121212 }";
        var sheet = CssConstructionFunctions.ParseStyleSheet(css);

        var list = sheet.StyleRules
            .Where(x =>
                (x.Selector is CompoundSelector selector &&
                 selector.Any(y => y is ClassSelector { Class: "sample-class" }))
                || x.Selector is ClassSelector { Class: "sample-class" }
            );

        Assert.AreEqual(2, list.Count());
    }
}
