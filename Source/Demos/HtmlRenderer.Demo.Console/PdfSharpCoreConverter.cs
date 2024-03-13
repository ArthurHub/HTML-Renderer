using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.Demo.Console
{
    public class PdfSharpCoreConverter : SampleConverterBase
    {
        public PdfSharpCoreConverter(string sampleRunIdentifier, string basePath) : base(sampleRunIdentifier, basePath)
        {
        }

        public async Task GenerateSampleAsync(HtmlSample sample)
        {
            var config = new PdfGenerateConfig();

            config.PageSize = PdfSharpCore.PageSize.A4;
            config.MarginLeft = 0;
            config.MarginRight = 0;
            config.MarginTop = 0;
            config.MarginBottom = 0;

            var pdf = await PdfGenerator.GeneratePdfAsync(sample.Html, config, imageLoad: OnImageLoaded);
            pdf.Save(GetSamplePath(sample));
        }
    }
}
