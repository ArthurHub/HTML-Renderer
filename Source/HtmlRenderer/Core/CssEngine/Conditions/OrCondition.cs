using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OrCondition : StylesheetNode, IConditionFunction
    {
        public bool Check()
        {
            var conditions = Children.OfType<IConditionFunction>();

            return conditions.Any(condition => condition.Check());
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var conditions = Children.OfType<IConditionFunction>();
            var first = true;

            foreach (var condition in conditions)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Write(" or ");
                }

                condition.ToCss(writer, formatter);
            }
        }
    }
}