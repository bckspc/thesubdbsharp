namespace TheSubDBSharp.Client
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The <see cref="ISubDBClient"/> interface.
    /// </summary>
    public interface ISubDBClient
    {
        #region Public Methods

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="hash">     The file hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        SubDBSubtitle Download(string hash, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileStream">The file stream to hash</param>
        /// <param name="languages"> The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        SubDBSubtitle Download(Stream fileStream, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileBytes">The file bytes to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        SubDBSubtitle Download(byte[] fileBytes, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="hash">     The file hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        Task<SubDBSubtitle> DownloadAsync(string hash, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileStream">The file stream to hash</param>
        /// <param name="languages"> The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        Task<SubDBSubtitle> DownloadAsync(Stream fileStream, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileBytes">The file bytes to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents or null if an error has occurred</returns>
        Task<SubDBSubtitle> DownloadAsync(byte[] fileBytes, string languages);

        /// <summary>
        /// Lists the available languages.
        /// </summary>
        /// <returns>A list of available languages or <c>null</c> if an error occurred</returns>
        IEnumerable<string> ListLanguages();

        /// <summary>
        /// Lists the available languages.
        /// </summary>
        /// <returns>A list of available languages or <c>null</c> if an error occurred</returns>
        Task<IEnumerable<string>> ListLanguagesAsync();

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="hash">          The filename hash specified in the site</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        IEnumerable<string> Search(string hash, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="file">          The filename stream to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        IEnumerable<string> Search(Stream file, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileBytes">     The file bytes to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        IEnumerable<string> Search(byte[] fileBytes, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="hash">          The filename hash specified in the site</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        Task<IEnumerable<string>> SearchAsync(string hash, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="file">          The filename stream to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        Task<IEnumerable<string>> SearchAsync(Stream file, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileBytes">     The file bytes to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        Task<IEnumerable<string>> SearchAsync(byte[] fileBytes, bool returnVersions = false);

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="hash">              The hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        SubDBUploadResponse Upload(string hash, Stream subtitleFileStream);

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="hash">             The hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        SubDBUploadResponse Upload(string hash, byte[] subtitleFileBytes);

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="fileStreamToHash">  The file stream to hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        SubDBUploadResponse Upload(Stream fileStreamToHash, Stream subtitleFileStream);

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="fileBytesToHash">  The file bytes to hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        SubDBUploadResponse Upload(byte[] fileBytesToHash, byte[] subtitleFileBytes);

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="hash">              The hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        Task<SubDBUploadResponse> UploadAsync(string hash, Stream subtitleFileStream);

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="hash">             The hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        Task<SubDBUploadResponse> UploadAsync(string hash, byte[] subtitleFileBytes);

        /// <summary>
        /// Uploads the specified file stream.
        /// </summary>
        /// <param name="fileStreamToHash">  The file stream to hash.</param>
        /// <param name="subtitleFileStream">The file stream.</param>
        /// <returns>The upload result</returns>
        Task<SubDBUploadResponse> UploadAsync(Stream fileStreamToHash, Stream subtitleFileStream);

        /// <summary>
        /// Uploads the specified file bytes.
        /// </summary>
        /// <param name="fileBytesToHash">  The file bytes to hash.</param>
        /// <param name="subtitleFileBytes">The file bytes.</param>
        /// <returns>The upload result</returns>
        Task<SubDBUploadResponse> UploadAsync(byte[] fileBytesToHash, byte[] subtitleFileBytes);

        #endregion Public Methods
    }
}