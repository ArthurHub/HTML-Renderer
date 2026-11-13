using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// The rule types without a rule class of their own (<see cref="RuleType.Unknown"/>,
/// <see cref="RuleType.RegionStyle"/>)
/// give <see langword="null"/>.
/// </summary>
[TestClass]
public sealed class ParserExtensionsCreateRuleTests
{
    [TestMethod]
    [DataRow((byte)RuleType.Charset, typeof(CharsetRule))]
    [DataRow((byte)RuleType.Document, typeof(DocumentRule))]
    [DataRow((byte)RuleType.FontFace, typeof(FontFaceRule))]
    [DataRow((byte)RuleType.Import, typeof(ImportRule))]
    [DataRow((byte)RuleType.Keyframe, typeof(KeyframeRule))]
    [DataRow((byte)RuleType.Keyframes, typeof(KeyframesRule))]
    [DataRow((byte)RuleType.Media, typeof(MediaRule))]
    [DataRow((byte)RuleType.Container, typeof(ContainerRule))]
    [DataRow((byte)RuleType.Namespace, typeof(NamespaceRule))]
    [DataRow((byte)RuleType.Page, typeof(PageRule))]
    [DataRow((byte)RuleType.Style, typeof(StyleRule))]
    [DataRow((byte)RuleType.Supports, typeof(SupportsRule))]
    [DataRow((byte)RuleType.Viewport, typeof(ViewportRule))]
    [DataRow((byte)RuleType.FontFeatureValues, typeof(FontFeatureValuesRule))]
    [DataRow((byte)RuleType.CounterStyle, typeof(CounterStyleRule))]
    public void CreateRule_KnownRuleType_ReturnsExpectedRuleSubtype(byte typeValue, Type expectedType)
    {
        var parser = new StylesheetParser();

        var rule = parser.CreateRule((RuleType)typeValue);

        Assert.IsNotNull(rule);
        Assert.IsInstanceOfType(rule, expectedType);
    }

    [TestMethod]
    [DataRow((byte)RuleType.Unknown)]
    [DataRow((byte)RuleType.RegionStyle)]
    public void CreateRule_RuleTypeWithNoSubtype_ReturnsNull(byte typeValue)
    {
        var parser = new StylesheetParser();

        Assert.IsNull(parser.CreateRule((RuleType)typeValue));
    }
}
