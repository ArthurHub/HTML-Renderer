# Quickstart

## Install

Download the [latest release zip](https://github.com/ArthurHub/HTML-Renderer/releases), worth having around for the [Demo application](./Demo-application).  
Either reference the proper DLL's from the downloaded release zip: HtmlRenderer.dll and one of: HtmlRenderer.WinForms.dll, HtmlRenderer.WPF.dll, HtmlRenderer.PdfSharp.dll. Note: add the targeted framework dlls you are targeting in your project, for PdfSharp you will also need to download [PdfSharp dll](https://github.com/empira/PDFsharp). Or, simply install the proper NuGet package using Visual Studio or command line:

* HTML WinForms ([HtmlRenderer.WinForms](https://www.nuget.org/packages/HtmlRenderer.WinForms))
* HTML WPF ([HtmlRenderer.WPF](https://www.nuget.org/packages/HtmlRenderer.WPF))
* Mono ([HtmlRenderer.Mono](https://www.nuget.org/packages/HtmlRenderer.Mono))
* PDF using PdfSharp ([HtmlRenderer.PdfSharp](https://www.nuget.org/packages/HtmlRenderer.PdfSharp))

## WinForms HtmlPanel

In form (Form1) add the following:

``` csharp
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

## WPF HtmlPanel

In window (Window1) XAML add the following:

``` xml
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

## Generate Image

Add the following snippet in your main method: (for more advance usage see [Image generation](image-generation))

``` csharp
class Program
{
    private static void Main(string[] args)
    {
        Image image = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImage("<p><h1>Hello World</h1>This is html rendered text</p>");
        image.Save("image.png", ImageFormat.Png);
    }
}
```

## Generate PDF

Add the following snippet in your main method: (for more advance usage see [PDF generation](pdf-generation))

``` csharp
class Program
{
    private static void Main(string[] args)
    {
        PdfDocument pdf = PdfGenerator.GeneratePdf("<p><h1>Hello World</h1>This is html rendered text</p>", PageSize.A4);
        pdf.Save("document.pdf");
    }
}
```
