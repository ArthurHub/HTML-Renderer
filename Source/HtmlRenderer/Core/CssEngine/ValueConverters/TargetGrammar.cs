using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Shared argument parsing for the GCPM functions that take a <c>&lt;target&gt;</c> (a
    /// <c>&lt;string&gt;</c>, <c>url()</c> or <c>attr()</c>), such as <c>target-counter()</c>,
    /// <c>target-counters()</c> and <c>target-text()</c>.
    /// </summary>
    internal static class TargetGrammar
    {
        /// <summary>
        /// Returns the function's arguments (commas and whitespace removed) when <paramref name="value"/> is
        /// exactly one function called <paramref name="name"/>; otherwise <c>null</c>.
        /// </summary>
        public static Token[] FunctionArguments(IEnumerable<Token> value, string name)
        {
            if (value.OnlyOrDefault() is not FunctionToken function ||
                !function.Data.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return function.ArgumentTokens
                .Where(t => t.Type != TokenType.Comma && t.Type != TokenType.Whitespace &&
                            t.Type != TokenType.RoundBracketClose)
                .ToArray();
        }

        /// <summary>Whether <paramref name="token"/> is a valid <c>&lt;target&gt;</c> argument.</summary>
        public static bool IsValidTarget(Token token)
        {
            return token.Type == TokenType.String || token.Type == TokenType.Url ||
                   (token is FunctionToken function &&
                    function.Data.Equals(FunctionNames.Attr, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Whether <paramref name="token"/> is a bare identifier.</summary>
        public static bool IsIdentifier(Token token) => token.Type == TokenType.Ident;
    }
}
