using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class DocumentFunction : StylesheetNode, IDocumentFunction
    {
        internal DocumentFunction(string name, string data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; }
        public string Data { get; }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(Name.StylesheetFunction(Data.StylesheetString()));
        }

        public abstract bool Matches(Url url);
    }
}