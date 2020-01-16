namespace Bonobo.Git.Server.Helpers
{
    internal static class ByteUtils
    {
        internal static int IndexOf(byte[] array, byte[] pattern, int offset)
        {
            var success = 0;
            for (var i = offset; i < array.Length; i++)
            {
                if (array[i] == pattern[success])
                {
                    success++;
                }
                else
                {
                    success = 0;
                }

                if (pattern.Length == success)
                {
                    return i - pattern.Length + 1;
                }
            }
            return -1;
        }
    }
}