using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.IntegrationTest;

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
    [Spec("css21", "page.html#allowed-page-breaks")]
    public async Task MonolithicContentTallerThanOnePage_IsLeftInPlace_NotMoved()
    {
        using var wrapper = new HtmlContainer();
        // A scroll container is monolithic, this one is taller than the 700px page.
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
        // Straddles the boundary it naturally crosses instead of moving to a page it wouldn't fit either.
        Assert.AreEqual(2, tree.Fragmentainers.Count);
    }
}
