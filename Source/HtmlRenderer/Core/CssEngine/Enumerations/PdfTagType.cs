namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Values for the PeachPDF-specific <c>-peachpdf-pdf-tag-type</c> property, which controls
    /// the PDF structure (tag) type an element maps to when tagged PDF output is enabled.
    /// </summary>
    internal enum PdfTagType
    {
        Auto,
        None,
        Part,
        Art,
        Sect,
        Div,
        Index,
        BlockQuote,
        Caption,
        Toc,
        Toci,
        P,
        H1,
        H2,
        H3,
        H4,
        H5,
        H6,
        L,
        Li,
        Lbl,
        LBody,
        Dl,
        DlDiv,
        Dt,
        Dd,
        Span,
        Quote,
        Table,
        Tr,
        Th,
        Td,
        THead,
        TBody,
        TFoot,
        BibEntry,
        Code,
        Figure,
        Formula,
        Artifact,
        Note,
        Reference,
        Link
    }
}
