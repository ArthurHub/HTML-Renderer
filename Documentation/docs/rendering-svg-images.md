# Rendering SVG Images

## Rendering SVG images in HTML Renderer

.NET framework doesn't support SVG format so HTML Renderer cannot natively load and render it.
To overcome this limitation a third-party library can be used to convert SVG formatted image to PNG formatted image that can be rendered by .NET.  
It can be done using `ImageLoad` delegate/event that allows to intercept image loading and overwrite SVG image loading with converted PNG image.

### Example

Generate image from HTML containing SVG image  
Using [SVG](https://github.com/vvvv/SVG) open-source project added using [NuGet](http://www.nuget.org/packages/Svg/) package.  
Rendering this [SVG image](http://www.tuxpaint.org/stamps/stamps/animals/insects/cartoon/monarchbutterfly.svg).

``` csharp
string html = "<body><h3>Rendered <b>SVG</b> image</h3><hr/><img src='d:\\svg.svg'/></body>";
CssData css = CssData.Parse("body { font:14pt Tahoma } h3 { color: navy; font-weight:normal; }");
Image generatedImage = HtmlRender.RenderToImage(html, cssData: css, imageLoad: (sender, args) =>
{
    var path = Path.GetExtension(args.Src);
    if( path != null && path.Equals(".svg", StringComparison.OrdinalIgnoreCase) )
    {
        SvgDocument svgDoc = SvgDocument.Open(args.Src);
        Bitmap svgImg = new Bitmap((int)svgDoc.Width, (int)svgDoc.Height, PixelFormat.Format32bppArgb);
        svgDoc.Draw(svgImg);
        args.Callback(svgImg);
    }
});
generatedImage.Save(@"d:\html.png", ImageFormat.Png);
```

### Output

![html.png](images/html.png "html.png")
