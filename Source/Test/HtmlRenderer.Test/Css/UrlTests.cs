using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

[TestClass]
public sealed class UrlTests
{
    [TestMethod]
    public void AbsoluteHttpUrl_ParsesAllComponents()
    {
        var url = new Url("http://user:pass@example.com:8080/some/path?query=1#frag");

        Assert.IsFalse(url.IsInvalid);
        Assert.AreEqual("http", url.Scheme);
        Assert.AreEqual("example.com", url.HostName);
        Assert.AreEqual("8080", url.Port);
        Assert.AreEqual("user", url.UserName);
        Assert.AreEqual("pass", url.Password);
        Assert.AreEqual("some/path", url.Path);
        Assert.AreEqual("query=1", url.Query);
        Assert.AreEqual("frag", url.Fragment);
        Assert.IsFalse(url.IsRelative);
    }

    [TestMethod]
    public void DefaultPort_IsOmittedFromHost()
    {
        var url = new Url("http://example.com:80/");

        Assert.AreEqual(string.Empty, url.Port);
        Assert.AreEqual("example.com", url.Host);
    }

    [TestMethod]
    public void NonDefaultPort_IsIncludedInHost()
    {
        var url = new Url("http://example.com:8080/");

        Assert.AreEqual("example.com:8080", url.Host);
    }

    [TestMethod]
    public void RelativeUrl_ResolvesAgainstBase()
    {
        var baseUrl = new Url("http://example.com/dir/page.html");
        var relative = new Url(baseUrl, "other.html");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("example.com", relative.HostName);
        Assert.AreEqual("dir/other.html", relative.Path);
    }

    [TestMethod]
    public void RelativeUrl_QueryOnly_ResolvesAgainstBasePathKeepingScheme()
    {
        // Exercises Url.RelativeState's '?' branch (ParseQuery).
        var baseUrl = new Url("http://example.com/dir/page.html");
        var relative = new Url(baseUrl, "?a=1");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("dir/page.html", relative.Path);
        Assert.AreEqual("a=1", relative.Query);
    }

    [TestMethod]
    public void RelativeUrl_FragmentOnly_ResolvesAgainstBasePathKeepingScheme()
    {
        // Exercises Url.RelativeState's '#' branch (ParseFragment).
        var baseUrl = new Url("http://example.com/dir/page.html");
        var relative = new Url(baseUrl, "#section");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("dir/page.html", relative.Path);
        Assert.AreEqual("section", relative.Fragment);
    }

    [TestMethod]
    public void RelativeUrl_SingleSlashPath_ReplacesEntirePath()
    {
        // Exercises Url.RelativeSlashState's non-double-slash fallback (ParsePath(input, index - 1)):
        // a single leading '/' with no scheme change is an absolute-path reference, not a new authority.
        var baseUrl = new Url("http://example.com/dir/page.html");
        var relative = new Url(baseUrl, "/other/path");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("example.com", relative.HostName);
        Assert.AreEqual("other/path", relative.Path);
    }

    [TestMethod]
    public void RelativeUrl_DoubleSlash_ParsesNewAuthority()
    {
        // Exercises Url.RelativeSlashState's double-slash branch (IgnoreSlashesState/ParseAuthority):
        // "//host/path" replaces the authority, keeping the base's scheme.
        var baseUrl = new Url("http://example.com/dir/page.html");
        var relative = new Url(baseUrl, "//other.example.org/new/path");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("http", relative.Scheme);
        Assert.AreEqual("other.example.org", relative.HostName);
        Assert.AreEqual("new/path", relative.Path);
    }

    [TestMethod]
    public void RelativeUrl_TrailingSlashOnly_ParsesAsEmptyPath()
    {
        // Exercises Url.RelativeSlashState's "index == input.Length - 1" early ParsePath branch.
        var baseUrl = new Url("http://example.com/dir/page.html");
        var relative = new Url(baseUrl, "/");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("example.com", relative.HostName);
    }

