using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

/// <summary>
/// Verifies the R3 stage of the fragmentation-engine-parity plan: <c>break-inside:avoid</c>/monolithic
/// relocation now relays the child out fresh at its target position (<c>CssBox.ResumeAt</c> + a second
/// <c>PerformLayout</c> call within the same pass) instead of shifting already-finished geometry with
/// <c>OffsetTop</c>.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StageR3RelocationTest
{
    private static HtmlContainerInt GetInternal(HtmlContainer wrapper)
    {
        var prop = typeof(HtmlContainer).GetProperty("HtmlContainerInt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (HtmlContainerInt)prop.GetValue(wrapper)!;
    }

    [TestMethod]
    public async Task MonolithicContentTallerThanOnePage_IsLeftInPlace_NotMoved()
    {
        using var wrapper = new HtmlContainer();
        // A scroll container (overflow:hidden, MonolithicContent.IsScrollContainer) taller than the
        // 700px page - RelocateIfNeeded's "fits on no single page" guard must leave it straddling the
        // boundary in place rather than moving it (nowhere to move it TO would help) or looping.
        await wrapper.SetHtml(
            """
            <html><body>
                <div style="margin:0; height:250px;">filler</div>
                <div style="margin:0; height:900px; overflow:hidden;">monolithic content taller than one page</div>
            </body></html>
            """);

        var container = GetInternal(wrapper);
        container.PageSize = new TheArtOfDev.HtmlRenderer.Adapters.Entities.RSize(500, 700);
        container.MarginTop = 0;
        wrapper.MaxSize = new SizeF(500, 0);

        using var bitmap = new Bitmap(500, 5000);
        using var g = Graphics.FromImage(bitmap);
        wrapper.PerformLayout(g);

        var tree = container.FragmentTree;
        Assert.IsNotNull(tree);
        // Straddles the one boundary it naturally crosses (250 + 900 = 1150, past the 700px mark) and
        // stops there - not moved to a later page (which would still not fit it whole) and not spun
        // into extra pages by a mistaken relocation attempt.
        Assert.AreEqual(2, tree.Fragmentainers.Count);
    }
}
