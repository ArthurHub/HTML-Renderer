using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The value grammar of <c>font-variant-alternates</c> (CSS Fonts 4 6.8):
    /// <c>normal | [ stylistic(&lt;feature-value-name&gt;) || historical-forms ||
    /// styleset(&lt;feature-value-name&gt;#) || character-variant(&lt;feature-value-name&gt;#) ||
    /// swash(&lt;feature-value-name&gt;) || ornaments(&lt;feature-value-name&gt;) ||
    /// annotation(&lt;feature-value-name&gt;) ]</c>. Each of the seven components may appear at most once, in
    /// any order. Only the grammar is validated: whether a name is registered by an
    /// <c>@font-feature-values</c> rule is a per-element, used-value question and not a parse error.
    /// </summary>
    internal static class FontVariantAlternatesGrammar
    {
        public const string HistoricalForms = "historical-forms";

        private static readonly string[] SingleIdentFunctions = {"stylistic", "swash", "ornaments", "annotation"};

        private static readonly string[] IdentListFunctions = {"styleset", "character-variant"};

        public static readonly IValueConverter Converter = new FontGrammarValueConverter(TryParse);

        internal static bool IsAlternatesToken(Token token)
        {
            return token.Type == TokenType.Function &&
                   (FontGrammar.Contains(SingleIdentFunctions, token.Data) ||
                    FontGrammar.Contains(IdentListFunctions, token.Data)) ||
                   FontGrammar.IsIdent(token, HistoricalForms);
        }

        public static bool TryParse(IReadOnlyList<Token> tokens, out string text)
        {
            text = null;

            if (tokens.Count == 0) return false;

            if (tokens.Count == 1 && FontGrammar.IsIdent(tokens[0], Keywords.Normal))
            {
                text = Keywords.Normal;
                return true;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var clauses = new List<string>();

            foreach (var token in tokens)
            {
                var clause = ParseClause(token, seen);

                if (clause == null) return false;

                clauses.Add(clause);
            }

            text = string.Join(" ", clauses);
            return true;
        }

        private static string ParseClause(Token token, HashSet<string> seen)
        {
            if (token.Type == TokenType.Ident)
            {
                return FontGrammar.IsIdent(token, HistoricalForms) && seen.Add(HistoricalForms)
                    ? HistoricalForms
                    : null;
            }

            if (!(token is FunctionToken function) || !seen.Add(function.Data)) return null;

            var name = function.Data.ToLowerInvariant();
            var arguments = function.ArgumentTokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (FontGrammar.Contains(SingleIdentFunctions, name))
            {
                return arguments.Length == 1 && arguments[0].Type == TokenType.Ident
                    ? name + "(" + arguments[0].Data + ")"
                    : null;
            }

            if (!FontGrammar.Contains(IdentListFunctions, name)) return null;

            // <feature-value-name># - one or more comma-separated identifiers.
            var names = new List<string>();
            var expectName = true;

            foreach (var argument in arguments)
            {
                if (expectName)
                {
                    if (argument.Type != TokenType.Ident) return null;
                    names.Add(argument.Data);
                }
                else if (argument.Type != TokenType.Comma)
                {
                    return null;
                }

                expectName = !expectName;
            }

            return names.Count > 0 && !expectName ? name + "(" + string.Join(", ", names) + ")" : null;
        }
    }
}
