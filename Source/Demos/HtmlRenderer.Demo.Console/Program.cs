using HtmlRenderer.Demo.Console;
using System.Diagnostics;
using TheArtOfDev.HtmlRenderer.Demo.Common;

//By default, write to a sub folder 'output'
string basePath= @".\Ouput";
if (args.Length > 0)
{
    //And if there's an output path given, use that.
    basePath = args[0];
}

//Probably won't be running a suite of tests more than once a second, so this will do.
string runIdentifier = DateTime.Now.ToString("ddMMyyyy-hhMMss");

var skia = new SkiaConverter(runIdentifier, basePath);
var pdfSharp = new PdfSharpCoreConverter(runIdentifier, basePath);

SamplesLoader.Init("Console", typeof(Program).Assembly.GetName().Version.ToString());

var samples = SamplesLoader.TestSamples;

foreach (var htmlSample in samples)
{
    skia.GenerateSampleAsync(htmlSample);
    await pdfSharp.GenerateSampleAsync(htmlSample);
}


//At this point.. there should be something!!