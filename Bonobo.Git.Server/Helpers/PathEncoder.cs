using System;
using System.Text;

namespace Bonobo.Git.Server.Helpers
{
    /// <summary>
    /// Helper class that encodes/decodes path fragments for safe use in a URL.
    /// </summary>
    /// <remarks>
    /// Avoids triggering IIS's "HTTP Error 404.11 URL Double Escaped" exception
    /// (http://support.microsoft.com/kb/942076) for paths with problem characters
    /// like '+' by encoding/decoding path fragments via modified base 64 for URL
    /// (http://en.wikipedia.org/wiki/Base_64).
    /// </remarks>
    public static class PathEncoder
    {
        /// <summary>
        /// Encodes a path fragment.
        /// </summary>
        /// <param name="path">Path fragment.</param>
        /// <returns>Encoded path fragment.</returns>
        public static string Encode(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // No need to encode; return as-is
                return path;
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(path)).Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// Decodes a path fragment.
        /// </summary>
        /// <param name="encodedPath">Path fragment.</param>
        /// <returns>Decoded path fragment.</returns>
        public static string Decode(string encodedPath)
        {
            if (string.IsNullOrEmpty(encodedPath))
            {
                // No need to decode; return as-is
                return encodedPath;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedPath.Replace('-', '+').Replace('_', '/')));
        }
    }
}
