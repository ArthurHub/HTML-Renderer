
********** Welcome to the HTML Renderer PdfSharp library! *****************************************

This library provides the ability to generate PDF documents from HTML snippets using static rendering code.
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
********** Quick Start: Create PDF from HTML snippet using PdfSharp

class Program
{
    private static void Main(string[] args)
    {
        PdfDocument pdf = PdfGenerator.GeneratePdf("<p><h1>Hello World</h1>This is html rendered text</p>", PageSize.A4);
        pdf.Save("document.pdf");
    }
}
