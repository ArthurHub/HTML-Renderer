using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class CompressedStyleFormatter : IStyleFormatter
    {
        public static readonly IStyleFormatter Instance = new CompressedStyleFormatter();

        string IStyleFormatter.Declaration(string name, string value, bool important)
        {
            var rest = string.Concat(value, important ? " !important" : string.Empty);
            return string.Concat(name, ": ", rest);
        }

        string IStyleFormatter.Constraint(string name, string value, string constraintDelimiter)
        {
            var ending = value != null ? constraintDelimiter + value : string.Empty;
            return string.Concat("(", name, ending, ")");
        }

        string IStyleFormatter.Rule(string name, string value)
        {
            return string.Concat(name, " ", value, ";");
        }

        string IStyleFormatter.Rule(string name, string prelude, string rules)
        {
            var text = string.IsNullOrEmpty(prelude) ? string.Empty : prelude + " ";
            return string.Concat(name, " ", text, rules);
        }

        string IStyleFormatter.Style(string selector, IStyleFormattable rules)
        {
            var sb = Pool.NewStringBuilder().Append(selector).Append(" { ");
            var length = sb.Length;
            using (var writer = new StringWriter(sb))
            {
                rules.ToCss(writer, this);
            }

            if (sb.Length > length) sb.Append(' ');
            return sb.Append('}').ToPool();
        }

        string IStyleFormatter.Comment(string data)
        {
            return string.Join("/* ", data, " */");
        }

        string IStyleFormatter.Sheet(IEnumerable<IStyleFormattable> rules)
        {
            var sb = Pool.NewStringBuilder();
            WriteJoined(sb, rules, Environment.NewLine);
            return sb.ToPool();
        }

        string IStyleFormatter.Block(IEnumerable<IStyleFormattable> rules)
        {
            var sb = Pool.NewStringBuilder().Append('{');
            using (var writer = new StringWriter(sb))
            {
                foreach (var rule in rules)
                {
                    writer.Write(' ');
                    rule.ToCss(writer, this);
                }
            }

            return sb.Append(' ').Append('}').ToPool();
        }

        string IStyleFormatter.Declarations(IEnumerable<string> declarations)
        {
            return string.Join("; ", declarations);
        }

        string IStyleFormatter.Medium(bool exclusive,
            bool inverse,
            string type,
            IEnumerable<IStyleFormattable> constraints)
        {
            var sb = Pool.NewStringBuilder();
            var first = true;
            if (exclusive)
                sb.Append("only ");
            else if (inverse) sb.Append("not ");

            if (!string.IsNullOrEmpty(type))
            {
                sb.Append(type);
                first = false;
            }

            WriteJoined(sb, constraints, " and ", first);
            return sb.ToPool();
        }

        private void WriteJoined(StringBuilder sb,
            IEnumerable<IStyleFormattable> elements,
            string separator,
            bool first = true)
        {
            using(var writer = new StringWriter(sb)){
                foreach (var element in elements)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.Write(separator);
                    }

                    element.ToCss(writer, this);
                }
            }
        }
    }
}