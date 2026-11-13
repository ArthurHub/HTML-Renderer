using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>leader()</c> function (CSS Generated Content 3): <c>leader(&lt;leader-type&gt;?)</c> where
    /// <c>&lt;leader-type&gt; = dotted | solid | space | &lt;string&gt;</c>. A bare <c>leader()</c> is
    /// equivalent to <c>leader(dotted)</c>.
    /// </summary>
    internal sealed class LeaderFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidKeywords = { Keywords.Dotted, Keywords.Solid, Keywords.Space };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = TargetGrammar.FunctionArguments(value, FunctionNames.Leader);

            if (args == null || args.Length > 1) return null;

            if (args.Length == 1 &&
                args[0].Type != TokenType.String &&
                !(TargetGrammar.IsIdentifier(args[0]) &&
                  ValidKeywords.Contains(args[0].Data, StringComparer.OrdinalIgnoreCase)))
            {
                return null;
            }

            return new GcpmFunctionValue(new TokenValue(value).Text, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GcpmFunctionValue>();
        }
    }
}
