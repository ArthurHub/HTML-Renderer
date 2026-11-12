using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Tests for <see cref="CssProperty{T}"/> (the typed box-side property value) and the Layer A →
/// <see cref="ITypedPropertyValue{T}"/> carrier that threads a converter's parse to the box without
/// re-parsing.
/// </summary>
[TestClass]
public sealed class TypedCssPropertyTests
{
    [TestMethod]
    public void FromValue_IsResolvedValue()
    {
        var p = CssProperty<string>.FromValue("100px 200px", "parsed");
        Assert.IsFalse(p.IsGlobalValue);
        Assert.IsFalse(p.IsUnresolved);
        Assert.IsNull(p.GlobalValue);
        Assert.AreEqual("parsed", p.Value);
        Assert.AreEqual("100px 200px", p.ToString());
    }

    [TestMethod]
    public void Unresolved_HasNoValue_AndKeepsRawText()
    {
        var p = CssProperty<string>.Unresolved("var(--cols)");
        Assert.IsTrue(p.IsUnresolved);
        Assert.IsFalse(p.IsGlobalValue);
        Assert.IsNull(p.Value);
        Assert.AreEqual("var(--cols)", p.ToString());
    }

    [TestMethod]
    [DataRow("inherit")]
    [DataRow("initial")]
    [DataRow("unset")]
    [DataRow("revert")]
    [DataRow("revert-layer")]
    public void Global_ExposesKeyword_AndRoundTripsText(string text)
    {
        Assert.IsTrue(CssGlobalKeywords.TryParse(text, out var keyword));
        var p = CssProperty<string>.Global(keyword);
        Assert.IsTrue(p.IsGlobalValue);
        Assert.IsFalse(p.IsUnresolved);
        Assert.AreEqual(keyword, p.GlobalValue);
        Assert.IsNull(p.Value);
        Assert.AreEqual(text, p.ToString());
    }

    [TestMethod]
    [DataRow("inherit")]
    [DataRow("INITIAL")]      // case-insensitive
    [DataRow("revert-layer")]
    public void CssGlobalKeywords_RoundTrip(string text)
    {
        Assert.IsTrue(CssGlobalKeywords.TryParse(text, out var keyword));
        Assert.AreEqual(text.ToLowerInvariant(), CssGlobalKeywords.ToText(keyword));
    }

    [TestMethod]
    [DataRow("100px")]
    [DataRow("auto")]
    [DataRow("")]
    public void CssGlobalKeywords_TryParse_RejectsNonKeywords(string text)
    {
        Assert.IsFalse(CssGlobalKeywords.TryParse(text, out _));
    }

    // ─── Layer A converter → typed carrier ──────────────────────────────────────

    private static CssProperty<GridTemplate>? ConvertTyped(string value)
    {
        var result = new GridTemplateValueConverter().Convert(CssValueParser.GetCssTokens(value));
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.TryGetValue<GridTemplate>(out var typed));
        return typed;
    }

    [TestMethod]
    public void Converter_TrackList_CarriesParsedTemplate()
    {
        var typed = ConvertTyped("100px 200px");
        Assert.IsNotNull(typed);
        Assert.IsFalse(typed!.IsGlobalValue);
        Assert.IsNotNull(typed.Value);
        Assert.AreEqual(2, typed.Value!.Tracks.Count);
    }

    [TestMethod]
    public void Converter_None_IsResolvedWithNullTemplate()
    {
        // `none` is a resolved value whose parsed template is null (no explicit tracks), not a global keyword.
        var typed = ConvertTyped("none");
        Assert.IsNotNull(typed);
        Assert.IsFalse(typed!.IsGlobalValue);
        Assert.IsFalse(typed.IsUnresolved);
        Assert.IsNull(typed.Value);
    }

    [TestMethod]
    public void Converter_Subgrid_CarriesSubgridTemplate()
    {
        var typed = ConvertTyped("subgrid");
        Assert.IsNotNull(typed);
        Assert.IsNotNull(typed!.Value);
        Assert.IsTrue(typed.Value!.IsSubgrid);
    }

    [TestMethod]
    public void Converter_RejectsInvalidTemplate()
    {
        Assert.IsNull(new GridTemplateValueConverter().Convert(CssValueParser.GetCssTokens("banana")));
    }

    [TestMethod]
    public void TryGetValue_WrongType_ReturnsFalse()
    {
        // The carrier is an ITypedPropertyValue<GridTemplate>; asking for a different T must not match.
        var result = new GridTemplateValueConverter().Convert(CssValueParser.GetCssTokens("100pt"));
        Assert.IsNotNull(result);
        Assert.IsFalse(result!.TryGetValue<int>(out var typed));
        Assert.IsNull(typed);
    }
}
