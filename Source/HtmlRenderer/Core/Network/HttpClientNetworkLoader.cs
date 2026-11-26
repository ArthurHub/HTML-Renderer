#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Core.Network
{
    /// <summary>
    /// An <see cref="RNetworkLoader"/> that fetches the root HTML document and every resource it references
    /// (stylesheets, images, fonts) over HTTP(S) using a caller-supplied <see cref="HttpClient"/>. The
    /// caller controls the <see cref="HttpClient"/>'s lifetime, so custom headers, authentication, proxies,
    /// timeouts, and <see cref="HttpMessageHandler"/> chains are all supported without any library-specific API.
    /// </summary>
    /// <param name="httpClient">The client used to fetch the root document and all referenced resources.</param>
    /// <param name="primaryContentsUri">The URI of the root HTML document; also used as <see cref="BaseUri"/> for resolving relative references.</param>
    public class HttpClientNetworkLoader(HttpClient httpClient, Uri? primaryContentsUri) : RNetworkLoader
    {
        /// <summary>
        /// Creates a loader for the root document at <paramref name="primaryContentsUri"/>.
        /// </summary>
        /// <param name="httpClient">The client used to fetch the root document and all referenced resources.</param>
        /// <param name="primaryContentsUri">The URI of the root HTML document; also used as <see cref="BaseUri"/> for resolving relative references.</param>
        public HttpClientNetworkLoader(HttpClient httpClient, string primaryContentsUri)
            : this(httpClient, new Uri(primaryContentsUri))
        {
        }

        /// <inheritdoc/>
        public override RUri? BaseUri => primaryContentsUri != null ? new RUri(primaryContentsUri) : null;

        /// <inheritdoc/>
        public override async Task<string> GetPrimaryContentsAsync()
        {
            if (BaseUri is null)
            {
                throw new InvalidOperationException("Primary contents URL is not set.");
            }

            var networkResponse = await GetResourceStreamAsync(BaseUri).ConfigureAwait(false);

            if (networkResponse?.ResourceStream is null)
            {
                throw new InvalidOperationException("Primary contents stream is null.");
            }

            using (var streamReader = new StreamReader(networkResponse.ResourceStream))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public override async Task<RNetworkResponse?> GetResourceStreamAsync(RUri uri)
        {
            var response = await httpClient.GetAsync(uri.Uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var headers = response.Headers
                .Union(response.Content.Headers)
                .ToDictionary(x => x.Key, x => x.Value.ToArray());

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return new RNetworkResponse(stream, headers);
        }
    }
}
