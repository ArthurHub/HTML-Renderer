using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>target-counter()</c> and <c>target-counters()</c> functions (CSS Generated Content 3):
    /// <c>target-counter(&lt;target&gt;, &lt;counter-name&gt; [, &lt;counter-style&gt;]?)</c> and
    /// <c>target-counters(&lt;target&gt;, &lt;counter-name&gt;, &lt;string&gt; [, &lt;counter-style&gt;]?)</c>.
    /// </summary>
    internal sealed class TargetCounterFunctionConverter : IValueConverter
    {
        private readonly bool _plural;

        public TargetCounterFunctionConverter(bool plural)
        {
            _plural = plural;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = TargetGrammar.FunctionArguments(
                value, _plural ? FunctionNames.TargetCounters : FunctionNames.TargetCounter);

            if (args == null) return null;

            // <target>, <counter-name>, then (plural only) a <string> separator, then an optional style.
            var required = _plural ? 3 : 2;
            if (args.Length < required || args.Length > required + 1) return null;
            if (!TargetGrammar.IsValidTarget(args[0]) || !TargetGrammar.IsIdentifier(args[1])) return null;
            if (_plural && args[2].Type != TokenType.String) return null;
            if (args.Length > required && !TargetGrammar.IsIdentifier(args[required])) return null;

            return new GcpmFunctionValue(new TokenValue(value).Text, value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GcpmFunctionValue>();
        }
    }
}