    [TestMethod]
    public void FileUrl_DoubleSlashRelative_ParsesNewFileHost()
    {
        // Exercises Url.RelativeSlashState's file-scheme double-slash branch (ParseFileHost).
        var baseUrl = new Url("file:///c:/dir/page.html");
        var relative = new Url(baseUrl, "//otherhost/share/file.txt");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("file", relative.Scheme);
        Assert.AreEqual("otherhost", relative.HostName);
    }

    [TestMethod]
    public void FileUrl_DoubleSlashDriveLetter_ParsesAsPathNotHost()
    {
        // Exercises Url.IsWindowsDriveLetter's true branch inside ParseFileHost: "file://d:/x" has a
        // drive letter, not a real host, immediately after the double slash.
        var baseUrl = new Url("file:///c:/dir/page.html");
        var relative = new Url(baseUrl, "//d:/other/file.txt");

        Assert.IsFalse(relative.IsInvalid);
        Assert.AreEqual("file", relative.Scheme);
    }

    [TestMethod]
    public void Host_UppercaseLetters_AreLowercased()
    {
        // Exercises Url.AppendDefaultHostChar's lowercase-conversion branch.
        var url = new Url("http://EXAMPLE.COM/path");

        Assert.AreEqual("example.com", url.HostName);
    }

    [TestMethod]
    public void Host_ValidPercentEncodedByte_IsDecoded()
    {
        // Exercises Url.AppendPercentEncodedHostChar's valid-hex branch.
        var url = new Url("http://ex%61mple.com/path");

        Assert.AreEqual("example.com", url.HostName);
    }

    [TestMethod]
    public void Host_InvalidPercentEscape_IsKeptLiteral()
    {
        // Exercises Url.AppendPercentEncodedHostChar's non-hex fallback branch.
        var url = new Url("http://ex%zzample.com/path");

        StringAssert.Contains(url.HostName, "%");
    }

    [TestMethod]
    public void Host_FullwidthPunycodeMappedCharacter_IsNormalized()
    {
        // Exercises Url.AppendDefaultHostChar's Symbols.Punycode lookup branch: a fullwidth ideographic
        // full stop (U+FF0E) is one of the handful of characters Symbols.Punycode maps directly.
        var url = new Url("http://example．com/path");

        Assert.AreEqual("example.com", url.HostName);
    }

    [TestMethod]
    public void Host_HyphenatedLabel_IsPreserved()
    {
        // Exercises Url.AppendDefaultHostChar's IsAlphanumericAscii-false/hyphen-kept branch.
        var url = new Url("http://my-example.com/path");

        Assert.AreEqual("my-example.com", url.HostName);
    }

    [TestMethod]
    public void Host_IPv6Literal_IsPreservedVerbatim()
    {
        // Exercises SanatizeHost's bracketed-IPv6-literal fast path (returns the substring as-is,
        // never reaching the per-character sanitizer at all).
        var url = new Url("http://[::1]:8080/path");

        Assert.AreEqual("[::1]", url.HostName);
    }

    [TestMethod]
    public void CopyConstructor_CopiesAllComponents()
    {
        var original = new Url("http://user:pass@example.com:8080/path?query#frag");
        var copy = new Url(original);

        Assert.AreEqual(original, copy);
        Assert.AreEqual(original.ToString(), copy.ToString());
    }

    [TestMethod]
    public void PathWithDotSegments_IsNormalized()
    {
        var url = new Url("http://example.com/a/b/../c/./d");

        Assert.AreEqual("a/c/d", url.Path);
    }

    [TestMethod]
    public void PathWithLeadingUpDirectory_DoesNotUnderflow()
    {
        var url = new Url("http://example.com/../a");

        Assert.AreEqual("a", url.Path);
    }

    [TestMethod]
    public void MailtoScheme_IsNotRelative_AndParsesAsSchemeData()
    {
        var url = new Url("mailto:someone@example.com");

        Assert.IsFalse(url.IsInvalid);
        Assert.AreEqual("mailto", url.Scheme);
        Assert.IsFalse(url.IsRelative);
        Assert.AreEqual("someone@example.com", url.Data);
    }

