using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// <see cref="CssLength"/> only knows em, ex, px, mm, cm, in, pt, pc and rem, a percentage is tracked through
/// <c>IsPercentage</c> as a fraction.
/// </summary>
[TestClass]
public sealed class LengthTests
{
    [TestMethod]
    public void Constructor_SetsValueAndType()
    {
        var length = new CssLength("10px");

        Assert.AreEqual(10d, length.Number);
        Assert.AreEqual(CssUnit.Pixels, length.Unit);
    }

    [TestMethod]
    [DataRow("em", "Ems")]
    [DataRow("ex", "Ex")]
    [DataRow("px", "Pixels")]
    [DataRow("mm", "Milimeters")]
    [DataRow("cm", "Centimeters")]
    [DataRow("in", "Inches")]
    [DataRow("pt", "Points")]
    [DataRow("pc", "Picas")]
    [DataRow("rem", "Rems")]
    [DataRow("bogus", "None")]
    public void Constructor_ParsesKnownUnitSuffixes(string suffix, string expectedUnitName)
    {
        // CssUnit is internal, a public test method can't take it as a parameter
        var length = new CssLength("1" + suffix);

        Assert.AreEqual(expectedUnitName, length.Unit.ToString());
    }

    [TestMethod]
    public void Constructor_ValidLength_ParsesSuccessfully()
    {
        var length = new CssLength("10px");

        Assert.IsFalse(length.HasError);
        Assert.AreEqual(10d, length.Number);
        Assert.AreEqual(CssUnit.Pixels, length.Unit);
    }

    [TestMethod]
    public void Constructor_ZeroWithoutUnit_ReturnsZeroLength()
    {
        var length = new CssLength("0");

        Assert.IsFalse(length.HasError);
        Assert.AreEqual(0d, length.Number);
        Assert.AreEqual(CssUnit.None, length.Unit);
    }

    [TestMethod]
    public void Constructor_NonZeroValueWithUnrecognizedUnit_HasError()
    {
        var length = new CssLength("10bogus");

        Assert.IsTrue(length.HasError);
    }

    [TestMethod]
    [DataRow("not-a-length")]
    [DataRow("abc")]
    public void Constructor_NonNumericInput_HasError(string input)
    {
        var length = new CssLength(input);

        Assert.IsTrue(length.HasError);
    }

    [TestMethod]
    public void Constructor_UnitlessNonZero_HasError()
    {
        // CSS Values & Units §5.1: only zero may omit its unit.
        var length = new CssLength("5");

        Assert.IsTrue(length.HasError);
    }

    [TestMethod]
    public void ConvertEmToPoints_NonEmUnit_Throws()
    {
        var length = new CssLength("1px");

        Assert.IsTrue(length.IsRelative);
        Assert.ThrowsExactly<InvalidOperationException>(() => length.ConvertEmToPoints(12));
    }

    [TestMethod]
    public void ConvertEmToPixels_UsesTheEmFactor()
    {
        var length = new CssLength("2em");

        var result = length.ConvertEmToPixels(12);

        Assert.AreEqual(CssUnit.Pixels, result.Unit);
        Assert.AreEqual(24d, result.Number);
    }

    [TestMethod]
    public void ToString_Zero_OmitsUnit()
    {
        var length = new CssLength("0");

        Assert.AreEqual("0", length.ToString());
    }

    [TestMethod]
    public void ToString_NonZero_IncludesUnit()
    {
        var length = new CssLength("10px");

        Assert.AreEqual("10px", length.ToString());
    }

    [TestMethod]
    [DataRow("1pt", false)]
    [DataRow("1in", false)]
    [DataRow("1cm", false)]
    [DataRow("1mm", false)]
    [DataRow("1pc", false)]
    [DataRow("1em", true)]
    [DataRow("1ex", true)]
    public void IsRelative_MatchesUnitCategory(string lengthText, bool expectedIsRelative)
    {
        var length = new CssLength(lengthText);

        Assert.AreEqual(expectedIsRelative, length.IsRelative);
    }

    [TestMethod]
    [Ignore("CssLength.IsRelative is an internal flag that is also true for px")]
    public void IsRelative_Pixels_IsFalse()
    {
        var length = new CssLength("1px");

        Assert.IsFalse(length.IsRelative);
    }

    [TestMethod]
    public void Rem_ResolvesAgainstTheRootFontSize()
    {
        var x = LayoutRem("html{font-size:10px} #x{font-size:20px;margin-left:2rem;padding-left:calc(1rem);text-indent:3rem;height:4rem;border-left:0.5rem solid}");

        Assert.AreEqual(20, x.ActualMarginLeft, 0.01);
        Assert.AreEqual(10, x.ActualPaddingLeft, 0.01);
        Assert.AreEqual(30, x.ActualTextIndent, 0.01);
        Assert.AreEqual(40, x.Size.Height, 0.01);
        Assert.AreEqual(5, x.ActualBorderLeftWidth, 0.01);
    }

    [TestMethod]
    public void Rem_InFontSize_ResolvesAgainstTheRootFontSize()
    {
        var x = LayoutRem("html{font-size:10px} body{font-size:30px} #x{font-size:1.5rem}");

        Assert.AreEqual(15, x.GetEmHeight(), 0.01);
    }

    [TestMethod]
    public void Rem_InTheRootFontSize_IsTheInitialFontSize()
    {
        var x = LayoutRem("html{font-size:2rem} #x{margin-left:1rem}");
        var initial = 11d * 96d / 72d;

        Assert.AreEqual(2 * initial, x.GetEmHeight(), 0.01);
        Assert.AreEqual(2 * initial, x.ActualMarginLeft, 0.01);
    }

    private static CssBox LayoutRem(string css)
    {
        var (root, _) = LayoutHarness.Layout($"<html><head><style>{css}</style></head><body><div id=x>a</div></body></html>");
        var x = LayoutHarness.FindById(root, "x");
        Assert.IsNotNull(x);
        return x!;
    }
}
