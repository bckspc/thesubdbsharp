namespace TheSubDBSharp.Client
{
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    /// <summary>
    /// The <see cref="SubDBClient"/> class.
    /// </summary>
    /// <seealso cref="TheSubDBSharp.Client.ISubDBClient" />
    /// <seealso cref="System.IDisposable" />
    public class SubDBClient : ISubDBClient
    {
        /// <summary>
        /// The user agent format
        /// </summary>
        private const string UserAgentFormat = "{0}/{1} ({2}/{3}; {4}";

        /// <summary>
        /// The protocol name
        /// </summary>
        private static string protocolName = "SubDB";

        /// <summary>
        /// The protocol version
        /// </summary>
        private static string protocolVersion = "1.0";

        /// <summary>
        /// The base URL
        /// </summary>
        private readonly Uri baseUrl;

        /// <summary>
        /// The user agent
        /// </summary>
        private readonly string userAgent;

        /// <summary>
        /// The rest client
        /// </summary>
        private Lazy<IRestClient> restClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubDBClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="clientUrl">The client URL.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "Client url can be an invalid url")]
        public SubDBClient(string baseUrl, string clientName, string clientVersion, string clientUrl)
            : this(new Uri(baseUrl), clientName, clientVersion, clientUrl)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubDBClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="clientUrl">The client URL.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "Client url can be an invalid url")]
        public SubDBClient(Uri baseUrl, string clientName, string clientVersion, string clientUrl)
        {
            this.baseUrl = baseUrl;
            this.userAgent = string.Format(UserAgentFormat, protocolName, protocolVersion, clientName, clientVersion, clientUrl);
            this.restClient = new Lazy<IRestClient>(() => new RestClient(this.baseUrl) { UserAgent = this.userAgent });
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        protected IRestClient RestClient
        {
            get
            {
                return this.restClient.Value;
            }
        }

        /// <summary>
        /// Gets the languages.
        /// </summary>
        /// <returns>
        /// A list of available languages or <c>null</c> if an error occurred
        /// </returns>
        public IEnumerable<string> ListLanguages()
        {
            var request = this.SetupRequest(SubDBAction.Languages);
            var response = this.ExecuteRequest(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.Split(',');
            }

            return null;
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="hash">The filename hash specified in the site</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>
        /// A list of available languages for the searched subtitle
        /// </returns>
        /// <exception cref="ArgumentException">You must search using a valid hash - hash</exception>
        public IEnumerable<string> Search(string hash, bool returnVersions = false)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("You must search using a valid hash", "hash");
            }

            var request = this.SetupRequest(SubDBAction.Search, hash);
            if (returnVersions)
            {
                request.AddQueryParameter("versions", string.Empty);
            }

            var response = this.ExecuteRequest(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.Split(',');
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<string>();
            }

            return null;
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileStream">The filename stream to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>
        /// A list of available languages for the searched subtitle
        /// </returns>
        /// <exception cref="ArgumentNullException">File stream must exist</exception>
        /// <exception cref="ArgumentException">File stream must be readable and seekable</exception>
        public IEnumerable<string> Search(Stream fileStream, bool returnVersions = false)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("File stream must exist", nameof(fileStream));
            }

            if (!fileStream.CanRead || !fileStream.CanSeek)
            {
                throw new ArgumentException("File stream must be readable and seekable", nameof(fileStream));
            }

            if (fileStream.Length < SubDBHashHelper.HashSize)
            {
                throw new ArgumentException(string.Format("File size must be greater than {0} bytes", SubDBHashHelper.HashSize), nameof(fileStream));
            }

            return this.Search(SubDBHashHelper.GetHashFromStream(fileStream), returnVersions);
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileBytes">The file bytes of the all file to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>
        /// A list of available languages for the searched subtitle
        /// </returns>
        public IEnumerable<string> Search(byte[] fileBytes, bool returnVersions = false)
        {
            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                return this.Search(ms, returnVersions);
            }
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="hash">The file hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>
        /// The subtitle contents, an empty string if not found and null if an error has occurred
        /// </returns>
        /// <exception cref="ArgumentException">
        /// You must search using a valid hash - hash
        /// or
        /// At least one language must be provided - languages
        /// </exception>
        public string Download(string hash, string languages)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("You must search using a valid hash", nameof(hash));
            }

            if (string.IsNullOrWhiteSpace(languages))
            {
                throw new ArgumentException("At least one language must be provided", nameof(languages));
            }

            var request = this.SetupRequest(SubDBAction.Download, hash);
            request.AddQueryParameter("language", languages);

            var response = this.ExecuteRequest(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileStream">The file stream to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>
        /// The subtitle contents, an empty string if not found and null if an error has occurred
        /// </returns>
        /// <exception cref="ArgumentNullException">File stream must exist</exception>
        /// <exception cref="ArgumentException">File stream must be readable and seekable</exception>
        public string Download(Stream fileStream, string languages)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("File stream must exist", nameof(fileStream));
            }

            if (!fileStream.CanRead || !fileStream.CanSeek)
            {
                throw new ArgumentException("File stream must be readable and seekable", nameof(fileStream));
            }

            if (fileStream.Length < SubDBHashHelper.HashSize)
            {
                throw new ArgumentException(string.Format("File size must be greater than {0} bytes", SubDBHashHelper.HashSize), nameof(fileStream));
            }

            return this.Download(SubDBHashHelper.GetHashFromStream(fileStream), languages);
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileBytes">The file bytes to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>
        /// The subtitle contents, an empty string if not found and null if an error has occurred
        /// </returns>
        public string Download(byte[] fileBytes, string languages)
        {
            using (var ms = new MemoryStream(fileBytes))
            {
                return this.Download(ms, languages);
            }
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The result of executing the rest request</returns>
        protected virtual IRestResponse ExecuteRequest(IRestRequest request)
        {
            return this.RestClient.Execute(request);
        }

        /// <summary>
        /// Setups the request.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="hash">The hash.</param>
        /// <returns>A pre-initialized rest request</returns>
        protected virtual IRestRequest SetupRequest(SubDBAction action, string hash = null)
        {
            var result = new RestRequest();
            result.Method = Method.GET;
            result.AddQueryParameter("action", action.ToString().ToLower());
            if (!string.IsNullOrWhiteSpace(hash))
            {
                result.AddQueryParameter("hash", hash);
            }

            return result;
        }
    }
}
