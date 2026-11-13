using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>target-text()</c> function (CSS Generated Content 3):
    /// <c>target-text(&lt;target&gt; [, content | before | after | first-letter]?)</c>.
    /// </summary>
    internal sealed class TargetTextFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidModes = { "content", "before", "after", "first-letter" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = TargetGrammar.FunctionArguments(value, FunctionNames.TargetText);

            if (args == null || args.Length == 0 || args.Length > 2 || !TargetGrammar.IsValidTarget(args[0]))
                return null;

            if (args.Length == 2 && (!TargetGrammar.IsIdentifier(args[1]) ||
                                     !ValidModes.Contains(args[1].Data, StringComparer.OrdinalIgnoreCase)))
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
