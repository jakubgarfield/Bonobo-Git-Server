using System.Net.Mime;
using System.Text;

namespace Bonobo.Git.Server.Helpers
{
    /// <summary>
    /// Http Content Disposition Util. Class exctracted from System.Web.Mvc.dll, v4.0.0.0
    /// </summary>
    internal static class ContentDispositionUtil
    {
        private const string HexDigits = "0123456789ABCDEF";

        private static void AddByteToStringBuilder(byte b, StringBuilder builder)
        {
            builder.Append('%');
            int num = (int)b;
            AddHexDigitToStringBuilder(num >> 4, builder);
            AddHexDigitToStringBuilder(num % 16, builder);
        }

        private static void AddHexDigitToStringBuilder(int digit, StringBuilder builder)
        {
            builder.Append("0123456789ABCDEF"[digit]);
        }

        private static string CreateRfc2231HeaderValue(string filename)
        {
            StringBuilder builder = new StringBuilder("attachment; filename*=UTF-8''");
            foreach (byte b in Encoding.UTF8.GetBytes(filename))
            {
                if (IsByteValidHeaderValueCharacter(b))
                    builder.Append((char)b);
                else
                    AddByteToStringBuilder(b, builder);
            }
            return ((object)builder).ToString();
        }

        public static string GetHeaderValue(string fileName)
        {
            foreach (int num in fileName)
            {
                if (num > (int)sbyte.MaxValue)
                    return CreateRfc2231HeaderValue(fileName);
            }
            return new ContentDisposition()
            {
                FileName = fileName
            }.ToString();
        }

        private static bool IsByteValidHeaderValueCharacter(byte b)
        {
            if (48 <= (int)b && (int)b <= 57 || 97 <= (int)b && (int)b <= 122 || 65 <= (int)b && (int)b <= 90)
                return true;
            byte num = b;
            if ((uint)num <= 46U)
            {
                switch (num)
                {
                    case (byte)33:
                    case (byte)36:
                    case (byte)38:
                    case (byte)43:
                    case (byte)45:
                    case (byte)46:
                        break;
                    default:
                        goto label_6;
                }
            }
            else if ((int)num != 58 && (int)num != 95 && (int)num != 126)
                goto label_6;
            return true;
        label_6:
            return false;
        }
    }
}