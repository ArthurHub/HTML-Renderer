using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Network;

namespace HtmlRenderer.Test.Html;

internal static class EmbeddedTestSupport
{
    /// <summary>An image the <see cref="MockAdapter"/> decodes as 40x30, whatever the bytes.</summary>
    public const string Png = "data:image/png;base64,iVBORw0KGgo=";

    /// <summary>Lays out <paramref name="body"/> with images loaded during layout and returns the box with the given id.</summary>
    public static CssBox Loaded(string body, string id = "x", MockAdapter? adapter = null)
    {
        var (root, _) = LayoutWithImages(body, adapter);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box, $"No box with id '{id}'");
        return box;
    }

    /// <summary>Lays out <paramref name="body"/> with images loaded during layout.</summary>
    public static (CssBox Root, TheArtOfDev.HtmlRenderer.Core.HtmlContainerInt Container) LayoutWithImages(string body, MockAdapter? adapter = null) =>
        LayoutHarness.Layout(LayoutHarness.Wrap(body), adapter: adapter, prepare: root =>
        {
            root.HtmlContainer!.AvoidAsyncImagesLoading = true;
            root.HtmlContainer.AvoidImagesLateLoading = true;
        });

    /// <summary>The image of the img box with the given id, null if none was loaded.</summary>
    public static object? ImageOf(CssBox box) => ((CssBoxImage)box).Image;

    /// <summary>The text of every word in the box and its descendants, space separated.</summary>
    public static string Text(CssBox box) =>
        string.Join(" ", LayoutHarness.Descendants(box).SelectMany(b => b.Words).Select(w => w.Text).Where(t => !string.IsNullOrEmpty(t)));

    /// <summary>Records the URLs the document fetches, and fetches nothing.</summary>
    public sealed class RecordingLoader(string baseUri) : RNetworkLoader
    {
        public List<string> Requests { get; } = [];

        public override RUri? BaseUri { get; } = new RUri(baseUri);

        public override Task<string> GetPrimaryContents() => Task.FromResult("");

        public override Task<RNetworkResponse?> GetResourceStream(RUri uri)
        {
            Requests.Add(uri.AbsoluteUri);
            return Task.FromResult<RNetworkResponse?>(null);
        }
    }
}
