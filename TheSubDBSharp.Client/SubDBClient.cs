namespace TheSubDBSharp.Client
{
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// The <see cref="SubDBClient"/> class.
    /// </summary>
    /// <seealso cref="TheSubDBSharp.Client.ISubDBClient"/>
    public class SubDBClient : ISubDBClient
    {
        #region Private Fields

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

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubDBClient"/> class.
        /// </summary>
        /// <param name="baseUrl">      The base URL.</param>
        /// <param name="clientName">   Name of the client.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="clientUrl">    The client URL.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "Client url can be an invalid url")]
        public SubDBClient(string baseUrl, string clientName, string clientVersion, string clientUrl)
            : this(new Uri(baseUrl), clientName, clientVersion, clientUrl)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubDBClient"/> class.
        /// </summary>
        /// <param name="baseUrl">      The base URL.</param>
        /// <param name="clientName">   Name of the client.</param>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="clientUrl">    The client URL.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "Client url can be an invalid url")]
        public SubDBClient(Uri baseUrl, string clientName, string clientVersion, string clientUrl)
            : this(new RestClient(baseUrl) { UserAgent = string.Format(UserAgentFormat, protocolName, protocolVersion, clientName, clientVersion, clientUrl) })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubDBClient"/> class.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        public SubDBClient(IRestClient restClient)
        {
            this.restClient = new Lazy<IRestClient>(() => restClient);
            this.baseUrl = restClient.BaseUrl;
            this.userAgent = restClient.UserAgent;
        }

        #endregion Public Constructors

        #region Protected Properties

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The rest client.</value>
        protected IRestClient RestClient
        {
            get
            {
                return this.restClient.Value;
            }
        }

        #endregion Protected Properties

        #region Public Methods

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="hash">     The file hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        /// <exception cref="ArgumentException">You must search using a valid hash or At least one language must be provided</exception>
        public SubDBSubtitle Download(string hash, string languages)
        {
            this.ValidateDownload(hash, languages);
            var request = this.SetupDownloadRequest(hash, languages);
            var response = this.ExecuteRequest(request);
            return this.ProcessDownloadResponse(response);
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileStream">The file stream to hash</param>
        /// <param name="languages"> The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        /// <exception cref="ArgumentNullException">File stream must exist</exception>
        /// <exception cref="ArgumentException">File stream must be readable and seekable</exception>
        public SubDBSubtitle Download(Stream fileStream, string languages)
        {
            this.ValidateFileStream(fileStream);
            return this.Download(SubDBHashHelper.GetHashFromStream(fileStream), languages);
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileBytes">The file bytes to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        /// <exception cref="System.ArgumentNullException">File must exist</exception>
        public SubDBSubtitle Download(byte[] fileBytes, string languages)
        {
            this.ValidateFileBytes(fileBytes);

            using (var ms = new MemoryStream(fileBytes))
            {
                return this.Download(ms, languages);
            }
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="hash">     The file hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        /// <exception cref="ArgumentException">You must search using a valid hash or At least one language must be provided</exception>
        public async Task<SubDBSubtitle> DownloadAsync(string hash, string languages)
        {
            this.ValidateDownload(hash, languages);
            var request = this.SetupDownloadRequest(hash, languages);
            return await this.ExecuteRequestAsync(request).ContinueWith(response => this.ProcessDownloadResponse(response.Result));
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileStream">The file stream to hash</param>
        /// <param name="languages"> The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        /// <exception cref="ArgumentNullException">File stream must exist</exception>
        /// <exception cref="ArgumentException">File stream must be readable and seekable</exception>
        public async Task<SubDBSubtitle> DownloadAsync(Stream fileStream, string languages)
        {
            this.ValidateFileStream(fileStream);
            return await this.DownloadAsync(SubDBHashHelper.GetHashFromStream(fileStream), languages);
        }

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileBytes">The file bytes to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        /// <exception cref="System.ArgumentNullException">File must exist</exception>
        public async Task<SubDBSubtitle> DownloadAsync(byte[] fileBytes, string languages)
        {
            this.ValidateFileBytes(fileBytes);

            using (var ms = new MemoryStream(fileBytes))
            {
                return await this.DownloadAsync(ms, languages);
            }
        }

        /// <summary>
        /// Gets the languages.
        /// </summary>
        /// <returns>A list of available languages or <c>null</c> if an error occurred</returns>
        public IEnumerable<string> ListLanguages()
        {
            var request = this.SetupRequest(SubDBAction.Languages);
            var response = this.ExecuteRequest(request);
            return this.ProcessListLanguagesResponse(response);
        }

        /// <summary>
        /// Gets the languages.
        /// </summary>
        /// <returns>A list of available languages or <c>null</c> if an error occurred</returns>
        public async Task<IEnumerable<string>> ListLanguagesAsync()
        {
            var request = this.SetupRequest(SubDBAction.Languages);
            return await this.ExecuteRequestAsync(request).ContinueWith<IEnumerable<string>>(response => this.ProcessListLanguagesResponse(response.Result));
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="hash">          The filename hash specified in the site</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        /// <exception cref="ArgumentException">You must search using a valid hash - hash</exception>
        public IEnumerable<string> Search(string hash, bool returnVersions = false)
        {
            this.ValidateHash(hash);
            var request = this.ProcessSearchRequest(hash, returnVersions);
            var response = this.ExecuteRequest(request);
            return this.ProcessSearchResponse(response);
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileStream">    The filename stream to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        /// <exception cref="ArgumentNullException">File stream must exist</exception>
        /// <exception cref="ArgumentException">File stream must be readable and seekable</exception>
        public IEnumerable<string> Search(Stream fileStream, bool returnVersions = false)
        {
            this.ValidateFileStream(fileStream);
            return this.Search(SubDBHashHelper.GetHashFromStream(fileStream), returnVersions);
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileBytes">     The file bytes of the all file to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        public IEnumerable<string> Search(byte[] fileBytes, bool returnVersions = false)
        {
            this.ValidateFileBytes(fileBytes);
            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                return this.Search(ms, returnVersions);
            }
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="hash">          The filename hash specified in the site</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        /// <exception cref="ArgumentException">You must search using a valid hash - hash</exception>
        public async Task<IEnumerable<string>> SearchAsync(string hash, bool returnVersions = false)
        {
            this.ValidateHash(hash);
            var request = this.ProcessSearchRequest(hash, returnVersions);
            return await this.ExecuteRequestAsync(request).ContinueWith(response => this.ProcessSearchResponse(response.Result));
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileStream">    The filename stream to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        /// <exception cref="ArgumentNullException">File stream must exist</exception>
        /// <exception cref="ArgumentException">File stream must be readable and seekable</exception>
        public async Task<IEnumerable<string>> SearchAsync(Stream fileStream, bool returnVersions = false)
        {
            this.ValidateFileStream(fileStream);
            return await this.SearchAsync(SubDBHashHelper.GetHashFromStream(fileStream), returnVersions);
        }

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileBytes">     The file bytes of the all file to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        public async Task<IEnumerable<string>> SearchAsync(byte[] fileBytes, bool returnVersions = false)
        {
            this.ValidateFileBytes(fileBytes);

            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                return await this.SearchAsync(ms, returnVersions);
            }
        }

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="fileStreamToHash">  The file stream to hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        public SubDBUploadResponse Upload(Stream fileStreamToHash, Stream subtitleFileStream)
        {
            this.ValidateFileStream(fileStreamToHash);
            this.ValidateFileStream(subtitleFileStream, true, false, false);

            using (var ms = new MemoryStream())
            {
                subtitleFileStream.CopyTo(ms);
                return this.Upload(SubDBHashHelper.GetHashFromStream(fileStreamToHash), ms.ToArray());
            }
        }

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="fileBytesToHash">  The file bytes to hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        public SubDBUploadResponse Upload(byte[] fileBytesToHash, byte[] subtitleFileBytes)
        {
            this.ValidateFileBytes(fileBytesToHash);
            this.ValidateFileBytes(subtitleFileBytes, false);
            var request = this.SetupUploadRequest(SubDBHashHelper.GetHashFromBytes(fileBytesToHash), subtitleFileBytes);
            var response = this.ExecuteRequest(request);
            return this.ProcessUploadResponse(response);
        }

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="hash">              The hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        public SubDBUploadResponse Upload(string hash, Stream subtitleFileStream)
        {
            this.ValidateHash(hash);
            this.ValidateFileStream(subtitleFileStream, true, false, false);

            using (var ms = new MemoryStream())
            {
                subtitleFileStream.CopyTo(ms);
                return this.Upload(hash, ms.ToArray());
            }
        }

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="hash">             The hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        public SubDBUploadResponse Upload(string hash, byte[] subtitleFileBytes)
        {
            this.ValidateHash(hash);
            this.ValidateFileBytes(subtitleFileBytes, false);
            var request = this.SetupUploadRequest(hash, subtitleFileBytes);
            var response = this.ExecuteRequest(request);
            return this.ProcessUploadResponse(response);
        }

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="fileStreamToHash">  The file stream to hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        public async Task<SubDBUploadResponse> UploadAsync(Stream fileStreamToHash, Stream subtitleFileStream)
        {
            this.ValidateFileStream(fileStreamToHash);
            this.ValidateFileStream(subtitleFileStream, true, false, false);

            using (var ms = new MemoryStream())
            {
                subtitleFileStream.CopyTo(ms);
                return await this.UploadAsync(SubDBHashHelper.GetHashFromStream(fileStreamToHash), ms.ToArray());
            }
        }

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="fileBytesToHash">  The file bytes to hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        public async Task<SubDBUploadResponse> UploadAsync(byte[] fileBytesToHash, byte[] subtitleFileBytes)
        {
            this.ValidateFileBytes(fileBytesToHash);
            this.ValidateFileBytes(subtitleFileBytes, false);
            var request = this.SetupUploadRequest(SubDBHashHelper.GetHashFromBytes(fileBytesToHash), subtitleFileBytes);
            return await this.ExecuteRequestAsync(request).ContinueWith(response => this.ProcessUploadResponse(response.Result));
        }

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="hash">              The hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        public async Task<SubDBUploadResponse> UploadAsync(string hash, Stream subtitleFileStream)
        {
            this.ValidateHash(hash);
            this.ValidateFileStream(subtitleFileStream, true, false, false);

            using (var ms = new MemoryStream())
            {
                subtitleFileStream.CopyTo(ms);
                return await this.UploadAsync(hash, ms.ToArray());
            }
        }

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="hash">             The hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        public async Task<SubDBUploadResponse> UploadAsync(string hash, byte[] subtitleFileBytes)
        {
            this.ValidateHash(hash);
            this.ValidateFileBytes(subtitleFileBytes, false);
            var request = this.SetupUploadRequest(hash, subtitleFileBytes);
            return await this.ExecuteRequestAsync(request).ContinueWith(response => this.ProcessUploadResponse(response.Result));
        }

        #endregion Public Methods

        #region Protected Methods

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
        /// Executes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The result of executing the rest request</returns>
        protected virtual Task<IRestResponse> ExecuteRequestAsync(IRestRequest request)
        {
            var tcs = new TaskCompletionSource<IRestResponse>();

            this.RestClient.ExecuteAsync(
                request,
                response =>
                {
                    tcs.SetResult(response);
                });

            return tcs.Task;
        }

        /// <summary>
        /// Setups the request.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="hash">  The hash.</param>
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

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Processes the download response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The subtitle content</returns>
        private SubDBSubtitle ProcessDownloadResponse(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var language = response.Headers?.SingleOrDefault(x => x.Name == "Content-Language")?.Value?.ToString();
                if (string.IsNullOrEmpty(language))
                {
                    language = null;
                }

                return new SubDBSubtitle { Content = response.Content, Language = language };
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new SubDBSubtitle { NotFound = true };
            }

            return null;
        }

        /// <summary>
        /// Processes the list languages response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>A list of available languages or <c>null</c> if an error occurred</returns>
        private IEnumerable<string> ProcessListLanguagesResponse(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.Split(',');
            }

            return null;
        }

        /// <summary>
        /// Processes the search request.
        /// </summary>
        /// <param name="hash">          The hash.</param>
        /// <param name="returnVersions">if set to <c>true</c> [return versions].</param>
        /// <returns>The search rest request</returns>
        private IRestRequest ProcessSearchRequest(string hash, bool returnVersions)
        {
            var request = this.SetupRequest(SubDBAction.Search, hash);
            if (returnVersions)
            {
                request.AddQueryParameter("versions", string.Empty);
            }

            return request;
        }

        /// <summary>
        /// Processes the search response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The search rest response</returns>
        private IEnumerable<string> ProcessSearchResponse(IRestResponse response)
        {
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
        /// Processes the upload response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The parsed upload response</returns>
        private SubDBUploadResponse ProcessUploadResponse(IRestResponse response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return SubDBUploadResponse.Uploaded;

                case HttpStatusCode.Forbidden:
                    return SubDBUploadResponse.Duplicated;

                case HttpStatusCode.UnsupportedMediaType:
                    return SubDBUploadResponse.Invalid;

                default:
                    return SubDBUploadResponse.Error;
            }
        }

        /// <summary>
        /// Setups the download request.
        /// </summary>
        /// <param name="hash">     The hash.</param>
        /// <param name="languages">The languages.</param>
        /// <returns>The request</returns>
        private IRestRequest SetupDownloadRequest(string hash, string languages)
        {
            var request = this.SetupRequest(SubDBAction.Download, hash);
            request.AddQueryParameter("language", languages);
            return request;
        }

        /// <summary>
        /// Setups the upload request.
        /// </summary>
        /// <param name="hash">     The hash.</param>
        /// <param name="fileBytes">The file bytes.</param>
        /// <returns>the upload rest request</returns>
        private IRestRequest SetupUploadRequest(string hash, byte[] fileBytes)
        {
            var request = this.SetupRequest(SubDBAction.Upload);
            request.Method = Method.POST;
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddParameter("hash", hash, null, ParameterType.RequestBody);
            request.AddFileBytes("file", fileBytes, "subtitle.srt", "application/octet-stream");

            return request;
        }

        /// <summary>
        /// Validates the download.
        /// </summary>
        /// <param name="hash">     The hash.</param>
        /// <param name="languages">The languages.</param>
        /// <exception cref="System.ArgumentException">At least one language must be provided - languages</exception>
        private void ValidateDownload(string hash, string languages)
        {
            this.ValidateHash(hash);

            if (string.IsNullOrWhiteSpace(languages))
            {
                throw new ArgumentException("At least one language must be provided", nameof(languages));
            }
        }

        /// <summary>
        /// Validates the upload.
        /// </summary>
        /// <param name="fileBytes">        The file bytes.</param>
        /// <param name="validateMinLength">if set to <c>true</c> [validate minimum length].</param>
        /// <exception cref="System.ArgumentException">The file bytes array must be filled</exception>
        private void ValidateFileBytes(byte[] fileBytes, bool validateMinLength = true)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new ArgumentNullException("The file bytes array must be filled", nameof(fileBytes));
            }

            if (validateMinLength && fileBytes.Length < SubDBHashHelper.HashSize)
            {
                throw new ArgumentException(string.Format("File size must be greater than {0} bytes", SubDBHashHelper.HashSize), nameof(fileBytes));
            }
        }

        /// <summary>
        /// Validates the download.
        /// </summary>
        /// <param name="fileStream">  The file stream.</param>
        /// <param name="validateRead">if set to <c>true</c> [validate read].</param>
        /// <param name="validateSeek">if set to <c>true</c> [validate seek].</param>
        /// <param name="validateSize">if set to <c>true</c> [validate size].</param>
        /// <exception cref="System.ArgumentNullException">File stream must exist</exception>
        /// <exception cref="System.ArgumentException">File stream must be readable and seekable or You must provide at least one language to download</exception>
        private void ValidateFileStream(Stream fileStream, bool validateRead = true, bool validateSeek = true, bool validateSize = true)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("File stream must exist", nameof(fileStream));
            }

            if ((validateRead && !fileStream.CanRead) || (validateSeek && !fileStream.CanSeek))
            {
                throw new ArgumentException("File stream must be readable and seekable", nameof(fileStream));
            }

            if (validateSize && fileStream.Length < SubDBHashHelper.HashSize)
            {
                throw new ArgumentException(string.Format("File size must be greater than {0} bytes", SubDBHashHelper.HashSize), nameof(fileStream));
            }
        }

        /// <summary>
        /// Validates the hash.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <exception cref="System.ArgumentException">You must search/download using a valid hash - hash</exception>
        private void ValidateHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("You must search/download using a valid hash", nameof(hash));
            }
        }

        #endregion Private Methods
    }
}