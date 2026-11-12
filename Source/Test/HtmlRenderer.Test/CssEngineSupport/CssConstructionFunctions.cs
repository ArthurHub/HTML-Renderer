using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.CssEngineSupport;

/// <summary>
/// Parses a CSS snippet with the internal CSS engine, for tests that don't need a whole document.
/// </summary>
internal static class CssConstructionFunctions
{
    internal static Stylesheet ParseStyleSheet(string source,
        bool includeUnknownRules = false,
        bool includeUnknownDeclarations = false,
        bool tolerateInvalidSelectors = false,
        bool tolerateInvalidValues = false,
        bool tolerateInvalidConstraints = false,
        bool preserveComments = false,
        bool preserveDuplicateProperties = false)
    {
        var parser = new StylesheetParser(
            includeUnknownRules,
            includeUnknownDeclarations,
            tolerateInvalidSelectors,
            tolerateInvalidValues,
            tolerateInvalidConstraints,
            preserveComments,
            preserveDuplicateProperties);

        return parser.Parse(source);
    }

    internal static Rule ParseRule(string source)
    {
        var parser = new StylesheetParser();
        return parser.ParseRule(source);
    }

    internal static Property ParseDeclaration(string source,
        bool includeUnknownRules = false,
        bool includeUnknownDeclarations = false,
        bool tolerateInvalidSelectors = false,
        bool tolerateInvalidValues = false,
        bool tolerateInvalidConstraints = false,
        bool preserveComments = false)
    {
        var parser = new StylesheetParser(
            includeUnknownRules,
            includeUnknownDeclarations,
            tolerateInvalidSelectors,
            tolerateInvalidValues,
            tolerateInvalidConstraints,
            preserveComments);
        return parser.ParseDeclaration(source);
    }

    internal static TokenValue ParseValue(string source)
    {
        var parser = new StylesheetParser();
        return parser.ParseValue(source);
    }

    internal static StyleDeclaration ParseDeclarations(string declarations)
    {
        var parser = new StylesheetParser();
        var style = new StyleDeclaration(parser);
        style.Update(declarations);
        return style;
    }

    internal static KeyframeRule ParseKeyframeRule(string source)
    {
        var parser = new StylesheetParser();
        return parser.ParseKeyframeRule(source);
    }

    internal static void TestForLegalValue<TProp>(string propertyName, string value) where TProp : Property
    {
        var snippet = $"{propertyName}: {value}";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual(propertyName, property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TProp>(property);
        var concrete = (TProp)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual(value, concrete.Value);
    }

    internal static IEnumerable<string> GlobalKeywordTestValues =>
    [
        Keywords.Inherit,
        Keywords.Initial,
        Keywords.Revert,
        Keywords.RevertLayer,
        Keywords.Unset
    ];

    internal static IEnumerable<object[]> LengthOrPercentOrGlobalTestValues =>
        new[]
        {
            new object[] { "0" },
            new object[] { "20px" },
            new object[] { "1em" },
            new object[] { "3vmin" },
            new object[] { "0.5cm" },
            new object[] { "10%" }
        }.Union(GlobalKeywordTestValues.ToObjectArray());
}
