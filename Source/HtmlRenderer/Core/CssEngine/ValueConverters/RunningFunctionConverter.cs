using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>running()</c> function used by <c>position</c> (CSS GCPM 3, running elements):
    /// <c>position: running(&lt;custom-ident&gt;)</c> takes the element out of flow so that it can be placed
    /// in a page margin box with <c>content: element(&lt;custom-ident&gt;)</c>.
    /// </summary>
    internal sealed class RunningFunctionConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = TargetGrammar.FunctionArguments(value, FunctionNames.Running);

            if (args == null || args.Length != 1 || !TargetGrammar.IsIdentifier(args[0])) return null;

            return new GcpmFunctionValue($"running({args[0].Data})", value);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<GcpmFunctionValue>();
        }
    }
}
