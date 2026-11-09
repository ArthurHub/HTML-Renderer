using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

[DoNotParallelize]
[TestClass]
public sealed class TextTransformIntegrationTests
{
    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "text.html#caps-prop")]
    [DataRow("uppercase", "abc dEF", "ABC DEF")]
    [DataRow("lowercase", "abc dEF", "abc def")]
    [DataRow("capitalize", "abc dEF", "Abc DEF")]
    public void TextTransform_ChangesTheCapitalization(string transform, string text, string expected)
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap($"<p id='p' style='text-transform:{transform}'>{text}</p>"));
        var words = LayoutHarness.Descendants(LayoutHarness.FindById(root, "p")!).SelectMany(b => b.Words).Select(w => w.Text);

        Assert.AreEqual(expected, string.Join(" ", words));
    }
}
