namespace TheSubDBSharp.Client
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// The <see cref="ISubDBClient"/> interface.
    /// </summary>
    public interface ISubDBClient
    {
        /// <summary>
        /// Lists the available languages.
        /// </summary>
        /// <returns>A list of available languages or <c>null</c> if an error occurred</returns>
        IEnumerable<string> ListLanguages();

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="hash">The filename hash specified in the site</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        IEnumerable<string> Search(string hash, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="file">The filename stream to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        IEnumerable<string> Search(Stream file, bool returnVersions = false);

        /// <summary>
        /// Searches for subtitles with the file hash
        /// </summary>
        /// <param name="fileBytes">The file bytes to be hashed</param>
        /// <param name="returnVersions">if set to <c>true</c> will return the language and how many versions per language with the format: <c>(LANGUAGE):(COUNT)</c>.</param>
        /// <returns>A list of available languages for the searched subtitle</returns>
        IEnumerable<string> Search(byte[] fileBytes, bool returnVersions = false);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="hash">The file hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents, an empty string if not found and null if an error has occurred</returns>
        string Download(string hash, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileStream">The file stream to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents, an empty string if not found and null if an error has occurred</returns>
        string Download(Stream fileStream, string languages);

        /// <summary>
        /// Downloads the specified subtitle.
        /// </summary>
        /// <param name="fileBytes">The file bytes to hash</param>
        /// <param name="languages">The parameter language can be a single language code (ex.: us), or a comma separated list in order of priority (ex.: us,nl). When using a comma separated list, the first subtitle found is returned.</param>
        /// <returns>The subtitle contents, an empty string if not found and null if an error has occurred</returns>
        string Download(byte[] fileBytes, string languages);
    }
}
