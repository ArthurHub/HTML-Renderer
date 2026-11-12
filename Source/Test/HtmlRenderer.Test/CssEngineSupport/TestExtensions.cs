using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssEngineSupport;

internal static class TestExtensions
{
    public static Stylesheet ToCssStylesheet(this string sourceCode)
    {
        var parser = new StylesheetParser();
        return parser.Parse(sourceCode);
    }

    public static Stylesheet ToCssStylesheet(this Stream content)
    {
        var parser = new StylesheetParser();
        return parser.Parse(content);
    }

    public static IEnumerable<Comment> GetComments(this StylesheetNode node) => node.GetAll<Comment>();

    public static IEnumerable<T> GetAll<T>(this IStylesheetNode node) where T : IStyleFormattable
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node is T t)
        {
            yield return t;
        }

        foreach (var entity in node.Children.SelectMany(m => m.GetAll<T>()))
        {
            yield return entity;
        }
    }
}
