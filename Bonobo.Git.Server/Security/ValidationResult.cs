using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Security
{
    public enum ValidationResult
    {
        Success,
        Failure,
        NotAuthorized
    }
}