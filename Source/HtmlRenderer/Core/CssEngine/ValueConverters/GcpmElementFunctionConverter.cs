using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The CSS GCPM <c>element()</c> content function:
    /// <c>element(&lt;custom-ident&gt; [, first | start | last | first-except]?)</c>, which places an element
    /// removed from flow by <c>position: running(&lt;custom-ident&gt;)</c>. It is distinct from the CSS
    /// Images <c>element(#id)</c> image function, whose argument is a hash and is not accepted here.
    /// </summary>
    internal sealed class GcpmElementFunctionConverter : IValueConverter
    {
        private static readonly string[] ValidKeywords = { "first", "start", "last", "first-except" };

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = TargetGrammar.FunctionArguments(value, FunctionNames.Element);

            if (args == null || args.Length == 0 || args.Length > 2 || !TargetGrammar.IsIdentifier(args[0]))
                return null;

            var keyword = "first";

            if (args.Length == 2)
            {
                if (!TargetGrammar.IsIdentifier(args[1]) ||
                    !ValidKeywords.Contains(args[1].Data, StringComparer.OrdinalIgnoreCase))
                {
                    return null;
                }

                keyword = args[1].Data.ToLowerInvariant();
            }

            var text = keyword == "first"
                ? $"element({args[0].Data})"
                : $"element({args[0].Data}, {keyword})";

            return new GcpmFunctionValue(text, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GcpmFunctionValue>();
        }
    }
}
