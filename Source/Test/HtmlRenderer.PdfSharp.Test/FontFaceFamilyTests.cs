using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution;

namespace HtmlRenderer.PdfSharp.Test;

// Parallel full layout passes race on the adapter singletons.
[TestClass]
[DoNotParallelize]
public sealed class FontFaceFamilyTests
{
    [TestMethod]
    [Spec("css-fonts-4", "#font-family-desc")]
    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    public void FontFaceNamedLikeASystemFamily_HidesTheSystemFaces()
    {
        var resolver = new FontResolver();
        var family = SystemFamilyWithABoldFace(resolver);

        using (var tuffy = Fallback("Tuffy.ttf"))
            resolver.AddFont(tuffy, family, 400, false, 100, null);

        var regular = resolver.ResolveTypeface(family, 400, false);
        var bold = resolver.ResolveTypeface(family, 700, false);

        Assert.AreEqual(regular.FaceName, bold.FaceName);
        Assert.IsTrue(bold.MustSimulateBold);
    }

    [TestMethod]
    [Spec("css-fonts-4", "#font-family-desc")]
    [Ignore("Gap, see Spec/css-fonts-4.yml")]
    public async Task FontFace_DoesntApplyToALaterDocumentWithoutIt()
    {
        var family = "Scoped" + Guid.NewGuid().ToString("N");
        var text = $"<p style=\"font-family: '{family}'\">a</p>";

        string fontFace;
        using (var tuffy = Fallback("Tuffy Bold Italic.ttf"))
        using (var bytes = new MemoryStream())
        {
            tuffy.CopyTo(bytes);
            fontFace = $"<style>@font-face {{ font-family: '{family}'; src: url(data:font/ttf;base64,{Convert.ToBase64String(bytes.ToArray())}) }}</style>";
        }

        using var withFontFace = await PdfGenerator.GeneratePdfAsync($"<html><head>{fontFace}</head><body>{text}</body></html>", PageSize.A4);
        using var without = await PdfGenerator.GeneratePdfAsync($"<html><body>{text}</body></html>", PageSize.A4);

        Assert.IsTrue(FontNames(withFontFace).Any(IsTuffyBoldItalic));
        Assert.IsFalse(FontNames(without).Any(IsTuffyBoldItalic));
    }

    private static bool IsTuffyBoldItalic(string baseFont) => baseFont.EndsWith("+Tuffy,BoldItalic", StringComparison.Ordinal);

    private static string SystemFamilyWithABoldFace(FontResolver resolver)
    {
        foreach (var family in resolver.DiscoverFontFamilies())
        {
            var regular = resolver.ResolveTypeface(family, 400, false);
            var bold = resolver.ResolveTypeface(family, 700, false);
            if (regular != null && bold != null && !bold.MustSimulateBold && bold.FaceName != regular.FaceName)
                return family;
        }

        Assert.Inconclusive("No system font family with a bold face");
        return string.Empty;
    }

    private static Stream Fallback(string fileName)
    {
        var assembly = typeof(PdfGenerator).Assembly;
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + fileName, StringComparison.Ordinal));
        return assembly.GetManifestResourceStream(name)!;
    }

    private static List<string> FontNames(PdfDocument document) =>
        document.Pages.Cast<PdfPage>()
            .Select(p => p.Resources.Elements.GetDictionary("/Font"))
            .Where(fonts => fonts != null)
            .SelectMany(fonts => fonts!.Elements.Values.Select(v => v is PdfReference r ? r.Value : v).OfType<PdfDictionary>())
            .Select(font => font.Elements.GetName("/BaseFont"))
            .ToList();
}
