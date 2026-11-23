#nullable enable
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Core.Network
{
    /// <summary>
    /// Controls how the root HTML document and every external resource it references (stylesheets,
    /// images, <c>@font-face</c> fonts) is loaded. Set an instance on <see cref="Adapters.RAdapter.NetworkLoader"/>
    /// to integrate with a custom resource source (a cloud blob store, a bundler manifest, an in-memory
    /// dictionary, etc.). Three concrete implementations are provided: <see cref="DataUriNetworkLoader"/>
    /// (the default when none is configured), <see cref="FileUriNetworkLoader"/> for local files, and
    /// <see cref="HttpClientNetworkLoader"/> for HTTP(S) sources. <c>data:</c> URIs are always handled
    /// internally regardless of which loader is configured, as are <c>file:</c> URIs unless
    /// <see cref="Adapters.RAdapter.AllowLocalFileAccess"/> is set to <c>false</c>, which refuses them outright.
    /// </summary>
    public abstract class RNetworkLoader
    {
        /// <summary>
        /// Returns the root HTML document as a string. Called once at the start of rendering when no HTML
        /// string is given directly to the render/generate call.
        /// </summary>
        public abstract Task<string> GetPrimaryContentsAsync();

        /// <summary>
        /// Returns the content of an external resource (a stylesheet, image, or font) referenced by the
        /// document, or <c>null</c> if the resource cannot be resolved.
        /// </summary>
        /// <param name="uri">The resource URI, resolved against <see cref="BaseUri"/> or a <c>&lt;base href&gt;</c> element if relative.</param>
        public abstract Task<RNetworkResponse?> GetResourceStreamAsync(RUri uri);

        /// <summary>
        /// The document's base URL, used to resolve relative <c>href</c>, <c>src</c>, and CSS <c>url()</c>
        /// references. If <c>null</c>, relative references are resolved against the local file system.
        /// </summary>
        public abstract RUri? BaseUri { get; }
    }
}
