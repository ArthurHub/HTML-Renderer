using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HtmlRenderer.Demo.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Usage: htmltopdf [input-file] [output-file]");
                return;
            }

            var pdf = PdfGenerator.GeneratePdf(File.ReadAllText(args[0]), PdfSharp.PageSize.A4);
            pdf.Save(args[1]);
        }
    }
}
