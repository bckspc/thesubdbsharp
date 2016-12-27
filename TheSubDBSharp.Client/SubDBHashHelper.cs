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
        #region Public Fields

        /// <summary>
        /// The chunk size (64 * 1024) ( chunks of 64kb)
        /// </summary>
        public const int ChunkSize = 65536;

        /// <summary>
        /// The hash size (2 * 64 * 1024) (two chunks of 64kb)
        /// </summary>
        public const int HashSize = ChunkSize * 2;

        #endregion Public Fields

        #region Public Methods

        /// <summary>
        /// Gets the hash from bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>An hash from the specified stream</returns>
        public static string GetHashFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return SubDBHashHelper.GetHashFromStream(ms);
            }
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

        /// <summary>
        /// Gets the hash from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>An hash from the specified stream</returns>
        public static string GetHashFromStream(Stream stream)
        {
            byte[] buffer = new byte[HashSize];
            stream.Seek(0, SeekOrigin.Begin);
            int bytesRead = stream.Read(buffer, 0, ChunkSize);
            stream.Seek(-1 * ChunkSize, SeekOrigin.End);
            stream.Read(buffer, bytesRead, ChunkSize);

            MD5 m = MD5.Create();
            byte[] data = m.ComputeHash(buffer);
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }

        #endregion Public Methods
    }
}