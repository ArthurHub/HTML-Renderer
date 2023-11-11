using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Demo.Common;
using TheArtOfDev.HtmlRenderer.SkiaSharp;

namespace HtmlRenderer.Demo.Console
{
    public class SkiaConverter : SampleConverterBase
    {
        public SkiaConverter(string sampleRunIdentifier, string basePath) : base(sampleRunIdentifier, basePath)
        {
        }

        public async Task GenerateSampleAsync(HtmlSample sample)
        {
            var config = new PdfGenerateConfig();

            config.PageSize = PageSize.A4;
            
            config.MarginLeft = 0;
            config.MarginRight = 0;
            config.MarginTop = 0;
            config.MarginBottom = 0;

            using (var fileStream = File.Open(GetSamplePath(sample), FileMode.CreateNew))
            {
                await PdfGenerator.GeneratePdfAsync(sample.Html, fileStream, config);
                fileStream.Flush();
            }
        }
    }
}
