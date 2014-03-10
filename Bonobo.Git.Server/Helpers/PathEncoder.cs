using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bonobo.Git.Server.Helpers
{
    /// <summary>
    /// Helper class that encodes/decodes path fragments for safe use in a URL.
    /// </summary>
    /// <remarks>
    /// Avoids triggering IIS's "HTTP Error 404.11 URL Double Escaped" exception
    /// (http://support.microsoft.com/kb/942076) for paths with problem characters
    /// like '+' by encoding/decoding path fragments in a manner similar to URL
    /// encoding (http://en.wikipedia.org/wiki/Percent-encoding) except that '~'
    /// is used as the escape character instead of '%' and ' ' is encoded instead
    /// of being mapped to '+'. '~' was chosen as the escape character because it
    /// avoids the double encoding problem of '%' and is the least commonly used
    /// unreserved character in path fragments.
    /// </remarks>
    public static class PathEncoder
    {
        /// <summary>
        /// Encodes a path fragment.
        /// </summary>
        /// <param name="path">Path fragment.</param>
        /// <param name="allowSlash">True if '/' should be allowed (i.e., not encoded).</param>
        /// <returns>Encoded path fragment.</returns>
        public static string Encode(string path, bool allowSlash = false)
        {
            // Check for trivial input
            if (string.IsNullOrEmpty(path))
            {
                // No need to encode; return as-is
                return path;
            }
            // Convert input to a sequence of bytes
            var bytes = Encoding.UTF8.GetBytes(path);
            // Create a StringBuilder with the expected (best case/minimum) size
            var sb = new StringBuilder(bytes.Length);
            // Encode each byte
            foreach (var b in bytes)
            {
                /// RFC 3986 section 2.3 "Unreserved Characters" (http://tools.ietf.org/html/rfc3986):
                /// ALPHA / DIGIT / '-' / '.' / '_' / '~'
                if ((('a' <= b) && (b <= 'z')) || // a-z
                    (('A' <= b) && (b <= 'Z')) || // A-Z
                    (('0' <= b) && (b <= '9')) || // 0-9
                    ('-' == b) || ('.' == b) || ('_' == b) || // - . _
                    (allowSlash && ('/' == b))) // Allow /
                {
                    // Unreserved characters don't need encoding
                    sb.Append((char)b);
                }
                else
                {
                    // Other characters (including the escape character '~') get URL encoded
                    sb.Append('~');
                    sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }
            }
            // Return encoded string
            return sb.ToString();
        }

        /// <summary>
        /// Decodes a path fragment.
        /// </summary>
        /// <param name="encodedPath">Path fragment.</param>
        /// <returns>Decoded path fragment.</returns>
        public static string Decode(string encodedPath)
        {
            // Check for trivial input
            if (string.IsNullOrEmpty(encodedPath))
            {
                // No need to decode; return as-is
                return encodedPath;
            }

            // Capture length
            var encodedPathLength = encodedPath.Length;
            // Create a list of bytes with the maximum (typical) size
            var bytes = new List<byte>(encodedPathLength);
            // Decode each byte
            for (var i = 0; i < encodedPathLength; i++)
            {
                // Capture current char/byte
                var c = encodedPath[i];
                var b = (byte)c;
                if (c != b)
                {
                    // Throw for invalid input (non-byte character)
                    throw new FormatException("Invalid non-byte input character.");
                }
                if ('~' == b)
                {
                    // Decode URL encoded character
                    byte value;
                    if ((encodedPathLength <= i + 2) ||
                        !byte.TryParse(encodedPath.Substring(i + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                    {
                        // Throw for invalid input (insufficient space or non-hex value)
                        throw new FormatException("Invalid format for encoded path character.");
                    }
                    // Add decoded byte and advance index
                    bytes.Add(value);
                    i += 2;
                }
                else
                {
                    // Add unreserved characters as-is
                    bytes.Add(b);
                }
            }
            // Return decoded string
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
