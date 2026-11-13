using HtmlRenderer.Test.CssEngineSupport;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.CssModules;

[TestClass]
public sealed class CssViewTransitionsTests
{
    private static readonly RColor Green = RColor.FromArgb(0, 128, 0);

    // ── 2.1 view-transition-name ─────────────────────────────────────────

    [Ignore("Gap, see Spec/css-view-transitions-1.yml")]
    [TestMethod]
    [Spec("css-view-transitions-1", "#view-transition-name-prop")]
    [DataRow("none")]
    [DataRow("hero")]
    public void ViewTransitionName_IsParsed(string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations("view-transition-name: " + value);

        Assert.AreEqual(value, style.GetPropertyValue("view-transition-name"));
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("a b")]
    public void ViewTransitionName_InvalidValue_IsRejected(string value)
    {
        var style = CssConstructionFunctions.ParseDeclarations("view-transition-name: " + value);

        Assert.AreEqual(string.Empty, style.GetPropertyValue("view-transition-name"));
    }

    // ── 3.2 View transition pseudo-elements ──────────────────────────────

    [TestMethod]
    [Spec("css-view-transitions-1", "#view-transition-pseudos")]
    [DataRow("::view-transition")]
    [DataRow("::view-transition-group(*)")]
    [DataRow("::view-transition-image-pair(hero)")]
    [DataRow("::view-transition-old(hero)")]
    [DataRow("::view-transition-new(root)")]
    public void Pseudo_NeverMatchesInAStaticRendering(string pseudo)
    {
        Assert.AreEqual(Green, Box("<style>p { color: green } :root" + pseudo + " { color: red }</style><p id=x>a</p>").ActualColor);
    }

    [TestMethod]
    [Spec("css-view-transitions-1", "#view-transition-pseudos")]
    [DataRow("::view-transition")]
    [DataRow("::view-transition-group(*)")]
    [DataRow("::view-transition-image-pair(hero)")]
    [DataRow("::view-transition-old(hero)")]
    [DataRow("::view-transition-new(root)")]
    public void Pseudo_KeepsTheRestOfTheSelectorListValid(string pseudo)
    {
        Assert.AreEqual(Green, Box("<style>p, :root" + pseudo + " { color: green }</style><p id=x>a</p>").ActualColor);
    }

    private static CssBox Box(string body, string id = "x")
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(body));
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }
}
