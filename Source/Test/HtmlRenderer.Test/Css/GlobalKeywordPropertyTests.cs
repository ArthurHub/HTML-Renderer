using HtmlRenderer.Test.CssEngineSupport;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Parsing of the CSS-wide keywords inherit, initial, unset, revert and revert-layer.
/// </summary>
[TestClass]
public sealed class GlobalKeywordPropertyTests
{
    // -- inherit ---------------------------------------------------------

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Color_Inherit_IsMarkedAsInherited()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: inherit");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInherited);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void MarginTop_Inherit_IsMarkedAsInherited()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: inherit");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInherited);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void FontSize_Inherit_IsMarkedAsInherited()
    {
        var property = CssConstructionFunctions.ParseDeclaration("font-size: inherit");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInherited);
    }

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Display_Inherit_IsMarkedAsInherited()
    {
        var property = CssConstructionFunctions.ParseDeclaration("display: inherit");
        Assert.AreEqual("display", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInherited);
    }

    // -- initial -----------------------------------------------------------

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Color_Initial_IsMarkedAsInitial()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: initial");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInitial);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void MarginTop_Initial_IsMarkedAsInitial()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: initial");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInitial);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void FontSize_Initial_IsMarkedAsInitial()
    {
        var property = CssConstructionFunctions.ParseDeclaration("font-size: initial");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInitial);
    }

    // -- unset ---------------------------------------------------------------

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Color_Unset_HasUnsetValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: unset");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("unset", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void MarginTop_Unset_HasUnsetValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: unset");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("unset", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void FontFamily_Unset_HasUnsetValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("font-family: unset");
        Assert.AreEqual("font-family", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("unset", property.Value);
    }

    // -- revert ----------------------------------------------------------------

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Color_Revert_HasRevertValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: revert");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("revert", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void FontSize_Revert_HasRevertValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("font-size: revert");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("revert", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void MarginTop_Revert_HasRevertValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: revert");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("revert", property.Value);
    }

    // -- revert-layer ------------------------------------------------------------

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Color_RevertLayer_HasRevertLayerValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: revert-layer");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("revert-layer", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void MarginTop_RevertLayer_HasRevertLayerValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: revert-layer");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("revert-layer", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#defaulting-keywords")]
    public void Display_RevertLayer_HasRevertLayerValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("display: revert-layer");
        Assert.AreEqual("display", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("revert-layer", property.Value);
    }

    // -- !important is preserved alongside global keywords -----------------------

    [TestMethod]
    [Spec("css21", "cascade.html#value-def-inherit")]
    [Spec("css-cascade-5", "#importance")]
    public void Color_InheritImportant_IsImportantAndInherited()
    {
        var property = CssConstructionFunctions.ParseDeclaration("color: inherit !important");
        Assert.AreEqual("color", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsTrue(property.IsInherited);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#importance")]
    public void MarginTop_UnsetImportant_IsImportantWithUnsetValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("margin-top: unset !important");
        Assert.AreEqual("margin-top", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.AreEqual("unset", property.Value);
    }

    [TestMethod]
    [Spec("css-cascade-5", "#importance")]
    public void FontSize_RevertImportant_IsImportantWithRevertValue()
    {
        var property = CssConstructionFunctions.ParseDeclaration("font-size: revert !important");
        Assert.AreEqual("font-size", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.AreEqual("revert", property.Value);
    }
}
