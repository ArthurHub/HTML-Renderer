namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
#if !NET40
#endif

    internal static class CharExtensions
    {
        public static int FromHex(this char c)
        {
            return c.IsDigit() ? c - Symbols.Zero : c - (c.IsLowercaseAscii() ? Symbols.CapitalW : Symbols.Seven);
        }

        public static string ToHex(this char character)
        {
            return ((int)character).ToString("x");
        }

        public static bool IsInRange(this char c, int lower, int upper)
        {
            return c >= lower && c <= upper;
        }

        public static bool IsNormalQueryCharacter(this char c)
        {
            return c.IsInRange(Symbols.ExclamationMark, Symbols.Tilde) &&
                   c != Symbols.DoubleQuote &&
                   c != Symbols.CurvedQuote && c != Symbols.Num &&
                   c != Symbols.LessThan && c != Symbols.GreaterThan;
        }

        public static bool IsNormalPathCharacter(this char c)
        {
            return c.IsInRange(Symbols.Space, Symbols.Tilde) &&
                   c != Symbols.DoubleQuote &&
                   c != Symbols.CurvedQuote && c != Symbols.Num &&
                   c != Symbols.LessThan && c != Symbols.GreaterThan &&
                   c != Symbols.Space && c != Symbols.QuestionMark;
        }

        public static bool IsUppercaseAscii(this char c)
        {
            return c >= Symbols.CapitalA && c <= Symbols.CapitalZ;
        }

        public static bool IsLowercaseAscii(this char c)
        {
            return c >= Symbols.LowerA && c <= Symbols.LowerZ;
        }

        public static bool IsAlphanumericAscii(this char c)
        {
            return c.IsDigit() || c.IsUppercaseAscii() || c.IsLowercaseAscii();
        }

        public static bool IsHex(this char c)
        {
            return c.IsDigit() || (c >= Symbols.CapitalA && c <= Symbols.CapitalF) ||
                   (c >= Symbols.LowerA && c <= Symbols.LowerF);
        }

        public static bool IsNonAscii(this char c)
        {
            return c != Symbols.EndOfFile && c >= Symbols.ExtendedAsciiStart;
        }

        public static bool IsNonPrintable(this char c)
        {
            return /*c >= Symbols.Null &&*/ c <= Symbols.Backspace
                || (c >= Symbols.ShiftOut && c <= Symbols.UnitSeparator)
                || (c >= Symbols.Delete && c < Symbols.NonBreakingSpace);
        }

        public static bool IsLetter(this char c)
        {
            return IsUppercaseAscii(c) || IsLowercaseAscii(c);
        }

        public static bool IsName(this char c)
        {
            return c.IsNonAscii() || c.IsLetter() || c == Symbols.Underscore || c == Symbols.Minus || c.IsDigit();
        }

        public static bool IsNameStart(this char c)
        {
            return c.IsNonAscii() || c.IsUppercaseAscii() || c.IsLowercaseAscii() || c == Symbols.Underscore;
        }

        public static bool IsLineBreak(this char c)
        {
            return c == Symbols.LineFeed || c == Symbols.CarriageReturn;
        }

        public static bool IsSpaceCharacter(this char c)
        {
            return c == Symbols.Space || c == Symbols.Tab || c == Symbols.LineFeed || c == Symbols.CarriageReturn || c == Symbols.FormFeed;
        }

        public static bool IsDigit(this char c)
        {
            return c >= Symbols.Zero && c <= Symbols.Nine;
        }

        // HTML forbids the use of Universal Character Set / Unicode code points
        // - 0 to 31, except 9, 10, and 13 C0 control characters
        // - 127 DEL character
        // - 128 to 159 (0x80 to 0x9F, C1 control characters
        // - 55296 to 57343 (0xD800 – xDFFF, UTF-16 surrogate halves)
        // - 65534 and 65535 (xFFFE – xFFFF, non-characters, related to xFEFF, the byte order mark)
        public static bool IsInvalid(this int c)
        {
            return c == 0 || c > Symbols.MaximumCodepoint ||
                   (c > Symbols.UTF16SurrogateMin && c < Symbols.UTF16SurrogateMax);
        }

        public static bool IsOneOf(this char c, char a, char b)
        {
            return a == c || b == c;
        }

        public static bool IsOneOf(this char c, char o1, char o2, char o3)
        {
            return c == o1 || c == o2 || c == o3;
        }

        public static bool IsOneOf(this char c, char o1, char o2, char o3, char o4)
        {
            return c == o1 || c == o2 || c == o3 || c == o4;
        }
    }
}