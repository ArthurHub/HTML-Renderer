using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StylesheetComposer
    {
        private readonly Lexer _lexer;
        private readonly StylesheetParser _parser;
        private readonly Stack<StylesheetNode> _nodes;

        // The source index (raw, into _lexer.Source) immediately before the most recently read token.
        // Captured by NextToken so a CSS-Nesting classification look-ahead can rewind to a construct's
        // exact start without any position arithmetic (which is unreliable across \r\n normalization and
        // unicode escapes) and can slice the nested prelude's source text.
        private int _markBeforeLastToken;

        public StylesheetComposer(Lexer lexer, StylesheetParser parser)
        {
            _lexer = lexer;
            _parser = parser;
            _nodes = new Stack<StylesheetNode>();
        }

        public Rule CreateAtRule(Token token)
        {
            if (token.Data.Is(RuleNames.Media)) return CreateMedia(token);

            if (token.Data.Is(RuleNames.FontFace)) return CreateFontFace(token);

            if (token.Data.Is(RuleNames.Keyframes) ||
                (_parser.Options.EnableVendorPrefixAliases && token.Data.Isi("-webkit-keyframes")))
            {
                return CreateKeyframes(token);
            }

            if (token.Data.Is(RuleNames.Import)) return CreateImport(token);

            if (token.Data.Is(RuleNames.Charset)) return CreateCharset(token);

            if (token.Data.Is(RuleNames.Namespace)) return CreateNamespace(token);

            if (token.Data.Is(RuleNames.Page)) return CreatePage(token);

            if (token.Data.Is(RuleNames.Supports)) return CreateSupports(token);

            if (token.Data.Is(RuleNames.ViewPort)) return CreateViewport(token);

            if (token.Data.Is(RuleNames.Container)) return CreateContainer(token);

            if (token.Data.Is(RuleNames.Property)) return CreateProperty(token);

            if (token.Data.Is(RuleNames.Layer)) return CreateLayer(token);

            if (token.Data.Is(RuleNames.StartingStyle)) return CreateStartingStyle(token);

            if (token.Data.Is(RuleNames.FontPaletteValues)) return CreateFontPaletteValues(token);

            if (token.Data.Is(RuleNames.CounterStyle)) return CreateCounterStyle(token);
            if (token.Data.Is(RuleNames.FontFeatureValues)) return CreateFontFeatureValues(token);

            return token.Data.Is(RuleNames.Document) ? CreateDocument(token) : CreateUnknown(token);
        }

        public Rule CreateRule(Token token)
        {
            switch (token.Type)
            {
                case TokenType.AtKeyword:
                    return CreateAtRule(token);

                case TokenType.CurlyBracketOpen:
                    RaiseErrorOccurred(ParseError.InvalidBlockStart, token.Position);
                    MoveToRuleEnd(ref token);
                    return null;

                case TokenType.String:
                case TokenType.Url:
                case TokenType.CurlyBracketClose:
                case TokenType.RoundBracketClose:
                case TokenType.SquareBracketClose:
                    RaiseErrorOccurred(ParseError.InvalidToken, token.Position);
                    MoveToRuleEnd(ref token);
                    return null;

                default:
                    return CreateStyle(token);
            }
        }

        public Rule CreateCharset(Token current)
        {
            var rule = new CharsetRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);

            if (token.Type == TokenType.String) rule.CharacterSet = token.Data;

            JumpToEnd(ref token);
            rule.StylesheetText = CreateView(start, token.Position);
            _nodes.Pop();
            return rule;
        }

        public Rule CreateDocument(Token current)
        {
            var rule = new DocumentRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            FillFunctions(function => rule.AppendChild(function), ref token);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillRules(rule);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateViewport(Token current)
        {
            var rule = new ViewportRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillDeclarations(rule, PropertyFactory.Instance.CreateViewport);

                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateFontFace(Token current)
        {
            var rule = new FontFaceRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillDeclarations(rule, PropertyFactory.Instance.CreateFont);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateProperty(Token current)
        {
            var rule = new PropertyRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            rule.Name = GetRuleName(ref token);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillDeclarations(rule, PropertyFactory.Instance.CreatePropertyDescriptor);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateStartingStyle(Token current)
        {
            var start = current.Position;
            var token = NextToken();
            ParseComments(ref token);

            // `@starting-style` takes no prelude; anything before the block is invalid and skipped.
            if (token.Type != TokenType.CurlyBracketOpen)
            {
                RaiseErrorOccurred(ParseError.InvalidToken, token.Position);
                return SkipDeclarations(token);
            }

            var rule = new StartingStyleRule(_parser);
            _nodes.Push(rule);
            var end = FillRules(rule);
            rule.StylesheetText = CreateView(start, end);
            _nodes.Pop();
            return rule;
        }

        public Rule CreateLayer(Token current)
        {
            var start = current.Position;
            var token = NextToken();
            ParseComments(ref token);

            // Read the (optional) comma-separated layer name list, stopping at '{' (block form) or
            // ';'/EOF (statement form).
            var names = ReadLayerNames(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                // Block form: `@layer name { rules }` (at most one name; anonymous if none).
                var rule = new LayerRule(_parser)
                {
                    Name = names.Count > 0 ? names[0] : string.Empty
                };
                _nodes.Push(rule);
                var end = FillRules(rule);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            // Statement form: `@layer a, b, c;` — declares layer order only, no rules. token is at ';'/EOF.
            var statement = new LayerStatementRule(_parser);
            statement.Names.AddRange(names);
            statement.StylesheetText = CreateView(start, token.Position);
            return statement;
        }

        /// <summary>
        /// Reads a <c>@layer</c> prelude: zero or more comma-separated layer names, each a dotted
        /// ident sequence (e.g. <c>framework.utilities</c>). Advances <paramref name="token"/> up to
        /// (but not past) the terminating <c>{</c>, <c>;</c>, or end of file.
        /// </summary>
        private List<string> ReadLayerNames(ref Token token)
        {
            var names = new List<string>();
            var current = new System.Text.StringBuilder();

            void Flush()
            {
                if (current.Length <= 0) return;
                names.Add(current.ToString());
                current.Clear();
            }

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketOpen, TokenType.Semicolon))
            {
                switch (token.Type)
                {
                    case TokenType.Ident:
                        current.Append(token.Data);
                        break;
                    case TokenType.Delim when token.Data == ".":
                        current.Append('.');
                        break;
                    case TokenType.Comma:
                        Flush();
                        break;
                    // Whitespace/comments and anything else separating names are ignored.
                }

                token = NextToken();
                ParseComments(ref token);
            }

            Flush();
            return names;
        }

        public Rule CreateFontPaletteValues(Token current)
        {
            var rule = new FontPaletteValuesRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            rule.Name = GetRuleName(ref token);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillDeclarations(rule, PropertyFactory.Instance.CreateFontPaletteDescriptor);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateCounterStyle(Token current)
        {
            var rule = new CounterStyleRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            rule.Name = GetRuleName(ref token);
            ParseComments(ref token);

            // The name is a <custom-ident> excluding "none" and the CSS-wide keywords; an invalid or
            // missing name invalidates the whole rule.
            if (token.Type == TokenType.CurlyBracketOpen && IsValidCounterStyleName(rule.Name))
            {
                var end = FillDeclarations(rule, PropertyFactory.Instance.CreateCounterStyleDescriptor);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        private static bool IsValidCounterStyleName(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                   !name.Isi("none") && !name.Isi("inherit") && !name.Isi("initial") && !name.Isi("unset") &&
                   !name.Isi("revert") && !name.Isi("revert-layer") && !name.Isi("default");
        }

        public Rule CreateFontFeatureValues(Token current)
        {
            var rule = new FontFeatureValuesRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            FillFontFamilyList(rule.FamilyList, ref token);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillFontFeatureValueSets(rule);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        // Reads the <family-name># prelude up to the block start. Unquoted names are joined by single spaces;
        // quoted names keep their quotes so the list round-trips.
        private void FillFontFamilyList(List<string> families, ref Token token)
        {
            var current = new StringBuilder();

            void Flush()
            {
                if (current.Length > 0) families.Add(current.ToString());
                current.Clear();
            }

            while (token.IsNot(TokenType.CurlyBracketOpen, TokenType.Semicolon, TokenType.EndOfFile))
            {
                switch (token.Type)
                {
                    case TokenType.Ident:
                        if (current.Length > 0) current.Append(' ');
                        current.Append(token.Data);
                        break;
                    case TokenType.String:
                        if (current.Length > 0) current.Append(' ');
                        current.Append('"').Append(token.Data).Append('"');
                        break;
                    case TokenType.Comma:
                        Flush();
                        break;
                    // Whitespace and comments between names are ignored.
                }

                token = NextToken();
            }

            Flush();
        }

        // Consumes the nested @styleset/@character-variant/@swash/@ornaments/@annotation/@stylistic blocks.
        // Anything else (unknown at-rules, bare declarations) is skipped; there is no unknown-descriptor
        // fallback in CSS Fonts 4.
        private TextPosition FillFontFeatureValueSets(FontFeatureValuesRule parent)
        {
            var token = NextToken();
            ParseComments(ref token);

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
            {
                if (token.Type == TokenType.AtKeyword && FontFeatureValueSetRule.IsSetName(token.Data))
                {
                    var rule = CreateFontFeatureValueSet(ref token);
                    if (rule != null) parent.Rules.Add(rule);
                }
                else
                {
                    SkipFontFeatureValueStrayContent(ref token);
                }

                ParseComments(ref token);
            }

            return token.Position;
        }

        // Skips one run of stray content, tracking its own brace depth from zero. MoveToRuleEnd cannot be
        // used: it would either swallow the enclosing rule's closing '}' (when the stray content has no
        // terminator of its own) or stop early on a nested '}'. A '}' at depth zero is the enclosing rule's
        // close and is left for the caller.
        private void SkipFontFeatureValueStrayContent(ref Token token)
        {
            var depth = 0;

            while (token.Type != TokenType.EndOfFile)
            {
                if (token.Type == TokenType.CurlyBracketOpen)
                {
                    depth++;
                }
                else if (token.Type == TokenType.CurlyBracketClose)
                {
                    if (depth == 0) return;

                    if (--depth == 0)
                    {
                        token = NextToken();
                        return;
                    }
                }
                else if (token.Type == TokenType.Semicolon && depth == 0)
                {
                    token = NextToken();
                    return;
                }

                token = NextToken();
            }
        }

        // Parses one nested block. On entry token is the at-keyword; on return it is positioned just past
        // the block's closing '}' (or at the offending token when the block has no '{').
        private Rule CreateFontFeatureValueSet(ref Token current)
        {
            var name = current.Data.ToLowerInvariant();
            var rule = new FontFeatureValueSetRule(_parser, name);
            var start = current.Position;
            _nodes.Push(rule);
            var token = NextToken();
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillDeclarations(rule, PropertyFactory.Instance.CreateFontFeatureValueDescriptor);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                current = NextToken();
                ParseComments(ref current);
                return rule;
            }

            _nodes.Pop();
            current = token;
            return null;
        }

        public Rule CreateImport(Token current)
        {
            var rule = new ImportRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);

            if (token.Is(TokenType.String, TokenType.Url))
            {
                rule.Href = token.Data;
                token = NextToken();
                ParseComments(ref token);
                FillMediaList(rule.Media, TokenType.Semicolon, ref token);
            }

            ParseComments(ref token);
            JumpToEnd(ref token);
            rule.StylesheetText = CreateView(start, token.Position);
            _nodes.Pop();
            return rule;
        }

        public Rule CreateKeyframes(Token current)
        {
            var rule = new KeyframesRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);

            // <keyframes-name> = <custom-ident> | <string>
            if (token.Type == TokenType.String)
            {
                rule.Name = token.Data;
                rule.NameIsString = true;
                token = NextToken();
            }
            else if (token.Type == TokenType.Ident && !IsReservedKeyframesName(token.Data))
            {
                rule.Name = GetRuleName(ref token);
            }
            else
            {
                // Missing name, or an ident that is not a valid <custom-ident> (none, CSS-wide keywords).
                _nodes.Pop();
                return SkipDeclarations(token);
            }

            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillKeyframeRules(rule);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        private static bool IsReservedKeyframesName(string name)
        {
            return name.Isi("none") || name.Isi("initial") || name.Isi("inherit") || name.Isi("unset") ||
                   name.Isi("default") || name.Isi("revert") || name.Isi("revert-layer");
        }

        public Rule CreateMedia(Token current)
        {
            var rule = new MediaRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            FillMediaList(rule.Media, TokenType.CurlyBracketOpen, ref token);
            ParseComments(ref token);

            if (token.Type != TokenType.CurlyBracketOpen)
                while (token.Type != TokenType.EndOfFile)
                {
                    if (token.Type == TokenType.Semicolon)
                    {
                        _nodes.Pop();
                        return null;
                    }

                    if (token.Type == TokenType.CurlyBracketOpen) break;

                    token = NextToken();
                }

            var end = FillRules(rule);
            rule.StylesheetText = CreateView(start, end);
            _nodes.Pop();
            return rule;
        }

        public Rule CreateContainer(Token current)
        {
            var rule = new ContainerRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            rule.Name = GetContainerName(ref token);
            ParseComments(ref token);
            // A style() query (CSS Containment 3), alone or combined with size features by and/or/not, is parsed
            // into rule.Condition; a prelude without one keeps the plain size-feature MediaList grammar.
            if (!TryParseStyleContainerCondition(rule, ref token))
            {
                FillMediaList(rule.Media, TokenType.CurlyBracketOpen, ref token);
                ParseComments(ref token);
            }

            if (token.Type != TokenType.CurlyBracketOpen)
                while (token.Type != TokenType.EndOfFile)
                {
                    if (token.Type == TokenType.Semicolon)
                    {
                        _nodes.Pop();
                        return null;
                    }

                    if (token.Type == TokenType.CurlyBracketOpen) break;

                    token = NextToken();
                }

            var end = FillRules(rule);
            rule.StylesheetText = CreateView(start, end);
            _nodes.Pop();
            return rule;

            //ParseComments(ref token);
            //rule.Condition = AggregateCondition(ref token);
            //ParseComments(ref token);

            //if (token.Type != TokenType.CurlyBracketOpen)
            //    while (token.Type != TokenType.EndOfFile)
            //    {
            //        if (token.Type == TokenType.Semicolon)
            //        {
            //            _nodes.Pop();
            //            return null;
            //        }

            //        if (token.Type == TokenType.CurlyBracketOpen) break;

            //        token = NextToken();
            //    }

            //var end = FillRules(rule);
            //rule.StylesheetText = CreateView(start, end);
            //_nodes.Pop();
            //return rule;
        }
        // The lexer tokenizes style(...) into one Function token carrying its whole argument list, so the
        // container-condition grammar is parsed from a token list with a local index rather than the live stream.
        private bool TryParseStyleContainerCondition(ContainerRule rule, ref Token token)
        {
            var rewindMark = _markBeforeLastToken;
            var tokens = new List<Token>();
            var scan = token;

            while (scan.Type is not (TokenType.EndOfFile or TokenType.CurlyBracketOpen or TokenType.Semicolon))
            {
                tokens.Add(scan);
                scan = _lexer.Get();
            }

            var hasStyle = tokens.Any(t => t.Type == TokenType.Function && t.Data.Isi(FunctionNames.Style));

            if (!hasStyle || scan.Type != TokenType.CurlyBracketOpen)
            {
                _lexer.RewindTo(rewindMark);
                token = NextToken();
                return false;
            }

            token = scan;
            var condition = ParseContainerCondition(tokens);

            if (condition != null)
                rule.Condition = condition;
            else
                // An invalid condition matches nothing (CSS Containment 3), so keep an always-false size query.
                rule.Media.MediaText = "not all";

            return true;
        }

        private static void SkipConditionWhitespace(List<Token> tokens, ref int index)
        {
            while (index < tokens.Count && tokens[index].Type is TokenType.Whitespace or TokenType.Comment)
                index++;
        }

        // <container-condition> = not <query-in-parens> | <query-in-parens> [[and <query-in-parens>]* | [or <query-in-parens>]*]
        // Returns null unless the whole token list is consumed.
        private static IContainerCondition ParseContainerCondition(List<Token> tokens)
        {
            return ParseCombinedCondition(tokens, ParseContainerTerm);
        }

        // <style-query> = not <style-in-parens> | <style-in-parens> [[and ...]* | [or ...]*] | <style-feature>
        private static IContainerCondition ParseStyleQuery(List<Token> tokens)
        {
            var first = 0;
            SkipConditionWhitespace(tokens, ref first);
            if (first < tokens.Count && tokens[first].Type == TokenType.Ident &&
                !tokens[first].Data.Isi(Keywords.Not) && !tokens[first].Data.Isi(Keywords.And) && !tokens[first].Data.Isi(Keywords.Or))
                return ParseStyleFeature(tokens, first);

            return ParseCombinedCondition(tokens, ParseStyleTerm);
        }

        private delegate IContainerCondition TermParser(List<Token> tokens, ref int index);

        private static IContainerCondition ParseCombinedCondition(List<Token> tokens, TermParser parseTerm)
        {
            var index = 0;
            SkipConditionWhitespace(tokens, ref index);
            if (index >= tokens.Count) return null;

            if (tokens[index].Type == TokenType.Ident && tokens[index].Data.Isi(Keywords.Not))
            {
                index++;
                var operand = parseTerm(tokens, ref index);
                SkipConditionWhitespace(tokens, ref index);
                return operand == null || index < tokens.Count ? null : ContainerCondition.Not(operand);
            }

            var term = parseTerm(tokens, ref index);
            if (term == null) return null;

            SkipConditionWhitespace(tokens, ref index);
            if (index >= tokens.Count) return term;

            var connector = tokens[index].Type == TokenType.Ident ? tokens[index].Data : null;
            if (connector == null || !(connector.Isi(Keywords.And) || connector.Isi(Keywords.Or))) return null;

            var terms = new List<IContainerCondition> { term };

            while (index < tokens.Count)
            {
                // Only one kind of connector may appear unparenthesised in a chain.
                if (tokens[index].Type != TokenType.Ident || !tokens[index].Data.Isi(connector)) return null;

                index++;
                var next = parseTerm(tokens, ref index);
                if (next == null) return null;

                terms.Add(next);
                SkipConditionWhitespace(tokens, ref index);
            }

            return connector.Isi(Keywords.And) ? ContainerCondition.And(terms) : ContainerCondition.Or(terms);
        }

        // Finds the token range inside the round bracket at tokens[open]; returns the index of its matching close or -1.
        private static int FindClosingBracket(List<Token> tokens, int open)
        {
            var depth = 0;

            for (var i = open; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.RoundBracketOpen) depth++;
                else if (tokens[i].Type == TokenType.RoundBracketClose && --depth == 0) return i;
            }

            return -1;
        }

        private static IContainerCondition ParseContainerTerm(List<Token> tokens, ref int index)
        {
            SkipConditionWhitespace(tokens, ref index);
            if (index >= tokens.Count) return null;
            var token = tokens[index];

            if (token.Type == TokenType.Function && token.Data.Isi(FunctionNames.Style))
            {
                index++;
                var query = ParseStyleQuery(new List<Token>(((FunctionToken) token).ArgumentTokens));
                return query == null ? null : ContainerCondition.Style(query);
            }

            if (token.Type != TokenType.RoundBracketOpen) return null;

            var close = FindClosingBracket(tokens, index);
            if (close < 0) return null;

            var inner = tokens.GetRange(index + 1, close - index - 1);
            var group = tokens.GetRange(index, close - index + 1);
            index = close + 1;

            var first = 0;
            SkipConditionWhitespace(inner, ref first);
            var nested = first < inner.Count && (inner[first].Type == TokenType.RoundBracketOpen ||
                (inner[first].Type == TokenType.Function && inner[first].Data.Isi(FunctionNames.Style)) ||
                (inner[first].Type == TokenType.Ident && inner[first].Data.Isi(Keywords.Not)));

            // A parenthesised group is either a nested condition or a size feature, whose text is kept as written.
            return nested ? ParseContainerCondition(inner) : ContainerCondition.Size(group.ToText().Trim());
        }

        private static IContainerCondition ParseStyleTerm(List<Token> tokens, ref int index)
        {
            SkipConditionWhitespace(tokens, ref index);
            if (index >= tokens.Count || tokens[index].Type != TokenType.RoundBracketOpen) return null;

            var close = FindClosingBracket(tokens, index);
            if (close < 0) return null;

            var inner = tokens.GetRange(index + 1, close - index - 1);
            index = close + 1;
            return ParseStyleQuery(inner);
        }

        // <style-feature> = <ident> [: <value>]; the value is kept as raw text and not validated against the property.
        private static IContainerCondition ParseStyleFeature(List<Token> tokens, int index)
        {
            var property = tokens[index].Data;
            index++;
            SkipConditionWhitespace(tokens, ref index);

            if (index >= tokens.Count) return ContainerCondition.Declaration(property, null);
            if (tokens[index].Type != TokenType.Colon) return null;

            var value = tokens.GetRange(index + 1, tokens.Count - index - 1).ToText().Trim();
            return value.Length == 0 ? null : ContainerCondition.Declaration(property, value);
        }

        public Rule CreateNamespace(Token current)
        {
            var rule = new NamespaceRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            rule.Prefix = GetRuleName(ref token);
            ParseComments(ref token);

            if (token.Type == TokenType.Url) rule.NamespaceUri = token.Data;

            JumpToEnd(ref token);
            rule.StylesheetText = CreateView(start, token.Position);
            _nodes.Pop();
            return rule;
        }

        public Rule CreatePage(Token current)
        {
            var rule = new PageRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);

            if (token.Type != TokenType.CurlyBracketOpen)
            {
                // A pseudo-selector exists.  Parse it prior
                // to declarations
                // e.g. @page :left{...}
                rule.Selector = CreatePageSelector(ref token);
                ParseComments(ref token);
            }

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillDeclarations(rule.Style);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateSupports(Token current)
        {
            var rule = new SupportsRule(_parser);
            var start = current.Position;
            var token = NextToken();
            _nodes.Push(rule);
            ParseComments(ref token);
            rule.Condition = AggregateCondition(ref token);
            ParseComments(ref token);

            if (token.Type == TokenType.CurlyBracketOpen)
            {
                var end = FillRules(rule);
                rule.StylesheetText = CreateView(start, end);
                _nodes.Pop();
                return rule;
            }

            _nodes.Pop();
            return SkipDeclarations(token);
        }

        public Rule CreateStyle(Token current)
        {
            var rule = new StyleRule(_parser);
            var start = current.Position;
            _nodes.Push(rule);
            ParseComments(ref current);
            rule.Selector = CreateSelector(ref current, IsTopLevelRule());
            var end = FillDeclarations(rule.Style);
            rule.StylesheetText = CreateView(start, end);
            _nodes.Pop();
            return rule.Selector != null ? rule : null;
        }

        /// <summary>
        /// Whether the rule being created sits directly in the stylesheet, rather than in a group rule's block
        /// (<c>@media</c>...) or in another rule. A top-level qualified rule's prelude runs to the next <c>{</c>
        /// whatever it holds, a stray <c>}</c> included (CSS Syntax 3, "consume a qualified rule"); the closing
        /// brace only ends a prelude inside a block, where it closes the block.
        /// </summary>
        private bool IsTopLevelRule()
        {
            // The rule itself is on top of the stack; what is under it is its parent.
            using (var nodes = _nodes.GetEnumerator())
            {
                if (!nodes.MoveNext()) return true;

                return !nodes.MoveNext() || nodes.Current is Stylesheet;
            }
        }

        public Rule CreateMarginStyle(ref Token current)
        {
            var rule = new MarginStyleRule(_parser);
            var start = current.Position;
            _nodes.Push(rule);
            ParseComments(ref current);
            rule.Selector = CreateMarginSelector(ref current);
            var end = FillDeclarations(rule.Style);
            rule.StylesheetText = CreateView(start, end);
            _nodes.Pop();
            return rule.Selector != null ? rule : null;
        }

        public KeyframeRule CreateKeyframeRule(Token current)
        {
            var rule = new KeyframeRule(_parser);
            var start = current.Position;
            _nodes.Push(rule);
            ParseComments(ref current);
            rule.Key = CreateKeyframeSelector(ref current);
            var end = FillDeclarations(rule.Style);
            rule.StylesheetText = CreateView(start, end);
            _nodes.Pop();
            return rule.Key != null ? rule : null;
        }

        public Rule CreateUnknown(Token current)
        {
            var start = current.Position;

            if (_parser.Options.IncludeUnknownRules)
            {
                var token = NextToken();
                var rule = new UnknownRule(current.Data, _parser);
                _nodes.Push(rule);

                while (token.IsNot(TokenType.CurlyBracketOpen, TokenType.Semicolon, TokenType.EndOfFile))
                    token = NextToken();

                if (token.Type == TokenType.CurlyBracketOpen)
                {
                    var curly = 1;

                    do
                    {
                        token = NextToken();

                        switch (token.Type)
                        {
                            case TokenType.CurlyBracketOpen:
                                curly++;
                                break;
                            case TokenType.CurlyBracketClose:
                                curly--;
                                break;
                            case TokenType.EndOfFile:
                                curly = 0;
                                break;
                        }
                    } while (curly != 0);
                }

                rule.StylesheetText = CreateView(start, token.Position);
                _nodes.Pop();
                return rule;
            }

            RaiseErrorOccurred(ParseError.UnknownAtRule, start);
            MoveToRuleEnd(ref current);
            return default(UnknownRule);
        }

        public TokenValue CreateValue(ref Token token)
        {
            return CreateValue(TokenType.CurlyBracketClose, ref token, out _);
        }

        public List<Medium> CreateMedia(ref Token token)
        {
            var list = new List<Medium>();
            ParseComments(ref token);

            while (token.Type != TokenType.EndOfFile)
            {
                var medium = CreateMedium(ref token);

                if (medium == null || token.IsNot(TokenType.Comma, TokenType.EndOfFile))
                    throw new ParseException("Unable to create medium or end of file reached unexpectedly");

                token = NextToken();
                ParseComments(ref token);
                list.Add(medium);
            }

            return list;
        }

        public TextPosition CreateRules(Stylesheet sheet)
        {
            var token = NextToken();
            _nodes.Push(sheet);
            ParseComments(ref token);

            while (token.Type != TokenType.EndOfFile)
            {
                var rule = CreateRule(token);
                token = NextToken();
                ParseComments(ref token);
                sheet.Rules.Add(rule);
            }

            _nodes.Pop();
            return token.Position;
        }

        public IConditionFunction CreateCondition(ref Token token)
        {
            ParseComments(ref token);
            return AggregateCondition(ref token);
        }

        public KeyframeSelector CreateKeyframeSelector(ref Token token)
        {
            var keys = new List<Percent>();
            var valid = true;
            var start = token.Position;
            ParseComments(ref token);

            while (token.Type != TokenType.EndOfFile)
            {
                if (keys.Count > 0)
                {
                    if (token.Type == TokenType.CurlyBracketOpen) break;
                    if (token.Type != TokenType.Comma)
                        valid = false;
                    else
                        token = NextToken();

                    ParseComments(ref token);
                }

                switch (token.Type)
                {
                    case TokenType.Percentage:
                        keys.Add(new Percent(((UnitToken) token).Value));
                        break;
                    case TokenType.Ident when token.Data.Is(Keywords.From):
                        keys.Add(Percent.Zero);
                        break;
                    case TokenType.Ident when token.Data.Is(Keywords.To):
                        keys.Add(Percent.Hundred);
                        break;
                    default:
                        valid = false;
                        break;
                }

                token = NextToken();
                ParseComments(ref token);
            }

            if (!valid) RaiseErrorOccurred(ParseError.InvalidSelector, start);

            return new KeyframeSelector(keys);
        }

        private PageSelector CreatePageSelector(ref Token token)
        {
            // The CSS Paged Media 3 selector grammar, per comma-separated entry: an optional <ident> page
            // name followed by an optional :<ident> pseudo-class - "@page chapter1:left, chapter2:left"
            // (name+pseudo), "@page chapter" (name only), "@page :first" (pseudo only). Page names are
            // case-sensitive custom-idents; pseudo-class keywords (first/left/right) are matched
            // case-insensitively by the caller.
            var entries = new List<PageSelectorEntry>();

            while (true)
            {
                string name = null;
                string pseudo = null;

                if (token.Type == TokenType.Ident)
                {
                    name = token.Data;
                    token = NextToken();
                }

                if (token.Type == TokenType.Colon)
                {
                    token = NextToken();
                    if (token.Type == TokenType.Ident)
                    {
                        pseudo = token.Data;
                        token = NextToken();
                    }
                }

                if (name != null || pseudo != null)
                    entries.Add(new PageSelectorEntry(name, pseudo));

                ParseComments(ref token);

                if (token.Type != TokenType.Comma) break;

                token = NextToken();
                ParseComments(ref token);
            }

            var selector = new PageSelector(entries);

            //var start = token.Position;

            //while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketOpen, TokenType.CurlyBracketClose))
            //{
            //    var a = 1;
            //    token = NextToken();
            //}

            //var result = selector.ToPool();

            //if (result is StylesheetNode node)
            //{
            //    var end = token.Position.Shift(-1);
            //node.StylesheetText = CreateView(start, end);
            //}

            //if (!selectorIsValid && !_parser.Options.AllowInvalidValues)
            //{
            //    RaiseErrorOccurred(ParseError.InvalidSelector, start);
            //    result = null;
            //}

            //return result;

            return selector;
        }

        public List<DocumentFunction> CreateFunctions(ref Token token)
        {
            var functions = new List<DocumentFunction>();
            ParseComments(ref token);
            FillFunctions(function => functions.Add(function), ref token);
            return functions;
        }

        public TextPosition FillDeclarations(StyleDeclaration style)
        {
            var finalProperties = new Dictionary<string, IProperty>(StringComparer.OrdinalIgnoreCase);
            var token = NextToken();
            _nodes.Push(style);
            ParseComments(ref token);

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
            {
                // @page selectors support declaration blocks in the form of at rules.  This 
                // conditional accounts for the nested at with a page parent
                //
                // @page {
                //   @top-left { ... /* document name */ }
                //   @bottom-center { ... /* page number */}
                // }
                if (token.Is(TokenType.AtKeyword))
                {
                    var parentPageRule = _nodes.FirstOrDefault(parent => parent is PageRule);
                    if (parentPageRule != null)
                    {
                        //var genericAtRule = CreateMarginRule(ref token);
                        //parentPageRule.AppendChild(genericAtRule);
                        // Rewind to capture the margin's @ symbol

                        var marginToken = new Token(TokenType.Ident, token.Data, token.Position);
                        var marginStyle = CreateMarginStyle(ref marginToken);
                        parentPageRule.AppendChild(marginStyle);
                        // CreateMarginStyle's inner FillDeclarations consumed through the margin box's
                        // closing '}' but left marginToken at the box's own '{'. Reusing it here re-enters
                        // the loop on a stale token, so any declaration after the margin box (and any
                        // further margin box) was dropped. Advance to the next real token instead.
                        token = NextToken();
                    }
                    else if (IsNestableGroupRule(token) && NearestStyleRule() is { Selector: { } } enclosingStyleRule)
                    {
                        // CSS Nesting 1 3.2: a conditional group rule inside a style rule. Its body is read as
                        // that rule's declarations and nested rules (see FillRules), under the group's condition.
                        if (CreateAtRule(token) is GroupingRule groupRule) enclosingStyleRule.AddNestedRule(groupRule);
                        token = NextToken();
                    }
                    else
                    {
                        // Advance to the next token or this is an endless loop
                        token = NextToken();
                    }
                }
                else if (TryCreateNestedRule(ref token))
                {
                    // CSS Nesting: a nested style rule was parsed and attached to the enclosing rule.
                }
                else
                {
                    var sourceProperty = CreateDeclarationWith(CreateProperty, ref token);
                    var resolvedProperties = new[] {sourceProperty};

                    if (sourceProperty is {HasValue: true})
                    {
                        // For shorthand properties we need to first find out what alternate set of properties they will
                        // end up resolving into so that we can compare them with their previously parsed counterparts (if any)
                        // and determine which one takes priority over the other.
                        // Example 1: "margin-left: 5px !important; text-align:center; margin: 3px;";
                        // Example 2: "margin: 5px !important; text-align:center; margin-left: 3px;";
                        if (sourceProperty is ShorthandProperty shorthandProperty)
                        {
                            if (shorthandProperty.DeclaredValue.Original.ContainsFunction(FunctionNames.Var))
                            {
                                // A var() reference can't be split into per-longhand slices at parse time -
                                // the referenced custom property's value is only known per-element, at cascade
                                // time. Keep the shorthand declaration whole so substitution and expansion can
                                // happen once it is resolved (CSS Variables 1 3.2).
                                resolvedProperties = new Property[] { shorthandProperty };
                            }
                            else
                            {
                                resolvedProperties = PropertyFactory.Instance.CreateLonghandsFor(shorthandProperty.Name);
                                shorthandProperty.Export(resolvedProperties);
                            }
                        }

                        foreach (var resolvedProperty in resolvedProperties)
                        {
                            // The following relies on the fact that the tokens are processed in 
                            // top-to-bottom order of how they are defined in the parsed style declaration.
                            // This handles exposing the correct value for a property when it appears multiple 
                            // times in the same style declaration.
                            // Example: "background-color:green !important; text-align:center; background-color:yellow;";
                            // In this example even though background-color yellow is defined last, the previous value
                            // of green should be the one exposed given it is tagged as important.
                            // ------------------------------------------------------------------------------------------
                            // Only set this property if one of the following conditions is true:
                            // a) It was not previously added or...
                            // b) The previously added property is not tagged as important or ...
                            // c) The previously added property is tagged as important but so is this new one.
                            var shouldSetProperty =
                                !finalProperties.TryGetValue(resolvedProperty.Name, out var previousProperty)
                                || !previousProperty.IsImportant
                                || resolvedProperty.IsImportant;

                            if (shouldSetProperty)
                            {
                                style.SetProperty(resolvedProperty);
                                finalProperties[resolvedProperty.Name] = resolvedProperty;
                            }
                        }
                    }
                }

                ParseComments(ref token);
            }

            _nodes.Pop();
            return token.Position;
        }

        /// <summary>
        /// CSS Nesting ([CSS Nesting 1](https://www.w3.org/TR/css-nesting-1/)): if the current construct
        /// in a declaration block is a nested style rule (a selector prelude followed by a <c>{ }</c>
        /// block) rather than a declaration, parses it, resolves its selector against the enclosing rule,
        /// attaches it, and returns true (with <paramref name="token"/> advanced past the block). Returns
        /// false for an ordinary declaration, having rewound the lexer so the caller re-reads it.
        /// </summary>
        private bool TryCreateNestedRule(ref Token token)
        {
            // Raw source index just before the current (first) token — the construct start, captured by
            // NextToken. Faithful across \r\n normalization, unlike deriving it from token.Position.
            var preludeStart = _markBeforeLastToken;

            if (!IsNestedRuleAhead(ref token, out var braceStart))
                return false;

            CreateNestedStyleRule(preludeStart, braceStart);
            token = NextToken();
            return true;
        }

        /// <summary>
        /// Looks ahead (bracket-aware) to classify the construct starting at <paramref name="token"/> as a
        /// nested style rule vs a declaration: the first top-level <c>{</c> means a nested rule (the stream
        /// is left just after it and its source index returned via <paramref name="braceStart"/>); a
        /// top-level <c>;</c>/<c>}</c>/EOF means a declaration, in which case the lexer is rewound to the
        /// construct start and <paramref name="token"/> re-read so the unchanged declaration path re-lexes
        /// it in value mode (avoiding the <c>#</c> value-mode tokenization hazard a token buffer would hit).
        /// Custom properties (<c>--x</c>) are always declarations, even with a <c>{</c> in their value.
        /// Function tokens (e.g. <c>url(…)</c>) are opaque — the lexer already consumed their parentheses —
        /// so only bare round/square brackets contribute to depth.
        /// </summary>
        private bool IsNestedRuleAhead(ref Token token, out int braceStart)
        {
            braceStart = 0;

            if (token.Type == TokenType.Ident && token.Data is { } name &&
                name.StartsWith("--", StringComparison.Ordinal))
                return false;

            var rewindMark = _markBeforeLastToken;   // raw source index before `token` (the first token)
            var depth = 0;
            var scan = token;
            var scanStart = rewindMark;              // raw source index before `scan`

            while (scan.Type != TokenType.EndOfFile)
            {
                switch (scan.Type)
                {
                    case TokenType.RoundBracketOpen:
                    case TokenType.SquareBracketOpen:
                        depth++;
                        break;
                    case TokenType.RoundBracketClose:
                    case TokenType.SquareBracketClose:
                        if (depth > 0) depth--;
                        break;
                    case TokenType.CurlyBracketOpen when depth == 0:
                        braceStart = scanStart;
                        token = scan;
                        return true;
                    case TokenType.CurlyBracketClose when depth == 0:
                    case TokenType.Semicolon when depth == 0:
                        _lexer.RewindTo(rewindMark);
                        token = NextToken();
                        return false;
                }

                scanStart = _lexer.InsertionPoint;   // raw index just before the next token
                scan = _lexer.Get();
            }

            _lexer.RewindTo(rewindMark);
            token = NextToken();
            return false;
        }

        /// <summary>
        /// Builds a <see cref="StyleRule"/> from a nested prelude (source span
        /// <c>[preludeStart, braceStart)</c>) resolved against the enclosing rule's selector, consumes
        /// its declaration block from the live stream, and attaches it to the parent rule via
        /// <see cref="StyleRule.AddNestedRule"/>. The resolved selector is absolute (<c>&amp;</c> →
        /// <c>:is(parent)</c>) so the cascade/matcher need no nesting awareness.
        /// </summary>
        private void CreateNestedStyleRule(int preludeStart, int braceStart)
        {
            var parent = _nodes.OfType<StyleRule>().FirstOrDefault();
            var preludeText = _lexer.Source.Text.Substring(preludeStart, braceStart - preludeStart);

            var selector = parent == null
                ? null
                : _parser.ParseSelector(ResolveNestedSelector(preludeText, parent.Selector?.Text));

            // Always consume the block (via a rule pushed on _nodes so nested-within-nested resolves) to
            // keep the parser in sync; only attach it when the selector actually resolved.
            var rule = new StyleRule(_parser);

            if (selector != null)
                rule.Selector = selector;

            _nodes.Push(rule);
            FillDeclarations(rule.Style);
            _nodes.Pop();

            if (parent != null && selector != null)
                parent.AddNestedRule(rule);
        }

        /// <summary>
        /// Resolves a nested selector's prelude text against its parent's selector text per CSS Nesting:
        /// each <c>&amp;</c> becomes <c>:is(parent)</c> (giving <c>&amp;</c> the parent's specificity); a
        /// prelude with no <c>&amp;</c> is made relative to the parent (<c>:is(parent) &lt;prelude&gt;</c>),
        /// which correctly yields both the implicit-descendant (<c>.b</c> → <c>:is(parent) .b</c>) and the
        /// leading-combinator (<c>&gt; .b</c> → <c>:is(parent) &gt; .b</c>) forms.
        /// </summary>
        private static string ResolveNestedSelector(string prelude, string parentText)
        {
            var trimmed = prelude.Trim();
            var parentIs = ":is(" + (string.IsNullOrEmpty(parentText) ? "*" : parentText) + ")";

            // Substitute `&` token-aware: only a `&` that is a real nesting selector is replaced, never a
            // `&` inside a string/attribute value (e.g. `[data-x="a&b"]`), which must be preserved. This
            // also decides the "has &" branch correctly, so a prelude whose only `&` is inside a string is
            // still scoped under the parent (:is(parent) <prelude>) rather than mis-taking the replace path.
            var sb = Pool.NewStringBuilder();
            var hasNestingSelector = false;
            var quote = '\0';

            for (var i = 0; i < trimmed.Length; i++)
            {
                var c = trimmed[i];

                if (quote != '\0')
                {
                    sb.Append(c);
                    if (c == '\\' && i + 1 < trimmed.Length)
                        sb.Append(trimmed[++i]);   // escaped char inside a string — copy verbatim
                    else if (c == quote)
                        quote = '\0';
                    continue;
                }

                switch (c)
                {
                    case '"':
                    case '\'':
                        quote = c;
                        sb.Append(c);
                        break;
                    case '&':
                        hasNestingSelector = true;
                        sb.Append(parentIs);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return hasNestingSelector ? sb.ToPool() : parentIs + " " + sb.ToPool();
        }

        public Property CreateDeclarationWith(Func<string, Property> createProperty, ref Token token)
        {
            var property = default(Property);

            var sb = Pool.NewStringBuilder();
            var start = token.Position;

            while (token.IsDeclarationName())
            {
                sb.Append(token.ToValue());
                token = NextToken();
            }

            var propertyName = sb.ToPool();

            if (propertyName.Length > 0)
            {
                property = createProperty(propertyName);

                if (property == null && _parser.Options.IncludeUnknownDeclarations)
                {
                    property = new UnknownProperty(propertyName);
                }

                if (property == null)
                    RaiseErrorOccurred(ParseError.UnknownDeclarationName, start);
                else
                    _nodes.Push(property);

                ParseComments(ref token);

                if (token.Type == TokenType.Colon)
                {
                    var value = CreateValue(TokenType.CurlyBracketClose, ref token, out var important);

                    if (value != null && property != null && _parser.Options.EnableVendorPrefixAliases)
                        value = VendorValueAliases.Normalize(propertyName, property.Name, value);

                    if (value == null)
                        RaiseErrorOccurred(ParseError.ValueMissing, token.Position);
                    else if (property != null)
                    {
                        if(property.TrySetValue(value))
                            property.IsImportant = important;
                        else if(_parser.Options.AllowInvalidValues)
                        {
                            _nodes.Pop();

                            property = new UnknownProperty(propertyName);
                            property.TrySetValue(value);
                            property.IsImportant = important;
                            _nodes.Push(property);
                        }
                    }
                        

                    ParseComments(ref token);
                }
                else
                {
                    RaiseErrorOccurred(ParseError.ColonMissing, token.Position);
                }

                JumpToDeclEnd(ref token);

                if (property != null) _nodes.Pop();
            }
            else if (token.Type != TokenType.EndOfFile)
            {
                RaiseErrorOccurred(ParseError.IdentExpected, start);
                JumpToDeclEnd(ref token);
            }

            if (token.Type == TokenType.Semicolon) token = NextToken();

            return property;
        }

        public Property CreateDeclaration(ref Token token)
        {
            ParseComments(ref token);
            return CreateDeclarationWith(CreateProperty, ref token);
        }

        // Property lookup for declarations; vendor-prefixed spellings resolve only when the parser option is on.
        private Property CreateProperty(string name)
        {
            return _parser.Options.EnableVendorPrefixAliases
                ? VendorPropertyAliases.Create(name)
                : PropertyFactory.Instance.Create(name);
        }

        public Medium CreateMedium(ref Token token)
        {
            var medium = new Medium();
            ParseComments(ref token);

            if (token.Type == TokenType.Ident)
            {
                var identifier = token.Data;

                if (identifier.Isi(Keywords.Not))
                {
                    medium.IsInverse = true;
                    token = NextToken();
                    ParseComments(ref token);
                }
                else if (identifier.Isi(Keywords.Only))
                {
                    medium.IsExclusive = true;
                    token = NextToken();
                    ParseComments(ref token);
                }
            }

            if (token.Type == TokenType.Ident)
            {
                medium.Type = token.Data;
                token = NextToken();
                ParseComments(ref token);

                if (token.Type != TokenType.Ident || !token.Data.Isi(Keywords.And)) return medium;

                token = NextToken();
                ParseComments(ref token);
            }

            do
            {
                if (token.Type != TokenType.RoundBracketOpen) return null;

                token = NextToken();
                ParseComments(ref token);
                var feature = CreateFeature(ref token);

                if (feature != null) medium.AppendChild(feature);

                if (token.Type != TokenType.RoundBracketClose) return null;

                token = NextToken();
                ParseComments(ref token);

                if (feature == null) return null;

                if (token.Type != TokenType.Ident || !token.Data.Isi(Keywords.And)) break;

                token = NextToken();
                ParseComments(ref token);
            } while (token.Type != TokenType.EndOfFile);

            return medium;
        }

        private void JumpToEnd(ref Token current)
        {
            while (current.IsNot(TokenType.EndOfFile, TokenType.Semicolon)) current = NextToken();
        }

        private void MoveToRuleEnd(ref Token current)
        {
            var scopes = 0;

            while (current.Type != TokenType.EndOfFile)
            {
                if (current.Type == TokenType.CurlyBracketOpen)
                    scopes++;
                else if (current.Type == TokenType.CurlyBracketClose) scopes--;

                if (scopes <= 0 && current.Is(TokenType.CurlyBracketClose, TokenType.Semicolon)) break;

                current = NextToken();
            }
        }

        private void JumpToArgEnd(ref Token current)
        {
            var arguments = 0;

            while (current.Type != TokenType.EndOfFile)
            {
                if (current.Type == TokenType.RoundBracketOpen)
                    arguments++;
                else if (arguments <= 0 && current.Type == TokenType.RoundBracketClose)
                    break;
                else if (current.Type == TokenType.RoundBracketClose) arguments--;

                current = NextToken();
            }
        }

        private void JumpToDeclEnd(ref Token current)
        {
            var scopes = 0;

            while (current.Type != TokenType.EndOfFile)
            {
                if (current.Type == TokenType.CurlyBracketOpen)
                    scopes++;
                else if (scopes <= 0 && current.Is(TokenType.CurlyBracketClose, TokenType.Semicolon))
                    break;
                else if (current.Type == TokenType.CurlyBracketClose) scopes--;

                current = NextToken();
            }
        }

        private Token NextToken()
        {
            _markBeforeLastToken = _lexer.InsertionPoint;
            return _lexer.Get();
        }

        private StylesheetText CreateView(TextPosition start, TextPosition end)
        {
            var range = new TextRange(start, end);
            return new StylesheetText(range, _lexer.Source);
        }

        private void ParseComments(ref Token token)
        {
            var preserveComments = _parser.Options.PreserveComments;

            while (token.Type == TokenType.Whitespace || token.Type == TokenType.Comment ||
                   token.Type == TokenType.Cdc || token.Type == TokenType.Cdo)
            {
                if (preserveComments && token.Type == TokenType.Comment)
                {
                    var current = _nodes.Peek();
                    var comment = new Comment(token.Data);
                    var start = token.Position;
                    var end = start.After(token.ToValue());
                    comment.StylesheetText = CreateView(start, end);
                    current.AppendChild(comment);
                }

                token = NextToken();
            }
        }

        private Rule SkipDeclarations(Token token)
        {
            RaiseErrorOccurred(ParseError.InvalidToken, token.Position);
            MoveToRuleEnd(ref token);
            return default;
        }

        private void RaiseErrorOccurred(ParseError code, TextPosition position)
        {
            _lexer.RaiseErrorOccurred(code, position);
        }

        private IConditionFunction AggregateCondition(ref Token token)
        {
            var condition = ExtractCondition(ref token);

            if (condition == null) return null;

            ParseComments(ref token);
            var conjunction = token.Data;
            var creator = conjunction.GetCreator();

            if (creator != null)
            {
                token = NextToken();
                ParseComments(ref token);
                var conditions = MultipleConditions(condition, conjunction, ref token);
                condition = creator.Invoke(conditions);
            }

            return condition;
        }

        private IConditionFunction ExtractCondition(ref Token token)
        {
            if (token.Type == TokenType.RoundBracketOpen)
            {
                token = NextToken();
                ParseComments(ref token);
                var condition = AggregateCondition(ref token);

                if (condition != null)
                {
                    var group = new GroupCondition
                    {
                        Content = condition
                    };
                    condition = group;
                }
                else if (token.Type == TokenType.Ident)
                {
                    condition = DeclarationCondition(ref token);
                }

                if (token.Type != TokenType.RoundBracketClose) return condition;
                token = NextToken();
                ParseComments(ref token);

                return condition;
            }

            if (token.Data.Isi(Keywords.Not))
            {
                var condition = new NotCondition();
                token = NextToken();
                ParseComments(ref token);
                condition.Content = ExtractCondition(ref token);
                return condition;
            }

            return null;
        }

        private IConditionFunction DeclarationCondition(ref Token token)
        {
            var writtenName = token.Data;
            var property = CreateProperty(writtenName) ?? new UnknownProperty(writtenName);
            var declaration = default(DeclarationCondition);
            token = NextToken();
            ParseComments(ref token);

            if (token.Type != TokenType.Colon) return null;

            var result = CreateValue(TokenType.RoundBracketClose, ref token, out var important);
            if (result != null && _parser.Options.EnableVendorPrefixAliases)
                result = VendorValueAliases.Normalize(writtenName, property.Name, result);
            property.IsImportant = important;

            if (result != null) declaration = new DeclarationCondition(property, result);

            return declaration;
        }

        private List<IConditionFunction> MultipleConditions(IConditionFunction condition, string connector, ref Token token)
        {
            var list = new List<IConditionFunction>();
            ParseComments(ref token);
            list.Add(condition);

            while (token.Type != TokenType.EndOfFile)
            {
                condition = ExtractCondition(ref token);

                if (condition == null) break;

                list.Add(condition);

                if (!token.Data.Isi(connector)) break;

                token = NextToken();
                ParseComments(ref token);
            }

            return list;
        }

        private void FillFunctions(Action<DocumentFunction> add, ref Token token)
        {
            do
            {
                var function = token.ToDocumentFunction();

                if (function == null) break;

                token = NextToken();
                ParseComments(ref token);
                add(function);

                if (token.Type != TokenType.Comma) break;

                token = NextToken();
                ParseComments(ref token);
            } while (token.Type != TokenType.EndOfFile);
        }

        private TextPosition FillKeyframeRules(KeyframesRule parentRule)
        {
            var token = NextToken();
            ParseComments(ref token);

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
            {
                var rule = CreateKeyframeRule(token);
                token = NextToken();
                ParseComments(ref token);
                parentRule.Rules.Add(rule);
            }

            return token.Position;
        }

        private TextPosition FillDeclarations(DeclarationRule rule, Func<string, Property> createProperty)
        {
            var token = NextToken();
            ParseComments(ref token);

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
            {
                var property = CreateDeclarationWith(createProperty, ref token);

                if (property is {HasValue: true})
                {
                    rule.SetProperty(property);
                }

                ParseComments(ref token);
            }

            return token.Position;
        }

        private TextPosition FillRules(GroupingRule group)
        {
            // A conditional group rule written inside a style rule holds that rule's declarations directly,
            // plus further nested rules: they apply to the parent's elements under the condition, as if
            // wrapped in `& { }`.
            if (NestedInStyleRule(group) is { } parentRule && parentRule.Selector?.Text is { } parentText)
            {
                var wrapped = new StyleRule(_parser) { Selector = _parser.ParseSelector(parentText) };
                _nodes.Push(wrapped);
                var blockEnd = FillDeclarations(wrapped.Style);
                _nodes.Pop();
                group.Rules.Add(wrapped);
                return blockEnd;
            }

            var token = NextToken();
            ParseComments(ref token);

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketClose))
            {
                // "Consume a block's contents" discards a <semicolon-token> outright, exactly as it does a
                // <whitespace-token> (CSS Syntax 3 5.5.5). Passing it on to CreateStyle instead feeds it to
                // SelectorConstructor, which has no recovery for an unexpected semicolon: it folds the token
                // into the next rule's selector and invalidates it, and because the run-on rule then keeps
                // consuming, the block's own closing brace is swallowed too and the following sibling rule
                // is lost with it.
                //
                // Note this is deliberately NOT done in CreateRules: "consume a stylesheet's contents"
                // (5.5.1) has no <semicolon-token> case, so a stray semicolon there falls to "anything else"
                // and is consumed into the next qualified rule's prelude, invalidating it. Only a block
                // passes <semicolon-token> as the stop token to "consume a qualified rule" (5.5.3).
                if (token.Type == TokenType.Semicolon)
                {
                    token = NextToken();
                    ParseComments(ref token);
                    continue;
                }

                var rule = CreateRule(token);
                token = NextToken();
                ParseComments(ref token);
                group.Rules.Add(rule);
            }

            return token.Position;
        }

        private static bool IsNestableGroupRule(Token token) =>
            token.Data.Is(RuleNames.Media) || token.Data.Is(RuleNames.Supports) ||
            token.Data.Is(RuleNames.Container) || token.Data.Is(RuleNames.Layer);

        /// <summary>The style rule whose declaration block is being filled, when the construct being read sits directly in one.</summary>
        private StyleRule NearestStyleRule()
        {
            foreach (var node in _nodes)
            {
                if (node is StyleRule styleRule) return styleRule;
                if (node is not StyleDeclaration) return null;
            }

            return null;
        }

        /// <summary>The style rule <paramref name="group"/> is written inside, or null when it is a rule of a stylesheet or of another group.</summary>
        private StyleRule NestedInStyleRule(GroupingRule group)
        {
            var seenGroup = false;
            foreach (var node in _nodes)
            {
                if (!seenGroup)
                {
                    seenGroup = ReferenceEquals(node, group);
                    continue;
                }

                if (node is StyleRule styleRule) return styleRule;
                if (node is not StyleDeclaration) return null;
            }

            return null;
        }

        private void FillMediaList(MediaList list, TokenType end, ref Token token)
        {
            _nodes.Push(list);

            if (token.Type != end)
            {
                while (token.Type != TokenType.EndOfFile)
                {
                    var medium = CreateMedium(ref token);

                    if (medium != null) list.AppendChild(medium);

                    if (token.Type != TokenType.Comma) break;

                    token = NextToken();
                    ParseComments(ref token);
                }

                if (token.Type != end || list.Length == 0)
                {
                    list.Clear();
                    list.AppendChild(new Medium
                    {
                        IsInverse = true,
                        Type = Keywords.All
                    });
                }
            }

            _nodes.Pop();
        }

        private ISelector CreateSelector(ref Token token, bool topLevel = false)
        {
            var selector = _parser.GetSelectorCreator();
            var start = token.Position;

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketOpen) &&
                   (topLevel || token.Type != TokenType.CurlyBracketClose))
            {
                selector.Apply(token);
                token = NextToken();
            }

            var selectorIsValid = selector.IsValid;
            var result = selector.ToPool();

            if (result is StylesheetNode node)
            {
                var end = token.Position.Shift(-1);
                node.StylesheetText = CreateView(start, end);
            }

            if (!selectorIsValid && !_parser.Options.AllowInvalidSelectors)
            {
                RaiseErrorOccurred(ParseError.InvalidSelector, start);
                result = null;
            }

            return result;
        }

        private ISelector CreateMarginSelector(ref Token token)
        {
            var selector = _parser.GetSelectorCreator();
            var start = token.Position;

            while (token.IsNot(TokenType.EndOfFile, TokenType.CurlyBracketOpen, TokenType.CurlyBracketClose))
            {
                selector.Apply(token);
                token = NextToken();
            }

            var selectorIsValid = selector.IsValid;
            var result = selector.ToPool();

            if (result is StylesheetNode node)
            {
                var end = token.Position.Shift(-1);
                node.StylesheetText = CreateView(start, end);
            }

            if (!selectorIsValid && !_parser.Options.AllowInvalidSelectors)
            {
                RaiseErrorOccurred(ParseError.InvalidSelector, start);
                result = null;
            }

            return result;
        }

        private TokenValue CreateValue(TokenType closing, ref Token token, out bool important)
        {
            var value = Pool.NewValueBuilder();
            _lexer.IsInValue = true;
            token = NextToken();
            var start = token.Position;

            while (token.IsNot(TokenType.EndOfFile, TokenType.Semicolon, closing))
            {
                value.Apply(token);
                token = NextToken();
            }

            important = value.IsImportant;
            _lexer.IsInValue = false;
            var valueIsValid = value.IsValid;
            var result = value.ToPool();

            if (result != null && _parser.Options.EnableVendorPrefixAliases)
                result = LegacyGradientSyntax.Rewrite(result);

            //var node = result as StylesheetNode;
            var node = (StylesheetNode) result;

            if (node != null)
            {
                var end = token.Position.Shift(-1);
                node.StylesheetText = CreateView(start, end);
            }

            if (!valueIsValid && !_parser.Options.AllowInvalidValues)
            {
                RaiseErrorOccurred(ParseError.InvalidValue, start);
                result = null;
            }

            return result;
        }

        private string GetContainerName(ref Token token)
        {
            // "none", "and", "or" and "not" are reserved: they begin a condition rather than name a container.
            if (token.Type == TokenType.Ident &&
                (token.Data.Isi("none") || token.Data.Isi("and") || token.Data.Isi("or") || token.Data.Isi("not")))
            {
                return string.Empty;
            }

            return GetRuleName(ref token);
        }

        private string GetRuleName(ref Token token)
        {
            var name = string.Empty;

            if (token.Type == TokenType.Ident)
            {
                name = token.Data;
                token = NextToken();
            }

            return name;
        }

        private MediaFeature CreateFeature(ref Token token)
        {
            if (token.Type == TokenType.Ident)
            {
                var start = token.Position;
                var val = TokenValue.Empty;
                var feature = _parser.Options.AllowInvalidConstraints
                    ? new UnknownMediaFeature(token.Data)
                    : MediaFeatureFactory.Instance.Create(token.Data);

                token = NextToken();
                ParseComments(ref token);
                var tokenDelimiter = TokenType.Colon;
                if (token.Type == TokenType.Colon ||
                    token.Type == TokenType.GreaterThan || token.Type == TokenType.LessThan ||
                    token.Type == TokenType.GreaterThanOrEqual || token.Type == TokenType.LessThanOrEqual)
                {
                    tokenDelimiter = token.Type;
                    var value = Pool.NewValueBuilder();
                    token = NextToken();

                    while (token.IsNot(TokenType.RoundBracketClose, TokenType.EndOfFile) || !value.IsReady)
                    {
                        value.Apply(token);
                        token = NextToken();
                    }

                    val = value.ToPool();
                }
                else if (token.Type == TokenType.EndOfFile)
                {
                    return null;
                }

                if (feature != null && feature.TrySetValue(val, tokenDelimiter))
                {
                    if (feature is StylesheetNode node)
                    {
                        var end = token.Position.Shift(-1);
                        node.StylesheetText = CreateView(start, end);
                    }

                    return feature;
                }
            }
            else
            {
                JumpToArgEnd(ref token);
            }

            return null;
        }
    }
}