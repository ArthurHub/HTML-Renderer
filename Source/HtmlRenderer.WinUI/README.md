# Welcome to the HTML Renderer WinUI 3 library!

This library provides the rich formatting power of HTML in your WinUI 3 .NET applications using
simple controls or static rendering code, drawn with Win2D.
For more info see HTML Renderer on GitHub: https://github.com/ArthurHub/HTML-Renderer

## REQUIREMENTS

- A WinUI 3 app on Windows App SDK 1.8 or newer, packaged or unpackaged
- Windows 10 version 1809 (build 17763) or newer
- A platform specific build (x86, x64 or arm64), as Win2D ships a native DLL per platform

## DEMO APPLICATION

HTML Renderer Demo application showcases HTML Renderer capabilities, use it to explore and learn
on the library: https://github.com/ArthurHub/HTML-Renderer/tree/master/Source/Demo/WinUI

## FEEDBACK / RELEASE NOTES

If you have problems, wish to report a bug, or have a suggestion, please open an issue on the
HTML Renderer issue page: https://github.com/ArthurHub/HTML-Renderer/issues

For full release notes and all versions see: https://github.com/ArthurHub/HTML-Renderer/releases

---

## Quick Start: Use HTML panel control on a WinUI 3 window

```xaml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:html="using:TheArtOfDev.HtmlRenderer.WinUI">
    <Grid>
        <html:HtmlPanel Text="&lt;p&gt; &lt;h1&gt; Hello World &lt;/h1&gt; This is html rendered text&lt;/p&gt;"/>
    </Grid>
</Window>
```

## Quick Start: Create image from HTML snippet

```csharp
using CanvasRenderTarget image = await HtmlRender.RenderToImageAsync("<p><h1>Hello World</h1>This is html rendered text</p>");
await image.SaveAsync(Path.GetFullPath("image.png"), CanvasBitmapFileFormat.Png);
```

## Quick Start: Use a font that isn't installed

```csharp
HtmlRender.AddFontFamily(new Uri(Path.GetFullPath("CustomFont.ttf")));
```
