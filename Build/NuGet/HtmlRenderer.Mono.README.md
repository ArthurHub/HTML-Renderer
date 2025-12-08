# Welcome to the HTML Renderer WinForms library!

This library provides the rich formatting power of HTML in Mono .NET applications using
simple controls or static rendering code.
For more info see HTML Renderer on GitHub: https://github.com/ArthurHub/HTML-Renderer

## DEMO APPLICATION

HTML Renderer Demo application showcases HTML Renderer capabilities, use it to explore and learn
on the library: https://codeplexarchive.org/ProjectTab/Wiki/HtmlRenderer/Documentation/Demo%20application

## FEEDBACK / RELEASE NOTES

If you have problems, wish to report a bug, or have a suggestion, please open an issue on the
HTML Renderer issue page: https://github.com/ArthurHub/HTML-Renderer/issues

For full release notes and all versions see: https://github.com/ArthurHub/HTML-Renderer/releases

## QUICK START

For more Quick Start see: https://codeplexarchive.org/ProjectTab/Wiki/HtmlRenderer/Documentation/Documentation

---

## Quick Start: Use HTML panel control on WinForms form

```csharp
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel htmlPanel = new TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel();
        htmlPanel.Text = "<p><h1>Hello World</h1>This is html rendered text</p>";
        htmlPanel.Dock = DockStyle.Fill;
        Controls.Add(htmlPanel);
    }
}
```

## Quick Start: Create image from HTML snippet

```csharp
class Program
{
    private static void Main(string[] args)
    {
        Image image = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImageGdiPlus("<p><h1>Hello World</h1>This is html rendered text</p>");
        image.Save("image.png", ImageFormat.Png);
    }
}
```