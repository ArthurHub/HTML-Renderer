
********** Welcome to the HTML Renderer WPF library! **********************************************

This library provides the rich formatting power of HTML in your WPF .NET applications using
simple controls or static rendering code.
For more info see HTML Renderer on CodePlex: http://htmlrenderer.codeplex.com

********** DEMO APPLICATION ***********************************************************************

HTML Renderer Demo application showcases HTML Renderer capabilities, use it to explore and learn
on the library: http://htmlrenderer.codeplex.com/wikipage?title=Demo%20application

********** FEEDBACK / RELEASE NOTES ***************************************************************

If you have problems, wish to report a bug, or have a suggestion please start a thread on 
HTML Renderer discussions page: http://htmlrenderer.codeplex.com/discussions

For full release notes and all versions see: http://htmlrenderer.codeplex.com/releases

********** QUICK START ****************************************************************************

For more Quick Start see: https://htmlrenderer.codeplex.com/wikipage?title=Quick%20start

***************************************************************************************************
********** Quick Start: Use HTML panel control on WPF window

<Window x:Class="Window1"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Window1" Height="300" Width="300"
        xmlns:wpf="clr-namespace:TheArtOfDev.HtmlRenderer.WPF;assembly=HtmlRenderer.WPF">
    <Grid>
        <wpf:HtmlPanel Text="&lt;p&gt; &lt;h1&gt; Hello World &lt;/h1&gt; This is html rendered text&lt;/p&gt;"/>
    </Grid>
</Window>

***************************************************************************************************
********** Quick Start: Create image from HTML snippet

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
