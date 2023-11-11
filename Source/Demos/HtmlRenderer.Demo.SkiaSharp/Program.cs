// See https://aka.ms/new-console-template for more information

using SkiaSharp;
using TheArtOfDev.HtmlRenderer.SkiaSharp;
/*
var info = new SKImageInfo(256, 256);
using (var surface = SKSurface.Create(info))
{
SKCanvas canvas = surface.Canvas;

canvas.Clear(SKColors.White);

// configure our brush
var redBrush = new SKPaint
{
Color = new SKColor(0xff, 0, 0),
IsStroke = true
};
var blueBrush = new SKPaint
{
Color = new SKColor(0, 0, 0xff),
IsStroke = true
};

for (int i = 0; i < 64; i += 8)
{
var rect = new SKRect(i, i, 256 - i - 1, 256 - i - 1);
canvas.DrawRect(rect, (i % 16 == 0) ? redBrush : blueBrush);
}

using (var image = surface.Snapshot())
using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
using (var stream = File.OpenWrite(Path.Combine(@"c:\temp\SkiaSharpTests", "1.png")))
{
// save the data to a stream
data.SaveTo(stream);
}

Console.ReadLine();
}*/

using (var outputFile = new FileStream(@"c:\temp\SkiaSharpTests\test.pdf", FileMode.Create))
{
    var pdfDocument = PdfGenerator.GeneratePdfAsync(@"
        <!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""
        ""http://www.w3.org/TR/html4/strict.dtd"">
        <html>
        <head><title>w3resource tutorial</title></head>
        <body>
        <h1>we are learning html</h1>
        <h2>we are learning html at w3resource.com.</h2>
        <p>This section covers the introduction to html</p>
        <p><a href=""/index.php"">Look here to get a list of the topics covered in
        w3resource.com</a></p>
        </body>
        </html>", outputFile);

    await outputFile.FlushAsync();
}
