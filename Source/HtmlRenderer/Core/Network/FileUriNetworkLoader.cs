#nullable enable

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Core.Network
{
    /// <summary>
    /// An <see cref="RNetworkLoader"/> that serves the root HTML document and every resource it references
    /// (stylesheets, images, fonts) from the local file system via <c>file:</c> URIs. Its
    /// <see cref="BaseUri"/> is set the way a browser sets a local document's base URL: to the loaded
    /// file's own <c>file:</c> URI (so relative references resolve against its directory), or — when no
    /// primary file is given — to the current working directory. <c>file:</c> resources resolve through
    /// this loader automatically regardless of which loader is configured (mirroring how <c>data:</c> URIs
    /// are always handled internally), so relative references in an in-memory HTML string load from disk
    /// by default. Setting <see cref="Adapters.RAdapter.AllowLocalFileAccess"/> to <c>false</c> turns that
    /// off: <c>file:</c> resource requests are then refused even for a document rendered through this
    /// loader, though the root document it was constructed with is still read.
    /// </summary>
    public class FileUriNetworkLoader : RNetworkLoader
    {
        private readonly string? _primaryFilePath;

        /// <summary>
        /// Creates a loader whose base URI is the current working directory, for rendering an in-memory
        /// HTML string whose relative references should resolve against the process's working directory.
        /// <see cref="GetPrimaryContentsAsync"/> is not supported on an instance created this way — pass the HTML
        /// directly instead.
        /// </summary>
        public FileUriNetworkLoader()
        {
            _primaryFilePath = null;
            BaseUri = new RUri(new Uri(EnsureTrailingSeparator(Directory.GetCurrentDirectory())));
        }

        /// <summary>
        /// Creates a loader whose root document is the file at <paramref name="primaryFilePath"/> and whose
        /// base URI is that file's own <c>file:</c> URI, so relative references resolve against the file's
        /// directory — exactly like opening a local <c>.html</c> file in a browser. Pass <c>null</c> as the
        /// HTML argument when rendering to render this file.
        /// </summary>
        /// <param name="primaryFilePath">Path to the root HTML file. Resolved against the current working directory if relative.</param>
        public FileUriNetworkLoader(string primaryFilePath)
        {
            if (primaryFilePath is null) throw new ArgumentNullException(nameof(primaryFilePath));

            _primaryFilePath = Path.GetFullPath(primaryFilePath);
            BaseUri = new RUri(new Uri(_primaryFilePath));
        }

        /// <inheritdoc/>
        public override RUri BaseUri { get; }

        /// <inheritdoc/>
        public override async Task<string> GetPrimaryContentsAsync()
        {
            if (_primaryFilePath is null)
            {
                throw new InvalidOperationException(
                    "This FileUriNetworkLoader was created without a primary file path; construct it with a file path to load the root document, or pass the HTML directly.");
            }

            using (var stream = File.OpenRead(_primaryFilePath))
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public override Task<RNetworkResponse?> GetResourceStreamAsync(RUri uri)
        {
            if (!uri.IsAbsoluteUri || uri.Scheme != "file")
            {
                return Task.FromResult<RNetworkResponse?>(null);
            }

            try
            {
                var path = uri.Uri.LocalPath;

                // A directory or a missing file resolves to "no such resource", matching the fail-soft
                // behavior of the other loaders (a null response rather than an exception).
                if (Directory.Exists(path) || !File.Exists(path))
                {
                    return Task.FromResult<RNetworkResponse?>(null);
                }

                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // Synthesize a Content-Type header so a local file is treated exactly like a fetched
                // network resource (the stylesheet loader's text/css gate, the image loader's
                // image/svg+xml detection).
                var headers = new Dictionary<string, string[]>
                {
                    ["Content-Type"] = new[] { MimeTypeResolver.GetMimeType(path) }
                };

                return Task.FromResult<RNetworkResponse?>(new RNetworkResponse(stream, headers));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                return Task.FromResult<RNetworkResponse?>(null);
            }
        }

        /// <summary>
        /// Ensures a directory path ends with a separator so that resolving a relative reference against
        /// its <c>file:</c> URI keeps the directory as the base (per RFC 3986 relative resolution) instead
        /// of replacing the directory's own last path segment.
        /// </summary>
        private static string EnsureTrailingSeparator(string directory)
        {
            return directory.EndsWith(Path.DirectorySeparatorChar.ToString()) || directory.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                ? directory
                : directory + Path.DirectorySeparatorChar;
        }
    }
}