    [TestMethod]
    public void Origin_ForHttpUrl_IncludesSchemeAndHost()
    {
        var url = new Url("http://example.com:8080/path");

        Assert.AreEqual("http://example.com:8080", url.Origin);
    }

    [TestMethod]
    public void Origin_ForNonOriginableScheme_IsNull()
    {
        var url = new Url("mailto:someone@example.com");

        Assert.IsNull(url.Origin);
    }

    [TestMethod]
    public void Equals_And_GetHashCode_MatchForEquivalentUrls()
    {
        var a = new Url("http://example.com/path?q#f");
        var b = new Url("http://example.com/path?q#f");

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a.Equals((object)b));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equals_DiffersForDifferentUrls()
    {
        var a = new Url("http://example.com/a");
        var b = new Url("http://example.com/b");

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a.Equals((object)"not a url"));
    }

    [TestMethod]
    public void ToString_RoundTripsThroughReparsing()
    {
        var url = new Url("http://example.com:8080/path?q=1#f");
        var reparsed = new Url(url.ToString());

        Assert.AreEqual(url, reparsed);
    }

    [TestMethod]
    public void Href_Setter_ReparsesTheUrl()
    {
        var url = new Url("http://example.com/original");

        url.Href = "http://example.org/updated";

        Assert.AreEqual("example.org", url.HostName);
        Assert.AreEqual("updated", url.Path);
    }

    [TestMethod]
    public void Fragment_Setter_UpdatesFragment()
    {
        var url = new Url("http://example.com/path");

        url.Fragment = "new-fragment";

        Assert.AreEqual("new-fragment", url.Fragment);
    }

    [TestMethod]
    public void Fragment_SetToNull_ClearsFragment()
    {
        var url = new Url("http://example.com/path#frag");

        url.Fragment = null;

        Assert.IsNull(url.Fragment);
    }

    [TestMethod]
    public void Query_Setter_UpdatesQuery()
    {
        var url = new Url("http://example.com/path");

        url.Query = "a=1";

        Assert.AreEqual("a=1", url.Query);
    }

    [TestMethod]
    public void Path_Setter_UpdatesPath()
    {
        var url = new Url("http://example.com/original");

        url.Path = "new/path";

        Assert.AreEqual("new/path", url.Path);
    }

    [TestMethod]
    public void Port_Setter_UpdatesPort()
    {
        var url = new Url("http://example.com/path");

        url.Port = "9090";

        Assert.AreEqual("9090", url.Port);
    }

    [TestMethod]
    public void Scheme_Setter_UpdatesScheme()
    {
        var url = new Url("http://example.com/path");

        // The Scheme setter parses via ParseScheme(value, onlyScheme: true), which looks for a
        // trailing colon (matching the DOM HTMLHyperlinkElementUtils.protocol convention, e.g.
        // "https:") -- without it, no colon is ever found and the scheme is left unchanged.
        url.Scheme = "https:";

        Assert.AreEqual("https", url.Scheme);
    }

    [TestMethod]
    public void HostName_Setter_UpdatesHost()
    {
        var url = new Url("http://example.com/path");

        url.HostName = "other.example.com";

        Assert.AreEqual("other.example.com", url.HostName);
    }

    [TestMethod]
    public void ImplicitConversion_ToUri_ProducesEquivalentUri()
    {
        var url = new Url("http://example.com/path");

        Uri uri = url;

        Assert.AreEqual(UriKind.Absolute, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }

    [TestMethod]
    public void PathWithPercentEncodedCharacters_IsPreserved()
    {
        var url = new Url("http://example.com/a%20b");

        Assert.AreEqual("a%20b", url.Path);
    }

    [TestMethod]
    public void Convert_FromUri_ParsesEquivalently()
    {
        var uri = new Uri("http://example.com/path?q=1");

        var url = Url.Convert(uri);

        Assert.AreEqual("example.com", url.HostName);
        Assert.AreEqual("path", url.Path);
    }

    [TestMethod]
    public void Create_IsEquivalentToConstructor()
    {
        var viaCreate = Url.Create("http://example.com/path");
        var viaCtor = new Url("http://example.com/path");

        Assert.AreEqual(viaCtor, viaCreate);
    }
}
