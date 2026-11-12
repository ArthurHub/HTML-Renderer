using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// An unknown property name gives null, the default parser doesn't keep unknown declarations.
/// </summary>
[TestClass]
public sealed class PropertyFactoryTests
{
    [TestMethod]
    public void CssPropertyFactoryCalls()
    {
        var parser = new StylesheetParser();
        var decl = new StyleDeclaration(parser);
        var invalid = decl.CreateProperty("invalid");
        var border = decl.CreateProperty("border");
        var color = decl.CreateProperty("color");
        decl.SetProperty(color);
        var colorAgain = decl.CreateProperty("color");

        Assert.IsNull(invalid);
        Assert.IsNotNull(border);
        Assert.IsNotNull(color);
        Assert.IsNotNull(colorAgain);

        Assert.IsInstanceOfType<BorderProperty>(border);
        Assert.IsInstanceOfType<ColorProperty>(color);
        Assert.AreEqual(color, colorAgain);
    }

    [TestMethod]
    public void CssUnknownPropertyPreservesCase()
    {
        var snippet = "my-Property: something";
        var property = ParseDeclaration(snippet, includeUnknownDeclarations: true);
        Assert.AreEqual("my-Property", property.Name);
        Assert.IsInstanceOfType<UnknownProperty>(property);
    }
}
