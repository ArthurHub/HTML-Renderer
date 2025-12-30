namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    public class FontMetadata
    {
        public string FilePath { get; set; }
    
        public string Family { get; set; }
        public string Subfamily { get; set; }
    
        public string PreferredFamily { get; set; }
        public string PreferredSubfamily { get; set; }
    
        public string FullName { get; set; }
        public string PostScriptName { get; set; }
    
        public FontAttributes Attributes { get; set; }
    }
}