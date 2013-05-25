using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update
{
    public interface IUpdateScript
    {
        string Command { get; }
        string Precondition { get; }
    }
}