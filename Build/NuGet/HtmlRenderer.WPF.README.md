# Welcome to the HTML Renderer WPF library!

This library provides the rich formatting power of HTML in your WPF .NET applications using
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

## Quick Start: Use HTML panel control on WPF window

```xaml
<Window x:Class="Window1"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Window1" Height="300" Width="300"
        xmlns:wpf="clr-namespace:TheArtOfDev.HtmlRenderer.WPF;assembly=HtmlRenderer.WPF">
    <Grid>
        <wpf:HtmlPanel Text="&lt;p&gt; &lt;h1&gt; Hello World &lt;/h1&gt; This is html rendered text&lt;/p&gt;"/>
    </Grid>
</Window>
```

## Quick Start: Create image from HTML snippet

```csharp
class Program
{
    private static void Main(string[] args)
    {
	BitmapFrame image = HtmlRender.RenderToImage("<p><h1>Hello World</h1>This is html rendered text</p>");
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(image);
        using (FileStream stream = new FileStream("image.png", FileMode.OpenOrCreate))
            encoder.Save(stream);
    }
}
```