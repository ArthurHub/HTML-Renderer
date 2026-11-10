using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace HtmlRenderer.IntegrationTest.Painting;

[DoNotParallelize]
[TestClass]
public sealed class VisibilityPaintIntegrationTests
{
    [TestMethod]
    [Spec("css21", "visufx.html#visibility")]
    [Spec("css-display-3", "#visibility")]
    [Spec("css-display-4", "#visibility")]
    public void Hidden_IsInheritedByDescendants()
    {
        var recording = Paint("<div style='visibility:hidden'><div style='height:10px;background:rgb(255,0,0)'>Child</div></div>");

        Assert.IsFalse(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(255, 0, 0)));
        Assert.IsFalse(recording.DrawStringCalls.Any(c => c.Text == "Child"));
    }

    [TestMethod]
    [Spec("css21", "visufx.html#visibility")]
    [Spec("css-display-3", "#visibility")]
    [Spec("css-display-4", "#visibility")]
    public void HiddenInline_PaintsNothingButKeepsItsSpace()
    {
        var hidden = Paint("<div>aaa <span style='visibility:hidden'>Hid</span> bbb</div>");
        var visible = Paint("<div>aaa <span>Hid</span> bbb</div>");

        Assert.IsFalse(hidden.DrawStringCalls.Any(c => c.Text == "Hid"));
        Assert.AreEqual(visible.DrawStringCalls.Single(c => c.Text == "bbb").Point, hidden.DrawStringCalls.Single(c => c.Text == "bbb").Point);
    }

    [Ignore("Gap, see Spec/css21.yml")]
    [TestMethod]
    [Spec("css21", "visufx.html#visibility")]
    [Spec("css-display-3", "#visibility")]
    [Spec("css-display-4", "#visibility")]
    public void VisibleDescendantOfAHiddenElement_IsPainted()
    {
        var recording = Paint("<div style='visibility:hidden'><div style='visibility:visible;height:10px;background:rgb(255,0,0)'>Shown</div></div>");

        Assert.IsTrue(recording.Log.OfType<RecordingGraphics.DrawRectCall>().Any(r => r.Color == RColor.FromArgb(255, 0, 0)));
        Assert.IsTrue(recording.DrawStringCalls.Any(c => c.Text == "Shown"));
    }

    private static string Doc(string body) => $"<!DOCTYPE html><html><head></head><body style='margin:0'>{body}</body></html>";

    private static RecordingGraphics Paint(string body)
    {
        var (_, container) = PaintHarness.Layout(Doc(body));
        return PaintHarness.PaintPage(container);
    }
}
