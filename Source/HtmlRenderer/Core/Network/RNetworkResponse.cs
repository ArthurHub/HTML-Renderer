#nullable enable
using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.Network
{
    /// <summary>
    /// The result of resolving a resource URI via <see cref="RNetworkLoader.GetResourceStreamAsync"/>.
    /// </summary>
    /// <param name="ResourceStream">The resource's content stream, or <c>null</c> if the resource has no body.</param>
    /// <param name="ResponseHeaders">
    /// HTTP-style response headers for the resource, if any. For stylesheet resources, this is inspected
    /// for a <c>Content-Type</c> header and the body is only accepted as CSS when it is <c>text/css</c>.
    /// </param>
    public record RNetworkResponse(Stream? ResourceStream, Dictionary<string, string[]>? ResponseHeaders);
}
