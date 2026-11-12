using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class TransformMatrixTests
{
    [TestMethod]
    public void Zero_HasAllZeroComponents()
    {
        Assert.AreEqual(0f, TransformMatrix.Zero.Tx);
        Assert.AreEqual(0f, TransformMatrix.Zero.Ty);
        Assert.AreEqual(0f, TransformMatrix.Zero.Tz);
    }

    [TestMethod]
    public void One_IsIdentityLikeMatrix()
    {
        Assert.AreEqual(0f, TransformMatrix.One.Tx);
        Assert.AreEqual(0f, TransformMatrix.One.Ty);
        Assert.AreEqual(0f, TransformMatrix.One.Tz);
    }

    [TestMethod]
    public void Constructor_16Values_SetsTranslationComponents()
    {
        var values = new float[16];
        // Column-major 4x4: translation lives in the 4th column (indices 12,13,14).
        values[12] = 1f;
        values[13] = 2f;
        values[14] = 3f;
        values[15] = 1f;

        var matrix = new TransformMatrix(values);

        Assert.AreEqual(1f, matrix.Tx);
        Assert.AreEqual(2f, matrix.Ty);
        Assert.AreEqual(3f, matrix.Tz);
    }

    [TestMethod]
    public void Constructor_16Values_Null_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new TransformMatrix(null!));
    }

    [TestMethod]
    public void Constructor_16Values_WrongLength_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new TransformMatrix(new float[4]));
    }

    [TestMethod]
    public void Constructor_Explicit_SetsTranslationComponents()
    {
        var matrix = new TransformMatrix(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1,
            10, 20, 30,
            0, 0, 0);

        Assert.AreEqual(10f, matrix.Tx);
        Assert.AreEqual(20f, matrix.Ty);
        Assert.AreEqual(30f, matrix.Tz);
    }

    [TestMethod]
    public void Equals_SameComponents_AreEqual()
    {
        var a = new TransformMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1, 5, 6, 7, 0, 0, 0);
        var b = new TransformMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1, 5, 6, 7, 0, 0, 0);

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a.Equals((object)b));
    }

    [TestMethod]
    public void Equals_DifferentComponents_AreNotEqual()
    {
        var a = new TransformMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1, 5, 6, 7, 0, 0, 0);
        var b = new TransformMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1, 5, 6, 8, 0, 0, 0);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a.Equals((object)"not a matrix"));
    }

    [TestMethod]
    public void GetHashCode_SameForEqualMatrices()
    {
        var a = new TransformMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1, 5, 6, 7, 0, 0, 0);
        var b = new TransformMatrix(1, 0, 0, 0, 1, 0, 0, 0, 1, 5, 6, 7, 0, 0, 0);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}
