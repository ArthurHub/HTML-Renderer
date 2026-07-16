using TheArtOfDev.HtmlRenderer.Core.CssEngine.Conditions;
using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class ParserExtensions
    {
        private static readonly Dictionary<string, Func<string, DocumentFunction>> FunctionTypes =
            new Dictionary<string, Func<string, DocumentFunction>>(StringComparer.OrdinalIgnoreCase)
            {
                {FunctionNames.Url, url => new UrlFunction(url)},
                {FunctionNames.Domain, url => new DomainFunction(url)},
                {FunctionNames.UrlPrefix, url => new UrlPrefixFunction(url)}
            };

        private static readonly Dictionary<string, Func<IEnumerable<IConditionFunction>, IConditionFunction>>
            GroupCreators =
                new Dictionary<string, Func<IEnumerable<IConditionFunction>, IConditionFunction>>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    {Keywords.And, CreateAndCondition},
                    {Keywords.Or, CreateOrCondition}
                };

        private static IConditionFunction CreateAndCondition(IEnumerable<IConditionFunction> conditions)
        {
            var andCondition = new AndCondition();
            foreach (var condition in conditions)
            {
                andCondition.AppendChild(condition);
            }
            return andCondition;
        }

        private static IConditionFunction CreateOrCondition(IEnumerable<IConditionFunction> conditions)
        {
            var orCondition = new OrCondition();
            foreach (var condition in conditions)
            {
                orCondition.AppendChild(condition);
            }
            return orCondition;
        }

        public static TokenType GetTypeFromName(this string functionName)
        {
            return FunctionTypes.TryGetValue(functionName, out _)
                ? TokenType.Url
                : TokenType.Function;
        }

        public static Func<IEnumerable<IConditionFunction>, IConditionFunction> GetCreator(this string conjunction)
        {
            GroupCreators.TryGetValue(conjunction, out var creator);
            return creator;
        }

        public static int GetCode(this ParseError code)
        {
            return (int)code;
        }

        public static bool Is(this Token token, TokenType a)
        {
            var type = token.Type;
            return type == a;
        }

        public static bool Is(this Token token, TokenType a, TokenType b)
        {
            var type = token.Type;
            return type == a || type == b;
        }

        public static bool IsNot(this Token token, TokenType a, TokenType b)
        {
            var type = token.Type;
            return type != a && type != b;
        }

        public static bool IsNot(this Token token, TokenType a, TokenType b, TokenType c)
        {
            var type = token.Type;
            return type != a && type != b && type != c;
        }

        public static bool IsDeclarationName(this Token token)
        {
            return token.Type != TokenType.EndOfFile &&
                   token.Type != TokenType.Colon &&
                   token.Type != TokenType.Whitespace &&
                   token.Type != TokenType.Comment &&
                   token.Type != TokenType.CurlyBracketOpen &&
                   token.Type != TokenType.Semicolon;
        }

        public static DocumentFunction ToDocumentFunction(this Token token)
        {
            switch (token.Type)
            {
                case TokenType.Url:
                    {
                        var functionName = ((UrlToken)token).FunctionName;
                        FunctionTypes.TryGetValue(functionName, out var creator);
                        return creator(token.Data);
                    }
                case TokenType.Function when token.Data.Isi(FunctionNames.Regexp):
                    {
                        var css = ((FunctionToken)token).ArgumentTokens.ToCssString();
                        if (css != null)
                        {
                            return new RegexpFunction(css);
                        }

                        break;
                    }
            }

            return null;
        }

        public static Rule CreateRule(this StylesheetParser parser, RuleType type)
        {
            switch (type)
            {
                case RuleType.Charset:
                    return new CharsetRule(parser);
                case RuleType.Document:
                    return new DocumentRule(parser);
                case RuleType.FontFace:
                    return new FontFaceRule(parser);
                case RuleType.Import:
                    return new ImportRule(parser);
                case RuleType.Keyframe:
                    return new KeyframeRule(parser);
                case RuleType.Keyframes:
                    return new KeyframesRule(parser);
                case RuleType.Media:
                    return new MediaRule(parser);
                case RuleType.Container:
                    return new ContainerRule(parser);
                case RuleType.Namespace:
                    return new NamespaceRule(parser);
                case RuleType.Page:
                    return new PageRule(parser);
                case RuleType.Style:
                    return new StyleRule(parser);
                case RuleType.Supports:
                    return new SupportsRule(parser);
                case RuleType.Viewport:
                    return new ViewportRule(parser);
                //case RuleType.Unknown:
                //case RuleType.RegionStyle:
                //case RuleType.FontFeatureValues:
                //case RuleType.CounterStyle:
                default:
                    return null;
            }
        }
    }
}