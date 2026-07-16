using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IStyleFormatter
    {
        string Sheet(IEnumerable<IStyleFormattable> rules);
        string Block(IEnumerable<IStyleFormattable> rules);
        string Declaration(string name, string value, bool important);
        string Declarations(IEnumerable<string> declarations);
        string Medium(bool exclusive, bool inverse, string type, IEnumerable<IStyleFormattable> constraints);
        string Constraint(string name, string value, string constraintDelimiter);
        string Rule(string name, string value);
        string Rule(string name, string prelude, string rules);
        string Style(string selector, IStyleFormattable rules);
        string Comment(string data);
    }
}