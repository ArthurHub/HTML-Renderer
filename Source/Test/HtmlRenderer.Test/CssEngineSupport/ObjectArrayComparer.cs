using System.Collections;
using System.Collections.Generic;

namespace HtmlRenderer.Test.CssEngineSupport;

/// <summary>
/// Used to de-duplicate <c>object[]</c> test-case rows
/// (e.g. when unioning a keyword list with the shared CSS-wide-keyword list) by comparing their contents
/// rather than reference identity.
/// </summary>
internal sealed class ObjectArrayComparer : IEqualityComparer, IEqualityComparer<object[]>
{
    public static readonly ObjectArrayComparer Instance = new();

    public bool Equals(object[]? x, object[]? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        if (x.Length != y.Length)
            return false;

        var comparer = EqualityComparer<object>.Default;
        for (var i = 0; i < x.Length; i++)
        {
            if (!comparer.Equals(x[i], y[i])) return false;
        }
        return true;
    }

    public int GetHashCode(object[]? obj)
    {
        if (obj == null || obj.Length == 0)
            return 0;

        var hash = obj[0]?.GetHashCode() ?? 0;
        var comparer = EqualityComparer<object>.Default;
        for (var i = 1; i < obj.Length; i++)
        {
            var temp = (uint)(hash << 5) | ((uint)hash >> 27);
            hash = ((int)temp + hash) ^ (obj[i]?.GetHashCode() ?? 0);
        }
        return hash;
    }

    bool IEqualityComparer.Equals(object? x, object? y)
    {
        if (x == y)
            return true;
        if (x == null || y == null)
            return false;
        if (x is object[] xArr && y is object[] yArr)
            return Equals(xArr, yArr);

        throw new System.ArgumentException("Type of argument is not compatible with this comparer.");
    }

    int IEqualityComparer.GetHashCode(object? obj)
    {
        if (obj == null)
            return 0;
        if (obj is object[] arr)
            return GetHashCode(arr);

        throw new System.ArgumentException("Type of argument is not compatible with this comparer.");
    }
}
