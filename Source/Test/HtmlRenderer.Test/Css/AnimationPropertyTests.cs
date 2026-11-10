using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Ported from PeachPDF.Tests/CSS/PropertyTests/AnimationPropertyTests.cs. Pure CSSOM parse tests - no layout involved.</summary>
[TestClass]
public sealed class AnimationPropertyTests
{
    [TestMethod]
    public void AnimationDurationMillisecondsLegal()
    {
        var snippet = "animation-duration : 60ms";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-duration", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDurationProperty>(property);
        var concrete = (AnimationDurationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("60ms", concrete.Value);
    }

    [TestMethod]
    public void AnimationDurationMultipleSecondsLegal()
    {
        var snippet = "animation-duration : 1s  , 2s  , 3s  , 4s";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-duration", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDurationProperty>(property);
        var concrete = (AnimationDurationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1s, 2s, 3s, 4s", concrete.Value);
    }

    [TestMethod]
    public void AnimationDelayMillisecondsLegal()
    {
        var snippet = "animation-delay : 0ms";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-delay", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDelayProperty>(property);
        var concrete = (AnimationDelayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0ms", concrete.Value);
    }

    [TestMethod]
    public void AnimationDelayZeroIllegal()
    {
        var snippet = "animation-delay : 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-delay", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDelayProperty>(property);
        var concrete = (AnimationDelayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationDelayZeroZeroSecondMillisecondsLegal()
    {
        var snippet = "animation-delay : 0s  , 0s  , 1s  , 20ms";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-delay", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDelayProperty>(property);
        var concrete = (AnimationDelayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0s, 0s, 1s, 20ms", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameDashSpecificLegal()
    {
        var snippet = "animation-name : -specific";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-name", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationNameProperty>(property);
        var concrete = (AnimationNameProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("-specific", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameSlidingVerticallyLegal()
    {
        var snippet = "animation-name : sliding-vertically";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-name", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationNameProperty>(property);
        var concrete = (AnimationNameProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("sliding-vertically", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameTest05Legal()
    {
        var snippet = "animation-name : test_05";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-name", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationNameProperty>(property);
        var concrete = (AnimationNameProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("test_05", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameNumberIllegal()
    {
        var snippet = "animation-name : 42";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-name", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationNameProperty>(property);
        var concrete = (AnimationNameProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationNameShouldKeepLetterCasing()
    {
        var snippet = "animation-name : MyAnimation";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-name", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationNameProperty>(property);
        var concrete = (AnimationNameProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("MyAnimation", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameMyAnimationOtherAnimationLegal()
    {
        var snippet = "animation-name : my-animation, other-animation";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-name", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationNameProperty>(property);
        var concrete = (AnimationNameProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("my-animation, other-animation", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountZeroLegal()
    {
        var snippet = "animation-iteration-count : 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-iteration-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationIterationCountProperty>(property);
        var concrete = (AnimationIterationCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountInfiniteLegal()
    {
        var snippet = "animation-iteration-count : infinite";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-iteration-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationIterationCountProperty>(property);
        var concrete = (AnimationIterationCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("infinite", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountInfiniteUppercaseLegal()
    {
        var snippet = "animation-iteration-count : INFINITE";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-iteration-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationIterationCountProperty>(property);
        var concrete = (AnimationIterationCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("infinite", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountFloatLegal()
    {
        var snippet = "animation-iteration-count : 2.3";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-iteration-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationIterationCountProperty>(property);
        var concrete = (AnimationIterationCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2.3", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountTwoZeroInfiniteLegal()
    {
        var snippet = "animation-iteration-count : 2, 0, infinite";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-iteration-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationIterationCountProperty>(property);
        var concrete = (AnimationIterationCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2, 0, infinite", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountNegativeIllegal()
    {
        var snippet = "animation-iteration-count : -1";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-iteration-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationIterationCountProperty>(property);
        var concrete = (AnimationIterationCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationTimingFunctionEaseUppercaseLegal()
    {
        var snippet = "animation-timing-function : EASE";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationTimingFunctionProperty>(property);
        var concrete = (AnimationTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("ease", concrete.Value);
    }

    [TestMethod]
    public void AnimationTimingFunctionNoneIllegal()
    {
        var snippet = "animation-timing-function : none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationTimingFunctionProperty>(property);
        var concrete = (AnimationTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationTimingFunctionEaseInOutLegal()
    {
        var snippet = "animation-timing-function : ease-IN-out";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationTimingFunctionProperty>(property);
        var concrete = (AnimationTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("ease-in-out", concrete.Value);
    }

    [TestMethod]
    public void AnimationTimingFunctionStepEndLegal()
    {
        var snippet = "animation-timing-function : step-END";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationTimingFunctionProperty>(property);
        var concrete = (AnimationTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("step-end", concrete.Value);
    }

    [TestMethod]
    public void AnimationTimingFunctionStepStartLinearLegal()
    {
        var snippet = "animation-timing-function : step-start  , LINeAr";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationTimingFunctionProperty>(property);
        var concrete = (AnimationTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("step-start, linear", concrete.Value);
    }

    [TestMethod]
    public void AnimationTimingFunctionStepStartCubicBezierLegal()
    {
        var snippet = "animation-timing-function : step-start  , cubic-bezier(0,1,1,1)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-timing-function", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationTimingFunctionProperty>(property);
        var concrete = (AnimationTimingFunctionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("step-start, cubic-bezier(0, 1, 1, 1)", concrete.Value);
    }

    [TestMethod]
    public void AnimationPlayStateRunningLegal()
    {
        var snippet = "animation-play-state: running";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-play-state", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationPlayStateProperty>(property);
        var concrete = (AnimationPlayStateProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("running", concrete.Value);
    }

    [TestMethod]
    public void AnimationPlayStatePausedUppercaseLegal()
    {
        var snippet = "animation-play-state: PAUSED";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-play-state", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationPlayStateProperty>(property);
        var concrete = (AnimationPlayStateProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("paused", concrete.Value);
    }

    [TestMethod]
    public void AnimationPlayStatePausedRunningPausedLegal()
    {
        var snippet = "animation-play-state: paused, Running, paused";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-play-state", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationPlayStateProperty>(property);
        var concrete = (AnimationPlayStateProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("paused, running, paused", concrete.Value);
    }

    [TestMethod]
    public void AnimationFillModeNoneLegal()
    {
        var snippet = "animation-fill-mode: none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-fill-mode", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationFillModeProperty>(property);
        var concrete = (AnimationFillModeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void AnimationFillModeZeroIllegal()
    {
        var snippet = "animation-fill-mode: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-fill-mode", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationFillModeProperty>(property);
        var concrete = (AnimationFillModeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationFillModeBackwardsLegal()
    {
        var snippet = "animation-fill-mode: backwards !important";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-fill-mode", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<AnimationFillModeProperty>(property);
        var concrete = (AnimationFillModeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("backwards", concrete.Value);
    }

    [TestMethod]
    public void AnimationFillModeForwardsUppercaseLegal()
    {
        var snippet = "animation-fill-mode: FORWARDS";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-fill-mode", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationFillModeProperty>(property);
        var concrete = (AnimationFillModeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("forwards", concrete.Value);
    }

    [TestMethod]
    public void AnimationFillModeBothBackwardsForwardsNoneLegal()
    {
        var snippet = "animation-fill-mode: both , backwards ,  forwards  ,NONE";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-fill-mode", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationFillModeProperty>(property);
        var concrete = (AnimationFillModeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("both, backwards, forwards, none", concrete.Value);
    }

    [TestMethod]
    public void AnimationDirectionNormalLegal()
    {
        var snippet = "animation-direction: normal";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-direction", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDirectionProperty>(property);
        var concrete = (AnimationDirectionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal", concrete.Value);
    }

    [TestMethod]
    public void AnimationDirectionReverseLegal()
    {
        var snippet = "animation-direction  : reverse";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-direction", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDirectionProperty>(property);
        var concrete = (AnimationDirectionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("reverse", concrete.Value);
    }

    [TestMethod]
    public void AnimationDirectionNoneIllegal()
    {
        var snippet = "animation-direction  : none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-direction", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDirectionProperty>(property);
        var concrete = (AnimationDirectionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationDirectionAlternateReverseUppercaseLegal()
    {
        var snippet = "animation-direction : alternate-REVERSE";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-direction", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDirectionProperty>(property);
        var concrete = (AnimationDirectionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("alternate-reverse", concrete.Value);
    }

    [TestMethod]
    public void AnimationDirectionNormalAlternateReverseAlternateReverseLegal()
    {
        var snippet = "animation-direction: normal,alternate  , reverse   ,ALTERNATE-reverse !important";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation-direction", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<AnimationDirectionProperty>(property);
        var concrete = (AnimationDirectionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("normal, alternate, reverse, alternate-reverse", concrete.Value);
    }

    [TestMethod]
    public void AnimationIterationCountLegal()
    {
        var snippet = "animation : 5";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameLegal()
    {
        var snippet = "animation : my-animation";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("my-animation", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameDurationDelayLegal()
    {
        var snippet = "animation : my-animation 2s 0.5s";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2s 0.5s my-animation", concrete.Value);
    }

    [TestMethod]
    public void AnimationNameDurationDelayEaseLegal()
    {
        var snippet = "animation : my-animation  200ms 0.5s    ease";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("200ms ease 0.5s my-animation", concrete.Value);
    }

    [TestMethod]
    public void AnimationCountDoubleIllegal()
    {
        var snippet = "animation : 10 20";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        // Two unitless numbers is ambiguous and should not parse successfully
        // ("10 20ms" would be valid: 10 iterations, 20ms duration)
        // But "10 20" with two unitless numbers has no clear interpretation
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void AnimationNameDurationCountEaseInOutLegal()
    {
        var snippet = "animation : my-animation  200ms 2.5   ease-in-out";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("200ms ease-in-out 2.5 my-animation", concrete.Value);
    }

    [TestMethod]
    public void AnimationMultipleLegal()
    {
        var snippet = "animation : my-animation 0s 10 ease,   other-animation  5 linear,yet-another 0s 1s  10 step-start !important";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("animation", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<AnimationProperty>(property);
        var concrete = (AnimationProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0s ease 10 my-animation, linear 5 other-animation, 0s step-start 1s 10 yet-another", concrete.Value);
    }

    [TestMethod]
    public void AnimationShorthandSetsEveryLonghand()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: slide 1s ease-in 2s infinite alternate both paused");

        Assert.AreEqual("slide", style.GetPropertyValue("animation-name"));
        Assert.AreEqual("1s", style.GetPropertyValue("animation-duration"));
        Assert.AreEqual("ease-in", style.GetPropertyValue("animation-timing-function"));
        Assert.AreEqual("2s", style.GetPropertyValue("animation-delay"));
        Assert.AreEqual("infinite", style.GetPropertyValue("animation-iteration-count"));
        Assert.AreEqual("alternate", style.GetPropertyValue("animation-direction"));
        Assert.AreEqual("both", style.GetPropertyValue("animation-fill-mode"));
        Assert.AreEqual("paused", style.GetPropertyValue("animation-play-state"));
    }

    [TestMethod]
    public void AnimationShorthandWithOneTimeSetsOnlyTheDuration()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: slide 1s");

        Assert.AreEqual("slide", style.GetPropertyValue("animation-name"));
        Assert.AreEqual("1s", style.GetPropertyValue("animation-duration"));
        Assert.AreEqual("initial", style.GetPropertyValue("animation-delay"));
    }

    [TestMethod]
    public void AnimationShorthandGivesKeywordsToTheOtherLonghandsFirst()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: 3s none backwards");

        Assert.AreEqual("3s", style.GetPropertyValue("animation-duration"));
        Assert.AreEqual("none", style.GetPropertyValue("animation-fill-mode"));
        Assert.AreEqual("backwards", style.GetPropertyValue("animation-name"));
    }

    [TestMethod]
    public void AnimationShorthandListSetsTheLonghandsPerItem()
    {
        var style = CssConstructionFunctions.ParseDeclarations("animation: a 1s 2s, b 3s 4s");

        Assert.AreEqual("a, b", style.GetPropertyValue("animation-name"));
        Assert.AreEqual("1s, 3s", style.GetPropertyValue("animation-duration"));
        Assert.AreEqual("2s, 4s", style.GetPropertyValue("animation-delay"));
    }
}
