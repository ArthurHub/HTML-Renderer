# Welcome to the HTML Renderer SkiaSharp library!

This library renders HTML snippets onto a SkiaSharp canvas, into images and into PDF documents, on Windows, Linux and macOS.
For more info see HTML Renderer on GitHub: https://github.com/ArthurHub/HTML-Renderer

## REQUIREMENTS

- .NET Standard 2.0 or .NET 8.0 and newer
- On Linux, references to `SkiaSharp.NativeAssets.Linux` and `HarfBuzzSharp.NativeAssets.Linux`, as the `SkiaSharp` and `HarfBuzzSharp` packages only ship the native libraries for Windows and macOS. Note that `SkiaSharp.NativeAssets.Linux.NoDependencies` works as well, but has no fontconfig, so there's no fallback font for characters the chosen font lacks, they render as boxes

## LIMITATIONS

- Text is shaped per word with HarfBuzz, so right-to-left words read correctly, but a line that mixes directions isn't reordered as a whole
- PDFs embed the complete font files, SkiaSharp's PDF backend doesn't subset them, so a PDF with a few fonts easily gets a few MB. Fallback fonts count as well: a single line of Chinese or Japanese pulls in a whole system font collection such as Microsoft YaHei or Yu Gothic, which can add well over 10 MB. If the size matters, provide a small font that covers the text via `@font-face` or `HtmlRender.AddFontFamily`

## FEEDBACK / RELEASE NOTES

If you have problems, wish to report a bug, or have a suggestion, please open an issue on the
HTML Renderer issue page: https://github.com/ArthurHub/HTML-Renderer/issues

For full release notes and all versions see: https://github.com/ArthurHub/HTML-Renderer/releases

---

## Quick Start: Create image from HTML snippet

```csharp
using SKImage image = await HtmlRender.RenderToImageAsync("<p><h1>Hello World</h1>This is html rendered text</p>");
using SKData png = image.Encode(SKEncodedImageFormat.Png, 100);
File.WriteAllBytes("image.png", png.ToArray());
```

## Quick Start: Create PDF from HTML snippet

```csharp
using FileStream stream = File.Create("document.pdf");
await PdfGenerator.GeneratePdfAsync("<p><h1>Hello World</h1>This is html rendered text</p>", stream);
```

The page size and margins are in points, e.g. `new PdfGenerateConfig { PageSize = PdfPageSizes.Letter }`.

## Quick Start: Draw onto an existing canvas

```csharp
SKSize size = await HtmlRender.RenderAsync(canvas, "<p><h1>Hello World</h1>This is html rendered text</p>", 10, 10, 500);
```

## Quick Start: Use a font that isn't installed

```csharp
HtmlRender.AddFontFamily(SKTypeface.FromFile("CustomFont.ttf"));
```
