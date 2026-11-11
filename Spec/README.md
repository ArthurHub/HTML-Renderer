# Spec compliance

**tl;dr:** `<spec>.yml` says per spec section how well HTML Renderer implements it, `[Spec]` on a test says which section it verifies, and `SpecComplianceTests` keeps the two in sync. The overview table shows up in the summary of the Windows .NET 8 CI job, the full report is its "Spec compliance report" artifact.

## Manifest

`<spec>.yml` has the spec's `title` and `url`, and one entry per section, keyed by its anchor, with the numbered title, the `status` and, if anything is missing, the `gaps`. Every section of the spec is listed, except the subsections of an `n/a` section, so the report also shows what hasn't been reviewed yet.

Each CSS module level has its own manifest, named like its /TR URL, e.g. `css-color-4.yml`. Where one level replaces the previous, only the newer one is tracked (`css-cascade-5`, `selectors-4`), a level that only adds to the previous one is tracked next to it (`css-color-4` and `css-color-5`). A module is a single page, so its sections are keyed by the bare anchor, e.g. `#break-between`.

The specs that change without a new name have a `snapshot`, the version their sections were checked against: for HTML, a living standard, the commit of [whatwg/html](https://github.com/whatwg/html), for a CSS module the dated URL of the published draft. When the snapshot moves, the sections that changed in between need a recheck.

Statuses:

- `supported`: implemented as specified, needs at least one test
- `partial`: implemented with known gaps, needs at least one test and the gaps listed on it or its subsections
- `parsed-only`: accepted, e.g. a CSS property or an HTML attribute, but layout and painting ignore it
- `unsupported`: not implemented
- `n/a`: nothing for a renderer to implement, e.g. informative examples, authoring requirements or DOM interfaces, it covers the subsections, they're only listed with a status of their own
- `unknown`: not reviewed yet

HTML spreads a feature over several sections: the element in chapter 4, how it's parsed in 13.2 and how it's rendered by default in 15. A gap goes to the section that states the requirement, e.g. a missing default style to 15. The element's own section describes the element as a whole, if it doesn't work at all, its gap points to where the cause is tracked ("Unknown element, rendered inline, see 15.3.3").

## Linking tests

```csharp
[TestMethod]
[Spec("css21", "box.html#collapsing-margins")]
public void NegativeSiblingMargins_CollapseToTheMostNegative()
```

The anchor is a section key from the manifest, the test counts for that section and all its parent sections. An `[Ignore]`d test doesn't count, use one to describe a gap with `[Ignore("Gap, see Spec/css21.yml")]`. A test can carry several `[Spec]`, e.g. when its `[DataRow]`s cover different elements, or `[Spec("css21", "page.html#page-break-props")]` together with `[Spec("css-break-3", "#page-break-properties")]` when a CSS module restates a CSS 2.1 section.

Keep the spec out of code comments, the manifest says what's supported, the tests show how, and the spec explains why. A comment cites the section ("CSS 2.1 §8.3.1", "HTML §4.4.8") and explains what the code can't.

## Commands

Run from the repository root:

```sh
# Validate manifests and [Spec] references, same as SpecComplianceTests
dotnet run --project Source/Tools/HtmlRenderer.SpecCompliance -- check

# Write the report, --summary <file> also appends the overview table to a file, e.g. the job summary in CI
dotnet run --project Source/Tools/HtmlRenderer.SpecCompliance -- report --output report.md
```
