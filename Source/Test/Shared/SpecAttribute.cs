namespace HtmlRenderer.TestSupport;

/// <summary>
/// Links a test to the spec section it verifies, e.g. <c>[Spec("css21", "box.html#collapsing-margins")]</c>.
/// The anchor is a section key in <c>Spec/&lt;spec&gt;.yml</c>, the test also counts for its parent sections.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class SpecAttribute(string spec, string anchor) : Attribute
{
    public string Spec { get; } = spec;

    public string Anchor { get; } = anchor;
}
