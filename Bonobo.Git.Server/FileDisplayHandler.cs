using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Xml;
using Ude;
using MimeTypes;

namespace Bonobo.Git.Server
{
    public static class FileDisplayHandler
    {
        public const string NoBrush = "nohighlight";

        public static bool IsImage(string fileName)
        {
            return MimeTypeMap.GetMimeType(Path.GetExtension(fileName.ToLower())).Contains("image");
        }

        public static string GetBrush(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)) 
            {
                throw new ArgumentNullException("fileName");
            }

            var extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".vb":
                    return "vb";

                case ".cs":
                    return "csharp";

                case ".as":
                    return "as3";

                case ".sh":
                    return "bash";

                case ".html":
                case ".htm":
                case ".xhtml":
                case ".xslt":
                case ".xml":
                case ".asp":
                case ".aspx":
                case ".cshtml":
                case ".xaml":
                case ".csproj":
                case ".config":
                    return "html";

                case ".cf":
                    return "cf";

                case ".h":
                case ".c":
                case ".cpp":
                    return "cpp";

                case ".css":
                    return "css";

                case ".pas":
                    return "delphi";

                case ".diff":
                case ".patch":
                    return "diff";

                case ".erl":
                case ".xlr":
                case ".hlr":
                    return "erlang";

                case ".groovy":
                    return "groovy";

                case ".js":
                case ".jscript":
                case ".javascript":
                    return "js";

                case ".java":
                    return "java";

                case ".fx":
                    return "jfx";

                case ".pir":
                case ".pm":
                case ".pl":
                    return "perl";

                case ".php":
                    return "php";

                case ".ps1":
                case ".psm1":
                    return "ps";

                case ".py":
                    return "python";

                case ".rb":
                    return "ruby";

                case ".scala":
                    return "scala";

                case ".sql":
                    return "sql";
                default:
                    return NoBrush;
            }
        }

        public static string GetText(byte[] data)
        {
            if (data.Length == 0)
            {
                return string.Empty;
            }

            Encoding encoding = GetEncoding(data);
            return encoding != null ? new StreamReader(new MemoryStream(data), encoding, true).ReadToEnd() : null;
        }

        public static Encoding GetEncoding(byte[] data)
        {
            ICharsetDetector cdet = new CharsetDetector();
            cdet.Feed(data, 0, data.Length);
            cdet.DataEnd();
            if (cdet.Charset != null)
            {
                if (cdet.Charset.ToLowerInvariant() == "big-5")
                {
                    return Encoding.GetEncoding("big5");
                }
                else
                {
                    try
                    {
                        return Encoding.GetEncoding(cdet.Charset);
                    }
                    catch
                    {
                        return Encoding.Default;
                    }
                }
            }

            return Encoding.Default;
        }



        /// <summary>
        /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
        /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
        /// </summary>
        public static string GetFileSizeString(long i)
        {
            long absolute_i = (i < 0 ? -i : i);
            string suffix;
            double readable;

            // GB is enough for a VCS I think
            if (absolute_i >= 0x40000000)
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000)
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400)
            {
                suffix = "kB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B");
            }
            // Divide by 1024 to get fractional value
            readable = readable / 1024;
            return readable.ToString("0.### ") + suffix;
        }
    }
}