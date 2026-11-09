# Spec compliance

**tl;dr:** `<spec>.yml` says per spec section how well HTML Renderer implements it, `[Spec]` on a test says which section it verifies, and `SpecComplianceTests` keeps the two in sync. The report shows up in the summary of the Windows .NET 8 CI job.

## Manifest

`<spec>.yml` has the spec's `title` and `url`, and one entry per section, keyed by its anchor, with the numbered title, the `status` and, if anything is missing, the `gaps`. Every section of the spec is listed, except the subsections of an `n/a` section, so the report also shows what hasn't been reviewed yet.

Statuses:

- `supported`: implemented as specified, needs at least one test
- `partial`: implemented with known gaps, needs at least one test and the gaps listed on it or its subsections
- `parsed-only`: the CSS engine accepts it, but layout and painting ignore it
- `unsupported`: not implemented
- `n/a`: nothing to implement, e.g. informative examples, it covers the subsections, they're only listed with a status of their own
- `unknown`: not reviewed yet

## Linking tests

```csharp
[TestMethod]
[Spec("css21", "box.html#collapsing-margins")]
public void NegativeSiblingMargins_CollapseToTheMostNegative()
```

The anchor is a section key from the manifest, the test counts for that section and all its parent sections. An `[Ignore]`d test doesn't count, use one to describe a gap with `[Ignore("Gap, see Spec/css21.yml")]`.

Keep the spec out of code comments, the manifest says what's supported, the tests show how, and the spec explains why. A comment cites the section ("CSS 2.1 §8.3.1") and explains what the code can't.

## Commands

Run from the repository root:

```sh
# Validate manifests and [Spec] references, same as SpecComplianceTests
dotnet run --project Source/Tools/HtmlRenderer.SpecCompliance -- check

# Write the report
dotnet run --project Source/Tools/HtmlRenderer.SpecCompliance -- report --output report.md
```
