using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Bonobo.Git.Server
{
    public class FileNameAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                return value.ToString().IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
            }

            return base.IsValid(value);
        }
    }
}