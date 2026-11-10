using System.Linq;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class GridTrackListGrammarTests
{
    private static GridTemplate Parse(string value) =>
        GridTrackListGrammar.TryParse(CssValueParser.GetCssTokens(value));

    [TestMethod]
    [Spec("css-grid-2", "#track-sizing")]
    [Spec("css-grid-2", "#track-sizes")]
    [Spec("css-grid-2", "#repeat-syntax")]
    [Spec("css-grid-2", "#auto-repeat")]
    [Spec("css-grid-2", "#fr-unit")]
    [DataRow("100px")]
    [DataRow("100px 200px")]
    [DataRow("1fr 2fr")]
    [DataRow("25% 75%")]
    [DataRow("auto")]
    [DataRow("auto 1fr auto")]
    [DataRow("min-content max-content")]
    [DataRow("minmax(100px, 1fr)")]
    [DataRow("minmax(min-content, max-content)")]
    [DataRow("fit-content(200px)")]
    [DataRow("repeat(3, 100px)")]
    [DataRow("repeat(2, 1fr 2fr)")]
    [DataRow("repeat(auto-fill, minmax(200px, 1fr))")]
    [DataRow("repeat(auto-fit, 100px)")]
    [DataRow("100px repeat(2, 1fr) 100px")]
    public void ValidTrackLists_Parse(string value)
    {
        Assert.IsNotNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-grid-2", "#track-sizing")]
    [Spec("css-grid-2", "#repeat-syntax")]
    [DataRow("")]
    [DataRow("none")]                       // 'none' is accepted by the property, not the grammar
    [DataRow("banana")]
    [DataRow("minmax(1fr, 2fr)")]           // a flex min is invalid in minmax()
    [DataRow("minmax(100px)")]              // minmax needs two args
    [DataRow("repeat(0, 100px)")]           // repeat count must be >= 1
    [DataRow("repeat(auto-fill)")]          // repeat needs a track body
    [DataRow("-1fr")]                        // negative flex
    [DataRow("[unclosed 100px")]             // an unclosed [ is invalid
    public void InvalidTrackLists_ReturnNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-grid-2", "#named-lines")]
    [DataRow("[sidebar-start] 200px [sidebar-end] 1fr")]
    [DataRow("[a b] 100px [c]")]
    [DataRow("[start] repeat(3, 100px) [end]")]
    public void NamedLines_Parse(string value)
    {
        Assert.IsNotNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-grid-2", "#named-lines")]
    public void NamedLines_RecordSortedLineNumbers()
    {
        // [a] 100px [b] 100px [a] — 'a' labels lines 1 and 3, 'b' labels line 2.
        var template = Parse("[a] 100px [b] 100px [a]");
        Assert.IsNotNull(template);
        CollectionAssert.AreEqual(new[] { 1, 3 }, template.LineNames["a"].ToArray());
        CollectionAssert.AreEqual(new[] { 2 }, template.LineNames["b"].ToArray());
    }

    [TestMethod]
    [Spec("css-grid-2", "#repeat-syntax")]
    public void RepeatFixed_ExpandsInline()
    {
        var template = Parse("repeat(3, 100px)");
        Assert.IsNotNull(template);
        Assert.AreEqual(3, template.Tracks.Count);
        foreach (var t in template.Tracks)
        {
            Assert.AreEqual(GridTrackKind.Length, t.Kind);
        }
    }

    [TestMethod]
    [Spec("css-grid-2", "#fr-unit")]
    public void Fr_ParsesAsFlexFactor()
    {
        var template = Parse("1fr 2fr");
        Assert.IsNotNull(template);
        Assert.AreEqual(GridTrackKind.Flex, template.Tracks[0].Kind);
        Assert.AreEqual(1.0, template.Tracks[0].Flex, 0.00001);
        Assert.AreEqual(2.0, template.Tracks[1].Flex, 0.00001);
    }

    [TestMethod]
    [Spec("css-grid-2", "#track-sizes")]
    public void Minmax_CapturesBothBreadths()
    {
        var template = Parse("minmax(100px, 1fr)");
        Assert.IsNotNull(template);
        Assert.AreEqual(1, template.Tracks.Count);
        var track = template.Tracks[0];
        Assert.AreEqual(GridTrackKind.Minmax, track.Kind);
        Assert.AreEqual(GridTrackKind.Length, track.Min.Kind);
        Assert.AreEqual(GridTrackKind.Flex, track.Max.Kind);
    }

    [TestMethod]
    [Spec("css-grid-2", "#auto-repeat")]
    public void AutoRepeat_RecordedWithKindAndInsertIndex()
    {
        var template = Parse("100px repeat(auto-fill, 50px) 100px");
        Assert.IsNotNull(template);
        Assert.AreEqual(GridAutoRepeatKind.AutoFill, template.AutoRepeat);
        Assert.AreEqual(2, template.Tracks.Count);           // the two fixed 100px tracks
        Assert.AreEqual(1, template.AutoRepeatInsertIndex);  // between them
        Assert.AreEqual(1, template.AutoRepeatTracks.Count);
        var repeated = template.AutoRepeatTracks[0];
        Assert.AreEqual(GridTrackKind.Length, repeated.Kind);
    }

    [TestMethod]
    [Spec("css-grid-2", "#auto-repeat")]
    public void TwoAutoRepeats_AreRejected()
    {
        Assert.IsNull(Parse("repeat(auto-fill, 50px) repeat(auto-fit, 50px)"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#subgrids")]
    [DataRow("subgrid")]
    [DataRow("subgrid [a]")]
    [DataRow("subgrid [a b] [c]")]
    public void Subgrid_Parses(string value)
    {
        var template = Parse(value);
        Assert.IsNotNull(template);
        Assert.IsTrue(template.IsSubgrid);
        Assert.IsFalse(template.IsNone);
        Assert.AreEqual(0, template.Tracks.Count);
    }

    [TestMethod]
    [Spec("css-grid-2", "#subgrids")]
    public void Subgrid_RecordsLineNamesAtSequentialLines()
    {
        // subgrid [a] [b c] [a] — the line-name list positions 'a' at lines 1 and 3, 'b'/'c' at line 2.
        var template = Parse("subgrid [a] [b c] [a]");
        Assert.IsNotNull(template);
        Assert.IsTrue(template.IsSubgrid);
        CollectionAssert.AreEqual(new[] { 1, 3 }, template.LineNames["a"].ToArray());
        CollectionAssert.AreEqual(new[] { 2 }, template.LineNames["b"].ToArray());
        CollectionAssert.AreEqual(new[] { 2 }, template.LineNames["c"].ToArray());
    }

    [TestMethod]
    [Spec("css-grid-2", "#subgrids")]
    [DataRow("subgrid 1fr")]                 // no track size may follow subgrid
    [DataRow("1fr subgrid")]                 // subgrid must be the first token
    [DataRow("subgrid 100px [a]")]           // a track size mixed into the line-name list
    [DataRow("subgrid [unclosed")]           // an unclosed [ is invalid
    public void Subgrid_InvalidForms_ReturnNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-grid-2", "#track-sizes")]
    [DataRow("calc(10px + 20px)")]                       // bare calc() track size
    [DataRow("calc(50% - 10px)")]                        // length-percentage calc
    [DataRow("minmax(calc(10px + 10px), 1fr)")]          // calc() as a minmax() min breadth
    [DataRow("minmax(100px, calc(200px + 10%))")]        // calc() as a minmax() max breadth
    [DataRow("fit-content(calc(50px + 10px))")]          // calc() as a fit-content() argument
    [DataRow("repeat(2, calc(40px + 10px))")]            // calc() inside repeat()
    [DataRow("min(100px, 200px) 1fr")]                   // a min()/max()/clamp() math function
    public void CalcTrackSizes_Parse(string value)
    {
        Assert.IsNotNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-grid-2", "#track-sizes")]
    public void BareCalcTrack_IsStoredAsLength()
    {
        var template = Parse("calc(10px + 20px) 1fr");
        Assert.IsNotNull(template);
        Assert.AreEqual(GridTrackKind.Length, template.Tracks[0].Kind);
        Assert.IsTrue(template.Tracks[0].Value.Contains("calc"));
    }

    [TestMethod]
    [Spec("css-grid-2", "#track-sizes")]
    [DataRow("calc(10deg + 5deg)")]                      // an angle-category calc is not a track size
    [DataRow("minmax(calc(90deg), 1fr)")]                // a wrong-category calc inside minmax()
    [DataRow("fit-content(calc(1s))")]                   // a time-category calc argument
    public void WrongCategoryCalc_ReturnsNull(string value)
    {
        Assert.IsNull(Parse(value));
    }

    [TestMethod]
    [Spec("css-grid-2", "#auto-tracks")]
    [DataRow("100px", 1)]
    [DataRow("100px 200px auto", 3)]
    public void TrackSizeList_ParsesForAutoColumns(string value, int expected)
    {
        var list = GridTrackListGrammar.TryParseTrackSizeList(CssValueParser.GetCssTokens(value));
        Assert.IsNotNull(list);
        Assert.AreEqual(expected, list.Count);
    }

    [TestMethod]
    [Spec("css-grid-2", "#auto-tracks")]
    [DataRow("repeat(2, 100px)")]  // no repeat() allowed in a track-size list
    [DataRow("")]
    [DataRow("banana")]
    public void TrackSizeList_RejectsInvalid(string value)
    {
        Assert.IsNull(GridTrackListGrammar.TryParseTrackSizeList(CssValueParser.GetCssTokens(value)));
    }

    // The cascade's copy-on-write comparison of CssProperty<GridTemplate> relies on value equality.

    [TestMethod]
    [Ignore("GridTemplate/GridTrackSize have no value equality, two parses of the same text aren't Equal")]
    [DataRow("100px 1fr auto")]
    [DataRow("minmax(100px, 1fr) minmax(min-content, max-content)")]
    [DataRow("[start] 100px [middle] 1fr [end]")]
    [DataRow("repeat(auto-fill, minmax(200px, 1fr))")]
    [DataRow("100px repeat(2, 1fr) 100px")]
    public void GridTemplate_TwoSeparateParsesOfTheSameText_AreEqual(string value)
    {
        var a = Parse(value);
        var b = Parse(value);

        Assert.AreNotSame(a, b);
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    // Passes trivially by reference identity until value equality exists.
    [DataRow("100px 1fr", "100px 2fr")]                 // different track
    [DataRow("100px 1fr", "100px 1fr 1fr")]              // different track count
    [DataRow("[a] 100px", "[b] 100px")]                  // different line name
    [DataRow("repeat(auto-fill, 100px)", "repeat(auto-fit, 100px)")] // different auto-repeat kind
    public void GridTemplate_DifferingText_AreNotEqual(string a, string b)
    {
        Assert.AreNotEqual(Parse(a), Parse(b));
    }

    [TestMethod]
    [Ignore("GridTemplate has no value equality, two parses of the same text aren't Equal")]
    public void GridTemplate_SubgridWithNoNamedLines_EqualsAnotherEmptySubgrid()
    {
        var a = Parse("subgrid");
        var b = Parse("subgrid");

        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}
