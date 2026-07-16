namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal enum ParseError : byte
    {
        EOF = 0,
        InvalidCharacter = 16, // 0x10,
        InvalidBlockStart = 17, // 0x11,
        InvalidToken = 18, // 0x12,
        ColonMissing = 19, // 0x13,
        IdentExpected = 20, // 0x14,
        InputUnexpected = 21, // 0x15,
        LineBreakUnexpected = 22, // 0x16,
        UnknownAtRule = 32, // 0x20,
        InvalidSelector = 48, // 0x30,
        InvalidKeyframe = 49, // 0x31,
        ValueMissing = 64, // 0x40,
        InvalidValue = 65, // 0x41,
        UnknownDeclarationName = 80 // 0x50
    }
}