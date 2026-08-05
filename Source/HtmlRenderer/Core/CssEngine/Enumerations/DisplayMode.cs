namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal enum DisplayMode : byte
    {
        None,
        Inline,
        Block,
        ListItem,
        InlineBlock,
        InlineTable,
        Table,
        TableCaption,
        TableCell,
        TableColumn,
        TableColumnGroup,
        TableFooterGroup,
        TableHeaderGroup,
        TableRow,
        TableRowGroup,
        Flex,
        InlineFlex,
        Grid,
        InlineGrid
    }
}