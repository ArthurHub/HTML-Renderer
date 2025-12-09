# FAQ

## Why Are SVG Images Not Working?

.NET doesn't support SVG format.  
A workaround is to convert SVG to PNG during rendering, see [Rendering SVG images](docs/rendering-svg-images).

## Is WPF Supported?

Yep, WPF is fully supported.

## Is AvaloniaUI Supported?

Not directly, but there's a separately maintained fork: https://github.com/AvaloniaUI/Avalonia.HtmlRenderer

## Is SkiaSharp Supported?

Not at the moment, some attempts [have been made](https://github.com/ArthurHub/HTML-Renderer/pull/200) in the past.  
Care to contribute?

## Are Metro, Xamarin, Unity, XNA or FNA Supported?

Some of them are not as relevant at the used to be.  
Is it something that enough people would like to see?

## Is Mono Supported?

It used to be supported, but since Mono has been largely merged into the .NET runtime, there's little point in maintaining this.

Starting with HtmlRenderer 1.5.1, Mono support has been halted.

## Is Silverlight Supported?

Silverlight has been discontinued in 2019.  
You may want to check your calendar.

## What About JavaScript?

JavaScript is extremely hard to support, so it is not really in scope.

## Why Can't I Navigate to a URL?

HTML Renderer is not a web-browser.

## Can I Generate Image From HTML?

Yep, see the [Image Generation](docs/image-generation) documentation.

## Can I Generate PDF Document From HTML?

Yep, see [PDF Generation](docs/pdf-generation) documentation.  
Currently powered by [PDFsharp](https://www.pdfsharp.com/) but it is quite easy to add more frameworks like [iTextSharp](https://github.com/itext/itext-dotnet).

## Can You Sign the Library With Strong-Name?

Strong-Names [seems to have been](https://github.com/octokit/octokit.net/issues/405) quite the religious war some years ago.  
These days, [Microsoft recommends](https://github.com/dotnet/runtime/blob/main/docs/project/strong-name-signing.md) to not strong-name assemblies.

## Can You Sign the Library With a Certificate?

While this would be awesome, it won't be happening anytime soon, because of two reasons:

* Getting a certificate to sign assemblies costs money, which this open source project doesn't have
* To get a certificate, you need a legal entity, which this open source project doesn't have

The only potential paths forward would be to either join the [Open Source Collective](https://oscollective.org/) or be adopted by the [.NET Foundation](https://dotnetfoundation.org/), both of which offer code signing.

## Can I Contribute?

Sure! go to [ArthurHub/HTML-Renderer](https://github.com/ArthurHub/HTML-Renderer) on GitHub and fork away.
