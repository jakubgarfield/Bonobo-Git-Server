using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data
{
    public interface INameProperty
    {
        Guid Id { get; }
        string Name { get; }
        string DisplayName { get; }
    }
}