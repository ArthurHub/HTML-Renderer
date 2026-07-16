using System;
using System.Globalization;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class Lexer : LexerBase
    {
        public event EventHandler<TokenizerError> Error;
        private TextPosition _position;

        public Lexer(TextSource source) : base(source)
        {
            IsInValue = false;
        }

        public Lexer(string source) : base(new TextSource(source))
        {
            IsInValue = false;
        }

        public bool IsInValue { get; set; }

        public Token Get()
        {
            var current = GetNext();
            _position = GetCurrentPosition();
            return Data(current);
        }

        internal void RaiseErrorOccurred(ParseError error, TextPosition position)
        {
            var handler = Error;
            if (handler == null) return;

            var errorEvent = new TokenizerError(error, position);
            handler.Invoke(this, errorEvent);
        }

        private Token Data(char current)
        {
            _position = GetCurrentPosition();
            switch (current)
            {
                case Symbols.FormFeed:
                case Symbols.LineFeed:
                case Symbols.CarriageReturn:
                case Symbols.Tab:
                case Symbols.Space:
                    return NewWhitespace(current);
                case Symbols.DoubleQuote:
                    return StringDoubleQuote();
                case Symbols.Num:
                    return IsInValue ? ColorLiteral() : HashStart();
                case Symbols.Dollar:
                    current = GetNext();
                    return current == Symbols.Equality ? NewMatch(Combinators.Ends) : NewDelimiter(GetPrevious());
                case Symbols.SingleQuote:
                    return StringSingleQuote();
                case Symbols.RoundBracketOpen:
                    return NewOpenRound();
                case Symbols.RoundBracketClose:
                    return NewCloseRound();
                case Symbols.Asterisk:
                    current = GetNext();
                    return current == Symbols.Equality ? NewMatch(Combinators.InText) : NewDelimiter(GetPrevious());
                case Symbols.Plus:
                    {
                        var c1 = GetNext();
                        if (c1 != Symbols.EndOfFile)
                        {
                            var c2 = GetNext();
                            Back(2);

                            if (c1.IsDigit() || c1 == Symbols.Dot && c2.IsDigit()) return NumberStart(current);
                        }
                        else
                        {
                            Back();
                        }

                        return NewDelimiter(current);
                    }
                case Symbols.Comma:
                    return NewComma();

                case Symbols.Dot:
                    {
                        var c = GetNext();
                        return c.IsDigit() ? NumberStart(GetPrevious()) : NewDelimiter(GetPrevious());
                    }
                case Symbols.Minus:
                    {
                        var c1 = GetNext();
                        if (c1 != Symbols.EndOfFile)
                        {
                            var c2 = GetNext();
                            Back(2);
                            if (c1.IsDigit() || c1 == Symbols.Dot && c2.IsDigit()) return NumberStart(current);
                            if (c1.IsNameStart()) return IdentStart(current);
                            if (c1 == Symbols.ReverseSolidus && !c2.IsLineBreak() && c2 != Symbols.EndOfFile)
                                return IdentStart(current);

                            if (c1 == Symbols.Minus)
                            {
                                if (c2 == Symbols.GreaterThan)
                                {
                                    Advance(2);
                                    return NewCloseComment();
                                }

                                return IdentStart(current);
                            }

                            return NewDelimiter(current);
                        }

                        Back();

                        return NewDelimiter(current);
                    }
                case Symbols.Solidus:
                    current = GetNext();
                    return current == Symbols.Asterisk
                        ? Comment()
                        : NewDelimiter(GetPrevious());

                case Symbols.ReverseSolidus:
                    current = GetNext();
                    if (current.IsLineBreak())
                    {
                        RaiseErrorOccurred(ParseError.LineBreakUnexpected);
                        return NewDelimiter(GetPrevious());
                    }

                    if (current != Symbols.EndOfFile) return IdentStart(GetPrevious());

                    RaiseErrorOccurred(ParseError.EOF);
                    return NewDelimiter(GetPrevious());

                case Symbols.Colon:
                    return NewColon();
                case Symbols.Semicolon:
                    return NewSemicolon();
                case Symbols.LessThan:
                    current = GetNext();
                    if (current == Symbols.ExclamationMark)
                    {
                        current = GetNext();
                        if (current == Symbols.Minus)
                        {
                            current = GetNext();
                            if (current == Symbols.Minus) return NewOpenComment();
                            // ReSharper disable once RedundantAssignment
                            current = GetPrevious();
                        }

                        GetPrevious();
                        return NewDelimiter(GetPrevious());
                    }
                    else if (current == Symbols.Equality)
                    {
                        Advance();
                        return NewLessThanOrEqual();
                    }
                    else
                    {
                        return NewLessThan();
                    }
                case Symbols.At:
                    return AtKeywordStart();
                case Symbols.SquareBracketOpen:
                    return NewOpenSquare();
                case Symbols.SquareBracketClose:
                    return NewCloseSquare();
                case Symbols.Accent:
                    current = GetNext();
                    return current == Symbols.Equality
                        ? NewMatch(Combinators.Begins)
                        : NewDelimiter(GetPrevious());
                case Symbols.CurlyBracketOpen:
                    return NewOpenCurly();
                case Symbols.CurlyBracketClose:
                    return NewCloseCurly();
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return NumberStart(current);
                case 'U':
                case 'u':
                    current = GetNext();
                    if (current != Symbols.Plus) return IdentStart(GetPrevious());
                    current = GetNext();
                    if (current.IsHex() || current == Symbols.QuestionMark) return UnicodeRange(current);
                    // ReSharper disable once RedundantAssignment
                    current = GetPrevious();
                    return IdentStart(GetPrevious());

                case Symbols.Pipe:
                    current = GetNext();
                    if (current == Symbols.Equality)
                        return NewMatch(Combinators.InToken);
                    else if (current == Symbols.Pipe) return NewColumn();

                    return NewDelimiter(GetPrevious());

                case Symbols.Tilde:
                    current = GetNext();
                    return current == Symbols.Equality
                        ? NewMatch(Combinators.InList)
                        : NewDelimiter(GetPrevious());

                case Symbols.EndOfFile:
                    return NewEof();
                case Symbols.ExclamationMark:
                    current = GetNext();
                    return current == Symbols.Equality
                        ? NewMatch(Combinators.Unlike)
                        : NewDelimiter(GetPrevious());
                case Symbols.GreaterThan:
                    if (GetNext() == Symbols.Equality)
                    {
                        Advance();
                        return NewGreaterThanOrEqual();
                    }
                    GetPrevious();
                    return NewGreaterThan();
                default:
                    return current.IsNameStart() ? IdentStart(current) : NewDelimiter(current);
            }
        }

        private Token StringDoubleQuote()
        {
            while (true)
            {
                var current = GetNext();
                switch (current)
                {
                    case Symbols.DoubleQuote:
                    case Symbols.EndOfFile:
                        return NewString(FlushBuffer(), Symbols.DoubleQuote);
                    case Symbols.FormFeed:
                    case Symbols.LineFeed:
                        RaiseErrorOccurred(ParseError.LineBreakUnexpected);
                        Back();
                        return NewString(FlushBuffer(), Symbols.DoubleQuote, true);
                    case Symbols.ReverseSolidus:
                        current = GetNext();
                        if (current.IsLineBreak())
                        {
                            StringBuffer.AppendLine();
                        }
                        else if (current != Symbols.EndOfFile)
                        {
                            StringBuffer.Append(ConsumeEscape(current));
                        }
                        else
                        {
                            RaiseErrorOccurred(ParseError.EOF);
                            Back();
                            return NewString(FlushBuffer(), Symbols.DoubleQuote, true);
                        }

                        break;
                    default:
                        StringBuffer.Append(current);
                        break;
                }
            }
        }

        private Token StringSingleQuote()
        {
            while (true)
            {
                var current = GetNext();
                switch (current)
                {
                    case Symbols.SingleQuote:
                    case Symbols.EndOfFile:
                        return NewString(FlushBuffer(), Symbols.SingleQuote);
                    case Symbols.FormFeed:
                    case Symbols.LineFeed:
                        RaiseErrorOccurred(ParseError.LineBreakUnexpected);
                        Back();
                        return NewString(FlushBuffer(), Symbols.SingleQuote, true);
                    case Symbols.ReverseSolidus:
                        current = GetNext();
                        if (current.IsLineBreak())
                        {
                            StringBuffer.AppendLine();
                        }
                        else if (current != Symbols.EndOfFile)
                        {
                            StringBuffer.Append(ConsumeEscape(current));
                        }
                        else
                        {
                            RaiseErrorOccurred(ParseError.EOF);
                            Back();
                            return NewString(FlushBuffer(), Symbols.SingleQuote, true);
                        }

                        break;
                    default:
                        StringBuffer.Append(current);
                        break;
                }
            }
        }

        private Token ColorLiteral()
        {
            var current = GetNext();
            while (current.IsHex())
            {
                StringBuffer.Append(current);
                current = GetNext();
            }

            Back();
            return NewColor(FlushBuffer());
        }

        private Token HashStart()
        {
            var current = GetNext();
            if (current.IsNameStart())
            {
                StringBuffer.Append(current);
                return HashRest();
            }

            if (IsValidEscape(current))
            {
                current = GetNext();
                StringBuffer.Append(ConsumeEscape(current));
                return HashRest();
            }

            if (current == Symbols.ReverseSolidus)
            {
                RaiseErrorOccurred(ParseError.InvalidCharacter);
                Back();
                return NewDelimiter(Symbols.Num);
            }

            Back();
            return NewDelimiter(Symbols.Num);
        }

        private Token HashRest()
        {
            while (true)
            {
                var current = GetNext();
                if (current.IsName())
                {
                    StringBuffer.Append(current);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    StringBuffer.Append(ConsumeEscape(current));
                }
                else if (current == Symbols.ReverseSolidus)
                {
                    RaiseErrorOccurred(ParseError.InvalidCharacter);
                    Back();
                    return NewHash(FlushBuffer());
                }
                else
                {
                    Back();
                    return NewHash(FlushBuffer());
                }
            }
        }

        private Token Comment()
        {
            var current = GetNext();
            while (current != Symbols.EndOfFile)
                if (current == Symbols.Asterisk)
                {
                    current = GetNext();
                    if (current == Symbols.Solidus) return NewComment(FlushBuffer());
                    StringBuffer.Append(Symbols.Asterisk);
                }
                else
                {
                    StringBuffer.Append(current);
                    current = GetNext();
                }

            RaiseErrorOccurred(ParseError.EOF);
            return NewComment(FlushBuffer(), true);
        }

        private Token AtKeywordStart()
        {
            var current = GetNext();
            if (current == Symbols.Minus)
            {
                current = GetNext();
                if (current.IsNameStart() || IsValidEscape(current))
                {
                    StringBuffer.Append(Symbols.Minus);
                    return AtKeywordRest(current);
                }

                Back(2);
                return NewDelimiter(Symbols.At);
            }

            if (current.IsNameStart())
            {
                StringBuffer.Append(current);
                return AtKeywordRest(GetNext());
            }

            if (IsValidEscape(current))
            {
                current = GetNext();
                StringBuffer.Append(ConsumeEscape(current));
                return AtKeywordRest(GetNext());
            }

            Back();
            return NewDelimiter(Symbols.At);
        }

        private Token AtKeywordRest(char current)
        {
            while (true)
            {
                if (current.IsName())
                {
                    StringBuffer.Append(current);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    StringBuffer.Append(ConsumeEscape(current));
                }
                else
                {
                    Back();
                    return NewAtKeyword(FlushBuffer());
                }

                current = GetNext();
            }
        }

        private Token IdentStart(char current)
        {
            if (current == Symbols.Minus)
            {
                current = GetNext();
                if (current.IsNameStart() || current == Symbols.Minus || IsValidEscape(current))
                {
                    StringBuffer.Append(Symbols.Minus);
                    return IdentRest(current);
                }

                Back();
                return NewDelimiter(Symbols.Minus);
            }

            if (current.IsNameStart())
            {
                StringBuffer.Append(current);
                return IdentRest(GetNext());
            }

            if (current == Symbols.ReverseSolidus && IsValidEscape(current))
            {
                current = GetNext();
                StringBuffer.Append(ConsumeEscape(current));
                return IdentRest(GetNext());
            }

            return Data(current);
        }

        private Token IdentRest(char current)
        {
            while (true)
            {
                if (current.IsName())
                {
                    StringBuffer.Append(current);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    StringBuffer.Append(ConsumeEscape(current));
                }
                else if (current == Symbols.RoundBracketOpen)
                {
                    var name = FlushBuffer();
                    var type = name.GetTypeFromName();
                    return type == TokenType.Function ? NewFunction(name) : UrlStart(name);
                }
                else
                {
                    Back();
                    return NewIdent(FlushBuffer());
                }

                current = GetNext();
            }
        }
        //private Token TransformFunctionWhitespace(char current)
        //{
        //    while (true)
        //    {
        //        current = GetNext();
        //        if (current == Symbols.RoundBracketOpen)
        //        {
        //            Back();
        //            return NewFunction(FlushBuffer());
        //        }
        //        if (!current.IsSpaceCharacter())
        //        {
        //            Back(2);
        //            return NewIdent(FlushBuffer());
        //        }
        //    }
        //}

        private Token NumberStart(char current)
        {
            while (true)
            {
                if (current.IsOneOf(Symbols.Plus, Symbols.Minus))
                {
                    StringBuffer.Append(current);
                    current = GetNext();
                    if (current == Symbols.Dot)
                    {
                        StringBuffer.Append(current);
                        StringBuffer.Append(GetNext());
                        return NumberFraction();
                    }

                    StringBuffer.Append(current);
                    return NumberRest();
                }

                if (current == Symbols.Dot)
                {
                    StringBuffer.Append(current);
                    StringBuffer.Append(GetNext());
                    return NumberFraction();
                }

                if (current.IsDigit())
                {
                    StringBuffer.Append(current);
                    return NumberRest();
                }

                current = GetNext();
            }
        }

        private Token NumberRest()
        {
            var current = GetNext();
            while (true)
            {
                if (current.IsDigit())
                {
                    StringBuffer.Append(current);
                }
                else if (current.IsNameStart())
                {
                    var number = FlushBuffer();
                    StringBuffer.Append(current);
                    return Dimension(number);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    var number = FlushBuffer();
                    StringBuffer.Append(ConsumeEscape(current));
                    return Dimension(number);
                }
                else
                {
                    break;
                }

                current = GetNext();
            }

            switch (current)
            {
                case Symbols.Dot:
                    current = GetNext();
                    if (current.IsDigit())
                    {
                        StringBuffer.Append(Symbols.Dot).Append(current);
                        return NumberFraction();
                    }

                    Back();
                    return NewNumber(FlushBuffer());
                case '%':
                    return NewPercentage(FlushBuffer());
                case 'e':
                case 'E':
                    return NumberExponential(current);
                case Symbols.Minus:
                    return NumberDash();
                default:
                    Back();
                    return NewNumber(FlushBuffer());
            }
        }

        private Token NumberFraction()
        {
            var current = GetNext();
            while (true)
            {
                if (current.IsDigit())
                {
                    StringBuffer.Append(current);
                }
                else if (current.IsNameStart())
                {
                    var number = FlushBuffer();
                    StringBuffer.Append(current);
                    return Dimension(number);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    var number = FlushBuffer();
                    StringBuffer.Append(ConsumeEscape(current));
                    return Dimension(number);
                }
                else
                {
                    break;
                }

                current = GetNext();
            }

            switch (current)
            {
                case 'e':
                case 'E':
                    return NumberExponential(current);
                case '%':
                    return NewPercentage(FlushBuffer());
                case Symbols.Minus:
                    return NumberDash();
                default:
                    Back();
                    return NewNumber(FlushBuffer());
            }
        }

        private Token Dimension(string number)
        {
            while (true)
            {
                var current = GetNext();
                if (current.IsLetter())
                {
                    StringBuffer.Append(current);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    StringBuffer.Append(ConsumeEscape(current));
                }
                else
                {
                    Back();
                    return NewDimension(number, FlushBuffer());
                }
            }
        }

        private Token SciNotation()
        {
            while (true)
            {
                var current = GetNext();
                if (current.IsDigit())
                {
                    StringBuffer.Append(current);
                }
                else
                {
                    Back();
                    return NewNumber(FlushBuffer());
                }
            }
        }

        private Token UrlStart(string functionName)
        {
            var current = SkipSpaces();
            switch (current)
            {
                case Symbols.EndOfFile:
                    RaiseErrorOccurred(ParseError.EOF);
                    return NewUrl(functionName, string.Empty, true);
                case Symbols.DoubleQuote:
                    return UrlDoubleQuote(functionName);
                case Symbols.SingleQuote:
                    return UrlSingleQuote(functionName);
                case Symbols.RoundBracketClose:
                    return NewUrl(functionName, string.Empty);
                default:
                    return UrlUnquoted(current, functionName);
            }
        }

        private Token UrlDoubleQuote(string functionName)
        {
            while (true)
            {
                var current = GetNext();
                if (current.IsLineBreak())
                {
                    RaiseErrorOccurred(ParseError.LineBreakUnexpected);
                    return UrlBad(functionName);
                }

                switch (current)
                {
                    case Symbols.EndOfFile:
                        return NewUrl(functionName, FlushBuffer());
                    case Symbols.DoubleQuote:
                        return UrlEnd(functionName);
                }

                if (current != Symbols.ReverseSolidus)
                {
                    StringBuffer.Append(current);
                }
                else
                {
                    current = GetNext();
                    if (current == Symbols.EndOfFile)
                    {
                        Back(2);
                        RaiseErrorOccurred(ParseError.EOF);
                        return NewUrl(functionName, FlushBuffer(), true);
                    }

                    if (current.IsLineBreak())
                        StringBuffer.AppendLine();
                    else
                        StringBuffer.Append(ConsumeEscape(current));
                }
            }
        }

        private Token UrlSingleQuote(string functionName)
        {
            while (true)
            {
                var current = GetNext();
                if (current.IsLineBreak())
                {
                    RaiseErrorOccurred(ParseError.LineBreakUnexpected);
                    return UrlBad(functionName);
                }

                switch (current)
                {
                    case Symbols.EndOfFile:
                        return NewUrl(functionName, FlushBuffer());
                    case Symbols.SingleQuote:
                        return UrlEnd(functionName);
                }

                if (current != Symbols.ReverseSolidus)
                {
                    StringBuffer.Append(current);
                }
                else
                {
                    current = GetNext();
                    if (current == Symbols.EndOfFile)
                    {
                        Back(2);
                        RaiseErrorOccurred(ParseError.EOF);
                        return NewUrl(functionName, FlushBuffer(), true);
                    }

                    if (current.IsLineBreak())
                        StringBuffer.AppendLine();
                    else
                        StringBuffer.Append(ConsumeEscape(current));
                }
            }
        }

        private Token UrlUnquoted(char current, string functionName)
        {
            while (true)
            {
                if (current.IsSpaceCharacter()) return UrlEnd(functionName);
                if (current.IsOneOf(Symbols.RoundBracketClose, Symbols.EndOfFile))
                    return NewUrl(functionName, FlushBuffer());
                if (current.IsOneOf(Symbols.DoubleQuote, Symbols.SingleQuote, Symbols.RoundBracketOpen) ||
                    current.IsNonPrintable())
                {
                    RaiseErrorOccurred(ParseError.InvalidCharacter);
                    return UrlBad(functionName);
                }

                if (current != Symbols.ReverseSolidus)
                {
                    StringBuffer.Append(current);
                }
                else if (IsValidEscape(current))
                {
                    current = GetNext();
                    StringBuffer.Append(ConsumeEscape(current));
                }
                else
                {
                    RaiseErrorOccurred(ParseError.InvalidCharacter);
                    return UrlBad(functionName);
                }

                current = GetNext();
            }
        }

        private Token UrlEnd(string functionName)
        {
            while (true)
            {
                var current = GetNext();
                if (current == Symbols.RoundBracketClose) return NewUrl(functionName, FlushBuffer());

                if (current.IsSpaceCharacter()) continue;

                RaiseErrorOccurred(ParseError.InvalidCharacter);
                Back();
                return UrlBad(functionName);
            }
        }

        private Token UrlBad(string functionName)
        {
            var current = Current;
            var curly = 0;
            var round = 1;
            while (current != Symbols.EndOfFile)
            {
                if (current == Symbols.Semicolon)
                {
                    Back();
                    return NewUrl(functionName, FlushBuffer(), true);
                }

                if (current == Symbols.CurlyBracketClose && --curly == -1)
                {
                    Back();
                    return NewUrl(functionName, FlushBuffer(), true);
                }

                if (current == Symbols.RoundBracketClose && --round == 0)
                    return NewUrl(functionName, FlushBuffer(), true);
                if (IsValidEscape(current))
                {
                    current = GetNext();
                    StringBuffer.Append(ConsumeEscape(current));
                }
                else
                {
                    if (current == Symbols.RoundBracketOpen)
                        ++round;
                    else if (curly == Symbols.CurlyBracketOpen) ++curly;
                    StringBuffer.Append(current);
                }

                current = GetNext();
            }

            RaiseErrorOccurred(ParseError.EOF);
            return NewUrl(functionName, FlushBuffer(), true);
        }

        private Token UnicodeRange(char current)
        {
            for (var i = 0; i < 6 && current.IsHex(); i++)
            {
                StringBuffer.Append(current);
                current = GetNext();
            }

            if (StringBuffer.Length != 6)
            {
                for (var i = 0; i < 6 - StringBuffer.Length; i++)
                {
                    if (current != Symbols.QuestionMark)
                    {
                        // ReSharper disable once RedundantAssignment
                        current = GetPrevious();
                        break;
                    }

                    StringBuffer.Append(current);
                    current = GetNext();
                }

                return NewRange(FlushBuffer());
            }

            if (current == Symbols.Minus)
            {
                current = GetNext();
                if (current.IsHex())
                {
                    var start = FlushBuffer();
                    for (var i = 0; i < 6; i++)
                    {
                        if (!current.IsHex())
                        {
                            // ReSharper disable once RedundantAssignment
                            current = GetPrevious();
                            break;
                        }

                        StringBuffer.Append(current);
                        current = GetNext();
                    }

                    var end = FlushBuffer();
                    return NewRange(start, end);
                }

                Back(2);
                return NewRange(FlushBuffer());
            }

            Back();
            return NewRange(FlushBuffer());
        }

        private Token NewMatch(string match)
        {
            return new Token(TokenType.Match, match, _position);
        }

        private Token NewColumn()
        {
            return new Token(TokenType.Column, Combinators.Column, _position);
        }

        private Token NewCloseCurly()
        {
            return new Token(TokenType.CurlyBracketClose, "}", _position);
        }

        private Token NewOpenCurly()
        {
            return new Token(TokenType.CurlyBracketOpen, "{", _position);
        }

        private Token NewCloseSquare()
        {
            return new Token(TokenType.SquareBracketClose, "]", _position);
        }

        private Token NewOpenSquare()
        {
            return new Token(TokenType.SquareBracketOpen, "[", _position);
        }

        private Token NewOpenComment()
        {
            return new Token(TokenType.Cdo, "<!--", _position);
        }

        private Token NewSemicolon()
        {
            return new Token(TokenType.Semicolon, ";", _position);
        }

        private Token NewColon()
        {
            return new Token(TokenType.Colon, ":", _position);
        }

        private Token NewCloseComment()
        {
            return new Token(TokenType.Cdc, "-->", _position);
        }

        private Token NewComma()
        {
            return new Token(TokenType.Comma, ",", _position);
        }

        private Token NewCloseRound()
        {
            return new Token(TokenType.RoundBracketClose, ")", _position);
        }

        private Token NewOpenRound()
        {
            return new Token(TokenType.RoundBracketOpen, "(", _position);
        }

        private Token NewString(string value, char quote, bool bad = false)
        {
            return new StringToken(value, bad, quote, _position);
        }

        private Token NewHash(string value)
        {
            return new KeywordToken(TokenType.Hash, value, _position);
        }

        private Token NewComment(string value, bool bad = false)
        {
            return new CommentToken(value, bad, _position);
        }

        private Token NewAtKeyword(string value)
        {
            return new KeywordToken(TokenType.AtKeyword, value, _position);
        }

        private Token NewIdent(string value)
        {
            return new KeywordToken(TokenType.Ident, value, _position);
        }

        private Token NewFunction(string value)
        {
            var function = new FunctionToken(value, _position);

            // Tracks paren depth so a bare (non-function) parenthesized group nested inside the
            // function's arguments - e.g. calc((1px + 2px) * 3) - doesn't get mistaken for the end of
            // the function: only the closing paren that matches this function's own opening paren (depth
            // returning to 0) should terminate it. A nested function call's own parens are already fully
            // consumed by its own (recursive) call to this method before it's added as a single token
            // here, so they never surface as bare RoundBracketOpen/Close tokens at this level.
            var depth = 1;
            var token = Get();
            while (token.Type != TokenType.EndOfFile)
            {
                function.AddArgumentToken(token);

                if (token.Type == TokenType.RoundBracketOpen)
                {
                    depth++;
                }
                else if (token.Type == TokenType.RoundBracketClose)
                {
                    depth--;
                    if (depth == 0) break;
                }

                token = Get();
            }

            return function;
        }

        private Token NewPercentage(string value)
        {
            return new UnitToken(TokenType.Percentage, value, "%", _position);
        }

        private Token NewDimension(string value, string unit)
        {
            return new UnitToken(TokenType.Dimension, value, unit, _position);
        }

        private Token NewUrl(string functionName, string data, bool bad = false)
        {
            return new UrlToken(functionName, data, bad, _position);
        }

        private Token NewRange(string range)
        {
            return new RangeToken(range, _position);
        }

        private Token NewRange(string start, string end)
        {
            return new RangeToken(start, end, _position);
        }

        private Token NewWhitespace(char character)
        {
            return new Token(TokenType.Whitespace, character.ToString(), _position);
        }

        private Token NewNumber(string number)
        {
            return new NumberToken(number, _position);
        }

        private Token NewDelimiter(char c)
        {
            return new Token(TokenType.Delim, c.ToString(), _position);
        }

        private Token NewColor(string text)
        {
            return new ColorToken(text, _position);
        }

        private Token NewEof()
        {
            return new Token(TokenType.EndOfFile, string.Empty, _position);
        }

        private Token NewGreaterThan() => new Token(TokenType.GreaterThan, ">", _position);
        private Token NewGreaterThanOrEqual() => new Token(TokenType.GreaterThanOrEqual, ">=", _position);
        private Token NewLessThan() => new Token(TokenType.LessThan, "<", _position);
        private Token NewLessThanOrEqual() => new Token(TokenType.LessThanOrEqual, "<=", _position);
        private Token NewEqual() => new Token(TokenType.Equal, "=", _position);


        private Token NumberExponential(char letter)
        {
            var current = GetNext();
            if (current.IsDigit())
            {
                StringBuffer.Append(letter).Append(current);
                return SciNotation();
            }

            if (current == Symbols.Plus || current == Symbols.Minus)
            {
                var op = current;
                current = GetNext();
                if (current.IsDigit())
                {
                    StringBuffer.Append(letter).Append(op).Append(current);
                    return SciNotation();
                }

                Back();
            }

            var number = FlushBuffer();
            StringBuffer.Append(letter);
            Back();
            return Dimension(number);
        }

        private Token NumberDash()
        {
            var current = GetNext();
            if (current.IsNameStart())
            {
                var number = FlushBuffer();
                StringBuffer.Append(Symbols.Minus).Append(current);
                return Dimension(number);
            }

            if (IsValidEscape(current))
            {
                current = GetNext();
                var number = FlushBuffer();
                StringBuffer.Append(Symbols.Minus).Append(ConsumeEscape(current));
                return Dimension(number);
            }

            Back(2);
            return NewNumber(FlushBuffer());
        }

        private string ConsumeEscape(char current)
        {
            if (!current.IsHex()) return current.ToString();

            var isHex = true;
            var escape = new char[6];
            var length = 0;
            while (isHex && length < escape.Length)
            {
                escape[length++] = current;
                current = GetNext();
                isHex = current.IsHex();
            }

            if (!current.IsSpaceCharacter()) Back();
            var code = int.Parse(new string(escape, 0, length), NumberStyles.HexNumber);
            if (!code.IsInvalid()) return code.ConvertFromUtf32();
            current = Symbols.Replacement;
            return current.ToString();
        }

        private bool IsValidEscape(char current)
        {
            if (current != Symbols.ReverseSolidus) return false;
            current = GetNext();
            Back();
            return current != Symbols.EndOfFile && !current.IsLineBreak();
        }

        private void RaiseErrorOccurred(ParseError code)
        {
            RaiseErrorOccurred(code, GetCurrentPosition());
        }
    }
}