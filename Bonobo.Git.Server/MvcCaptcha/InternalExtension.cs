using System.Text;

namespace TSharp.Core.Mvc
{
    public static class InternalExtension
    {
        public static StringBuilder Append(this string origion, string next)
        {
            return new StringBuilder(origion).Append(next);
        }
    }
}