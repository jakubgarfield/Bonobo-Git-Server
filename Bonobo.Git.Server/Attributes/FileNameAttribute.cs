using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Bonobo.Git.Server
{
    public class FileNameAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                return value.ToString().IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            }

            return base.IsValid(null);
        }
    }
}