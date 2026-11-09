using System.Drawing;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace HtmlRenderer.IntegrationTest.Text;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class TextLevelSemanticsIntegrationTests
{
    [TestMethod]
    [Ignore("Gap, see Spec/html.yml")]
    [Spec("html", "text-level-semantics.html#the-bdo-element")]
    public async Task Bdo_Rtl_ReversesTheCharacters()
    {
        using var overridden = await Render("<bdo dir=rtl>abc</bdo>");
        using var reversed = await Render("<span>cba</span>");

        Assert.IsTrue(SamePixels(overridden, reversed));
    }

    private static async Task<Bitmap> Render(string body) =>
        (Bitmap)await HtmlRender.RenderToImageAsync(
            $"<!DOCTYPE html><html><body style='margin:0;font:20px Arial'>{body}</body></html>",
            new Size(200, 40),
            Color.White);

    private static bool SamePixels(Bitmap a, Bitmap b)
    {
        if (a.Size != b.Size)
            return false;

        for (var y = 0; y < a.Height; y++)
        for (var x = 0; x < a.Width; x++)
        {
            if (a.GetPixel(x, y) != b.GetPixel(x, y))
                return false;
        }

        return true;
    }
}
