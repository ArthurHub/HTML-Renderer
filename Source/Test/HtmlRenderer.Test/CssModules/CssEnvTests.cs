using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssEnvTests
{
    private const double Delta = 0.01;

    [TestMethod]
    [Ignore("Gap, see Spec/css-env-1.yml")]
    [Spec("css-env-1", "#safe-area-insets")]
    [Spec("css-env-1", "#safe-area-max-insets")]
    [DataRow("safe-area-inset-top")]
    [DataRow("safe-area-max-inset-left")]
    public void SafeAreaInsets_AreZeroOnARectangularDisplay(string name)
    {
        Assert.AreEqual(0, Styled($"#x{{margin-left:5px;margin-left:env({name})}}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-env-1.yml")]
    [Spec("css-env-1", "#viewport-segments")]
    public void ViewportSegments_AreUndefinedWithoutASplitViewport()
    {
        Assert.AreEqual(7, Styled("#x{margin-left:env(viewport-segment-width 0 0, 7px)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-env-1.yml")]
    [Spec("css-env-1", "#text-zoom")]
    public void PreferredTextScale_IsOne()
    {
        Assert.AreEqual(10, Styled("#x{margin-left:calc(10px * env(preferred-text-scale))}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-env-1.yml")]
    [Spec("css-env-1", "#env-function")]
    public void UnknownVariable_UsesTheFallback()
    {
        Assert.AreEqual(7, Styled("#x{margin-left:env(foo, 7px)}").ActualMarginLeft, Delta);
    }

    [TestMethod]
    [Ignore("Gap, see Spec/css-env-1.yml")]
    [Spec("css-env-1", "#env-in-shorthands")]
    public void Env_InAShorthand_IsSubstituted()
    {
        Assert.AreEqual(7, Styled("#x{margin:env(foo, 1px 2px 3px 7px)}").ActualMarginLeft, Delta);
    }

    private static CssBox Styled(string css, string body = "<div id=x>a</div>")
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body style='margin:0'>{body}</body></html>", 800, 600);
        var box = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(box);
        return box;
    }
}
