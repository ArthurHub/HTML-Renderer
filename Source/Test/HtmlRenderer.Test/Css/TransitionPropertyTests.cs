using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/TransitionProperty.cs (source class
/// <c>CssTransitionPropertyTests</c>).
/// <see cref="TransitionPropertyProperty"/> (via a restricted known-property-name list matching the
/// "-specific"/"sliding-vertically"/"test_05"-illegal cases), <see cref="TransitionTimingFunctionProperty"/>,
/// <see cref="TransitionDurationProperty"/>, <see cref="TransitionDelayProperty"/>, and the
/// <see cref="TransitionProperty"/> shorthand all exist and match PeachPDF's grammar.
/// </summary>
[TestClass]
public sealed class TransitionPropertyTests
{
    [TestMethod]
    public void CssTransitionPropertyNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-property : none");
        Assert.AreEqual("transition-property", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionPropertyProperty>(property);
        var concrete = (TransitionPropertyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionPropertyAllLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-property : ALL");
        Assert.AreEqual("transition-property", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionPropertyProperty>(property);
        var concrete = (TransitionPropertyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("all", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionPropertyWidthHeightLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-property : width   , height");
        Assert.AreEqual("transition-property", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionPropertyProperty>(property);
        var concrete = (TransitionPropertyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("width, height", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionPropertyDashSpecificIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-property : -specific");
        Assert.AreEqual("transition-property", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionPropertyProperty>(property);
        var concrete = (TransitionPropertyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssTransitionPropertySlidingVerticallyIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-property : sliding-vertically");
        Assert.AreEqual("transition-property", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionPropertyProperty>(property);
        var concrete = (TransitionPropertyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssTransitionPropertyTest05Illegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-property : test_05");
        Assert.AreEqual("transition-property", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionPropertyProperty>(property);
        var concrete = (TransitionPropertyProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionEaseLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : ease");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("ease", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionEaseInLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : ease-IN");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("ease-in", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionStepStartLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : step-start");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("step-start", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionStepStartStepEndLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : step-start  , step-end");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("step-start, step-end", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionStepStartStepEndLinearEaseInOutLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : step-start  , step-end,linear,ease-IN-OUT");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("step-start, step-end, linear, ease-in-out", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionCubicBezierLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : cubic-bezier(0, 1, 0.5, 1)");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("cubic-bezier(0, 1, 0.5, 1)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionStepsStartLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : steps(10, start)");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("steps(10, start)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionStepsEndLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : steps(25, end)");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("steps(25, end)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionTimingFunctionStepsLinearCubicBezierLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-timing-function : steps(25), linear, cubic-bezier(0.25, 1, 0.5, 1)");
        Assert.AreEqual("transition-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionTimingFunctionProperty>(property);
        var concrete = (TransitionTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("steps(25), linear, cubic-bezier(0.25, 1, 0.5, 1)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionDurationSecondsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-duration : 6s");
        Assert.AreEqual("transition-duration", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionDurationProperty>(property);
        var concrete = (TransitionDurationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("6s", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionDurationMillisecondsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-duration : 60ms");
        Assert.AreEqual("transition-duration", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionDurationProperty>(property);
        var concrete = (TransitionDurationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60ms", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionDurationMillisecondsSecondsSecondsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-duration : 60ms, 1s, 2s");
        Assert.AreEqual("transition-duration", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionDurationProperty>(property);
        var concrete = (TransitionDurationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60ms, 1s, 2s", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionDelayMillisecondsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-delay : 60ms");
        Assert.AreEqual("transition-delay", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionDelayProperty>(property);
        var concrete = (TransitionDelayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60ms", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionDelayMillisecondsSecondsSecondsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition-delay : 60ms, 1s, 2s");
        Assert.AreEqual("transition-delay", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionDelayProperty>(property);
        var concrete = (TransitionDelayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60ms, 1s, 2s", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionMillisecondsSecondsSecondsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition : 60ms, 1s, 2s");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60ms, 1s, 2s", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionStepsLinearCubicBezierLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition : steps(25), linear, cubic-bezier(0.25, 1, 0.5, 1)");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("steps(25), linear, cubic-bezier(0.25, 1, 0.5, 1)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionWidthHeightLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition : width   , height");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("width, height", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionEaseLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition : ease");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("ease", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionSecondsEaseAllLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition : all 1s ease");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("all 1s ease", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionSecondsEaseAllHeightMsStepsLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("transition : all 1s ease, height steps(5) 50ms");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("all 1s ease, height 50ms steps(5)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionSecondsEaseAllHeightMsStepsWidthCubicBezierLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration(
            "transition : all 1s ease, height step-start 50ms,width,cubic-bezier(0.2,0.5 , 1  ,  1)");
        Assert.AreEqual("transition", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TransitionProperty>(property);
        var concrete = (TransitionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("all 1s ease, height 50ms step-start, width, cubic-bezier(0.2, 0.5, 1, 1)", concrete.Value);
    }

    [TestMethod]
    public void CssTransitionShorthandWithOneTimeSetsOnlyTheDuration()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: color 1s");

        Assert.AreEqual("color", style.GetPropertyValue("transition-property"));
        Assert.AreEqual("1s", style.GetPropertyValue("transition-duration"));
        Assert.AreEqual("initial", style.GetPropertyValue("transition-delay"));
    }

    [TestMethod]
    public void CssTransitionShorthandAssignsTheSecondTimeToTheDelay()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: 1s 2s");

        Assert.AreEqual("1s", style.GetPropertyValue("transition-duration"));
        Assert.AreEqual("2s", style.GetPropertyValue("transition-delay"));
    }

    [TestMethod]
    public void CssTransitionShorthandNoneSetsThePropertyToNone()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: none");

        Assert.AreEqual("none", style.GetPropertyValue("transition-property"));
    }

    [TestMethod]
    public void CssTransitionShorthandWithNoneInAListIsInvalid()
    {
        var style = CssConstructionFunctions.ParseDeclarations("transition: none, color 1s");

        Assert.AreEqual(string.Empty, style.GetPropertyValue("transition"));
    }
}
