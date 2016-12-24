namespace TheSubDBSharp.Client
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// The <see cref="SubDBHashHelper"/> class.
    /// </summary>
    public static class SubDBHashHelper
    {
        /// <summary>
        /// The hash size (2 * 64 * 1024) (two chunks of 64kb)
        /// </summary>
        public const int HashSize = 131072;

        /// <summary>
        /// Gets the hash from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>An hash from the specified stream</returns>
        public static string GetHashFromStream(Stream stream)
        {
            byte[] buffer = new byte[HashSize * 2];
            stream.Seek(0, SeekOrigin.Begin);
            int bytesRead = stream.Read(buffer, 0, HashSize);
            stream.Seek(-1 * HashSize, SeekOrigin.End);
            stream.Read(buffer, bytesRead, HashSize);

            MD5 m = MD5.Create();
            byte[] data = m.ComputeHash(buffer);
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }

        /// <summary>
        /// Gets the hash from stream.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>An hash from the file</returns>
        public static string GetHashFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return GetHashFromStream(stream);
            }
        }
    }
}
