using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Git
{
    public interface IGitRepositoryLocator
    {
        DirectoryInfo GetRepositoryDirectoryPath(string repository);
    }
}