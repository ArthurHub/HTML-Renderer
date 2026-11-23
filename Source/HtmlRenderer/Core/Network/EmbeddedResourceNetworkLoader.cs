#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Core.Network
{
    /// <summary>
    /// Resolves <c>embedded://&lt;assemblySimpleName&gt;/&lt;manifestResourceName&gt;</c> URIs against a
    /// loaded .NET assembly's own embedded resources (<see cref="Assembly.GetManifestResourceStream(string)"/>).
    /// The assembly to search is named directly in the URI's authority rather than configured on the
    /// loader instance, so - like <c>data:</c>/<c>file:</c> - this always resolves the same way regardless
    /// of which <see cref="RNetworkLoader"/> is configured on <see cref="Adapters.RAdapter.NetworkLoader"/>:
    /// <see cref="Adapters.RAdapter.GetResourceStreamAsync"/> intercepts this scheme unconditionally, the same
    /// way it already does for <c>data:</c>/<c>file:</c>.
    /// </summary>
    /// <remarks>
    /// Useful for a library/app that ships its own bundled resources (fonts, images, stylesheets) as
    /// embedded resources and wants <c>@font-face src: url(...)</c>/<c>&lt;img src&gt;</c>/etc. to
    /// reference them directly, without a custom <see cref="RNetworkLoader"/> of its own.
    /// </remarks>
    public sealed class EmbeddedResourceNetworkLoader : RNetworkLoader
    {
        public const string Scheme = "embedded";

        private readonly RUri? _primaryContentsUri;

        /// <summary>
        /// Creates a loader with no primary document - it only resolves <c>embedded:</c> URIs it's asked
        /// for via <see cref="GetResourceStreamAsync"/> (which is how <see cref="Adapters.RAdapter"/>'s internal,
        /// always-on instance of this loader is used). <see cref="GetPrimaryContentsAsync"/> is not supported on
        /// an instance created this way — pass the HTML directly instead, or use the other constructor.
        /// </summary>
        public EmbeddedResourceNetworkLoader()
        {
        }

        /// <summary>
        /// Creates a loader whose root document is the embedded resource named
        /// <paramref name="manifestResourceName"/> in <paramref name="assembly"/>, and whose base URI is
        /// that resource's own <c>embedded:</c> URI (so relative references resolve against it). Pass
        /// <c>null</c> as the HTML argument when rendering to render this resource.
        /// </summary>
        public EmbeddedResourceNetworkLoader(Assembly assembly, string manifestResourceName)
        {
            _primaryContentsUri = BuildUri(assembly, manifestResourceName);
        }

        /// <summary>Builds an <c>embedded:</c> URI for <paramref name="manifestResourceName"/> in <paramref name="assembly"/>.</summary>
        public static RUri BuildUri(Assembly assembly, string manifestResourceName)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));
            if (manifestResourceName is null) throw new ArgumentNullException(nameof(manifestResourceName));

            return new RUri($"{Scheme}://{assembly.GetName().Name}/{manifestResourceName}");
        }

        /// <inheritdoc/>
        public override RUri? BaseUri => _primaryContentsUri;

        /// <inheritdoc/>
        public override async Task<string> GetPrimaryContentsAsync()
        {
            if (_primaryContentsUri is null)
            {
                throw new InvalidOperationException(
                    "This EmbeddedResourceNetworkLoader was created without a primary resource; construct it with an assembly and manifest resource name to load the root document, or pass the HTML directly.");
            }

            var networkResponse = await GetResourceStreamAsync(_primaryContentsUri).ConfigureAwait(false);

            if (networkResponse?.ResourceStream is null)
            {
                throw new InvalidOperationException("Primary contents stream is null.");
            }

            using (var reader = new StreamReader(networkResponse.ResourceStream))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public override Task<RNetworkResponse?> GetResourceStreamAsync(RUri uri)
        {
            var assemblyName = uri.Uri.Host;
            var resourceName = uri.Uri.AbsolutePath.TrimStart('/');

            var assembly = FindOrLoadAssembly(assemblyName);
            var stream = assembly?.GetManifestResourceStream(resourceName);

            return Task.FromResult(stream != null ? new RNetworkResponse(stream, null) : null);
        }

        /// <summary>
        /// Finds <paramref name="assemblyName"/> among already-loaded assemblies first (cheap, and the
        /// common case - the assembly that built this URI via <see cref="BuildUri"/> is, by construction,
        /// already loaded), falling back to <see cref="Assembly.Load(AssemblyName)"/> - which actively
        /// resolves and loads the assembly from the app's own probing paths - for the case this URI came
        /// from parsed CSS/HTML text (e.g. an <c>@font-face src: url(embedded://...)</c> written directly
        /// into a bundled sample) rather than <see cref="BuildUri"/>, where nothing else in the calling
        /// process may have referenced a type from that assembly yet to trigger .NET's normal lazy load.
        /// </summary>
        private static Assembly? FindOrLoadAssembly(string assemblyName)
        {
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));
            if (loaded != null)
            {
                return loaded;
            }

            try
            {
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch
            {
                return null;
            }
        }
    }
}
