using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal class StylesheetParser
    {
        internal static readonly StylesheetParser Default = new StylesheetParser();

        public StylesheetParser(
            bool includeUnknownRules = false,
            bool includeUnknownDeclarations = false,
            bool tolerateInvalidSelectors = false,
            bool tolerateInvalidValues = false,
            bool tolerateInvalidConstraints = false,
            bool preserveComments = false,
            bool preserveDuplicateProperties = false
        )
        {
            Options = new ParserOptions
            {
                IncludeUnknownRules = includeUnknownRules,
                IncludeUnknownDeclarations = includeUnknownDeclarations,
                AllowInvalidSelectors = tolerateInvalidSelectors,
                AllowInvalidValues = tolerateInvalidValues,
                AllowInvalidConstraints = tolerateInvalidConstraints,
                PreserveComments = preserveComments,
                PreserveDuplicateProperties = preserveDuplicateProperties,
            };
        }

        internal ParserOptions Options { get; }

        public Stylesheet Parse(string content)
        {
            var source = new TextSource(content);
            return Parse(source);
        }

        public Stylesheet Parse(Stream content)
        {
            var source = new TextSource(content);
            return Parse(source);
        }

        public Task<Stylesheet> ParseAsync(string content)
        {
            return ParseAsync(content, CancellationToken.None);
        }

        public async Task<Stylesheet> ParseAsync(string content, CancellationToken cancelToken)
        {
            var source = new TextSource(content);
            await source.PrefetchAllAsync(cancelToken).ConfigureAwait(false);
            return Parse(source);
        }

        public Task<Stylesheet> ParseAsync(Stream content)
        {
            return ParseAsync(content, CancellationToken.None);
        }

        public async Task<Stylesheet> ParseAsync(Stream content, CancellationToken cancelToken)
        {
            var source = new TextSource(content);
            await source.PrefetchAllAsync(cancelToken).ConfigureAwait(false);
            return Parse(source);
        }

        public ISelector ParseSelector(string selectorText)
        {
            var tokenizer = CreateTokenizer(selectorText);
            var token = tokenizer.Get();
            var creator = GetSelectorCreator();
            while (token.Type != TokenType.EndOfFile)
            {
                creator.Apply(token);
                token = tokenizer.Get();
            }

            var valid = creator.IsValid;
            var result = creator.ToPool();

            return valid || Options.AllowInvalidSelectors ? result : null;
        }

        internal KeyframeSelector ParseKeyframeSelector(string keyText)
        {
            return Parse(keyText, (b, t) => Tuple.Create(b.CreateKeyframeSelector(ref t), t));
        }

        internal SelectorConstructor GetSelectorCreator()
        {
            var attributeSelector = AttributeSelectorFactory.Instance;
            var pseudoClassSelector = PseudoClassSelectorFactory.Instance;
            var pseudoElementSelector = new PseudoElementSelectorFactory(this);
            return Pool.NewSelectorConstructor(attributeSelector, pseudoClassSelector, pseudoElementSelector);
        }

        internal Stylesheet Parse(TextSource source)
        {
            var sheet = new Stylesheet(this);
            var tokenizer = new Lexer(source);
            var start = tokenizer.GetCurrentPosition();
            var builder = new StylesheetComposer(tokenizer, this);
            var end = builder.CreateRules(sheet);
            var range = new TextRange(start, end);
            sheet.StylesheetText = new StylesheetText(range, source);
            return sheet;
        }

        internal async Task<Stylesheet> ParseAsync(Stylesheet sheet, TextSource source)
        {
            await source.PrefetchAllAsync(CancellationToken.None).ConfigureAwait(false);
            var tokenizer = new Lexer(source);
            var start = tokenizer.GetCurrentPosition();
            var builder = new StylesheetComposer(tokenizer, this);
            //var tasks = new List<Task>();
            var end = builder.CreateRules(sheet);
            var range = new TextRange(start, end);
            sheet.StylesheetText = new StylesheetText(range, source);

            foreach (var rule in sheet.Rules)
            {
                if (rule.Type == RuleType.Charset) continue;
                if (rule.Type != RuleType.Import) break;
            }

            //await TaskEx.WhenAll(tasks).ConfigureAwait(false);
            return sheet;
        }

        internal TokenValue ParseValue(string valueText)
        {
            var tokenizer = CreateTokenizer(valueText);
            var token = default(Token);
            var builder = new StylesheetComposer(tokenizer, this);
            var value = builder.CreateValue(ref token);
            return token.Type == TokenType.EndOfFile ? value : null;
        }

        internal Rule ParseRule(string ruleText)
        {
            return Parse(ruleText, (b, t) => b.CreateRule(t));
        }

        internal Property ParseDeclaration(string declarationText)
        {
            return Parse(declarationText, (b, t) => Tuple.Create(b.CreateDeclaration(ref t), t));
        }

        internal List<Medium> ParseMediaList(string mediaText)
        {
            return Parse(mediaText, (b, t) => Tuple.Create(b.CreateMedia(ref t), t));
        }

        internal IConditionFunction ParseCondition(string conditionText)
        {
            return Parse(conditionText, (b, t) => Tuple.Create(b.CreateCondition(ref t), t));
        }

        internal List<DocumentFunction> ParseDocumentRules(string documentText)
        {
            return Parse(documentText, (b, t) => Tuple.Create(b.CreateFunctions(ref t), t));
        }

        internal Medium ParseMedium(string mediumText)
        {
            return Parse(mediumText, (b, t) => Tuple.Create(b.CreateMedium(ref t), t));
        }

        internal KeyframeRule ParseKeyframeRule(string ruleText)
        {
            return Parse(ruleText, (b, t) => b.CreateKeyframeRule(t));
        }

        internal void AppendDeclarations(StyleDeclaration style, string declarations)
        {
            var tokenizer = CreateTokenizer(declarations);
            var builder = new StylesheetComposer(tokenizer, this);
            builder.FillDeclarations(style);
        }

        private T Parse<T>(string source, Func<StylesheetComposer, Token, T> create)
        {
            var tokenizer = CreateTokenizer(source);
            var token = tokenizer.Get();
            var builder = new StylesheetComposer(tokenizer, this);
            var rule = create(builder, token);
            return tokenizer.Get().Type == TokenType.EndOfFile ? rule : default;
        }

        private T Parse<T>(string source, Func<StylesheetComposer, Token, Tuple<T, Token>> create)
        {
            var tokenizer = CreateTokenizer(source);
            var token = tokenizer.Get();
            var builder = new StylesheetComposer(tokenizer, this);
            var pair = create(builder, token);
            return pair.Item2.Type == TokenType.EndOfFile ? pair.Item1 : default;
        }

        private static Lexer CreateTokenizer(string sourceCode)
        {
            var source = new TextSource(sourceCode);
            return new Lexer(source);
        }
    }
}